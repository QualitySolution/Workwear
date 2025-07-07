using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using NLog.Targets;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Utilities;
using QS.ViewModels.Extension;
using QS.ViewModels.Resolve;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.Journal.ViewModels.Company;
using Workwear.Models.Operations;
using Workwear.Repository.Operations;
using Workwear.Repository.Regulations;
using Workwear.Tools;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Tools
{
	[DontUseAsDefaultViewModel]
	public class EmployeeProcessingJournalViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeProcessingJournalNode>, IDialogDocumentation
	{
		NLog.Logger loggerProcessing = NLog.LogManager.GetLogger("EmployeeProcessing");
		private string logFile = NLog.LogManager.Configuration.FindTargetByName<FileTarget>("EmployeeProcessing").FileName.Render(new NLog.LogEventInfo { TimeStamp = DateTime.Now });

		private readonly IInteractiveService interactive;
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
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("manipulation.html#employee-processing");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip(Title);
		#endregion
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
			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			progressCreator.UserCanCancel = true;
			JournalFilter = Filter = autofacScope.Resolve<EmployeeFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			Filter.CanShowOnlyWithoutNorms = true;
			 
			TableSelectionMode = JournalSelectionMode.Multiple;
			CreateActions();

			(DataLoader as ThreadDataLoader<EmployeeProcessingJournalNode>).PostLoadProcessingFunc = delegate (System.Collections.IList items, uint addedSince) {
				foreach(EmployeeProcessingJournalNode item in items) {
					if(Results.ContainsKey(item.Id)) {
						item.Result = Results[item.Id].text;
						item.ResultColor = Results[item.Id].color;
					}
				}
			};
		}

		protected override IQueryOver<EmployeeCard> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeProcessingJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			Department departmentAlias = null;
			EmployeeCard employeeAlias = null;
			Norm normAlias = null;

			var employees = uow.Session.QueryOver<EmployeeCard>(() => employeeAlias);
			if(Filter.ShowOnlyWork)
				employees.Where(x => x.DismissDate == null);
			if(Filter.ShowOnlyWithoutNorms)
				employees.Where(() => normAlias.Id == null);
			if(Filter.Subdivision != null)
				employees.Where(x => x.Subdivision.Id == Filter.Subdivision.Id);
			if(Filter.Department != null)
				employees.Where(x => x.Department.Id == Filter.Department.Id);
			if(Filter.Post != null)
				employees.Where(x => x.Post.Id == Filter.Post.Id);
			if(Filter.Norm != null) 
				employees.Where(x => normAlias.Id == Filter.Norm.Id);

			var normProjection = CustomProjections.GroupConcat(Projections.SqlFunction("coalesce", NHibernateUtil.String, Projections.Property(() => normAlias.Name), Projections.Property(() => normAlias.Id)), separator: "\n");

			return employees
				.Where(GetSearchCriterion(
					() => employeeAlias.Id,
					() => employeeAlias.CardNumber,
					() => employeeAlias.PersonnelNumber,
					() => employeeAlias.LastName,
					() => employeeAlias.FirstName,
					() => employeeAlias.Patronymic,
					() => normAlias.Name,
					() => normAlias.Id,
					() => postAlias.Name,
					() => subdivisionAlias.Name,
					() => departmentAlias.Name
 					))
				.JoinAlias(() => employeeAlias.Post, () => postAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Department, () => departmentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
					.Select(() => departmentAlias.Name).WithAlias(() => resultAlias.Department)
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
			NodeActionsList.Clear();
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

			#region Установить нормы
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
			#endregion
			
			#region Пересчитать
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

			var updateLastIssueAction = new JournalAction("Сроки носки у последего полученного",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => CatchExceptionAndCloseProgress(UpdateLastIssue, selected.Cast<EmployeeProcessingJournalNode>().ToArray())
					);
			recalculateAction.ChildActionsList.Add(updateLastIssueAction);
			
			var update2LastIssueAction = new JournalAction("Сроки носки у 2 последих получений",
				(selected) => selected.Any(),
				(selected) => true,
				(selected) => CatchExceptionAndCloseProgress(Update2LastIssue, selected.Cast<EmployeeProcessingJournalNode>().ToArray())
			);
			recalculateAction.ChildActionsList.Add(update2LastIssueAction);
			#endregion

			#region Заменить
			var replaceAction = new JournalAction("Заменить",
					(selected) => selected.Any(),
					(selected) => true
					);
			NodeActionsList.Add(replaceAction);
			
			var replaceSubdivisionAction = new JournalAction("Подразделение",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => ReplaceSubdivision(selected.Cast<EmployeeProcessingJournalNode>().ToArray())
					);
			replaceAction.ChildActionsList.Add(replaceSubdivisionAction);
			
			var replaceDepartmentAction = new JournalAction("Отдел",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => ReplaceDepartment(selected.Cast<EmployeeProcessingJournalNode>().ToArray())
					);
			replaceAction.ChildActionsList.Add(replaceDepartmentAction);
			
			var replacePostAction = new JournalAction("Должность",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => ReplacePost(selected.Cast<EmployeeProcessingJournalNode>().ToArray())
					);
				
			var replaceActiveNormItemAction = new JournalAction("Нормы на текущие у 2 последних выдач",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => Active2LastNormItem(selected.Cast<EmployeeProcessingJournalNode>().ToArray())
					);
			replaceAction.ChildActionsList.Add(replaceActiveNormItemAction);
			
			var replaceActiveNormItemFromNomenclaturesAction = new JournalAction("Нормы на текущие у 2 последних выдач по номенклатурам",
				(selected) => selected.Any(),
				(selected) => true,
				(selected) => Active2LastNormItem(selected.Cast<EmployeeProcessingJournalNode>().ToArray(), true)
			);
			replaceAction.ChildActionsList.Add(replaceActiveNormItemFromNomenclaturesAction);
				
			replaceAction.ChildActionsList.Add(replacePostAction);
			#endregion
			
			var logAction = new JournalAction("Лог выполнения",
					(selected) => File.Exists(logFile),
					(selected) => true,
					(selected) => System.Diagnostics.Process.Start(logFile)
					);
			NodeActionsList.Add(logAction);
		}

		private Dictionary<int, (string text, string color)> Results = new Dictionary<int, (string text, string color)>();

		#region Установить нормы
		void UpdateNorms(EmployeeProcessingJournalNode[] nodes)
		{
			progressCreator.Start(nodes.Length + 3, text: "Загружаем сотрудников");
			var cancellation = progressCreator.CancellationToken;
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id));
			progressCreator.Add(text: "Загружаем нормы");
			var norms = normRepository.GetNormsForPost(UoW, employees.Select(x => x.Post).Where(x => x != null).Distinct().ToArray());

			var step = 0;
			var removeNorms = interactive.Question("Удалить прочие нормы у сотрудников?");

			foreach(var employee in employees) {
				if(cancellation.IsCancellationRequested) {
					break;
				}
				progressCreator.Add(text: $"Обработка {employee.ShortName}");
				if(employee.Post == null) {
					Results[employee.Id] = ("Отсутствует должность", "red");
					continue;
				}
				var norm = norms.FirstOrDefault(x => x.IsActive && x.Posts.Contains(employee.Post) && !x.Archival);
				if(norm != null) {
					step++;
					if(removeNorms)
						employee.UsedNorms.Clear();
					employee.AddUsedNorm(norm);
					UoW.Save(employee);
					Results[employee.Id] = ("ОК", "green");
					if(step % 10 == 0)
						UoW.Commit();
				}
				else {
					Results[employee.Id] = ("Подходящая норма не найдена", "red");
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

			var step = 0;
			var removeNorms = interactive.Question("Удалить прочие нормы у сотрудников?");
			
			foreach(var employee in employees) {
				if(cancellation.IsCancellationRequested) {
					break;
				}
				progressCreator.Add(text: $"Обработка {employee.ShortName}");
				if(employee.Post == null) {
					Results[employee.Id] = ("Отсутствует должность", "red");
					continue;
				}
				if(removeNorms) {
					step++;
					employee.UsedNorms.Clear();
					UoW.Save(employee);
				}
				
				var count = employee.NormFromPost(UoW,normRepository);
				if(count != 0) {
					step++;
					UoW.Save(employee);
					Results[employee.Id] = ($"ОК({count})", "green");
				}
				else
					Results[employee.Id] = ("Подходящая норма не найдена", "red");

				if(step > 10) {
					UoW.Commit();
					step = 0;
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
					Results[employee.Id] = ($"Удалено {normCount} норм", "green");
					if(step % 10 == 0)
						UoW.Commit();
				}
				else {
					Results[employee.Id] = ("Нормы не установлены", "red");
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
		#endregion

		#region Пресчет
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
						Results[employee.Id] = (NumberToTextRus.FormatCase(changes.Length, "изменена {0} строка", "изменено {0} строки", "изменено {0} строк"), "green");
						foreach(var message in changes)
							loggerProcessing.Info(message);
					}
					else
						Results[employee.Id] = ("Без изменений", "gray");
				});
			if(cancellation.IsCancellationRequested)
				return;
			progressCreator.Add(text: "Завершаем...");
			UoW.Commit();
			progressCreator.Add(text: "Обновляем журнал");
			Refresh();
			progressCreator.Close();
		}

		void UpdateLastIssue(EmployeeProcessingJournalNode[] nodes) => UpdateIssue(nodes, employeeIssueRepository.GetLastIssueOperationsForEmployee);
		void Update2LastIssue(EmployeeProcessingJournalNode[] nodes) => UpdateIssue(nodes, employeeIssueRepository.GetLast2IssueOperationsForEmployee);
		
		void UpdateIssue(EmployeeProcessingJournalNode[] nodes, Func<IEnumerable<EmployeeCard>, IList<EmployeeIssueOperation>> repoFunc) {
			loggerProcessing.Info($"Пересчет сроков носки последних выдач для {nodes.Length} сотрудников ");
			loggerProcessing.Info($"База данных: {dataBaseInfo.Name}");
			progressCreator.Start(nodes.Length + 1, text: "Загружаем сотрудников");
			var cancellation = progressCreator.CancellationToken;
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id)).ToArray();
			
			progressCreator.Add(text: "Получаем последние выдачи");
			var operations = repoFunc(employees);

			progressCreator.Update("Проверка выданного");
			HashSet<int> operationsEmployeeIds = new HashSet<int>(operations.Select(x => x.Id)); 
			progressCreator.Update("Проверка выданного");
			
			HashSet<int> employeeIds = new HashSet<int>(operations.Select(x => x.Employee.Id)); 
			foreach(var employee in employees) {
				progressCreator.Add();
				if(!employeeIds.Contains(employee.Id)) 
					Results[employee.Id] = ("Нет выданного", "red");
			}

			progressCreator.Close();
			if (cancellation.IsCancellationRequested)
				return;
			progressCreator.Start(operations.Count + 4, text: "Пересчет даты последней выдачи");
			cancellation = progressCreator.CancellationToken;
			issueModel.RecalculateDateOfIssue(operations, baseParameters, interactive, progress: progressCreator, cancellation: cancellation, 
				changeLog: (employee, changes) => {
					if(changes.Length > 0) {
						Results[employee.Id] =
							(NumberToTextRus.FormatCase(changes.Length, "изменена {0} дата", "изменено {0} даты", "изменено {0} дат"), "green");
						foreach(var message in changes)
							loggerProcessing.Info(message);
					}
					else
						Results[employee.Id] = ("Без изменений", "gray");
				});
			if (cancellation.IsCancellationRequested)
				return;
			progressCreator.Add(text: "Завершаем...");
			UoW.Commit();
			progressCreator.Add(text: "Обновляем журнал");
			Refresh();
			progressCreator.Close();
		}
		
		#endregion

		#region Замена
		void ReplaceSubdivision(EmployeeProcessingJournalNode[] nodes) {
			var pageSelectionSubdivision = NavigationManager.OpenViewModel<SubdivisionJournalViewModel>(this, OpenPageOptions.AsSlave);
			pageSelectionSubdivision.ViewModel.SelectionMode = JournalSelectionMode.Single;
			pageSelectionSubdivision.ViewModel.OnSelectResult += (sender, e) => {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Корректировка сотрудников -> Замена подразделения")) {
					var selectedSubdivision = uow.GetById<Subdivision>(e.SelectedObjects.First().GetId());
					if(selectedSubdivision == null)
						throw new InvalidOperationException("Не удалось получить подразделение.");
					var ids = nodes.Select(n => n.Id).ToArray();
					
					var changed = uow.GetAll<EmployeeCard>()
						.Where(n => ids.Contains(n.Id))
						.UpdateBuilder()
						.Set(n => n.Subdivision, selectedSubdivision)
						.Update();
					loggerProcessing.Info($"Замена подразделения. Заменено у {changed} сотрудников.");
				}

				foreach(var node in nodes)
					Results[node.Id] = ("ОК", "green");
				Refresh();
			};
		}
		
		void ReplaceDepartment(EmployeeProcessingJournalNode[] nodes) {
			var pageSelectionDepartment = NavigationManager.OpenViewModel<DepartmentJournalViewModel>(this, OpenPageOptions.AsSlave);
			pageSelectionDepartment.ViewModel.SelectionMode = JournalSelectionMode.Single;
			pageSelectionDepartment.ViewModel.OnSelectResult += (sender, e) => {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Корректировка сотрудников -> Замена отдела")) {
					var selectedDepartment = uow.GetById<Department>(e.SelectedObjects.First().GetId());
					if(selectedDepartment == null)
						throw new InvalidOperationException("Не удалось получить отдел.");
					
					var ids = nodes.Select(n => n.Id).ToArray();
					var builder = uow.GetAll<EmployeeCard>()
						.Where(n => ids.Contains(n.Id))
						.UpdateBuilder()
						.Set(n => n.Department, selectedDepartment);
					if(selectedDepartment.Subdivision != null && interactive.Question("Установить так же подразделение из отдела?")) {
						builder.Set(n => n.Subdivision, selectedDepartment.Subdivision);
					}
					
					var changed = builder.Update();
					loggerProcessing.Info($"Замена отдела. Заменено у {changed} сотрудников.");
				}

				foreach(var node in nodes)
					Results[node.Id] = ("ОК", "green");
				Refresh();
			};
		}

		void ReplacePost(EmployeeProcessingJournalNode[] nodes) {
			var pageSelectionPost = NavigationManager.OpenViewModel<PostJournalViewModel>(this, OpenPageOptions.AsSlave);
			pageSelectionPost.ViewModel.SelectionMode = JournalSelectionMode.Single;
			pageSelectionPost.ViewModel.OnSelectResult += (sender, e) => {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Корректировка сотрудников -> Замена должности")) {
					var selectedPost = uow.GetById<Post>(e.SelectedObjects.First().GetId());
					if(selectedPost == null)
						throw new InvalidOperationException("Не удалось получить должность.");
					var ids = nodes.Select(n => n.Id).ToArray();
					
					var builder =  uow.GetAll<EmployeeCard>()
						.Where(n => ids.Contains(n.Id))
						.UpdateBuilder()
						.Set(n => n.Post, selectedPost);
					if(selectedPost.Subdivision != null && selectedPost.Department != null){
						if(interactive.Question("Установить так же подразделение и отдел из должности?")) {
							builder.Set(n => n.Subdivision, selectedPost.Subdivision);
							builder.Set(n => n.Department, selectedPost.Department);
						}
					} else if(selectedPost.Subdivision != null && interactive.Question("Установить так же подразделение из должности?")) {
						builder.Set(n => n.Subdivision, selectedPost.Subdivision);
					} else if(selectedPost.Department != null && interactive.Question("Установить так же отдел из должности?")) {
						builder.Set(n => n.Department, selectedPost.Department);
					}
					var changed	= builder.Update();
					loggerProcessing.Info($"Замена должности. Заменено у {changed} сотрудников.");
				}

				foreach(var node in nodes)
					Results[node.Id] = ("ОК", "green");
				Refresh();
			};
		}
		
		void Active2LastNormItem(EmployeeProcessingJournalNode[] nodes, bool fromNomenclature = false) {
			loggerProcessing.Info($"Переустановка строк нормы в 2 последих опирациях на актуальные у {nodes.Length} сотрудников");
			loggerProcessing.Info(fromNomenclature ? "По номенклатурам" : "По номенклатурам нормы");
			loggerProcessing.Info($"База данных: {dataBaseInfo.Name}");
			progressCreator.Start(3, text: "Загружаем сотрудников");
			var cancellation = progressCreator.CancellationToken;
			
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id)).ToArray();
			progressCreator.Add(text: "Получаем последние выдачи");
			var operations = employeeIssueRepository.GetLast2IssueOperationsForEmployee(employees);

			progressCreator.Update("Проверка выданного");
			HashSet<int> operationsEmployeeIds = new HashSet<int>(operations.Select(x => x.Employee.Id)); 
			foreach(var employee in employees) {
				progressCreator.Add();
				if(!operationsEmployeeIds.Contains(employee.Id)) 
					Results[employee.Id] = ("Нет выданного", "red");
			}

			progressCreator.Close();
			if (cancellation.IsCancellationRequested)
				return;
			progressCreator.Start(operations.Count + 4, text: "Замена норм");
			cancellation = progressCreator.CancellationToken;

			var changes = new Dictionary<int, int>();
			foreach(var operation in operations) {
				if(!changes.ContainsKey(operation.Employee.Id))
					changes[operation.Employee.Id] = 0;
				progressCreator.Add(text: $"Обработка {operation.Employee.ShortName}");

				if(fromNomenclature) { //подбираем потребность по номенклатуре
					if(!operation.Employee.WorkwearItems.Select(i => i.ProtectionTools)
						   .Any(pt => DomainHelper.EqualDomainObjects(pt, operation.ProtectionTools))) {
						var protectionTools = operation.Employee.WorkwearItems.Select(i => i.ProtectionTools)
							.FirstOrDefault(pt => pt.Nomenclatures.Any(n => DomainHelper.EqualDomainObjects(n, operation.Nomenclature)));
						if(protectionTools != null)
							operation.ProtectionTools = protectionTools;
					}
				}

				if(operation.ProtectionTools != null //проставляем если не было norm item
					&& operation.Employee.WorkwearItems.Select(wc => wc.ActiveNormItem)
				   .Any(ni => DomainHelper.EqualDomainObjects(ni, operation.NormItem))) {
					continue;
				}else {
					var normItem = operation.Employee.WorkwearItems.Select(n => n.ActiveNormItem) 
						.FirstOrDefault(p => DomainHelper.EqualDomainObjects(p.ProtectionTools, operation.ProtectionTools));
					if(normItem != null) {
						loggerProcessing.Info($"У Сотрудника |{operation.Employee.Id}|{operation.Employee.ShortName}|" +
						                      $" меняем norm item c |{operation.NormItem?.Id}| на |{normItem.Id}|" +
						                      $" нормы с |{operation.NormItem?.Norm.Id}| на |{normItem.Norm.Id}|" +
						                      $" в операции |{operation.Id}|{operation.ProtectionTools?.Name}");
						operation.NormItem = normItem;
						changes[operation.Employee.Id]++;
						UoW.Save(operation);
					}
				}
				if(cancellation.IsCancellationRequested) {
					loggerProcessing.Info("Прервано пользователем");
					progressCreator.Close();
					return;
				}
			}

			foreach(var change in changes) {
				if(change.Value == 0)
					Results[change.Key] = ("Без изменений", "gray");
				else 
					Results[change.Key] = ($"Отредактировано {change.Value} выдач", "orange");
			}
			progressCreator.Add(text: "Завершаем...");
			loggerProcessing.Info("Переустановка норм завершена");
			if(UoW.HasChanges) {
				UoW.Commit();
				loggerProcessing.Info("Сохранено в базу");
			}
			progressCreator.Add(text: "Обновляем журнал");
			Refresh();
			progressCreator.Close();
		}
		#endregion
		
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
		[SearchHighlight]
		public string Department { get; set; }
		
		public bool Dismiss { get { return DismissDate.HasValue; } }
		public DateTime? DismissDate { get; set; }
		
		[SearchHighlight]
		public string Norms { get; set; }

		public string Result { get; set; }
		public string ResultColor { get; set; }
	}
}
