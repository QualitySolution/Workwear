using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using NLog;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
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
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Regulations;

namespace Workwear.ViewModels.Stock {
	public class ExpenseDutyNormViewModel : EntityDialogViewModelBase<ExpenseDutyNorm>{
		
		private readonly IInteractiveService interactive;
		private readonly BaseParameters baseParameters;
		private static Logger logger = LogManager.GetCurrentClassLogger();
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
			IUserService userService,
			IValidator validator,
			BaseParameters baseParameters,
			StockBalanceModel stockBalanceModel,
			SizeService sizeService, 
			FeaturesService featutesService,
			StockRepository stockRepository,
			DutyNorm dutyNorm = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.StockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			
			var entityEntryBuilder = new CommonEEVMBuilderFactory<ExpenseDutyNorm>(this, Entity, UoW, navigation, autofacScope);
			var vmEntryBuilder = new CommonEEVMBuilderFactory<ExpenseDutyNormViewModel>(this, this, UoW, navigation, autofacScope);

			if(Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featutesService, autofacScope.Resolve<IUserService>().CurrentUserId);
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
			ResponsibleEmployeeCardEntryViewModel = entityEntryBuilder.ForProperty(x => x.ResponsibleEmployee)
				.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
			DutyNormEntryViewModel =
				vmEntryBuilder.ForProperty(x => x.DutyNorm)
				.UseViewModelJournalAndAutocompleter<DutyNormsJournalViewModel>()
				.UseViewModelDialog<DutyNormViewModel>()
				.Finish();

			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));
		}
		
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
		public bool CanDelSelectedItem => SelectedItem != null;
		public bool CanChooseStockPositionsSelectedItem => SelectedItem != null && SelectedItem.ProtectionTools != null;
		public bool SensitiveDocNumber => !AutoDocNumber;
	
		public IEnumerable<ProtectionTools> ProtectionToolsListFromNorm => Entity.DutyNorm.ProtectionToolsList;
		
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
				"<span color='blue'>●</span> — выдаваемого количества не достаточно\n" +
				"<span color='green'>●</span> — выдаётся больше необходимого\n" +
				"<span color='red'>●</span> — нет подходящих вариантов\n"
			);
		}
		
		public string GetRowColor(ExpenseDutyNormItem item) {
			if(item.Document.Id != 0) return "black";
			
			var requiredIssue = item.DutyNormItem.CalculateRequiredIssue(baseParameters, Entity.Date);
			if(requiredIssue > 0 && item.Nomenclature == null)
				return "red";
			if(requiredIssue <= 0 && item.Amount == 0)
				return "gray";
			if(requiredIssue < item.Amount)
				return "green";
			if(requiredIssue > item.Amount)
				return "blue";
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

			Entity.UpdateOperations(UoW, interactive);
			UoW.Session.SaveOrUpdate(Entity);
			UoW.Commit();
			
			return true;
		}
		#endregion
	}
}
