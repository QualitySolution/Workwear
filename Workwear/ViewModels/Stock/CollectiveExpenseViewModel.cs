using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NLog;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Utilities.Debug;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using workwear;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.Models.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.User;
using Workwear.ViewModels.Statements;

namespace Workwear.ViewModels.Stock
{
	public class CollectiveExpenseViewModel : PermittingEntityDialogViewModelBase<CollectiveExpense>, ISelectItem, IDialogDocumentation
	{
		private ILifetimeScope autofacScope;
		private readonly CurrentUserSettings currentUserSettings;
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public CollectiveExpenseItemsViewModel CollectiveExpenseItemsViewModel;
		private IEnumerable<EmployeeCard> Employees;
		private IInteractiveQuestion interactive;
		private readonly EmployeeIssueModel issueModel;
		private readonly CommonMessages commonMessages;
		private readonly FeaturesService featuresService;
		private readonly BaseParameters baseParameters;
		private readonly ModalProgressCreator progressCreator;
		private readonly IEntityChangeWatcher changeWatcher;

		public CollectiveExpenseViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IValidator validator,
			IUserService userService,
			CurrentUserSettings currentUserSettings,
			IInteractiveService interactive,
			ICurrentPermissionService permissionService,
			EmployeeIssueModel issueModel,
			StockRepository stockRepository,
			CommonMessages commonMessages,
			FeaturesService featuresService,
			BaseParameters baseParameters,
			IProgressBarDisplayable globalProgress,
			ModalProgressCreator progressCreator,
			IEntityChangeWatcher changeWatcher,
			IssuanceRequest issuanceRequest = null,
			Warehouse warehouse = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, permissionService, interactive, validator, unitOfWorkProvider)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.currentUserSettings = currentUserSettings ?? throw new ArgumentNullException(nameof(currentUserSettings));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			this.changeWatcher = changeWatcher ?? throw new ArgumentNullException(nameof(changeWatcher));
			SetDocumentDateProperty(e => e.Date);

			var performance = new ProgressPerformanceHelper(globalProgress, 12, "Предзагрузка данных документа", logger);
			var entryBuilder = new CommonEEVMBuilderFactory<CollectiveExpense>(this, Entity, UoW, navigation, autofacScope);
			if (UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
			}
			else {
				UoW.Session.QueryOver<CollectiveExpense>()
					.Where(x => x.Id == Entity.Id)
					.Fetch(SelectMode.ChildFetch, x => x)
					.Fetch(SelectMode.Fetch, x => x.CreatedbyUser)
					.Fetch(SelectMode.Fetch, x => x.Warehouse)
					.SingleOrDefault();
			}
			
			performance.CheckPoint("Warehouse");
			if(Entity.Warehouse == null && issuanceRequest == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);

			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			WarehouseEntryViewModel.IsEditable = CanEdit;
			TransferAgentEntryViewModel = entryBuilder.ForProperty(x => x.TransferAgent).MakeByType().Finish();
			TransferAgentEntryViewModel.IsEditable = CanEdit;
			
			performance.StartGroup("CollectiveExpenseItemsViewModel");
			var parameterModel = new TypedParameter(typeof(CollectiveExpenseViewModel), this);
			var parameterPerformance = new TypedParameter(typeof(PerformanceHelper), performance);
			CollectiveExpenseItemsViewModel = this.autofacScope.Resolve<CollectiveExpenseItemsViewModel>(parameterModel, parameterPerformance);
			performance.EndGroup();
			
			this.changeWatcher.BatchSubscribe(Subscriber)
				.OnlyForUow(UoW)
				.IfEntity<EmployeeIssueOperation>()
				.AndChangeType(TypeOfChangeEvent.Insert)
				.AndChangeType(TypeOfChangeEvent.Update);
			
