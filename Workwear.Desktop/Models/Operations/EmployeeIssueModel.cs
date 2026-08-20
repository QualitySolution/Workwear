using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace Workwear.Models.Operations {
	/// <summary>
	/// Выполняет полные сценарии работы с потребностями и выдачами сотрудников:
	/// загружает операции, строит графы, пересчитывает потребности и сохраняет изменения в текущий UnitOfWork.
	/// Методы обновления потребностей и следующих выдач не выполняют Commit — им управляет вызывающий код.
	/// </summary>
	public class EmployeeIssueModel {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private Dictionary<string, IssueGraph> graphs = new Dictionary<string, IssueGraph>();
		
		public EmployeeIssueModel(EmployeeIssueRepository employeeIssueRepository, UnitOfWorkProvider unitOfWorkProvider = null) {
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.unitOfWorkProvider = unitOfWorkProvider;
		}

		#region Helpers
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		#endregion

		#region public
		/// <summary>
		/// Выполняет пресчёт даты(даты начала использования и даты списания) выдачи в переданных операциях
		/// </summary>
		/// <param name="operations">Операции которые требуется пересчитать</param>
		/// <param name="baseParameters">Для параметров учета</param>
		/// <param name="interactive">Для вопросов при пересчете.</param>
		/// <param name="uow">Используется для сохранения измененных строк сотрудника.</param>
		/// <param name="progress">Прогрес бар, можно передать уже начатый. Количество шагов метода будет равно количеству операция + 2.</param>
		/// <param name="cancellation">Токен отмены операции</param>
		public void RecalculateDateOfIssue(IList<EmployeeIssueOperation> operations, BaseParameters baseParameters, IInteractiveQuestion interactive, IProgressBarDisplayable progress = null, CancellationToken? cancellation = null, IUnitOfWork uow = null, Action<EmployeeCard, string[]> changeLog = null) {
			uow = uow ?? unitOfWorkProvider.UoW;
			bool needClose = false;
			if(progress != null && !progress.IsStarted) {
				progress.Start(operations.Count() + 2);
				needClose = true;
			}
			progress?.Add(text: "Получаем информацию о прошлых выдачах");
			CheckAndPrepareGraphs(operations.Select(o => o.Employee).Distinct().ToArray(), operations.Select(o => o.ProtectionTools).Distinct().ToArray());
			foreach(var employeeGroup in operations.GroupBy(x => x.Employee)) {
				if (cancellation?.IsCancellationRequested == true)
					return;
				progress?.Update($"Обработка {employeeGroup.Key.ShortName}");
				var changes = new List<string>();

				foreach(var operation in employeeGroup.OrderBy(x => x.OperationTime)) {
					if (cancellation?.IsCancellationRequested == true)
						return;
					progress?.Add();
					if (operation.ProtectionTools == null)
						continue;
					var oldExpiry = operation.ExpiryByNorm;
					var graph = graphs[GetKey(employeeGroup.Key, operation.ProtectionTools)];
					var cardItem = operation.Employee.WorkwearItems
						.FirstOrDefault(x =>
							DomainHelper.EqualDomainObjects(x.ProtectionTools, operation.ProtectionTools));

					operation.RecalculateDatesOfIssueOperation(graph, baseParameters, interactive);
					uow.Save(operation);
					graph.Refresh();

					if(operation.ExpiryByNorm?.Date != oldExpiry?.Date) {
						changes.Add($"Изменена дата окончания носки с {oldExpiry:d} на {operation.ExpiryByNorm:d} для выдачи {operation.OperationTime} [{operation.Title}]");
					}

					if(cardItem != null) {
						var oldNextIssue = cardItem.NextIssue;
						cardItem.Graph = graph;
						cardItem.UpdateNextIssue(uow);
						if(cardItem.NextIssue?.Date != oldNextIssue?.Date) {
							uow.Save(cardItem);
							changes.Add($"Изменена дата следующей выдачи с {oldNextIssue:d} на {cardItem.NextIssue:d} для потребности [{cardItem.Title}]");
						}
					}
				}
				changeLog?.Invoke(employeeGroup.Key, changes.ToArray());
			}

			progress?.Add(text: "Завершаем...");
			uow.Commit();
			if(needClose)
				progress.Close();
		}

		/// <summary>
		/// Одним запросом загружает историю выдач и пересчитывает даты следующей выдачи для всех сотрудников.
		/// Метод сохраняет изменения в UnitOfWork, но не выполняет Commit.
		/// Для массовой обработки можно передать callback, вызываемый после каждого обработанного пакета.
		/// </summary>
		public void UpdateNextIssueAll(
			EmployeeCard[] employees,
			IProgressBarDisplayable progress = null,
			CancellationToken? cancellation = null,
			Action<EmployeeCard, string[]> changeLog = null,
			EmployeeIssueOperation[] notSavedOperations = null,
			IUnitOfWork uow = null,
			int batchSize = 0,
			Action batchProcessed = null) {
			if(batchProcessed != null && batchSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(batchSize), "Размер пакета должен быть больше нуля.");

			bool needClose = false;
			uow = GetRequiredUow(uow);
			if(progress != null && !progress.IsStarted) {
				progress.Start(employees.Length * 2 + 3);
				needClose = true;
			}
			FillWearReceivedInfo(employees, notSavedOperations, progress, uow);

			int processedCount = 0;
			foreach(var employee in employees) {
				if(cancellation?.IsCancellationRequested ?? false) {
					break;
				}
				progress?.Add(text: $"Обработка {employee.ShortName}");
				var oldDates = employee.WorkwearItems.Select(x => x.NextIssue).ToArray();
				
				foreach(var wearItem in employee.WorkwearItems) {
					wearItem.UpdateNextIssue(uow);
				}
				
				var changes = employee.WorkwearItems.Select((x, i) => x.NextIssue?.Date != oldDates[i]?.Date ? $"Изменена дата следующей выдачи с {oldDates[i]:d} на {x.NextIssue:d} для потребности [{x.Title}]" : null)
					.Where(x => x != null).ToArray();
				changeLog?.Invoke(employee, changes);
				if(changes.Any())
					uow.Save(employee);

				processedCount++;
				if(batchProcessed != null && processedCount % batchSize == 0)
					batchProcessed();
			}
			if (progress != null && !(cancellation?.IsCancellationRequested ?? false))
				progress.Add(text: "Готово");
			if(needClose)
				progress.Close();
		}

		/// <summary>
		/// Выполняет полный пересчёт потребностей: перестраивает коллекции по нормам,
		/// загружает прошлые выдачи, обновляет даты и сохраняет сотрудников без Commit.
		/// </summary>
		public void UpdateWorkwearItems(
			IEnumerable<EmployeeCard> employees,
			IUnitOfWork uow = null,
			IProgressBarDisplayable progress = null,
			CancellationToken? cancellation = null) {
			if(employees == null)
				throw new ArgumentNullException(nameof(employees));

			uow = GetRequiredUow(uow);
			var employeeArray = employees.Distinct().ToArray();
			foreach(var employee in employeeArray)
				employee.UpdateWorkwearItemsCollection();

			UpdateWorkwearItemsAfterCollectionChanged(employeeArray, uow, progress, cancellation);
		}

		/// <summary>
		/// Добавляет норму и выполняет полный пересчёт потребностей сотрудника без Commit.
		/// </summary>
		public bool AddUsedNorm(EmployeeCard employee, Norm norm, IUnitOfWork uow = null) {
			if(employee == null)
				throw new ArgumentNullException(nameof(employee));

			var oldCount = employee.UsedNorms.Count;
			employee.AddUsedNorm(norm);
			if(employee.UsedNorms.Count == oldCount)
				return false;

			UpdateWorkwearItemsAfterCollectionChanged(new[] { employee }, uow);
			return true;
		}

		/// <summary>
		/// Добавляет нормы и выполняет один полный пересчёт потребностей сотрудника без Commit.
		/// </summary>
		public int AddUsedNorms(EmployeeCard employee, IEnumerable<Norm> norms, IUnitOfWork uow = null) {
			if(employee == null)
				throw new ArgumentNullException(nameof(employee));

			var oldCount = employee.UsedNorms.Count;
			employee.AddUsedNorms(norms);
			var addedCount = employee.UsedNorms.Count - oldCount;
			if(addedCount > 0)
				UpdateWorkwearItemsAfterCollectionChanged(new[] { employee }, uow);
			return addedCount;
		}

		/// <summary>
		/// Удаляет норму и выполняет полный пересчёт потребностей сотрудника без Commit.
		/// </summary>
		public bool RemoveUsedNorm(EmployeeCard employee, Norm norm, IUnitOfWork uow = null) {
			if(employee == null)
				throw new ArgumentNullException(nameof(employee));

			var oldCount = employee.UsedNorms.Count;
			employee.RemoveUsedNorm(norm);
			if(employee.UsedNorms.Count == oldCount)
				return false;

			UpdateWorkwearItemsAfterCollectionChanged(new[] { employee }, uow);
			return true;
		}

		private void UpdateWorkwearItemsAfterCollectionChanged(
			EmployeeCard[] employees,
			IUnitOfWork uow,
			IProgressBarDisplayable progress = null,
			CancellationToken? cancellation = null) {
			uow = GetRequiredUow(uow);
			UpdateNextIssueAll(employees, progress, cancellation, uow: uow);
			foreach(var employee in employees)
				uow.Save(employee);
		}

		private IUnitOfWork GetRequiredUow(IUnitOfWork uow) =>
			uow ?? unitOfWorkProvider?.UoW ?? throw new InvalidOperationException(
				$"Для выполнения сценария {nameof(EmployeeIssueModel)} необходим UnitOfWork.");
		
		/// <summary>
		/// Выполняет пересчет всех даты следующих выдач связанные с перечисленными операциями.
		/// </summary>
		/// <param name="progress">Можно предать начатый прогресс, количество шагов прогресса равно количеству операций + 1</param>
		public void UpdateNextIssue(EmployeeIssueOperation[] operations, IProgressBarDisplayable progress = null, CancellationToken? cancellation = null, Action<EmployeeIssueOperation, string> changeLog = null) {
			bool needClose = false;
			IUnitOfWork uow = unitOfWorkProvider.UoW;
			if(progress != null && !progress.IsStarted) {
				progress.Start(operations.Length + 1);
				needClose = true;
			}
			progress?.Add(text: "Получаем информацию о прошлых выдачах");
			var employees = operations.Select(x => x.Employee).Distinct().ToArray();
			var protectionTools = operations.Select(x => x.ProtectionTools).Distinct().ToArray();
			CheckAndPrepareGraphs(employees, protectionTools);

			int step = 0;
			foreach(var operation in operations) {
				if(cancellation?.IsCancellationRequested ?? false) {
					break;
				}

				var employee = operation.Employee;
				progress?.Add(text: $"Обработка {employee.ShortName}");
				step++;
				
				if(operation.ProtectionTools == null)
					continue;

				var wearItem = employee.WorkwearItems.FirstOrDefault(i => operation.ProtectionTools.IsSame(i.ProtectionTools));
				if(wearItem == null) 
					continue;
				
				var oldDate = wearItem.NextIssue;
				
				wearItem.Graph = GetPreparedOrEmptyGraph(employee, wearItem.ProtectionTools);
				wearItem.UpdateNextIssue(uow);

				if(wearItem.NextIssue != oldDate) {
					changeLog?.Invoke(operation, $"Изменена дата следующей выдачи с {oldDate:d} на {wearItem.NextIssue:d} для потребности [{wearItem.Title}]");
					uow.Save(wearItem);
				}
			}
			progress?.Add(text: "Готово");
			if(needClose)
				progress.Close();
		}

		/// <summary>
		/// Справочник (id операции выдачи / кол-во уже списанное из неё) 
		/// </summary>
		/// <param name="operations">Операции выдачи для которых нужно считать списания</param>
		/// <param name="uow"></param>
		/// <param name="onDate"></param>
		/// <returns></returns>
		public Dictionary<int, int> CalculateWrittenOff(EmployeeIssueOperation[] operations, IUnitOfWork uow, DateTime? onDate = null) {
			var wo = uow.Session.QueryOver<EmployeeIssueOperation>()
					.Where(o => o.IssuedOperation.Id
						.IsIn(operations.Select(x => x.Id).ToArray()));
			if(onDate != null)
				wo.Where(o => o.OperationTime <= onDate);
			
			return wo.List()
				?.GroupBy(o => o.Id)
				.ToDictionary(g => g.Key, g => g.Sum(o => o.Returned));
		}
		#endregion

		#region Graph
		private void CheckAndPrepareGraphs(EmployeeCard[] employees = null, ProtectionTools[] protectionTools = null) {
			if((!employees?.Any() ?? false) && (!protectionTools?.Any() ?? false))
				throw new ArgumentNullException(nameof(employees), $"{nameof(employees)} или {nameof(protectionTools)} должны быть переданы.");

			var operations = employeeIssueRepository.AllOperationsFor(employees, protectionTools);
			foreach(var employeeGroup in operations.GroupBy(o => o.Employee)) {
				foreach(var protectionToolsGroup in employeeGroup.GroupBy(o => o.ProtectionTools)) {
					if(protectionToolsGroup.Key == null)
						continue; //В пересчёте нет смысла без потребности
					graphs[GetKey(employeeGroup.Key, protectionToolsGroup.Key)] = new IssueGraph(protectionToolsGroup.ToList<IGraphIssueOperation>());
				}
			}
		}

		private IssueGraph GetPreparedOrEmptyGraph(EmployeeCard employee, ProtectionTools protectionTools) {
			var key = GetKey(employee, protectionTools);
			if(graphs.TryGetValue(key, out IssueGraph graph))
				return graph;
			return new IssueGraph(new List<IGraphIssueOperation>());
		} 
		
		private static string GetKey(EmployeeCard e, ProtectionTools p) => $"{e.Id}_{p.Id}";
		#endregion

		#region Заполение данных по потребностям
		public IList<EmployeeCardItem>  LoadWearItemsForProtectionTools(params int[] protectionToolsIds) {
			UoW.Session.QueryOver<ProtectionTools>()
				.Where(p => p.Id.IsIn(protectionToolsIds))
				.Fetch(SelectMode.Fetch, p => p.Type)
				.Fetch(SelectMode.Fetch, p => p.Type.Units)
				.Fetch(SelectMode.Fetch, p => p.Nomenclatures)
				.Future();
			
			EmployeeCard employee = null;
			var query = UoW.Session.QueryOver<EmployeeCardItem>()
				.Where(i => i.ProtectionTools.IsIn(protectionToolsIds))
				.Fetch(SelectMode.ChildFetch, x => x.EmployeeCard)
				.Fetch(SelectMode.Fetch, x => x.ActiveNormItem)
				.Fetch(SelectMode.Fetch, x => x.ActiveNormItem.Norm)
				.Fetch(SelectMode.Fetch, x => x.ActiveNormItem.NormCondition)
				.JoinAlias(x => x.EmployeeCard, () => employee)
				.Where(() => employee.DismissDate == null)
				.Future();

			return query.ToList();
		}
		
		public void FillWearReceivedInfo(EmployeeCardItem[] employeeCardItems, IProgressBarDisplayable progress = null) {
			progress?.Add(text: "Подгружаем операции");
			if(!employeeCardItems.Any())
				return;
			var employeeIdsByProtectionTools = employeeCardItems
				.GroupBy(x => x.ProtectionTools.Id)
				.ToDictionary(
					group => group.Key,
					group => group.Select(x => x.EmployeeCard.Id).Distinct().ToArray());
			var operations = employeeIssueRepository.AllOperationsForGraph(employeeIdsByProtectionTools);
			logger.Info(
				"Для {0} потребностей по {1} сотрудникам загружено {2} операций для построения графов.",
				employeeCardItems.Length,
				employeeCardItems.Select(x => x.EmployeeCard.Id).Distinct().Count(),
				operations.Count);
			var operationsByEmployee = GroupOperationsByEmployeeAndProtectionTools(operations);
			
			foreach (var item in employeeCardItems) {
				var operationsForItem = operationsByEmployee.TryGetValue(item.EmployeeCard.Id, out var byProtectionTools)
					&& byProtectionTools.TryGetValue(item.ProtectionTools.Id, out var result)
					? result
					: new List<IGraphIssueOperation>();
				item.Graph = new IssueGraph(operationsForItem);
			}
		}
		#endregion
		
		#region Заполение данных в сотрудников
		/// <summary>
		/// Заполняет графы и обновляет дату последней выдачи.
		/// </summary>
		/// <param name="progress">Можно предать начатый прогресс, количество шагов прогресса равно количеству сотрудников + 2</param>
		public void FillWearReceivedInfo(EmployeeCard[] employees, EmployeeIssueOperation[] notSavedOperations = null, IProgressBarDisplayable progress = null, IUnitOfWork uow = null) {
			bool needClose = false;
			if(progress != null && !progress.IsStarted) {
				progress.Start(employees.Length + 2);
				needClose = true;
			}
			progress?.Add(text: "Подгружаем операции");
			if(!employees.Any())
				return;
			
			// Загружаем только нужные поля через проекцию — полные сущности НЕ попадают в Identity Map.
			// Это снижает потребление памяти на крупных базах с ~2 ГБ до десятков МБ.
			var employeeIds = employees.Select(e => e.Id).Where(id => id > 0).ToArray();
			var dtos = employeeIds.Any()
				? employeeIssueRepository.AllOperationsForGraph(employeeIds, uow)
				: new List<GraphIssueOperationDto>();

			progress?.Add(text: "Добавляем несохранённые");
			
			var byEmployee = GroupOperationsByEmployeeAndProtectionTools(dtos);

			// Объединяем с несохранёнными операциями (если есть)
			if(notSavedOperations != null) {
				foreach(var op in notSavedOperations.Where(x => x.ProtectionTools != null)) {
					var empId = op.Employee.Id;
					var ptId = op.ProtectionTools.Id;
					if(!byEmployee.ContainsKey(empId))
						byEmployee[empId] = new Dictionary<int, List<IGraphIssueOperation>>();
					if(!byEmployee[empId].ContainsKey(ptId))
						byEmployee[empId][ptId] = new List<IGraphIssueOperation>();
					byEmployee[empId][ptId].Add(op);
				}
			}

			foreach(var employee in employees) {
				progress?.Add(text: $"Заполняем {employee.ShortName}");
				var byPt = byEmployee.TryGetValue(employee.Id, out var dict)
					? dict
					: new Dictionary<int, List<IGraphIssueOperation>>();
				employee.FillWearReceivedInfo(byPt);
			}
			progress?.Update("Готово");
			if(needClose)
				progress.Close();
		}

		private static Dictionary<int, Dictionary<int, List<IGraphIssueOperation>>>
			GroupOperationsByEmployeeAndProtectionTools(IEnumerable<GraphIssueOperationDto> operations) =>
			operations
				.Where(x => x.ProtectionToolsId != 0)
				.GroupBy(x => x.EmployeeId)
				.ToDictionary(
					employeeGroup => employeeGroup.Key,
					employeeGroup => employeeGroup
						.GroupBy(x => x.ProtectionToolsId)
						.ToDictionary(
							protectionToolsGroup => protectionToolsGroup.Key,
							protectionToolsGroup => protectionToolsGroup.ToList<IGraphIssueOperation>()));

		public void PreloadWearItems(params int[] employeeIds) {
			//Загружаем строки
			EmployeeCardItem employeeCardItemAlias = null;
			var employees = UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.Fetch(SelectMode.ChildFetch, x => x)
				.JoinAlias(x => x.WorkwearItems, () => employeeCardItemAlias, JoinType.LeftOuterJoin)
				.Fetch(SelectMode.Fetch, x => x.WorkwearItems)
				.Fetch(SelectMode.Fetch, () => employeeCardItemAlias.ActiveNormItem)
				.Fetch(SelectMode.Fetch, () => employeeCardItemAlias.ActiveNormItem.NormCondition)
				.List();
			
			//Загружаем СИЗы
			var protectionToolsIds = employees
				.SelectMany(e => e.WorkwearItems)
				.Select(x => x.ProtectionTools.Id)
				.Distinct().ToArray();
			PreloadWearItemsDependencies(employeeIds, protectionToolsIds);
		}

		/// <summary>
		/// Загружает только потребности для переданных пар «сотрудник + номенклатура нормы».
		/// Ключ словаря — Id номенклатуры нормы, значение — Id сотрудников.
		/// </summary>
		public IList<EmployeeCardItem> PreloadWearItems(
			IReadOnlyDictionary<int, int[]> employeeIdsByProtectionTools) {
			if(employeeIdsByProtectionTools == null)
				throw new ArgumentNullException(nameof(employeeIdsByProtectionTools));
			if(!employeeIdsByProtectionTools.Any(x => x.Value?.Any() == true))
				return new List<EmployeeCardItem>();

			EmployeeCardItem itemAlias = null;
			var requestedPairs = Restrictions.Disjunction();
			foreach(var pair in employeeIdsByProtectionTools.Where(x => x.Value?.Any() == true)) {
				requestedPairs.Add(Restrictions.Conjunction()
					.Add(Restrictions.In(
						Projections.Property(() => itemAlias.EmployeeCard.Id),
						pair.Value.Cast<object>().ToArray()))
					.Add(Restrictions.Eq(
						Projections.Property(() => itemAlias.ProtectionTools.Id),
						pair.Key)));
			}

			var items = UoW.Session.QueryOver(() => itemAlias)
				.Where(requestedPairs)
				.Fetch(SelectMode.Fetch, x => x.ActiveNormItem)
				.Fetch(SelectMode.Fetch, x => x.ActiveNormItem.NormCondition)
				.List();

			var employeeIds = employeeIdsByProtectionTools.Values.SelectMany(x => x).Distinct().ToArray();
			var protectionToolsIds = employeeIdsByProtectionTools.Keys.ToArray();
			PreloadWearItemsDependencies(employeeIds, protectionToolsIds);
			return items.Distinct().ToList();
		}

		private void PreloadWearItemsDependencies(int[] employeeIds, int[] protectionToolsIds) {
			//Загружаем выбор номенклатуры сотрудника, чтобы не запускать lazy-запрос при отрисовке таблицы
			EmployeeSelectedNomenclature selectedNomenclatureAlias = null;
			UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.JoinAlias(x => x.SelectedNomenclatures, () => selectedNomenclatureAlias, JoinType.LeftOuterJoin)
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Fetch, x => x.SelectedNomenclatures)
				.Fetch(SelectMode.Fetch, () => selectedNomenclatureAlias.ProtectionTools)
				.Fetch(SelectMode.Fetch, () => selectedNomenclatureAlias.Nomenclature)
				.Future();

			var query = UoW.Session.QueryOver<ProtectionTools>()
				.Where(p => p.Id.IsIn(protectionToolsIds))
				.Fetch(SelectMode.Fetch, p => p.Type)
				.Fetch(SelectMode.Fetch, p => p.Type.Units)
				.Future();

			ProtectionToolsNomenclature protNomAlias = null;
			UoW.Session.QueryOver<ProtectionTools>()
				.Where(p => p.Id.IsIn(protectionToolsIds))
				.JoinAlias(p => p.ProtectionToolsNomenclatures, () => protNomAlias, JoinType.LeftOuterJoin)
				.Fetch(SelectMode.Fetch, p => p.ProtectionToolsNomenclatures)
				.Fetch(SelectMode.Fetch, () => protNomAlias.Nomenclature)
				.Future();

			query.ToList();
		}

		public IList<EmployeeCard> PreloadEmployeeInfo(params int[] employeeIds) {
			var query = UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.Future();
			
			UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Fetch, x => x.Vacations)
				.Future();
			
			UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Fetch, x => x.Sizes)
				.Future();
			return query.ToList();
		}
		
		/// <summary>
		/// Заполняет в сотрудниках(не обязательно в одного) информацию по складским остаткам для строк карточек.
		/// </summary>
		/// <param name="progressStep">Метод вызывается перед каждым шагом, передавая название шага. Метод выполняет 4 шага.</param>
		public void FillWearInStockInfo(
			IEnumerable<EmployeeCard> employees,
			StockBalanceModel stockBalanceModel,
			Action<string> progressStep = null)
		{
			progressStep?.Invoke("Получаем строки потребностей");
			var items = employees.SelectMany(x => x.WorkwearItems).ToList();
			FillWearInStockInfo(items, stockBalanceModel, progressStep);
		}

		/// <summary>
		/// Заполняет в сотрудниках(не обязательно в одного) информацию по складским остаткам для строк карточек.
		/// </summary>
		/// <param name="progressStep">Метод вызывается перед каждым шагом, передавая название шага. Метод выполняет 3 шага.</param>
			public void FillWearInStockInfo(
				IEnumerable<EmployeeCardItem> items,
				StockBalanceModel stockBalanceModel,
				Action<string> progressStep = null)
			{
			progressStep?.Invoke("Получаем список номенклатур");
			var allNomenclatures = 
				items.SelectMany(x => x.ProtectionTools.Nomenclatures).Distinct().ToList();
			progressStep?.Invoke("Обновляем складские остатки при необходимости");
			stockBalanceModel.AddNomenclatures(allNomenclatures);
			progressStep?.Invoke("Заполняем строки карточек");
			foreach(var item in items) {
				item.StockBalanceModel = stockBalanceModel;
			}
		}
		
		/// <summary>
		/// Заполняем в сотрудника информацию по складским остаткам для строк карточек.
		/// </summary>
		/// <param name="progressStep">Метод вызывается перед каждым шагом, передавая название шага. Метод выполняет 4 шага.</param>
		public void FillWearInStockInfo(
			EmployeeCard employee,
			StockBalanceModel stockBalanceModel,
			Action<string> progressStep = null)
		{
			FillWearInStockInfo(new [] {employee}, stockBalanceModel, progressStep);
		}

		public IList<EmployeeCard> LoadEmployeeFullInfo(int[] employeeIds) {
			var query = UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.Fetch(SelectMode.Fetch, x => x.Post)
				.Fetch(SelectMode.Fetch, x => x.Department)
				.Fetch(SelectMode.Fetch, x => x.Subdivision)
				.Future();
				
			UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Fetch, x => x.Vacations)
				.Future();
			
			UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id.IsIn(employeeIds))
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Fetch, x => x.Sizes)
				.Future();
			
				return query.ToList();
		}

		#endregion
	}
}
