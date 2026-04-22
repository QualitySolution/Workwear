using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NLog;
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
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Features;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Models;

namespace Workwear.ViewModels.Stock 
{
	public sealed class OverNormViewModel : PermittingEntityDialogViewModelBase<OverNorm>, IDialogDocumentation
	{
		private readonly IOverNormFactory overNormFactory;
		private readonly BarcodeService barcodeService;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public EntityEntryViewModel<Warehouse> EntryWarehouseViewModel { get; }

		#region IDialogDocumentation
//// Сделать документацию	
public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#overnorm");
public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion


		private OverNormModelBase overNormModel; 
		public OverNormModelBase OverNormModel { get => overNormModel; set => SetField(ref overNormModel, value); } 

		
		public OverNormViewModel(IEntityUoWBuilder uowBuilder,
			ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, IUserService userService,
			IOverNormFactory overNormFactory,
			BarcodeService barcodeService,
			StockRepository stockRepository,
			FeaturesService featuresService,
			ICurrentPermissionService permissionService,
			IInteractiveService interactive,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, permissionService, interactive, validator, unitOfWorkProvider) 
		{
			logger.Info("Создание вьюмодели документа переквалификации норм");
			
			if (autofacScope == null) throw new ArgumentNullException(nameof(autofacScope));
			this.overNormFactory = overNormFactory ?? throw new ArgumentNullException(nameof(overNormFactory));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			if(featuresService == null) throw new ArgumentNullException(nameof(featuresService));
			
			SetDocumentDateProperty(e => e.Date);
			
			foreach (OverNormItem item in Entity.Items) {
				OverNormOperation operation = item.OverNormOperation;
				item.Param =
					new OverNormParam(operation.Employee, operation.WarehouseOperation.Nomenclature, operation.WarehouseOperation.Amount, operation.WarehouseOperation.WearSize,
						operation.WarehouseOperation.Height, operation.SubstitutedIssueOperation, operation.BarcodeOperations.Select(x => x.Barcode).ToList());
			}
			
			var builder = new CommonEEVMBuilderFactory<OverNorm>(this, Entity, UoW, NavigationManager, autofacScope);
			EntryWarehouseViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			EntryWarehouseViewModel.IsEditable = CanEdit;
			EntryWarehouseViewModel.Changed += (sender, args) => 
				OnPropertyChanged(nameof(CanAddItems));
			
			OverNormModel = overNormFactory.CreateModel(UoW, Entity.Type);
			
			if (Entity.Id < 1) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				logger.Info("Создание нового документа ");
			}
			else {
				AutoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
				logger.Info($"Открытие документа вдачи вне нормы ID:{Entity.Id}");
			}
			
			if(Entity.Warehouse == null)
				Entity.Warehouse =
					stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			
			Entity.Items.ContentChanged += CalculateTotal;
			CalculateTotal(null, null);
		}

		#region View Properties
		public bool CanAddItems => CanEdit && Entity.Warehouse != null && OverNormModel.Editable;
		public bool CanRemoveActiveItem => CanEdit && SelectedItem != null;
		public bool CanChoiseForActiveItem => CanEdit && SelectedItem != null;
		public bool SensitiveDocNumber => !AutoDocNumber;
		public bool CanChangeDocDate => CanEdit && PermissionService.ValidatePresetPermission("can_change_document_date");
		
