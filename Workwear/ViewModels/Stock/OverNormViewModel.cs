using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
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
using Workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Models;

namespace Workwear.ViewModels.Stock 
{
	public sealed class OverNormViewModel : PermittingEntityDialogViewModelBase<OverNorm>, IDialogDocumentation
	{
		private readonly IOverNormFactory overNormFactory;
		private readonly FeaturesService featuresService;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public EntityEntryViewModel<Warehouse> EntryWarehouseViewModel { get; }

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#over-norm-issue");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion


		private OverNormModelBase overNormModel; 
		public OverNormModelBase OverNormModel { get => overNormModel; set => SetField(ref overNormModel, value); } 

		
		public OverNormViewModel(IEntityUoWBuilder uowBuilder,
			ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, IUserService userService,
			IOverNormFactory overNormFactory,
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
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			
			SetDocumentDateProperty(e => e.Date);
			
			var builder = new CommonEEVMBuilderFactory<OverNorm>(this, Entity, UoW, NavigationManager, autofacScope);
			EntryWarehouseViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			EntryWarehouseViewModel.IsEditable = CanEdit;
			EntryWarehouseViewModel.Changed += (sender, args) => 
				OnPropertyChanged(nameof(CanAddItems));
			
			if (Entity.Id < 1) {
				Entity.Type = AvailableOverNormTypes.FirstOrDefault();
				Entity.CreatedbyUser = userService.GetCurrentUser();
				logger.Info("Создание нового документа ");
			}
			else {
				AutoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
				logger.Info($"Открытие документа вдачи вне нормы ID:{Entity.Id}");
			}

			OverNormModel = overNormFactory.CreateModel(UoW, Entity.Type);
			
			if(Entity.Warehouse == null)
				Entity.Warehouse =
					stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			
			Entity.Items.ContentChanged += CalculateTotal;
			CalculateTotal(null, null);
		}

		#region View Properties
		public bool CanAddItems => CanEdit && Entity.Warehouse != null && OverNormModel.Editable;
		public bool CanRemoveActiveItem => CanEdit && SelectedItem != null;
		public bool CanChoiceForActiveItem => CanEdit && SelectedItem != null;
		public bool SensitiveDocNumber => !AutoDocNumber;
		public bool CanChangeDocDate => CanEdit && PermissionService.ValidatePresetPermission("can_change_document_date");
		public IEnumerable<OverNormType> AvailableOverNormTypes =>
			Enum.GetValues(typeof(OverNormType))
				.Cast<OverNormType>()
				.Where(featuresService.AvailableOverNorm);
		public IEnumerable<object> HiddenOverNormTypes =>
			Enum.GetValues(typeof(OverNormType))
				.Cast<OverNormType>()
				.Where(type => !featuresService.AvailableOverNorm(type))
				.Cast<object>();
		
