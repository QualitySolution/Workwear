using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NLog;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using workwear;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.Tools.User;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Regulations;
using Workwear.ViewModels.Statements;

namespace Workwear.ViewModels.Stock {
	public class ExpenseDutyNormViewModel : PermittingEntityDialogViewModelBase<ExpenseDutyNorm>, IDialogDocumentation{
		
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IInteractiveService interactive;
		private readonly BaseParameters baseParameters;
		private readonly CurrentUserSettings currentUserSettings;
		private readonly CommonMessages commonMessages;
		private readonly FeaturesService featuresService;
		public SizeService SizeService { get; }
		
		#region ViewModels
		public readonly EntityEntryViewModel<DutyNorm> DutyNormEntryViewModel;
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public readonly EntityEntryViewModel<EmployeeCard> ResponsibleEmployeeCardEntryViewModel;
		public StockBalanceModel StockBalanceModel { get; set; }

		#endregion

		public ExpenseDutyNormViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope autofacScope, 
			INavigationManager navigation,
			IInteractiveService interactive,
			ICurrentPermissionService permissionService,
			IUserService userService,
			CurrentUserSettings currentUserSettings,
			IValidator validator,
			BaseParameters baseParameters,
			StockBalanceModel stockBalanceModel,
			SizeService sizeService, 
			CommonMessages commonMessages,
			FeaturesService featuresService,
			StockRepository stockRepository,
			DutyNorm dutyNorm = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, permissionService, interactive, validator, unitOfWorkProvider) {
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.StockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.currentUserSettings = currentUserSettings ?? throw new ArgumentNullException(nameof(currentUserSettings));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			SetDocumentDateProperty(e => e.Date);
			
			var entityEntryBuilder = new CommonEEVMBuilderFactory<ExpenseDutyNorm>(this, Entity, UoW, navigation, autofacScope);
			var vmEntryBuilder = new CommonEEVMBuilderFactory<ExpenseDutyNormViewModel>(this, this, UoW, navigation, autofacScope);

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			if(Entity.Id == 0) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				Entity.DutyNorm = dutyNorm;
				FillUnderreceivedp(dutyNorm);
			} else {
				autoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
				Entity.DutyNorm.UpdateItems(UoW);
			}
			
			WarehouseEntryViewModel = entityEntryBuilder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			WarehouseEntryViewModel.IsEditable = CanEdit;
				
			ResponsibleEmployeeCardEntryViewModel = entityEntryBuilder.ForProperty(x => x.ResponsibleEmployee)
				.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
			ResponsibleEmployeeCardEntryViewModel.CanCleanEntity = ResponsibleEmployeeCleanSensitive;
			ResponsibleEmployeeCardEntryViewModel.IsEditable = CanEdit;
			
			DutyNormEntryViewModel =
				vmEntryBuilder.ForProperty(x => x.DutyNorm)
				.UseViewModelJournalAndAutocompleter<DutyNormsJournalViewModel>()
				.UseViewModelDialog<DutyNormViewModel>()
				.Finish();
			DutyNormEntryViewModel.IsEditable = CanEdit;
				
			Entity.PropertyChanged += EntityChange;
			
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
		}
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#duty-issue");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region Методы
		public void AddItems() {
			if(Entity.Warehouse is null || DutyNorm is null) {
				interactive.ShowMessage(ImportanceLevel.Warning,
					(Entity.Warehouse is null ? "Склад должен быть указан.\n" : "" ) + 
					(DutyNorm is null ? "Дежурная норма должна быть указана.\n": ""));
				return;
			}
				
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceJournalViewModel>
				(this, QS.Navigation.OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.Warehouse = Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadItems;
		}

		private void LoadItems(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				var stockPosition = node.GetStockPosition(UoW);
				var item = Entity.AddItem(stockPosition);
			}

		}

		public void DeleteItem(ExpenseDutyNormItem item) {
			Entity.RemoveItem(item);
		}

		public void ChooseStockPosition(ExpenseDutyNormItem item) {
			var selectJournal = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave,
				addingRegistrations: builder => {
					builder.RegisterInstance<Action<StockBalanceFilterViewModel>>(
						filter => {
							filter.WarehouseEntry.IsEditable = false;
							filter.Warehouse = Entity.Warehouse;
							filter.ProtectionTools = item.ProtectionTools;
						});
				});
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			selectJournal.Tag = item;
			selectJournal.ViewModel.OnSelectResult += ChooseStockPositionLoad;
		}
		
		public void ChooseStockPositionLoad(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var page = NavigationManager.FindPage((DialogViewModelBase)sender);
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				var item = page.Tag as ExpenseDutyNormItem;
					item.StockPosition = node.GetStockPosition(UoW);
					item.UpdateOperation(UoW);
			}
		}
		