		public OverNormType DocType {
			get => Entity.Type;
			set {
				if (Entity.Type == value) 
					return;
				Entity.Type = value;
				Entity.Items.Clear();
				OverNormModel = overNormFactory.CreateModel(UoW, value);
			}
		}
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumber))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public bool AutoDocNumber { get => autoDocNumber; set => SetField(ref autoDocNumber, value); }
		public string DocNumber {
			get => AutoDocNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.DocNumber;
			set => Entity.DocNumber = (AutoDocNumber || value == "авто") ? null : value;
		}
		
		private OverNormItem selectedItem;
		[PropertyChangedAlso(nameof(CanRemoveActiveItem))]
		[PropertyChangedAlso(nameof(CanChoiseForActiveItem))]
		public OverNormItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}
		#endregion
		
		private void CalculateTotal(object sender, EventArgs eventArgs)
		{
			Total = $"Позиций в документе: {Entity.Items.Count}  " +
			        $"Количество единиц: {Entity.Items.Sum(x => x.OverNormOperation.WarehouseOperation?.Amount)}";
		}
		
		#region Commands

		#region Substitute
		public void SelectEmployeeIssue() 
		{
			IPage<EmployeeBalanceJournalViewModel> selectJournal = 
				NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.DateSensitive = false;
			selectJournal.ViewModel.Filter.CheckShowWriteoffVisible = false;
			selectJournal.ViewModel.Filter.Date = Entity.Date;
			selectJournal.ViewModel.Filter.EmployeeSensitive = true;
			selectJournal.ViewModel.Filter.AddAmount = AddedAmount.One;
			selectJournal.ViewModel.OnSelectResult += AddNomenclatureFromEmployee;
		}
		
		private void AddNomenclatureFromEmployee(object sender, JournalSelectedEventArgs e) 
		{
			IList<EmployeeIssueOperation> operations = 
				UoW.GetById<EmployeeIssueOperation>(e.GetSelectedObjects<EmployeeBalanceJournalNode>().Select(x => x.Id));

			foreach (EmployeeIssueOperation empOp in operations) {
				if (Entity.Items.Any(x => x.OverNormOperation.SubstitutedIssueOperation.Id == empOp.Id)) 
					continue;
				Entity.AddItem(new OverNormOperation() { Employee = empOp.Employee, SubstitutedIssueOperation = empOp });
			}
		}
		#endregion

		#region Guest and Simple
		public void SelectEmployees()
		{
			IPage<EmployeeJournalViewModel> selectJournal = NavigationManager.OpenViewModel<EmployeeJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddEmployees;
		}
		
		private void AddEmployees(object sender, JournalSelectedEventArgs e) 
		{
			IList<EmployeeCard> employees = 
				UoW.GetById<EmployeeCard>(e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id));
			foreach (EmployeeCard employee in employees) 
				Entity.AddItem(new OverNormOperation() { Employee = employee });
		}
		#endregion
		
		public void SelectNomenclature(OverNormItem item)
		{
			IPage<StockBalanceJournalViewModel> selectJournal = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>
				(this,
				OpenPageOptions.AsSlave,
				addingRegistrations: builder => {
					builder.RegisterInstance<Action<StockBalanceFilterViewModel>>(
						filter => {
							filter.ShowNegativeBalance = false;
////1289 Выдача без штрихкода отдельно
							filter.ShowWithBarcodes = true; //OverNormModel.UseBarcodes;
							filter.CanChangeShowWithBarcodes = false; //OverNormModel.CanChangeUseBarcodes;
							filter.Warehouse = Entity.Warehouse;
							filter.SensetiveWarehouse = false;
						});
				});
			selectJournal.Tag = item;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Single;
            selectJournal.ViewModel.OnSelectResult += AddNomenclature;
		}
		public void AddNomenclature(object sender, JournalSelectedEventArgs e) {
			StockBalanceJournalNode node = e.GetSelectedObjects<StockBalanceJournalNode>().First();
			StockPosition stockPosition = node.GetStockPosition(UoW);
////1289			
//var size = UoW.GetById<Size>(node.SizeIdn);
//var height = UoW.GetById<Size>(node.HeightIdn);

			Barcode barcode = null;
			IPage page = NavigationManager.FindPage((StockBalanceJournalViewModel)sender);
			OverNormItem item = (OverNormItem)page.Tag;
			var employee = item.Employee;

			if(stockPosition.Nomenclature.UseBarcode) {
				IPage<BarcodeJournalViewModel> barcodeJournal =
					NavigationManager.OpenViewModel<BarcodeJournalViewModel>(this, OpenPageOptions.AsSlave);
				barcodeJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
				barcodeJournal.ViewModel.Filter.CanUseFilter = false;
				barcodeJournal.ViewModel.Filter.Warehouse = Entity.Warehouse;
				barcodeJournal.ViewModel.Filter.StockPosition= node.GetStockPosition(UoW);
				barcodeJournal.ViewModel.OnSelectResult += (o, args) => {
					IList<BarcodeJournalNode> nodes = args.GetSelectedObjects<BarcodeJournalNode>();
					IList<Barcode> barcodes = UoW.GetById<Barcode>(nodes.Select(x => x.Id));

					Entity.DeleteItem(item);
					
					var addedItems = barcodes.GroupBy(b => new { b.Nomenclature, b.Size, b.Height });
					foreach(var i in addedItems.ToList()) {
						var param = new OverNormParam(
							employee,
							i.Key.Nomenclature,
							i.Count(),
							i.Key.Size,
							i.Key.Height,
							null,
							i.ToList());
						if(!Entity.Items.Any(x => x.Param.Equals(param))) {
							OverNormItem newItem = new OverNormItem(Entity, new OverNormOperation());
							newItem.Param = param;
							AddOrUpdateItem(newItem);
						} else {
							item.Param =
								new OverNormParam(item.OverNormOperation.Employee, stockPosition.Nomenclature, barcodes.Count,
									stockPosition.WearSize, stockPosition.Height,
									item.OverNormOperation.SubstitutedIssueOperation, barcodes);
							AddOrUpdateItem(item);
						}
					}
				};
			}
		}

		public void DeleteItem(OverNormItem item) {
			Entity.DeleteItem(item);
		}

		public void DeleteBarcodeFromItem(OverNormItem item, Barcode barcode) {
			if (item.Param.Barcodes.Count == 1) {
				DeleteItem(item);
				return;
			}

			OverNormModel.UseBarcodes = true;
			item.Param.Barcodes.Remove(barcode);
			AddOrUpdateItem(item);
		} 
		
		private void AddOrUpdateItem(OverNormItem item)
		{
			if (item.Id < 1) {
				Entity.Items.Remove(item);
				OverNormModel.AddOperation(Entity, item.Param, Entity.Warehouse);
			} else 
				OverNormModel.UpdateOperation(item, item.Param);
		}
		#endregion

		#region Save
		public override bool Save() 
		{
			logger.Info("Запись документа  вдачи вне нормы...");
			
			if (!Validate()) {
				logger.Warn("Валидация не пройдена, сохранение отменено.");
				return false;
			}
			logger.Info("Валидация пройдена.");

			logger.Info("Обновление складских операций...");
			foreach(OverNormItem item in Entity.Items) {
				item.OverNormOperation.WarehouseOperation.OperationTime = Entity.Date;
				UoW.Save(item.OverNormOperation.WarehouseOperation);
				
				item.OverNormOperation.OperationTime = Entity.Date;
				UoW.Save(item.OverNormOperation);
				
				foreach(BarcodeOperation bo in item.OverNormOperation.BarcodeOperations) 
                    UoW.Save(bo);
			}
			
			bool result = base.Save();
			if(result)
				logger.Info("Документ  вдачи вне нормы.");
			else
				logger.Warn("Ошибка при сохранении документа.");
			return result;
		}
		#endregion
		
	}

	
	
//// Потенциально мусор	
	public class OverNormTempItem : PropertyChangedBase 
	{
		private OverNormItem item;
		public OverNormItem Item {
			get => item;
			set => SetField(ref item, value);
		}

		private OverNormParam param;
		public OverNormParam Param {
			get => param;
			set => SetField(ref param, value);
		}

		public OverNormTempItem(OverNormItem item, OverNormParam param) {
			Item = item;
			Param = param;
		}
	}
}
