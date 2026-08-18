using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.Report;
using QS.Report.ViewModels;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Features;
using Workwear.ViewModels.ClothingService;
using Workwear.ViewModels.Postomats;

namespace Workwear.ViewModels.Stock.Documents
{
	public class WarehouseTransferViewModel : PermittingEntityDialogViewModelBase<Transfer>, IDialogDocumentation
	{
		public EntityEntryViewModel<Organization> OrganizationEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseToEntryViewModel;
		private readonly BarcodeRepository barcodeRepository;
		private readonly BarcodeService barcodeService;
		private readonly BaseParameters baseParameters;
		public readonly FeaturesService FeaturesService;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly IInteractiveService interactive;
		
		public IList<Owner> Owners { get; }

		public WarehouseTransferViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigationManager, 
			ILifetimeScope autofacScope, 
			IValidator validator, 
			IUserService userService,
			BaseParameters baseParameters,
			BarcodeRepository barcodeRepository,
			BarcodeService barcodeService,
			OrganizationRepository organizationRepository,
			StockBalanceModel stockBalanceModel,
			IInteractiveService interactive,
			ICurrentPermissionService permissionService,
			FeaturesService featuresService
			) : base(uowBuilder, unitOfWorkFactory, navigationManager, permissionService, interactive, validator, unitOfWorkProvider)
		{
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			SetDocumentDateProperty(e => e.Date);
			
			if(Entity.Id == 0) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				Entity.Organization =
					organizationRepository.GetDefaultOrganization(UoW, autofacScope.Resolve<IUserService>().CurrentUserId);
			}else 
				autoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);

			autoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);

			var entryBuilder = new CommonEEVMBuilderFactory<Transfer>(this, Entity, UoW, navigationManager) {
				AutofacScope = autofacScope
			};
			
			OrganizationEntryViewModel = entryBuilder.ForProperty(x => x.Organization).MakeByType().Finish();
			OrganizationEntryViewModel.IsEditable = CanEdit;
			WarehouseFromEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseFrom).MakeByType().Finish();
			WarehouseFromEntryViewModel.IsEditable = CanEdit;
			WarehouseToEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseTo).MakeByType().Finish();
			WarehouseToEntryViewModel.IsEditable = CanEdit;
			
			Entity.PropertyChanged += Entity_PropertyChanged;
			Entity.Items.ContentChanged += (sender, args) => UpdateWarehouseFromEditable();
			UpdateWarehouseFromEditable();
			Owners = UoW.GetAll<Owner>().ToList();

			//Переопределяем параметры валидации
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, 
					new Dictionary<object, object> { { nameof(BaseParameters), baseParameters } })));

			this.FeaturesService = featuresService;
			
			//Заполняем складские остатки
			stockBalanceModel.Warehouse = Entity.WarehouseFrom;
			stockBalanceModel.OnDate = Entity.Date;
			if(Entity.Items.Any()) {
				stockBalanceModel.ExcludeOperations = Entity.Items.Select(x => x.WarehouseOperation).ToList();
				stockBalanceModel.AddNomenclatures(Entity.Items.Select(x => x.Nomenclature));
				foreach(var item in Entity.Items) {
					item.StockBalanceModel = this.stockBalanceModel;
				}
				LoadBarcodes();
			}
		}

		private void LoadBarcodes() {
			var itemsWithWarehouseOperation = Entity.Items.Where(i => i.WarehouseOperation?.Id > 0).ToList();
			if(!itemsWithWarehouseOperation.Any())
				return;

			var warehouseOperationIds = itemsWithWarehouseOperation.Select(i => i.WarehouseOperation.Id).ToArray();
			var barcodeOperations = UoW.Session.QueryOver<BarcodeOperation>()
				.WhereRestrictionOn(x => x.WarehouseOperation.Id).IsIn(warehouseOperationIds)
				.List();

			foreach(var item in itemsWithWarehouseOperation)
				item.WarehouseBarcodeOperations = barcodeOperations.Where(bo => bo.WarehouseOperation.Id == item.WarehouseOperation.Id).ToList();
		}

		#region IDialogDocumentation
        public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#transfer");
        public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
        #endregion
        
		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(Entity.WarehouseFrom) && Entity.WarehouseFrom != stockBalanceModel.Warehouse) {
				if(Entity.Items.Any()) {
					if(interactive.Question("При изменении склада отправителя строки документа будут очищены. Продолжить?")) {
						Entity.Items.Clear();
					}
					else {
						//Возвращаем назад старый склад
						Entity.WarehouseFrom = stockBalanceModel.Warehouse;
						return;
					}
				}
				
				stockBalanceModel.Warehouse = Entity.WarehouseFrom;
				OnPropertyChanged(nameof(CanAddItem));
			}

			if(e.PropertyName == nameof(Entity.Date))
				stockBalanceModel.OnDate = Entity.Date;
		}

		private void UpdateWarehouseFromEditable() {
			WarehouseFromEntryViewModel.IsEditable = CanEdit && !Entity.Items.Any();
		}
		#region Sensetive
		public bool CanChangeDocDate => CanEdit && PermissionService.ValidatePresetPermission("can_change_document_date");
		public bool CanAddItem => CanEdit && Entity.WarehouseFrom != null;
		public bool SensitiveDocNumber => CanEdit && !AutoDocNumber;
		public bool BarcodesVisible => FeaturesService.Available(WorkwearFeature.Barcodes);
		#endregion

		#region Свойства

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
		public void AddItems() {
			var selectPage = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave,
				addingRegistrations: builder => {
					builder.RegisterInstance<Action<StockBalanceFilterViewModel>>(
						filter => {
							filter.WarehouseEntry.IsEditable = false;
							filter.Warehouse = Entity.WarehouseFrom;
							filter.CanChooseAmount = true;
						});
				});
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += ViewModel_OnSelectResult_AddItems;
		}

		private void ViewModel_OnSelectResult_AddItems(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var addedAmount = ((StockBalanceJournalViewModel)sender).Filter.AddAmount;
			var items = new List<TransferItem>();
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				var position = node.GetStockPosition(UoW);
				var item = Entity.AddItem(position,
					addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : node.Amount));
				if (item != null)
					items.Add(item);
			}
			stockBalanceModel.AddNomenclatures(items.Select(x => x.Nomenclature));
			foreach(var item in items) {
				item.StockBalanceModel = stockBalanceModel;
			}
		}
		
		public void AddFromScan() {
			//Здесь зануления других моделей обязательно чтобы их не создавал DI
			NavigationManager.OpenViewModelTypedArgs<ClothingAddViewModel>(
				this,
				new[] { typeof(PostomatDocumentViewModel), typeof(OverNormViewModel), typeof(ReturnViewModel), typeof(WriteOffViewModel), typeof(WarehouseTransferViewModel) },
				new object[] { null, null, null, null, this });
		}

		public void AddBarcode() {
			var barcodeJournal = NavigationManager.OpenViewModel<BarcodeJournalViewModel>(
				this,
				OpenPageOptions.AsSlave,
				addingRegistrations: builder => {
					builder.RegisterInstance<Action<BarcodeJournalFilterViewModel>>(filter => {
						filter.Warehouse = Entity.WarehouseFrom;
						filter.WarehouseEntry.IsEditable = false;
					});
				});
			barcodeJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			barcodeJournal.ViewModel.OnSelectResult += ViewModel_OnSelectResult_AddBarcode;
		}

		private void ViewModel_OnSelectResult_AddBarcode(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var items = new List<TransferItem>();
			foreach(var node in e.GetSelectedObjects<BarcodeJournalNode>()) {
				var barcode = UoW.GetById<Barcode>(node.Id);
				var position = node.GetStockPosition(UoW);
				var item = Entity.AddItem(position, 1, new[] { barcode });
				if(item != null)
					items.Add(item);
			}

			stockBalanceModel.AddNomenclatures(items.Select(x => x.Nomenclature));
			foreach(var item in items) {
				item.StockBalanceModel = stockBalanceModel;
			}
		}

		public string ValidateBarcodeForScan(Barcode barcode) {
			if(barcode == null)
				return null;
			barcode = UoW.GetById<Barcode>(barcode.Id);
			var lastOperation = barcodeRepository.GetLastOperationAt(barcode, Entity.Date);
			if(lastOperation?.CurrentWarehouse == null)
				return $"{barcode.Title} на {Entity.Date:d} не числится ни на одном складе.";
			if(lastOperation.CurrentWarehouse != Entity.WarehouseFrom)
				return $"{barcode.Title} числится на складе «{lastOperation.CurrentWarehouse.Name}», а не на складе отправителе «{Entity.WarehouseFrom.Name}».";
			return null;
		}

		public void AddBarcode(Barcode barcode) {
			barcode = UoW.GetById<Barcode>(barcode.Id);
			var lastOperation = barcodeRepository.GetLastOperationAt(barcode, Entity.Date);

			if(lastOperation?.CurrentWarehouse == null || lastOperation.CurrentWarehouse != Entity.WarehouseFrom) {
				interactive.ShowMessage(ImportanceLevel.Warning, $"{barcode.Title}: не числится на складе отправления.");
				return;
			}

			var stockPosition = new StockPosition(
				barcode.Nomenclature,
				lastOperation.WarehouseOperation.WearPercent,
				barcode.Size,
				barcode.Height,
				lastOperation.WarehouseOperation.Owner);
			var existing = Entity.Items.FirstOrDefault(i => stockPosition.Equals(i.StockPosition));
			if(existing == null) {
				var newItem = Entity.AddItem(stockPosition, 1, new[] { barcode });
				if(newItem != null) {
					stockBalanceModel.AddNomenclatures(new[] { newItem.Nomenclature });
					newItem.StockBalanceModel = stockBalanceModel;
				}
				return;
			}
			if(!existing.CanEditAmount) {
				existing.AddBarcode(barcode);
				return;
			}
			interactive.ShowMessage(ImportanceLevel.Warning,
				$"{barcode.Title}: по этой позиции в документе уже есть перемещение без штрихкода, добавить со штрихкодом нельзя.");
		}

		#region Номера комплектов и печать штрихкодов

		public bool NeedsKitNumberRecalc =>
			Entity.Items.SelectMany(i => i.WarehouseBarcodeOperations).Any(bo => !(bo.KitNumber > 0));

		public void RecalculateKitNumbers() {
			var pending = Entity.Items
				.SelectMany(i => i.WarehouseBarcodeOperations)
				.Where(bo => !(bo.KitNumber > 0))
				.ToList();
			if(!pending.Any())
				return;

			if(baseParameters.KitNumberingMode == KitNumberingMode.PerNomenclature) {
				foreach(var group in pending.GroupBy(bo => bo.WarehouseOperation.Nomenclature)) {
					var list = group.ToList();
					var kitNumbers = barcodeService.GetNextKitNumbers(UoW, Entity.WarehouseTo, group.Key, list.Count, baseParameters.KitNumberingMode);
					for(var i = 0; i < list.Count; i++)
						list[i].KitNumber = kitNumbers[i];
				}
			} else {
				var kitNumbers = barcodeService.GetNextKitNumbers(UoW, Entity.WarehouseTo, pending[0].WarehouseOperation.Nomenclature, pending.Count, baseParameters.KitNumberingMode);
				for(var i = 0; i < pending.Count; i++)
					pending[i].KitNumber = kitNumbers[i];
			}
		}

		private void OfferRecalculateKitNumbers() {
			if(NeedsKitNumberRecalc && interactive.Question(
				   "Есть промаркированные позиции, для которых не пересчитан номер комплекта для склада получателя. Пересчитать номера?"))
				RecalculateKitNumbers();
		}

		public void PrintBarcodes(IEnumerable<TransferItem> items) {
			var barcodeIds = items.SelectMany(i => i.Barcodes).Select(b => b.Id).Distinct().ToList();
			if(!barcodeIds.Any())
				return;

			OfferRecalculateKitNumbers();
			if(UoW.HasChanges && !interactive.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if(!Save())
				return;

			var reportInfo = new ReportInfo {
				Title = "Штрихкод",
				Identifier = "Barcodes.Barcode",
				Parameters = new Dictionary<string, object> { { "barcodes", barcodeIds } }
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#endregion

		public void RemoveItems(IEnumerable<TransferItem> items) {
			foreach(var item in items) {
				Entity.Items.Remove(item);
			}
		}
		public void OpenNomenclature(Nomenclature nomenclature) {
			NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(
				this, EntityUoWBuilder.ForOpen(nomenclature.Id));
		}
		public override bool Save() {
			if(AutoDocNumber)
				Entity.DocNumber = null;
			else if(String.IsNullOrWhiteSpace(Entity.DocNumber))
				Entity.DocNumber = Entity.DocNumberText;
			OfferRecalculateKitNumbers();
			Entity.UpdateOperations(UoW, null);
			return base.Save();
		}
		public override void Dispose() {
			base.Dispose();
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
		public bool ValidateNomenclature(TransferItem transferItem) {
			return transferItem.Amount <= transferItem.AmountInStock;
		}
		
		public void Print() {
			if(UoW.HasChanges && !interactive.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = String.Format("Накладная на внутреннее перемещение №{0}", Entity.DocNumber ?? Entity.Id.ToString()),
				Identifier = "Documents.TransferInvoice",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
	}
}