/// <summary>
/// Заполнение документа по текущим потребностям
/// </summary>
/// <param name="dutyNorm">Норма для которой заполняется. Если null документ будет очищен.</param>
		private void FillUnderreceivedp(DutyNorm dutyNorm) {
			Entity.Items.Clear();
			if(dutyNorm == null)
				return;
			
			dutyNorm.UpdateItems(UoW);
			StockBalanceModel.AddNomenclatures(dutyNorm.Items.SelectMany(i => i.ProtectionTools.Nomenclatures));
			
			foreach(var item in dutyNorm.Items) {
				item.StockBalanceModel = StockBalanceModel;
				var position = item.BestChoiceInStock.FirstOrDefault()?.Position;
				if(position != null)
					Entity.AddItem(position, item.CalculateRequiredIssue(baseParameters, Entity.Date), item);
				else
					Entity.AddItem(item.ProtectionTools, 0);
			}
		}
		#endregion
		
		#region Свойства
		private ExpenseDutyNormItem selectedItem;
		[PropertyChangedAlso(nameof(CanDelSelectedItem))]
		[PropertyChangedAlso(nameof(CanChooseStockPositionsSelectedItem))]
		public virtual ExpenseDutyNormItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}
		
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
		
		#region Для View свойства, методы и пробросы
		public bool CanDelSelectedItem => CanEdit && SelectedItem != null;
		public bool CanChooseStockPositionsSelectedItem => CanEdit && SelectedItem != null && SelectedItem.ProtectionTools != null;
		public bool SensitiveDocNumber => CanEdit && !AutoDocNumber;
	
		public IEnumerable<ProtectionTools> ProtectionToolsListFromNorm => Entity.DutyNorm.ProtectionToolsList;
		
		public bool IssuanceSheetCreateSensitive => CanEdit && Entity.IssuanceSheet == null;		
		public bool IssuanceSheetCreateVisible => Entity.IssuanceSheet == null;
		public bool IssuanceSheetOpenVisible => Entity.IssuanceSheet != null;
		public bool IssuanceSheetPrintVisible => Entity.IssuanceSheet != null;
		public bool ResponsibleEmployeeCleanSensitive => CanEdit && Entity.IssuanceSheet == null;
		
		public virtual DutyNorm DutyNorm {
			get => Entity.DutyNorm;
			set { if(Entity.DutyNorm != value){
					if(Entity.Items.Count == 0
					   || interactive.Question("Документ будет очищен"+ (value != null ?" и заполнен для новой нормы":"") +". Продолжить?",
						   "В документе уже есть строки.")) {
						Entity.DutyNorm = value;
						FillUnderreceivedp(value);
					}
					OnPropertyChanged();
				}
			}
		}
		
		public void ShowLegend() {
			MessageDialogHelper.RunInfoDialog(
				"<span color='black'>●</span> — обычная выдача\n" +
				"<span color='gray'>●</span> — выдача не требуется\n" +
				"<span color='red'>●</span> — нет подходящих вариантов или не выбрана номенклатура нормы\n" +
				"<span color='orange'>●</span> — выдаваемого количества не достаточно\n" +
				"<span color='purple'>●</span> — выдаётся больше необходимого\n"
			);
		}
		
		public string GetRowColor(ExpenseDutyNormItem item) {
			if(item.ProtectionTools == null || item.DutyNormItem == null)
				return "red";
			if(item.Document.Id != 0) 
				return "black";
			var requiredIssue = item.DutyNormItem.CalculateRequiredIssue(baseParameters, Entity.Date);
			if(requiredIssue > 0 && item.Nomenclature == null)
				return "red";
			if(requiredIssue <= 0 && item.Amount == 0)
				return "gray";
			if(requiredIssue < item.Amount)
				return "purple";
			if(requiredIssue > item.Amount)
				return "orange";
			return "black";
		}
		#endregion

		#region Валидация и сохранение
		public override bool Save()
		{
			logger.Info("Проверка документа...");
			if(!Validate()) {
				logger.Warn("Документ не корректен, сохранение отменено.");
				return false;
			}
			
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			
			if(AutoDocNumber)
				Entity.DocNumber = null;
			else if(String.IsNullOrWhiteSpace(Entity.DocNumber))
				Entity.DocNumber = Entity.DocNumberText;

			foreach(var item in Entity.Items) 
				if(item.ProtectionTools != null)
					item.DutyNormItem = Entity.DutyNorm.Items.First(x => x.ProtectionTools == item.ProtectionTools);
			
			foreach(var item in Entity.Items.Where(x => x.Amount <= 0).ToList()) 
				DeleteItem(item);
			
			Entity.UpdateIssuanceSheet();
			if(Entity.IssuanceSheet != null)
				UoW.Save(Entity.IssuanceSheet);

			Entity.UpdateOperations(UoW, interactive);
			UoW.Session.SaveOrUpdate(Entity);
			UoW.Commit();
			
			return true;
		}
		#endregion

		#region Ведомость

		public void OpenIssuanceSheet()
		{
			if(UoW.HasChanges) {
				if(!interactive.Question("Сохранить документ выдачи перед открытием ведомости?") || !Save())
					return;
			}
			MainClass.MainWin.NavigationManager.OpenViewModel<IssuanceSheetViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Entity.IssuanceSheet.Id));
		}
		public void CreateIssuanceSheet()
		{
			if(Entity.ResponsibleEmployee == null) {
				interactive.ShowMessage(ImportanceLevel.Warning,"Ответственный сотрудник должен быть указан");
				return;
			}

			if(Validate()) {
				var defaultOrganization = UoW.GetInSession(currentUserSettings.Settings.DefaultOrganization);
				var defaultLeader = UoW.GetInSession(currentUserSettings.Settings.DefaultLeader);
				var defaultResponsiblePerson = UoW.GetInSession(currentUserSettings.Settings.DefaultResponsiblePerson);
				Entity.CreateIssuanceSheet(defaultOrganization, defaultLeader, defaultResponsiblePerson);
			}
			
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
		
		public void EntityChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(Entity.IssuanceSheet):
					OnPropertyChanged(nameof(IssuanceSheetCreateVisible));
					OnPropertyChanged(nameof(IssuanceSheetOpenVisible));
					OnPropertyChanged(nameof(IssuanceSheetPrintVisible));
					ResponsibleEmployeeCardEntryViewModel.CanCleanEntity = ResponsibleEmployeeCleanSensitive;
					break;
			}
		}
	}
}