			if(issuanceRequest != null) {
				Entity.IssuanceRequest = issuanceRequest;
				Entity.Warehouse = warehouse;
				CollectiveExpenseItemsViewModel.AddEmployeesList(issuanceRequest.Employees, performance);
			}

			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				logger.Info($"Создание Нового документа Коллективной выдачи выдачи.");
			} else AutoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
			
			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
			performance.End();
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#collective-issue");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region EntityViewModels
		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public EntityEntryViewModel<EmployeeCard> TransferAgentEntryViewModel;
		#endregion
		
		#region Свойства View
		public bool SensitiveDocNumber => CanEdit && !AutoDocNumber;
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumberText))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public bool AutoDocNumber {
			get => autoDocNumber;
			set => SetField(ref autoDocNumber, value);
		}

		public string DocNumberText {
			get => AutoDocNumber ? (Entity.Id == 0 ? "авто" : Entity.Id.ToString()) : Entity.DocNumberText;
			set { 
				if(!AutoDocNumber) 
					Entity.DocNumber = value; 
			}
		}
		#endregion

		#region Сохранение
		private void Subscriber(EntityChangeEvent[] changeevents) {
			changedOperations = changeevents.Select(x => x.GetEntity<EmployeeIssueOperation>()).ToArray();
		}

		private EmployeeIssueOperation[] changedOperations = new EmployeeIssueOperation[0];

		public override bool Save()
		{
			if(!Validate())
				return false;
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			if(AutoDocNumber)
				Entity.DocNumber = null;
			else if(String.IsNullOrWhiteSpace(Entity.DocNumber))
				Entity.DocNumber = Entity.DocNumberText;

			var performance = new ProgressPerformanceHelper(progressCreator, 6, "Подготовка документа...", logger, showProgressText: true);
			Entity.CleanupItems();
			Entity.UpdateOperations(UoW, baseParameters, interactive);
			performance.CheckPoint("Обновление ведомости...");
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);

			performance.CheckPoint("Сохранение...");
			UoWGeneric.Save();
			UoW.Commit();
			performance.CheckPoint("Обновление карточек сотрудников...");
			if(changedOperations.Any()) {
				progressCreator.UpdateMax(6 + changedOperations.Length + 1);
				issueModel.UpdateNextIssue(changedOperations, progressCreator);
			}
			
			performance.CheckPoint("Завершение...");
			UoWGeneric.Commit();
			performance.End();
			logger.Info("Ok");
			return true;
		}
		#endregion

		#region Ведомости
		public bool CanCreateIssuanceSheet => CanEdit;
		public bool IssuanceSheetCreateVisible => Entity.IssuanceSheet == null;
		public bool IssuanceSheetOpenVisible => Entity.IssuanceSheet != null;
		public bool IssuanceSheetPrintVisible => Entity.IssuanceSheet != null;
		
		public void OpenIssuanceSheet()
		{
			if(UoW.HasChanges) {
				if(!interactive.Question("Сохранить документ выдачи перед открытием ведомости?") || !Save())	
					return;
			}
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}

		public void CreateIssuanceSheet() {
			var defaultOrganization = UoW.GetInSession(currentUserSettings.Settings.DefaultOrganization);
			var defaultLeader = UoW.GetInSession(currentUserSettings.Settings.DefaultLeader);
			var defaultResponsiblePerson = UoW.GetInSession(currentUserSettings.Settings.DefaultResponsiblePerson);
			Entity.CreateIssuanceSheet(defaultOrganization, defaultLeader, defaultResponsiblePerson);
			
			OnPropertyChanged(nameof(IssuanceSheetCreateVisible));
			OnPropertyChanged(nameof(IssuanceSheetOpenVisible));
			OnPropertyChanged(nameof(IssuanceSheetPrintVisible));
		}
		public void PrintIssuanceSheet(IssuedSheetPrint doc)
		{
			if(UoW.HasChanges) {
				if(!commonMessages.SaveBeforePrint(Entity.GetType(), doc == IssuedSheetPrint.AssemblyTask ? "задания на сборку" : "ведомости") || !Save())
					return;
			}

			var reportInfo = new ReportInfo {
				Title = doc == IssuedSheetPrint.AssemblyTask ? $"Задание на сборку №{Entity.IssuanceSheet.DocNumber ?? Entity.IssuanceSheet.Id.ToString()}" 
					: $"Ведомость №{Entity.IssuanceSheet.DocNumber ?? Entity.IssuanceSheet.Id.ToString()} (МБ-7)",
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.IssuanceSheet.Id },
					{"printPromo", featuresService.Available(WorkwearFeature.PrintPromo)}
				}
			};

			//Если пользователь не хочет сворачивать ФИО и табельник (настройка в базе)
			if((doc == IssuedSheetPrint.IssuanceSheet || doc == IssuedSheetPrint.IssuanceSheetVertical) && !baseParameters.CollapseDuplicateIssuanceSheet)
				reportInfo.Source = File.ReadAllText(reportInfo.GetPath()).Replace("<HideDuplicates>Data</HideDuplicates>", "<HideDuplicates></HideDuplicates>");

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
		#endregion

		#region ISelectItem
		public void SelectItem(int id) {
			CollectiveExpenseItemsViewModel.SelectedItem = Entity.Items.First(x => x.Id == id);
		}
		#endregion
	}
}