		public OverNormType DocType {
			get => Entity.Type;
			set {
				if (Entity.Type == value) 
					return;
				if(!featuresService.AvailableOverNorm(value))
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
		[PropertyChangedAlso(nameof(CanChoiceForActiveItem))]
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
				if (Entity.Items.Any(x => x.OverNormOperation.SubstitutedIssueOperation?.Id == empOp.Id)) 
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
		
		/// <summary>
		/// Выбор из складских остатков для выдачи вне нормы на конкретного сотрудника
		/// </summary>
		/// <param name="item"></param>
		public void SelectNomenclature(OverNormItem item)
		{
			IPage<StockBalanceJournalViewModel> selectJournal = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>
				(this,
				OpenPageOptions.AsSlave,
				addingRegistrations: builder => {
					builder.RegisterInstance<Action<StockBalanceFilterViewModel>>(
						filter => {
							filter.ShowNegativeBalance = false;
							filter.ShowWithBarcodes = !OverNormModel.CanUseWithoutBarcodes;
							filter.CanChangeShowWithBarcodes = OverNormModel.CanChangeUseBarcodes;
							filter.CanChooseAmount = OverNormModel.CanUseWithoutBarcodes;
							filter.AddAmount = AddedAmount.One;
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
			IPage page = NavigationManager.FindPage((StockBalanceJournalViewModel)sender);
			OverNormItem item = (OverNormItem)page.Tag;

			if(stockPosition.Nomenclature.UseBarcode) {
				IPage<BarcodeJournalViewModel> barcodeJournal =
					NavigationManager.OpenViewModel<BarcodeJournalViewModel>(
						this,
						OpenPageOptions.AsSlave,
						addingRegistrations: builder => {
							builder.RegisterInstance<Action<BarcodeJournalFilterViewModel>>(filter => {
								filter.CanUseFilter = false;
								filter.Warehouse = Entity.Warehouse;
								filter.StockPosition = stockPosition;
							});
						}

					);
				barcodeJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
				barcodeJournal.ViewModel.OnSelectResult += (s, args) => AddWithBarcode(s, args, item);
			}
			else if(OverNormModel.CanUseWithoutBarcodes) {
				var selectVM = sender as StockBalanceJournalViewModel;
				var addedAmount = selectVM.Filter.AddAmount;
				var addedParam = new OverNormParam(
					item.Employee,
					stockPosition.Nomenclature,
					addedAmount == AddedAmount.All ? node.Amount : 1,
					stockPosition.WearSize,
					stockPosition.Height,
					item.OverNormOperation?.SubstitutedIssueOperation,
					wearPercent: stockPosition.WearPercent,
					owner: stockPosition.Owner);
				OverNormModel.UseBarcodes = false;
				AddOrUpdateItem(item, addedParam).MaxAmount = node.Amount;
			}
		}

		private void AddWithBarcode(object sender, JournalSelectedEventArgs e, OverNormItem item) {
			IList<BarcodeJournalNode> barcodeNodes = e.GetSelectedObjects<BarcodeJournalNode>();
			BarcodeOperation barcodeOperationAlias = null;
			IList<Barcode> barcodes = UoW.Session.QueryOver<Barcode>()
				.WhereRestrictionOn(x => x.Id).IsIn(barcodeNodes.Select(x => x.Id).ToArray())
				.Left.JoinAlias(x => x.BarcodeOperations, () => barcodeOperationAlias)
				.Fetch(SelectMode.Fetch, x => x.BarcodeOperations)
				.List()
				.Distinct()
				.ToList();
			var addedItems = barcodes.GroupBy(b => new { b.Nomenclature, b.Size, b.Height }).ToList();
			if(addedItems.Count == 1) {
				var addedItem = addedItems.First();
				var addedParam = new OverNormParam(
					item.Employee,
					addedItem.Key.Nomenclature,
					addedItem.Count(),
					addedItem.Key.Size,
					addedItem.Key.Height,
					item.OverNormOperation?.SubstitutedIssueOperation,
					addedItem.ToList());
				OverNormModel.UseBarcodes = addedParam.Barcodes.Any();
				AddOrUpdateItem(item, addedParam);
			}
			else
				throw new NotImplementedException("Выбраны несколько разных складских позиций. Нужно добавлять строки, пока это реализовано.");
		}
		
		public void DeleteItem(OverNormItem item) {
			Entity.DeleteItem(item);
		}

		public void DeleteBarcodeFromItem(OverNormItem item, Barcode barcode) {
			if (item.OverNormOperation.BarcodeOperations.Count == 1) {
				DeleteItem(item);
				return;
			}

			var barcodeOperation = item.OverNormOperation.BarcodeOperations
				.FirstOrDefault(x => x.Barcode == barcode || x.Barcode.Id > 0 && x.Barcode.Id == barcode.Id);
			if(barcodeOperation != null) {
				barcodeOperation.Barcode?.BarcodeOperations.Remove(barcodeOperation);
				item.OverNormOperation.BarcodeOperations.Remove(barcodeOperation);
			}
		} 
		
		private OverNormItem AddOrUpdateItem(OverNormItem item, OverNormParam param) {
			OverNormModel.UpdateOperation(item, param);
			return item;
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
				logger.Info("Документ вдачи вне нормы.");
			else
				logger.Warn("Ошибка при сохранении документа.");
			return result;
		}
		#endregion
		
	}
}
