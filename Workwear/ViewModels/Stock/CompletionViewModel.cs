using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using workwear;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock
{
	public class CompletionViewModel: PermittingEntityDialogViewModelBase<Completion>, IDialogDocumentation
	{
		private readonly IInteractiveQuestion interactive;
		public readonly FeaturesService featuresService;
		private Warehouse lastWarehouse;
		public SizeService SizeService { get; }
		public CompletionViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IUserService userService,
			StockRepository stockRepository,
			FeaturesService featuresService,
			ILifetimeScope autofacScope,
			BaseParameters baseParameters,
			IInteractiveService interactive,
			ICurrentPermissionService permissionService,
			SizeService sizeService,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, permissionService, interactive, validator)
		{
			if(autofacScope == null) throw new ArgumentNullException(nameof(autofacScope));
			if(userService == null) throw new ArgumentNullException(nameof(userService));
			if(baseParameters == null) throw new ArgumentNullException(nameof(baseParameters));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			SetDocumentDateProperty(e => e.Date);
			
			var entryBuilder = new CommonEEVMBuilderFactory<Completion>(this, Entity, UoW, navigation, autofacScope);
			
			if(UoW.IsNew) 
				Entity.CreatedbyUser = userService.GetCurrentUser();
			else 
				autoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
			
			if(Entity.SourceWarehouse == null)
				Entity.SourceWarehouse = stockRepository.GetDefaultWarehouse
					(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			
			WarehouseExpenseEntryViewModel = entryBuilder.ForProperty(x => x.SourceWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			WarehouseExpenseEntryViewModel.IsEditable = CanEdit;

			if(Entity.ResultWarehouse == null)
				Entity.ResultWarehouse = stockRepository.GetDefaultWarehouse
					(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);

			WarehouseReceiptEntryViewModel = entryBuilder.ForProperty(x => x.ResultWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			WarehouseReceiptEntryViewModel.IsEditable = CanEdit;

			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, 
				new ValidationContext(Entity, new Dictionary<object, object> { { nameof(BaseParameters), baseParameters }, {nameof(IUnitOfWork), UoW} })));
			Entity.PropertyChanged += Entity_PropertyChanged;
			Entity.ResultItems.ContentChanged  += (sender, args) => OnPropertyChanged(nameof(ResultAmountText));
			Entity.SourceItems.ContentChanged += (sender, args) => OnPropertyChanged(nameof(SourceAmountText));
            lastWarehouse = Entity.SourceWarehouse;

			Owners = UoW.GetAll<Owner>().ToList();
		}
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#complectation");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		#region View
		public bool SensitiveDellSourceItemButton => CanEdit && SelectedSourceItem != null;
		public bool SensitiveDellResultItemButton => CanEdit && SelectedResultItem != null;
		public bool SensitiveDocNumber => CanEdit && !AutoDocNumber;
		public bool SensitiveAddSizesResultButton => CanEdit && SelectedResultItem != null && SelectedResultItem.WearSizeType != null;
		public string ResultAmountText => $"Общее количество: {Entity.ResultItems.Sum(x=>x.Amount)}";
		public string SourceAmountText => $"Общее количество: {Entity.SourceItems.Sum(x=>x.Amount)}";
		
		private CompletionResultItem selectedResultItem;
		public virtual CompletionResultItem SelectedResultItem {
			get => selectedResultItem;
			set {
				selectedResultItem = value;	
				OnPropertyChanged(nameof(SensitiveAddSizesResultButton));
				OnPropertyChanged(nameof(SensitiveDellResultItemButton));
			}
		}
		private CompletionSourceItem selectedSourceItem;
		public virtual CompletionSourceItem SelectedSourceItem {
			get => selectedSourceItem;
			set {
				selectedSourceItem = value;
				OnPropertyChanged(nameof(SensitiveDellSourceItemButton));
			}
		}

		public bool ShowWarehouses => featuresService.Available(WorkwearFeature.Warehouses);

		private IList<Owner> owners;
		public IList<Owner> Owners {
			get => owners;
			set => SetField(ref owners, value);
		}
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		[PropertyChangedAlso(nameof(DocNumberText))]
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
		#region EntityViewModels
		public readonly EntityEntryViewModel<Warehouse> WarehouseExpenseEntryViewModel;
		public readonly EntityEntryViewModel<Warehouse> WarehouseReceiptEntryViewModel;

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(Entity.SourceWarehouse) || Entity.SourceWarehouse == lastWarehouse || Entity.SourceItems is null) return;
			if(Entity.SourceItems.Any()) {
				if(interactive.Question("При изменении склада комплектующих строки документа будут очищены. Продолжить?")) {
					Entity.SourceItems.Clear();
				}
				else { //Возвращаем назад старый склад
					Entity.SourceWarehouse = lastWarehouse;
					return;
				}
			}
			lastWarehouse = Entity.SourceWarehouse;
		}
		#endregion
		#region Items
		public void AddSourceItems(){
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<StockBalanceJournalViewModel>(this, QS.Navigation.OpenPageOptions.AsSlave,
					addingRegistrations: builder => {
						builder.RegisterInstance<Action<StockBalanceFilterViewModel>>(
							filter => {
								if (Entity.SourceWarehouse != null) {
									filter.WarehouseEntry.IsEditable = false;
									filter.Warehouse = Entity.SourceWarehouse;
									filter.CanChooseAmount = true;
								}
							});
					});
			
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		}

		private void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			var selectVM = sender as StockBalanceJournalViewModel;
			var addedAmount = selectVM.Filter.AddAmount;
			foreach (var node in e.GetSelectedObjects<StockBalanceJournalNode>())
				Entity.AddSourceItem(node.GetStockPosition(UoW), selectVM.Filter.Warehouse,
					addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : node.Amount));
		}

		public void AddResultItems() {
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<NomenclatureJournalViewModel>(this, QS.Navigation.OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddNomenclature_OnSelectResult;
		}

		private void AddNomenclature_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => Entity.AddResultItem(n));
		}
		public void DelSourceItems(CompletionSourceItem item) {
			Entity.SourceItems.Remove(item);
		}
		public void DelResultItems(CompletionResultItem item) {
			Entity.ResultItems.Remove(item);
		}
		public void AddSizesResultItems(CompletionResultItem item) {
			if(item == null || item.Nomenclature == null)
				return;
			var existItems = Entity.ResultItems.Cast<IDocItemSizeInfo>().ToList();
			var page = base.NavigationManager.OpenViewModel<SizeWidgetViewModel,IDocItemSizeInfo, IUnitOfWork, IList<IDocItemSizeInfo>>
				(null, item, UoW,existItems);
			page.ViewModel.AddedSizes += (i, args) => AddResultSizes(item, args);
		}
		private void AddResultSizes(CompletionResultItem item, AddedSizesEventArgs args) {
			foreach(var s in args.SizesWithAmount) {
				var exist = Entity.FindResultItem(item.Nomenclature, s.Size, args.Height, item.Owner);
				if(exist != null)
					exist.Amount = s.Amount;
				else
					Entity.AddResultItem(item.Nomenclature, s.Size, args.Height, s.Amount, item.Owner);
			}
		}
		#endregion
		#region Save
		public override bool Save() {
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			if(AutoDocNumber)
				Entity.DocNumber = null;
			Entity.UpdateItems();
			return base.Save();
		}
		#endregion
	}
}
