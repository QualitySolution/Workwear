using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using NLog.Targets;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Services;
using QS.Utilities;
using QS.Utilities.Text;
using QS.ViewModels.Resolve;
using Workwear.Domain.Company;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using workwear.Journal.Filter.ViewModels.Company;
using Workwear.Models.Operations;
using Workwear.Repository.Operations;
using Workwear.Repository.Regulations;
using Workwear.Tools;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Tools
{
	[DontUseAsDefaultViewModel]
	public class EmployeeProcessingJournalViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeProcessingJournalNode>
	{
		NLog.Logger loggerProcessing = NLog.LogManager.GetLogger("EmployeeProcessing");
		private string logFile = NLog.LogManager.Configuration.FindTargetByName<FileTarget>("EmployeeProcessing").FileName.Render(new NLog.LogEventInfo { TimeStamp = DateTime.Now });

		private readonly IInteractiveService interactive;
		private readonly ILifetimeScope autofacScope;
		private readonly NormRepository normRepository;
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly BaseParameters baseParameters;
		private readonly IDataBaseInfo dataBaseInfo;
		private readonly EmployeeIssueModel issueModel;
		/// <summary>
		/// Внимание все диалоги создаются отменяемыми!!! Не забывайте использовать токен отмены.
		/// </summary>
		private readonly ModalProgressCreator progressCreator;

		public EmployeeFilterViewModel Filter { get; private set; }

		public EmployeeProcessingJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService, ILifetimeScope autofacScope, 
			NormRepository normRepository,
			EmployeeIssueRepository employeeIssueRepository,
			BaseParameters baseParameters,
			IDataBaseInfo dataBaseInfo,
			EmployeeIssueModel issueModel,
			ModalProgressCreator progressCreator,
			UnitOfWorkProvider unitOfWorkProvider,
			ICurrentPermissionService currentPermissionService = null) 
										: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = false;
			Title = "Корректировка сотрудников";
			unitOfWorkProvider.UoW = UoW;
			this.interactive = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			progressCreator.UserCanCancel = true;
			JournalFilter = Filter = autofacScope.Resolve<EmployeeFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			//Обход проблемы с тем что SelectionMode одновременно управляет и выбором в журнале, и самим режимом журнала.
			//То есть создает действие выбора. Удалить после того как появится рефакторинг действий журнала. 
			SelectionMode = JournalSelectionMode.Multiple;
			NodeActionsList.Clear();
			CreateActions();

			(DataLoader as ThreadDataLoader<EmployeeProcessingJournalNode>).PostLoadProcessingFunc = delegate (System.Collections.IList items, uint addedSince) {
				foreach(EmployeeProcessingJournalNode item in items) {
					if(Results.ContainsKey(item.Id))
						item.Result = Results[item.Id];
				}
			};
		}

		protected override IQueryOver<EmployeeCard> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeProcessingJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeCard employeeAlias = null;
			Norm normAlias = null;

			var employees = uow.Session.QueryOver<EmployeeCard>(() => employeeAlias);
			if(Filter.ShowOnlyWork)
				employees.Where(x => x.DismissDate == null);
			if(Filter.Subdivision != null)
				employees.Where(x => x.Subdivision.Id == Filter.Subdivision.Id);
			if(Filter.Department != null)
				employees.Where(x => x.Department.Id == Filter.Department.Id);

			var normProjection = CustomProjections.GroupConcat(Projections.SqlFunction("coalesce", NHibernateUtil.String, Projections.Property(() => normAlias.Name), Projections.Property(() => normAlias.Id)), separator: "\n");

			return employees
				.Where(GetSearchCriterion(
					() => employeeAlias.Id,
					() => employeeAlias.CardNumber,
					() => employeeAlias.PersonnelNumber,
					() => employeeAlias.LastName,
					() => employeeAlias.FirstName,
					() => employeeAlias.Patronymic,
					() => postAlias.Name,
					() => subdivisionAlias.Name
 					))
				.JoinAlias(() => employeeAlias.Post, () => postAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => employeeAlias.UsedNorms, () => normAlias)
				.SelectList((list) => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CardNumber).WithAlias(() => resultAlias.CardNumber)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.LastName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => employeeAlias.DismissDate).WithAlias(() => resultAlias.DismissDate)
					.Select(() => postAlias.Name).WithAlias(() => resultAlias.Post)
	   				.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(normProjection).WithAlias(() => resultAlias.Norms)
 					)
				.OrderBy(() => employeeAlias.LastName).Asc
				.ThenBy(() => employeeAlias.FirstName).Asc
				.ThenBy(() => employeeAlias.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeProcessingJournalNode>());
		} 

		#region Действия
		void CreateActions()
		{
			var loadAllAction = new JournalAction("Загрузить всех",
					(selected) => true,
					(selected) => true,
					(selected) => LoadAll()
					);
			NodeActionsList.Add(loadAllAction);

			var editAction = new JournalAction("Открыть сотрудника",
					(selected) => selected.Any(),
					(selected) => VisibleEditAction,
					(selected) => selected.Cast<EmployeeProcessingJournalNode>().ToList().ForEach(EditEntityDialog)
					);
			NodeActionsList.Add(editAction);
			RowActivatedAction = editAction;

			var updateStatusAction = new JournalAction("Установить нормы",
				(selected) => selected.Any(),
				(selected) => true
			);
			NodeActionsList.Add(updateStatusAction);

			var updateOnlyFirstNorm = new JournalAction("Только первую норму для должности",
				selected => selected.Any(),
				selected => true,
				selected => CatchExceptionAndCloseProgress(UpdateNorms, selected.Cast<EmployeeProcessingJournalNode>().ToArray())
			);
			updateStatusAction.ChildActionsList.Add(updateOnlyFirstNorm);

			var updateAllNorms = new JournalAction("Все нормы для должности",
				selected => selected.Any(),
				selected => true,
				selected => CatchExceptionAndCloseProgress(UpdateAllNorms, selected.Cast<EmployeeProcessingJournalNode>().ToArray())
			);
			updateStatusAction.ChildActionsList.Add(updateAllNorms);
			
			var removeAllNorms = new JournalAction("Удалить все нормы",
				selected => selected.Any(),
				selected => true,
				selected => CatchExceptionAndCloseProgress(RemoveAllNorms, selected.Cast<EmployeeProcessingJournalNode>().ToArray())
			);
			updateStatusAction.ChildActionsList.Add(removeAllNorms);

			var recalculateAction = new JournalAction("Пересчитать",
					(selected) => selected.Any(),
					(selected) => true
					);
			NodeActionsList.Add(recalculateAction);

			var updateNextIssueAction = new JournalAction("Даты следующего получения",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => CatchExceptionAndCloseProgress(UpdateNextIssue, selected.Cast<EmployeeProcessingJournalNode>().ToArray())
					);
			recalculateAction.ChildActionsList.Add(updateNextIssueAction);

			var updateLastIssueAction = new JournalAction("Сроки носки у полученного",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => CatchExceptionAndCloseProgress(UpdateLastIssue, selected.Cast<EmployeeProcessingJournalNode>().ToArray())
					);
			recalculateAction.ChildActionsList.Add(updateLastIssueAction);

			var logAction = new JournalAction("Лог выполнения",
					(selected) => File.Exists(logFile),
					(selected) => true,
					(selected) => System.Diagnostics.Process.Start(logFile)
					);
			NodeActionsList.Add(logAction);
		}

		private Dictionary<int, string> Results = new Dictionary<int, string>();

		void UpdateNorms(EmployeeProcessingJournalNode[] nodes)
		{
			progressCreator.Start(nodes.Length + 3, text: "Загружаем сотрудников");
			var cancellation = progressCreator.CancellationToken;
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id));
			progressCreator.Add(text: "Загружаем нормы");
			var norms = normRepository.GetNormsForPost(UoW, employees.Select(x => x.Post).Where(x => x != null).Distinct().ToArray());

			int step = 0;

			foreach(var employee in employees) {
				if(cancellation.IsCancellationRequested) {
					break;
				}
				progressCreator.Add(text: $"Обработка {employee.ShortName}");
				if(employee.Post == null) {
					Results[employee.Id] = "Отсутствует должность";
					continue;
				}
				var norm = norms.FirstOrDefault(x => x.IsActive && x.Posts.Contains(employee.Post));
				if(norm != null) {
					step++;
					employee.UsedNorms.Clear();
					employee.AddUsedNorm(norm);
					UoW.Save(employee);
					Results[employee.Id] = "ОК";
					if(step % 10 == 0)
						UoW.Commit();
				}
				else {
					Results[employee.Id] = "Подходящая норма не найдена";
				}
			}

			if(!cancellation.IsCancellationRequested) {
				progressCreator.Add(text: "Готово");
				UoW.Commit();
				progressCreator.Add(text: "Обновляем журнал");
				Refresh();
				progressCreator.Close();
			}
		}

		private void UpdateAllNorms(EmployeeProcessingJournalNode[] nodes) {
			progressCreator.Start(nodes.Length + 3, text: "Загружаем сотрудников");
			var cancellation = progressCreator.CancellationToken;
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id));
			progressCreator.Add(text: "Загружаем нормы");
			var norms = normRepository.GetNormsForPost(UoW, employees.Select(x => x.Post)
				.Where(x => x != null)
				.Distinct()
				.ToArray());

			var step = 0;

			foreach(var employee in employees) {
				if(cancellation.IsCancellationRequested) {
					break;
				}
				progressCreator.Add(text: $"Обработка {employee.ShortName}");
				if(employee.Post == null) {
					Results[employee.Id] = "Отсутствует должность";
					continue;
				}

				var normsForEmployee = norms
					.Where(x => x.IsActive && x.Posts.Contains(employee.Post))
					.Distinct()
					.ToList();
				if(normsForEmployee.Any()) {
					step++;
					employee.UsedNorms.Clear();
					employee.AddUsedNorms(normsForEmployee);
					UoW.Save(employee);
					Results[employee.Id] = $"ОК({normsForEmployee.Count})";
					if(step % 10 == 0)
						UoW.Commit();
				}
				else
					Results[employee.Id] = "Подходящая норма не найдена";
			}

			if(!cancellation.IsCancellationRequested) {
				progressCreator.Add(text: "Завершаем транзакцию");
				UoW.Commit();
				progressCreator.Add(text: "Обновляем журнал");
				Refresh();
				progressCreator.Close();
			}
		}

		private void RemoveAllNorms(EmployeeProcessingJournalNode[] nodes) {
			progressCreator.Start(nodes.Length + 2, text: "Загружаем сотрудников");
			var cancellation = progressCreator.CancellationToken;
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id));
			var step = 0;

			foreach(var employee in employees) {
				if(cancellation.IsCancellationRequested) {
					break;
				}
				progressCreator.Add(text: $"Обработка {employee.ShortName}");
				if(employee.UsedNorms.Any()) {
					var normCount = employee.UsedNorms.Count;
					step++;
					employee.UsedNorms.Clear();
					employee.WorkwearItems.Clear();
					UoW.Save(employee);
					Results[employee.Id] = $"Удалено {normCount} норм";
					if(step % 10 == 0)
						UoW.Commit();
				}
				else {
					Results[employee.Id] = "Нормы не установлены";
				}
			}

			if(!cancellation.IsCancellationRequested) {
				progressCreator.Add(text: "Завершаем транзакцию");
				UoW.Commit();
				progressCreator.Add(text: "Обновляем журнал");
				Refresh();
				progressCreator.Close();
			}
		}

		void UpdateNextIssue(EmployeeProcessingJournalNode[] nodes)
		{
			loggerProcessing.Info($"Пересчет даты следующией выдачи для {nodes.Length} сотрудников");
			loggerProcessing.Info($"База данных: {dataBaseInfo.Name}");
			
			progressCreator.Start(nodes.Length + 3, text: "Загружаем сотрудников");
			var cancellation = progressCreator.CancellationToken;
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id)).ToArray();
			
			issueModel.UpdateNextIssueAll(employees, progressCreator, cancellation, 10,
				(employee, changes) => {
					if(changes.Length > 0) {
						Results[employee.Id] = NumberToTextRus.FormatCase(changes.Length, "изменена {0} строка", "изменено {0} строки", "изменено {0} строк");
						foreach(var message in changes)
							loggerProcessing.Info(message);
					}
					else
						Results[employee.Id] = "Без изменений";
				});
			if(cancellation.IsCancellationRequested)
				return;
			progressCreator.Add(text: "Завершаем...");
			UoW.Commit();
			progressCreator.Add(text: "Обновляем журнал");
			Refresh();
			progressCreator.Close();
		}

		void UpdateLastIssue(EmployeeProcessingJournalNode[] nodes)
		{
			loggerProcessing.Info($"Пересчет сроков носки получного для {nodes.Length} сотрудников");
			loggerProcessing.Info($"База данных: {dataBaseInfo.Name}");
			progressCreator.Start(nodes.Length + 1, text: "Загружаем сотрудников");
			var cancellation = progressCreator.CancellationToken;
			
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id)).ToArray();
			progressCreator.Add(text: "Получаем последние выдачи");
			var operations = employeeIssueRepository.GetLastIssueOperationsForEmployee(employees);

			progressCreator.Update("Проверка выданного");
			HashSet<int> operationsEmployeeIds = new HashSet<int>(operations.Select(x => x.Id)); 
			foreach(var employee in employees) {
				progressCreator.Add();
				if(!operationsEmployeeIds.Contains(employee.Id)) 
					Results[employee.Id] = "Нет выданного";
			}

			progressCreator.Close();
			if (cancellation.IsCancellationRequested)
				return;
			progressCreator.Start(operations.Count + 3, text: "Пересчет даты последней выдачи");
			cancellation = progressCreator.CancellationToken;
			issueModel.RecalculateDateOfIssue(operations, baseParameters, interactive, progress: progressCreator, cancellation: cancellation, 
				changeLog: (employee, changes) => {
					if(changes.Length > 0) {
						Results[employee.Id] =
							NumberToTextRus.FormatCase(changes.Length, "изменена {0} дата", "изменено {0} даты", "изменено {0} дат");
						foreach(var message in changes)
							loggerProcessing.Info(message);
					}
					else
						Results[employee.Id] = "Без изменений";
				});
			if (cancellation.IsCancellationRequested)
				return;
			progressCreator.Add(text: "Обновляем журнал");
			Refresh();
			progressCreator.Close();
		}

		void LoadAll()
		{
			DataLoader.DynamicLoadingEnabled = false;
			Refresh();
		}
		#endregion

		#region Помощьники

		private void CatchExceptionAndCloseProgress(Action<EmployeeProcessingJournalNode[]> action, EmployeeProcessingJournalNode[] arg) {
			try {
				action(arg);
			}
			catch(Exception e) {
				if (progressCreator.IsStarted)
					progressCreator.Close();
				throw e;
			}
		}

		#endregion
	}

	public class EmployeeProcessingJournalNode
	{
		public int Id { get; set; }
		[SearchHighlight]
		public string CardNumber { get; set; }

		[SearchHighlight]
		public string CardNumberText {
			get {
				return CardNumber ?? Id.ToString();
			}
		}

		[SearchHighlight]
		public string PersonnelNumber { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }

		[SearchHighlight]
		public string FIO {
			get {
				return String.Join(" ", LastName, FirstName, Patronymic);
			}
		}
		[SearchHighlight]
		public string Post { get; set; }
		[SearchHighlight]
		public string Subdivision { get; set; }

		public bool Dismiss { get { return DismissDate.HasValue; } }

		public DateTime? DismissDate { get; set; }

		public string Norms { get; set; }

		public string Result { get; set; }
	}
}
