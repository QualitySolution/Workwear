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
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.Report;
using QS.Report.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Stock
{
	public class WarehouseTransferViewModel : EntityDialogViewModelBase<Transfer>
	{
		public EntityEntryViewModel<Organization> OrganizationEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseFromEntryViewModel;
		public EntityEntryViewModel<Warehouse> WarehouseToEntryViewModel;
		public readonly FeaturesService FeaturesService;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly IInteractiveQuestion interactive;
		
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
			OrganizationRepository organizationRepository,
			StockBalanceModel stockBalanceModel,
			IInteractiveQuestion interactive,
			FeaturesService featuresService) : base(uowBuilder, unitOfWorkFactory, navigationManager, validator, unitOfWorkProvider) {
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			if(UoW.IsNew) {
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
			WarehouseFromEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseFrom).MakeByType().Finish();
			WarehouseToEntryViewModel = entryBuilder.ForProperty(x => x.WarehouseTo).MakeByType().Finish();
			
			Entity.PropertyChanged += Entity_PropertyChanged;
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
			}
		}

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
		#region Sensetive
		public bool CanAddItem => Entity.WarehouseFrom != null;
		public bool SensitiveDocNumber => !AutoDocNumber;
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
			selectPage.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}

		private void ViewModel_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
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
