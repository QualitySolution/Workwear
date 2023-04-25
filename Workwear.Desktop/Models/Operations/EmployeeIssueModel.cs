using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
	public class EmployeeIssueModel {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private Dictionary<string, IssueGraph> graphs = new Dictionary<string, IssueGraph>();
		
		public EmployeeIssueModel(EmployeeIssueRepository employeeIssueRepository, UnitOfWorkProvider unitOfWorkProvider = null) {
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.unitOfWorkProvider = unitOfWorkProvider;
		}

		#region public
		/// <summary>
		/// Выполняет пресчёт даты(даты начала использования и даты списания) выдачи в переданных операциях
		/// </summary>
		/// <param name="operations">Операции которые требуется пересчитать</param>
		/// <param name="baseParameters">Для параметров учета</param>
		/// <param name="interactive">Для вопросов при пересчете.</param>
		/// <param name="uow">Используется для сохранения измененных строк сотрудника.</param>
		/// <param name="progress">Прогрес бар, можно передать уже начатый. Количество шагов метода будет равно количеству операция + 3.</param>
		/// <param name="cancellation">Токен отмены операции</param>
		public void RecalculateDateOfIssue(IList<EmployeeIssueOperation> operations, BaseParameters baseParameters, IInteractiveQuestion interactive, IProgressBarDisplayable progress = null, CancellationToken? cancellation = null, IUnitOfWork uow = null, Action<EmployeeCard, string[]> changeLog = null) {
			uow = uow ?? unitOfWorkProvider.UoW;
			bool needClose = false;
			if(progress != null && !progress.IsStarted) {
				progress.Start(operations.Count() + 3);
				needClose = true;
			}
			progress?.Add(text: "Получаем информацию о прошлых выдачах");
			CheckAndPrepareGraphs(operations.Select(o => o.Employee).Distinct().ToArray(), operations.Select(o => o.ProtectionTools).Distinct().ToArray());
			progress?.Add();
			foreach(var employeeGroup in operations.GroupBy(x => x.Employee)) {
				progress?.Update($"Обработка {employeeGroup.Key.ShortName}");
				var changes = new List<string>();

				foreach(var operation in employeeGroup.OrderBy(x => x.OperationTime)) {
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
					progress?.Add();
				}
				changeLog?.Invoke(employeeGroup.Key, changes.ToArray());
			}

			progress?.Add(text: "Завершаем...");
			uow.Commit();
			if(needClose)
				progress.Close();
		}

		/// <summary>
		/// Выполняет пересчет всех даты следующих выдач для всех сотрудников.
		/// </summary>
		/// <param name="progress">Можно предать начатый прогресс, количество шагов прогресса равно количеству сотрудников + 1</param>
		public void UpdateNextIssueAll(EmployeeCard[] employees, IProgressBarDisplayable progress = null, CancellationToken? cancellation = null, uint commitBatchSize = 1000, Action<EmployeeCard, string[]> changeLog = null) {
			bool needClose = false;
			IUnitOfWork uow = unitOfWorkProvider.UoW;
			if(progress != null && !progress.IsStarted) {
				progress.Start(employees.Length + 1);
				needClose = true;
			}
			progress?.Add(text: "Получаем информацию о прошлых выдачах");
			CheckAndPrepareGraphs(employees);

			int step = 0;
			foreach(var employee in employees) {
				if(cancellation?.IsCancellationRequested ?? false) {
					break;
				}
				progress?.Add(text: $"Обработка {employee.ShortName}");
				step++;
				var oldDates = employee.WorkwearItems.Select(x => x.NextIssue).ToArray();
				
				foreach(var wearItem in employee.WorkwearItems) {
					wearItem.Graph = GetPreparedOrEmptyGraph(employee, wearItem.ProtectionTools);
					wearItem.UpdateNextIssue(uow);
				}
				
				var changes = employee.WorkwearItems.Select((x, i) => x.NextIssue?.Date != oldDates[i]?.Date ? $"Изменена дата следующей выдачи с {oldDates[i]:d} на {x.NextIssue:d} для потребности [{x.Title}]" : null)
					.Where(x => x != null).ToArray();
				changeLog?.Invoke(employee, changes);
				if(changes.Any())
					uow.Save(employee);
				if(step % commitBatchSize == 0)
					uow.Commit();
			}
			progress?.Add(text: "Готово");
			if(needClose)
				progress.Close();
		}
		
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
					graphs[GetKey(employeeGroup.Key, protectionToolsGroup.Key)] = new IssueGraph(protectionToolsGroup.ToList());
				}
			}
		}

		private IssueGraph GetPreparedOrEmptyGraph(EmployeeCard employee, ProtectionTools protectionTools) {
			var key = GetKey(employee, protectionTools);
			if(graphs.TryGetValue(key, out IssueGraph graph))
				return graph;
			return new IssueGraph(new List<EmployeeIssueOperation>());
		} 
		
		private static string GetKey(EmployeeCard e, ProtectionTools p) => $"{e.Id}_{p.Id}";
		#endregion

		#region Employees
		public void FillWearReceivedInfo(EmployeeCard[] employees, EmployeeIssueOperation[] notSavedOperations = null) {
			if(!employees.Any())
				return;
			var operations = employeeIssueRepository.AllOperationsFor(employees).ToList();
			if(notSavedOperations != null)
				operations.AddRange(notSavedOperations);
			foreach(var employee in employees) {
				employee.FillWearReceivedInfo(operations.Where(x => x.Employee.IsSame(employee)).ToList());
			}
		}
		#endregion
	}
}
