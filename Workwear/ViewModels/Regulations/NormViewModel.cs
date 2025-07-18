using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Utilities.Debug;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Regulations.NormChildren;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Regulations
{
	public class NormViewModel : EntityDialogViewModelBase<Norm>, ISelectItem, IDialogDocumentation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly IInteractiveService interactive;
		private readonly IEntityChangeWatcher changeWatcher;
		private readonly EmployeeRepository employeeRepository;
		private readonly BaseParameters baseParameters;
		private readonly EmployeeIssueModel issueModel;
		private readonly ModalProgressCreator progressCreator;

		public NormViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			EmployeeIssueRepository employeeIssueRepository,
			INavigationManager navigation, 
			IInteractiveService interactive,
			IEntityChangeWatcher changeWatcher,
			EmployeeRepository employeeRepository,
			BaseParameters baseParameters,
			EmployeeIssueModel issueModel,
			ModalProgressCreator progressCreator,
			FeaturesService featuresService,
			ILifetimeScope autofacScope,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.interactive = interactive;
			this.changeWatcher = changeWatcher ?? throw new ArgumentNullException(nameof(changeWatcher));
			this.employeeRepository = employeeRepository;
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));

			var performance = new PerformanceHelper(logger: logger);
			var normConditionQuery = UoW.Session.QueryOver<NormCondition>()
				.Future();

			UoW.Session.QueryOver<NormItem>()
				.Where(x => x.Norm.Id == Entity.Id)
				.Fetch(SelectMode.Fetch, x => x.ProtectionTools)
				.Fetch(SelectMode.Fetch, x => x.ProtectionTools.Type)
				.Future();

			var regulationQuery = UoW.Session.QueryOver<RegulationDoc>()
				.Fetch(SelectMode.Fetch, x => x.Annexes)
				.Future();
			
			NormConditions = normConditionQuery.ToList();
			NormConditions.Insert(0, null);
			RegulationDocs = regulationQuery.ToList();
			if(Entity.Id == 0)
				LastUpdate = "Новая норма";
			else {
				List<DateTime> lastUpdates = new List<DateTime>();
				lastUpdates.Add(Entity.LastUpdate);
				if(Entity.Items.Any())
					lastUpdates.Add(Entity.Items.Max(x => x.LastUpdate));
				LastUpdate = lastUpdates.Max().ToString("dd/M/yyyy");
			}
			performance.CheckPoint("Запрос основных данных");
			VisibleNormCondition = featuresService.Available(WorkwearFeature.ConditionNorm);

			var thisViewModel = new TypedParameter(typeof(NormViewModel), this);
			PostsViewModel = autofacScope.Resolve<NormPostsViewModel>(thisViewModel);
			EmployeesViewModel = autofacScope.Resolve<NormEmployeesViewModel>(thisViewModel);
			performance.CheckPoint("Создание дочерних вию моделей");
			
			this.changeWatcher.BatchSubscribe(SubscriberItems).OnlyForUow(UoW)
				.IfEntity<NormItem>()
				.AndWhere(x => x.Norm.Id == Entity.Id)
				.AndChangeType(TypeOfChangeEvent.Update)
				.AndChangeType(TypeOfChangeEvent.Insert)
				.AndDiffAnyOfProperties(
					x => x.ProtectionTools, 
					x => x.Amount,
					x => x.NormCondition,
					x => x.NormPeriod,
					x => x.PeriodCount,
					x => x.IsDisabled);

			this.changeWatcher.BatchSubscribe(e => needUpdateEmployees = true)
				.IfEntity<Norm>()
				.AndWhere(x => x.Id == Entity.Id)
				.AndChangeType(TypeOfChangeEvent.Update)
				.AndDiffAnyOfProperties(x => x.Archival);
			
			performance.CheckPoint("Конец");
			performance.PrintAllPoints(logger);
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("regulations.html#norms");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		/// <summary>
		/// Копирует существующую в базе норму по id
		/// </summary>
		public void CopyNormFrom(int normId)
		{
			var norm = UoW.GetById<Norm>(normId);
			Entity.CopyFromNorm(norm);
		}

		#region Дочерние ViewModels
		public NormEmployeesViewModel EmployeesViewModel { get; }
		public NormPostsViewModel PostsViewModel { get; }
		#endregion
		
		#region Sensetive

		private bool saveSensitive = true;
		public virtual bool SaveSensitive {
			get => saveSensitive;
			set => SetField(ref saveSensitive, value);
		}

		private bool cancelSensitive = true;
		public virtual bool CancelSensitive {
			get => cancelSensitive;
			set => SetField(ref cancelSensitive, value);
		}

		#endregion
		
		#region Visible
		public bool VisibleNormCondition { get; }
		#endregion

		#region Свойства
		public List<NormCondition> NormConditions { get; set; }
		public List<RegulationDoc> RegulationDocs { get; set; }
		
		private NormItem selectedItem;
		public virtual NormItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}

		private int currentTab = 1;
		public virtual int CurrentTab {
			get => currentTab;
			set {
				SetField(ref currentTab, value);
				if(currentTab == 2)
					PostsViewModel.OnShow();
				if(currentTab == 3)
					EmployeesViewModel.OnShow();
			}
		}
		public virtual String LastUpdate { get; }
		
		#endregion

		#region Действия View
		
		#region Строки нормы
		public void AddItem()
		{
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Protection_OnSelectResult;
		}

		void Protection_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var protectionNode in e.SelectedObjects) {
				var protectionTools = UoW.GetById<ProtectionTools>(protectionNode.GetId());
				Entity.AddItem(protectionTools);
			}
		}

		public void RemoveItem(NormItem toRemove)
		{
			IList<EmployeeCard> worksEmployees = null;

			if(toRemove.Id > 0) {
				logger.Info("Поиск ссылок на удаляемую строку нормы...");
				worksEmployees = EmployeeRepository.GetEmployeesDependenceOnNormItem(UoW, toRemove);
				if(worksEmployees.Count > 0) {
					List<string> operations = new List<string>();
					foreach(var emp in worksEmployees) {
						bool canSwitch = emp.UsedNorms.SelectMany(x => x.Items)
							.Any(i => i.Id != toRemove.Id && i.ProtectionTools.Id == toRemove.ProtectionTools.Id);
						if(canSwitch)
							operations.Add(String.Format("* У сотрудника {0} требование спецодежды будет переключено на другую норму.", emp.ShortName));
						else
							operations.Add(String.Format("* У сотрудника {0} будет удалено требование выдачи спецодежды.", emp.ShortName));
					}

					var mes = "При удалении строки нормы будут выполнены следующие операции:\n";
					mes += String.Join("\n", operations.Take(10));
					if(operations.Count > 10)
						mes += String.Format("\n... и еще {0}", operations.Count - 10);
					mes += "\nОткрытые диалоги этих сотрудников будут закрыты.\nВы уверены что хотите выполнить удаление?";
					logger.Info("Ок");
					if(!interactive.Question(mes))
						return;
				}
			}
			Entity.RemoveItem(toRemove);

			if(worksEmployees != null) {
				SaveSensitive = CancelSensitive = false;
				var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(this); 
				progressPage.ViewModel.Progress.Start(worksEmployees.Count, text: "Обработка сотрудников...");

				foreach(var emp in worksEmployees) {
					emp.UoW = UoW;
					emp.UpdateWorkwearItems();
					UoW.Save(emp);
					progressPage.ViewModel.Progress.Add();
				}

				SaveSensitive = CancelSensitive = true;
				NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
			}
		}

		public void ReplaceNomenclature(NormItem item)
		{
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.Tag = item;
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			page.ViewModel.OnSelectResult += ProtectionReplace_OnSelectResult;
		}

		void ProtectionReplace_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var page = NavigationManager.FindPage((DialogViewModelBase)sender);
			var item = (NormItem)page.Tag;
			var newProtectionTools = UoW.GetById<ProtectionTools>(e.GetSelectedObjects<ProtectionToolsJournalNode>().First().Id);
			item.ProtectionTools = newProtectionTools;

			if(item.Id > 0) {
				logger.Info("Поиск ссылок на заменяемую строку нормы...");
				IList<EmployeeCard> worksEmployees = EmployeeRepository.GetEmployeesDependenceOnNormItem(UoW, item);
				var operations = employeeIssueRepository.GetOperationsForNormItem(new []{item}, q => q.Fetch(SelectMode.Fetch, x => x.Employee));
				if(worksEmployees.Count > 0) {
					var names = worksEmployees.Union(operations.Select(x => x.Employee)).Distinct().Select(x => x.ShortName).ToList();
					var mes = "Замена номенклатуры нормы затронет потребности и прошлые выдачи следующих сотрудников:\n";
					mes += String.Join(", ", names.Take(50));
					if(names.Count > 50)
						mes += String.Format("\n... и еще {0}", names.Count - 50);
					mes += "\nОткрытые диалоги этих сотрудников будут закрыты. Норма будет сохранена.\nВы уверены что хотите выполнить замену?";
					logger.Info("Ок");
					if(!interactive.Question(mes) || !Validate()) //В этом месте проводим валидацию, так как дальше норма будет сохранена, не надо проводить замену если норму нельзя сохранить.
						return;
					SaveSensitive = CancelSensitive = false;
					
					var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(this);
					progressPage.ViewModel.Progress.Update("Обработка произведенных выдач...");
					foreach(var operation in operations) {
						operation.ProtectionTools = newProtectionTools;
						UoW.Save(operation);
						progressPage.ViewModel.Progress.Add();
					}
					logger.Info($"Заменены номенклатуры нормы в {operations.Count} операциях");
					
					progressPage.ViewModel.Progress.Start(worksEmployees.Count + operations.Count + 1, text: "Обработка сотрудников...");
					foreach(var emp in worksEmployees) {
						emp.UoW = UoW;
						foreach(var employeeItem in emp.WorkwearItems) {
							if(item.IsSame(employeeItem.ActiveNormItem))
								employeeItem.ProtectionTools = newProtectionTools;
						}
						emp.UpdateWorkwearItems();
						UoW.Save(emp);
						progressPage.ViewModel.Progress.Add();
					}
					logger.Info($"Заменены потребности у {worksEmployees.Count} сотрудников");
					
					progressPage.ViewModel.Progress.Update("Сохранение нормы...");
					UoW.Commit(); //Здесь комит нужен для того чтобы при пересчете графф строился уже по новой номенклатуре нормы.
					Save();
					progressPage.ViewModel.Progress.Add();
					SaveSensitive = CancelSensitive = true;
					NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
				}
			}
		}
		#endregion

		#region Контекстное меню строки нормы
		/// <summary>
		/// Ручной перечет операций выдачи через контекстное меню строки нормы.
		/// </summary>
		public void ReSaveLastIssue(NormItem normItem) 
		{
			if(UoW.HasChanges && !Save())
				return;
				
			logger.Info("Пересчитываем последнии выдачи сотрудников");
			var operations = employeeIssueRepository.GetOperationsForNormItem(
				new []{normItem}, 
				q => q.Fetch(
					SelectMode.Fetch, 
					x => x.Employee),
				beginDate: Entity.DateFrom);

			if(!operations.Any()) {
				interactive.ShowMessage(ImportanceLevel.Warning, "Последние выдачи отсутствуют. Нечего пересчитывать.");
				logger.Info("Нечего пересчитывать.");
				return;
			}

			var operationsLasts = operations
				.GroupBy(x => x.Employee)
				.Select(o => 
					o.OrderByDescending(d => d.OperationTime).First())
				.ToList();
			
			var answer = interactive.Question(
				new[] { "Все выдачи", "Только последние" },
				(Entity.DateFrom.HasValue ? $"C {Entity.DateFrom:d} по " : "По ") +
					  $"строке нормы было выполнено {operations.Count} выдач из них последних {operationsLasts.Count}. " +
				$"В зависимости от настроек учета, данный пересчет может так же изменять сроки начала использования СИЗ. " +
					  $"Какие выдачи пересчитывать?");
			if(answer == null)
				return;
			
			var modifiableOperations = answer == "Только последние" ? operationsLasts : operations;
			progressCreator.Title = "Обновляем операции выдачи";
			issueModel.RecalculateDateOfIssue(modifiableOperations, baseParameters, interactive, progress: progressCreator);
			logger.Info($"{modifiableOperations.Count()} операций обновлено.");
		}
		
		public void OpenProtectionTools(NormItem normItem) 
		{
			NavigationManager.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(normItem.ProtectionTools.Id));
		}

		public void DisableNormItem(NormItem normItem) {
			Entity.Items.First(x => DomainHelper.EqualDomainObjects(x, normItem)).IsDisabled = true;
		}

		public void EnableNormItem(NormItem normItem) {
			Entity.Items.First(x => DomainHelper.EqualDomainObjects(x, normItem)).IsDisabled = false;
		}
		#endregion
		#endregion
		#region Сохранение
		List<NormItem> needRecalculateIssue = new List<NormItem>();
		bool needUpdateEmployees;
		
		private void SubscriberItems(EntityChangeEvent[] changeevents) {
			needUpdateEmployees = true;
			needRecalculateIssue = changeevents.Where(x => x.EventType == TypeOfChangeEvent.Update)
				.Select(x => x.GetEntity<NormItem>()).ToList();
		}
		
		public override bool Save() 
		{
			needUpdateEmployees = false;
			needRecalculateIssue.Clear();
			if(!base.Save())
				return false;
			
			//Проверяем если есть активные выдачи измененным строкам нормы, предлагаем пользователю их пересчитать.
			if(needRecalculateIssue.Any()) {
				var operations = employeeIssueRepository.GetOperationsForNormItem(
					needRecalculateIssue.ToArray(), 
					q => q.Fetch(
						SelectMode.Fetch, 
						x => x.Employee),
					beginDate: Entity.DateFrom
				);
				//Оставляем только последние
				var operationsLasts = operations
					.GroupBy(x => $"{x.Employee.Id}.{x.NormItem.Id}")
					.Select(o => o.OrderByDescending(d => d.OperationTime).First())
					.ToList();

				if(operations.Any()) {
					var answer = interactive.Question(
						new[] { "Все выдачи", "Только последние", "Не пересчитывать" },
						(Entity.DateFrom.HasValue ? $"C {Entity.DateFrom:d} по " : "По ") + 
						$"измененным строкам нормы было выполнено {operations.Count} выдач из них последних {operationsLasts.Count}. " + 
						"Пересчитать сроки носки у уже выданного в соответствии с изменениями?");
					if(answer == "Все выдачи" || answer == "Только последние"){
						var modifiableOperations = answer == "Только последние" ? operationsLasts : operations;
						progressCreator.Title = "Обновляем операции выдачи";
						issueModel.RecalculateDateOfIssue(modifiableOperations, baseParameters, interactive, progress: progressCreator);
					}
				}
			}

			var employees = employeeRepository.GetEmployeesUseNorm(new []{Entity}, UoW);
			
			if (employees.Any() && needUpdateEmployees) 
			{
				logger.Info("Пересчитываем сотрудников");
				var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(null);
				var progress = progressPage.ViewModel.Progress;
				progress.Start(employees.Count, text: "Обновляем потребности сотрудников");
				foreach (var employee in employees) {
					progress.Add(text: $"Обработка {employee.ShortName}");
					employee.UpdateWorkwearItems();
					UoW.Save(employee);
				}
				progress.Add(text: "Завершаем...");
				UoW.Commit();
				NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
				logger.Info("Ok");
			}
			
			return true;
		}
		#endregion

		public void SelectItem(int id) {
			SelectedItem = Entity.Items.FirstOrDefault(x => x.Id == id);
		}
	}
}
