using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.Filter.ViewModels.Company;
using Workwear.Journal.Filter.ViewModels.Regulations;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;
using Workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;
using Workwear.Repository.Regulations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.ClothingService;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Postomats;

namespace Workwear.ViewModels.Stock
{
    public class WriteOffViewModel : PermittingEntityDialogViewModelBase<Writeoff>, IDialogDocumentation
    {
	    private readonly EmployeeIssueModel employeeIssueModel;
	    private readonly StockBalanceModel stockBalanceModel;
	    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public SizeService SizeService { get; }
        public EmployeeCard Employee { get;}
        public DutyNorm DutyNorm { get;}
        public Warehouse CurWarehouse { get; set; }
        public FeaturesService FeaturesService { get; }
        private OrganizationRepository organizationRepository;
        private IInteractiveService interactive;
        public IList<Owner> Owners { get; }
        public IList<CausesWriteOff> CausesWriteOffs { get; }
        private DutyNormRepository dutyNormRepository;
        private readonly BarcodeService barcodeService;
        private readonly BarcodeRepository barcodeRepository;
        private readonly BarcodeOperationRepository barcodeOperationRepository;

        public WriteOffViewModel(
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            INavigationManager navigation,
            IInteractiveService interactive,
			ICurrentPermissionService permissionService,
            ILifetimeScope autofacScope,
            IUserService userService,
            BaseParameters baseParameters,
            UnitOfWorkProvider unitOfWorkProvider,
            SizeService sizeService,
            FeaturesService featuresService,
            EmployeeIssueModel issueModel,
            StockBalanceModel stockBalanceModel,
            OrganizationRepository organizationRepository,
            DutyNormRepository dutyNormRepository,
            BarcodeService barcodeService,
            BarcodeRepository barcodeRepository,
            BarcodeOperationRepository barcodeOperationRepository,
            EmployeeCard employee = null,
            DutyNorm dutyNorm = null,
            IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, permissionService, interactive, validator, unitOfWorkProvider) {
	        this.employeeIssueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
	        this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
	        FeaturesService = featuresService;
            SizeService = sizeService;
            NavigationManager = navigation;
            this.interactive = interactive;
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.dutyNormRepository=dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
            this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
            this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
            this.barcodeOperationRepository = barcodeOperationRepository ?? throw new ArgumentNullException(nameof(barcodeOperationRepository));
            SetDocumentDateProperty(e => e.Date);
            
            Entity.Items.ContentChanged += (sender, args) =>  CalculateTotal();
            CalculateTotal();
            
            if (Entity.Id == 0) {
	            Entity.CreatedbyUser = userService.GetCurrentUser();
            }
            Employee = UoW.GetInSession(employee);
            DutyNorm = UoW.GetInSession(dutyNorm);
            Owners = UoW.GetAll<Owner>().ToList();
            CausesWriteOffs = UoW.GetAll<CausesWriteOff>().ToList();
            var entryBuilder = new CommonEEVMBuilderFactory<Writeoff>(this, Entity, UoW, navigation) {
	            AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope))
            };
            
            ResponsibleDirectorPersonEntryViewModel = entryBuilder.ForProperty(x => x.Director)
	            .UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
	            .UseViewModelDialog<LeadersViewModel>()
	            .Finish();
            ResponsibleDirectorPersonEntryViewModel.IsEditable = CanEdit;
	            
            ResponsibleChairmanPersonEntryViewModel = entryBuilder.ForProperty(x => x.Chairman)
	            .UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
	            .UseViewModelDialog<LeadersViewModel>()
	            .Finish();
            ResponsibleChairmanPersonEntryViewModel.IsEditable = CanEdit;
	            
            ResponsibleOrganizationEntryViewModel = entryBuilder.ForProperty(x => x.Organization)
	            .UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
	            .UseViewModelDialog<OrganizationViewModel>()
	            .Finish();
            ResponsibleOrganizationEntryViewModel.IsEditable = CanEdit;
	            
            if(Entity.Id == 0)
	            Entity.Organization = organizationRepository.GetDefaultOrganization(UoW, autofacScope.Resolve<IUserService>().CurrentUserId);

            if(Entity.Id == 0) {
	            Entity.CreatedbyUser = userService.GetCurrentUser();
	            logger.Info($"Создание Нового документа Списания");
            } else {
	            AutoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
	            LoadWarehouseBarcodes();
            }

            Validations.Clear();
            Validations.Add(
	            new ValidationRequest(Entity, 
		            new ValidationContext(Entity, 
			            new Dictionary<object, object> { {nameof(BaseParameters), baseParameters} } 
		            )));
        }
        
        #region IDialogDocumentation
        public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#writeoff");
        public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
        #endregion
        
        #region ViewProperty
        
        public EntityEntryViewModel<Leader> ResponsibleDirectorPersonEntryViewModel { get; set; }
        public EntityEntryViewModel<Leader> ResponsibleChairmanPersonEntryViewModel { get; set; }
        public EntityEntryViewModel<Organization> ResponsibleOrganizationEntryViewModel { get; set; }
        
        public bool CanChangeDocDate => CanEdit && PermissionService.ValidatePresetPermission("can_change_document_date");
        public bool SensitiveDocNumber => CanEdit && !AutoDocNumber;
        public bool BarcodesVisible => FeaturesService.Available(WorkwearFeature.Barcodes);
        
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
        
        private string total;
        public string Total {
            get => total;
            set => SetField(ref total, value);
        }
        #endregion

        private void CalculateTotal() {
            Total = $"Позиций в документе: {Entity.Items.Count}  " +
                    $"Количество единиц: {Entity.Items.Sum(x => x.Amount)}";
        }

        #region Items
        public void AddFromStock() {
            var selectJournal = 
                NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave,
	                addingRegistrations: builder => {
		                builder.RegisterInstance<Action<StockBalanceFilterViewModel>>(
			                filter => {
				                if(CurWarehouse != null) {
					                filter.Warehouse = CurWarehouse;
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
	        var warehouse = selectVM.Filter.Warehouse;
	        foreach (var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
		        var count = addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : node.Amount);
		        AddFromStockPosition(node.GetStockPosition(UoW), warehouse, count);
	        }
	        CalculateTotal();
        }

        /// <summary>
        /// Добавление позиции склада в списание. Если для этой позиции на складе есть промаркированные штрихкодом
        /// единицы - открывает диалог выбора конкретных меток
        /// </summary>
        private void AddFromStockPosition(StockPosition stockPosition, Warehouse warehouse, int count) {
	        var markedCount = count > 0 ? barcodeService.CountBarcodesOnWarehouse(UoW, stockPosition, warehouse) : 0;
	        if(markedCount <= 0) {
		        Entity.AddItem(stockPosition, warehouse, count);
		        return;
	        }

	        var barcodeJournal = NavigationManager.OpenViewModel<BarcodeJournalViewModel>(
		        this,
		        OpenPageOptions.AsSlave,
		        addingRegistrations: builder => {
			        builder.RegisterInstance<Action<BarcodeJournalFilterViewModel>>(filter => {
				        filter.CanUseFilter = false;
				        filter.Warehouse = warehouse;
				        filter.StockPosition = stockPosition;
			        });
		        });
	        barcodeJournal.Tag = new StockBarcodeAddContext(stockPosition, warehouse, count);
	        barcodeJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
	        barcodeJournal.ViewModel.OnSelectResult += AddSelectedStockBarcodes;
        }

        private void AddSelectedStockBarcodes(object sender, JournalSelectedEventArgs e) {
	        var page = NavigationManager.FindPage((BarcodeJournalViewModel)sender);
	        var context = (StockBarcodeAddContext)page.Tag;
	        var barcodes = GetSelectedBarcodes(e);

	        if(barcodes.Any())
		        Entity.AddItem(context.StockPosition, context.Warehouse, barcodes.Count, barcodes: barcodes);

	        var remainder = context.Count - barcodes.Count;
	        if(remainder > 0)
		        Entity.AddItem(context.StockPosition, context.Warehouse, remainder);

	        CalculateTotal();
        }

        private class StockBarcodeAddContext {
	        public StockBarcodeAddContext(StockPosition stockPosition, Warehouse warehouse, int count) {
		        StockPosition = stockPosition;
		        Warehouse = warehouse;
		        Count = count;
	        }
	        public StockPosition StockPosition { get; }
	        public Warehouse Warehouse { get; }
	        public int Count { get; }
        }

        public void AddFromEmployee() {
            var selectJournal = 
                NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel, EmployeeCard>(
                    this,
                    Employee,
                    OpenPageOptions.AsSlave,
                    addingRegistrations: builder => { builder.RegisterInstance<Action<EmployeeBalanceFilterViewModel>>(
	                    filter => {
		                    filter.DateSensitive = false;
		                    filter.CheckShowWriteoffVisible = false;
		                    filter.SubdivisionSensitive = Employee == null;
		                    filter.EmployeeSensitive = Employee == null;
		                    filter.Date = Entity.Date;
		                    filter.CanChooseAmount = true;
	                    });
                    });
            selectJournal.ViewModel.OnSelectResult += SelectFromEmployee_Selected;
        }
        private void SelectFromEmployee_Selected(object sender, JournalSelectedEventArgs e)
        {
            var operations = UoW.GetById<EmployeeIssueOperation>(e.GetSelectedObjects<EmployeeBalanceJournalNode>().Select(x => x.Id));
            var addedAmount = ((EmployeeBalanceJournalViewModel)sender).Filter.AddAmount;
            var balance = e.GetSelectedObjects<EmployeeBalanceJournalNode>().ToDictionary(k => k.Id, v => v.Balance);

            foreach (var operation in operations)
	            AddEmployeeOperation(operation, addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : balance[operation.Id]));

            CalculateTotal();
        }

        private void AddEmployeeOperation(EmployeeIssueOperation operation, int amount) {
	        if(amount <= 0) {
		        Entity.AddItem(operation, amount);
		        return;
	        }
	        var availableBarcodeIds = barcodeOperationRepository.GetAvailableBarcodeIdsForReturn(operation, UoW);
	        if(!availableBarcodeIds.Any()) {
		        Entity.AddItem(operation, amount);
		        return;
	        }
	        if(availableBarcodeIds.Count > 1) {
		        OpenBarcodeSelector(operation, availableBarcodeIds, AddSelectedEmployeeBarcodes);
		        return;
	        }
	        var barcodes = barcodeOperationRepository.GetBarcodes(availableBarcodeIds, UoW);
	        Entity.AddItem(operation, 1, barcodes: barcodes);
        }

        public void AddFromDutyNorm() 
        {
	        var selectJournal = NavigationManager.OpenViewModel<DutyNormBalanceJournalViewModel, DutyNorm>(
		        this,
		        DutyNorm,
		        OpenPageOptions.AsSlave,
		        addingRegistrations: builder => {
			        builder.RegisterInstance<Action<DutyNormBalanceFilterViewModel>>(
				        filter => {
					        filter.DateSensitive = false;
					        filter.SubdivisionSensitive =  DutyNorm == null;
					        filter.DutyNormSensitive = DutyNorm == null;
					        filter.Date = Entity.Date;
				        });
		        });
	        selectJournal.ViewModel.OnSelectResult += SelectFromDutyNorm_Selected;
        }

        private void SelectFromDutyNorm_Selected(object sender, JournalSelectedEventArgs e)
        {
	        var operations=UoW.GetById<DutyNormIssueOperation>(e.GetSelectedObjects<DutyNormBalanceJournalNode>().Select(x => x.Id));
	        var balance = e.GetSelectedObjects<DutyNormBalanceJournalNode>().ToDictionary(k => k.Id, v => v.Balance);
	        foreach(var operation in operations)
		        AddDutyNormOperation(operation, balance[operation.Id]);
	        CalculateTotal();
        }

        private void AddDutyNormOperation(DutyNormIssueOperation operation, int amount) {
	        if(amount <= 0) {
		        Entity.AddItem(operation, amount);
		        return;
	        }
	        var availableBarcodeIds = barcodeOperationRepository.GetAvailableBarcodeIdsForReturn(operation, UoW);
	        if(!availableBarcodeIds.Any()) {
		        Entity.AddItem(operation, amount);
		        return;
	        }
	        if(availableBarcodeIds.Count > 1) {
		        OpenBarcodeSelector(operation, availableBarcodeIds, AddSelectedDutyNormBarcodes);
		        return;
	        }
	        var barcodes = barcodeOperationRepository.GetBarcodes(availableBarcodeIds, UoW);
	        Entity.AddItem(operation, 1, barcodes: barcodes);
        }

        private void OpenBarcodeSelector<T>(T operation, IList<int> availableBarcodeIds, EventHandler<JournalSelectedEventArgs> selectHandler) {
	        var barcodeJournal = NavigationManager.OpenViewModel<BarcodeJournalViewModel>(
		        this,
		        OpenPageOptions.AsSlave,
		        addingRegistrations: builder => {
			        builder.RegisterInstance<Action<BarcodeJournalFilterViewModel>>(filter => {
				        filter.CanUseFilter = false;
				        filter.AllowedBarcodeIds = availableBarcodeIds;
			        });
		        });
	        barcodeJournal.Tag = operation;
	        barcodeJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
	        barcodeJournal.ViewModel.OnSelectResult += selectHandler;
        }

        private void AddSelectedEmployeeBarcodes(object sender, JournalSelectedEventArgs e) {
	        var page = NavigationManager.FindPage((BarcodeJournalViewModel)sender);
	        var operation = (EmployeeIssueOperation)page.Tag;
	        var barcodes = GetSelectedBarcodes(e);

	        Entity.AddItem(operation, barcodes.Count, barcodes: barcodes);
	        CalculateTotal();
        }

        private void AddSelectedDutyNormBarcodes(object sender, JournalSelectedEventArgs e) {
	        var page = NavigationManager.FindPage((BarcodeJournalViewModel)sender);
	        var operation = (DutyNormIssueOperation)page.Tag;
	        var barcodes = GetSelectedBarcodes(e);

	        Entity.AddItem(operation, barcodes.Count, barcodes: barcodes);
	        CalculateTotal();
        }

        private IList<Barcode> GetSelectedBarcodes(JournalSelectedEventArgs e) {
	        var barcodeIds = e.GetSelectedObjects<BarcodeJournalNode>()
		        .Select(x => x.Id)
		        .ToArray();
	        return barcodeOperationRepository.GetBarcodes(barcodeIds, UoW);
        }

        #region Сканирование штрихкода

        public void AddFromScan() {
	        //Здесь зануления других моделей обязательно чтобы их не создавал DI
	        NavigationManager.OpenViewModelTypedArgs<ClothingAddViewModel>(
		        this,
		        new[] { typeof(PostomatDocumentViewModel), typeof(OverNormViewModel), typeof(ReturnViewModel), typeof(WriteOffViewModel) },
		        new object[] { null, null, null, this });
        }

        public string ValidateBarcodeForScan(Barcode barcode) {
	        if(barcode == null)
		        return null;
	        barcode = UoW.GetById<Barcode>(barcode.Id);
	        var lastOperation = barcodeRepository.GetLastOperationAt(barcode, Entity.Date);
	        if((lastOperation?.EmployeeIssueOperation?.Issued ?? 0) <= 0
	           && (lastOperation?.DutyNormIssueOperation?.Issued ?? 0) <= 0
	           && lastOperation?.CurrentWarehouse == null)
		        return $"{barcode.Title} на {Entity.Date:d} не числится ни за сотрудником, ни за дежурной нормой, ни на складе.";
	        return null;
        }

        public void AddBarcode(Barcode barcode) {
	        barcode = UoW.GetById<Barcode>(barcode.Id);
	        var lastOperation = barcodeRepository.GetLastOperationAt(barcode, Entity.Date);

	        if((lastOperation?.EmployeeIssueOperation?.Issued ?? 0) > 0)
		        AddOrExtendEmployeeItem(barcode, lastOperation.EmployeeIssueOperation);
	        else if((lastOperation?.DutyNormIssueOperation?.Issued ?? 0) > 0)
		        AddOrExtendDutyNormItem(barcode, lastOperation.DutyNormIssueOperation);
	        else if(lastOperation?.CurrentWarehouse != null)
		        AddOrExtendWarehouseItem(barcode, lastOperation);
	        else {
		        interactive.ShowMessage(ImportanceLevel.Warning, $"{barcode.Title}: ни к чему не привязан.");
		        return;
	        }
	        CalculateTotal();
        }

        private void AddOrExtendEmployeeItem(Barcode barcode, EmployeeIssueOperation issuedOperation) {
	        var existing = Entity.Items.FirstOrDefault(i => i.WriteoffFrom == WriteoffFrom.Employee
		        && DomainHelper.EqualDomainObjects(i.EmployeeWriteoffOperation?.IssuedOperation, issuedOperation));
	        if(existing == null) {
		        Entity.AddItem(issuedOperation, 1, new[] { barcode });
		        return;
	        }
	        if(!existing.CanEditAmount) {
		        existing.AddBarcode(barcode);
		        return;
	        }
	        interactive.ShowMessage(ImportanceLevel.Warning,
		        $"{barcode.Title}: по этой выдаче в документе уже есть списание без штрихкода, добавить со штрихкодом нельзя.");
        }

        private void AddOrExtendDutyNormItem(Barcode barcode, DutyNormIssueOperation issuedOperation) {
	        var existing = Entity.Items.FirstOrDefault(i => i.WriteoffFrom == WriteoffFrom.DutyNorm
		        && DomainHelper.EqualDomainObjects(i.DutyNormWriteOffOperation?.IssuedOperation, issuedOperation));
	        if(existing == null) {
		        Entity.AddItem(issuedOperation, 1, new[] { barcode });
		        return;
	        }
	        if(!existing.CanEditAmount) {
		        existing.AddBarcode(barcode);
		        return;
	        }
	        interactive.ShowMessage(ImportanceLevel.Warning,
		        $"{barcode.Title}: по этой выдаче в документе уже есть списание без штрихкода, добавить со штрихкодом нельзя.");
        }

        private void AddOrExtendWarehouseItem(Barcode barcode, BarcodeOperation lastOperation) {
	        var warehouse = lastOperation.CurrentWarehouse;
	        var stockPosition = new StockPosition(
		        barcode.Nomenclature,
		        lastOperation.WarehouseOperation.WearPercent,
		        barcode.Size,
		        barcode.Height,
		        lastOperation.WarehouseOperation.Owner);
	        var existing = Entity.Items.FirstOrDefault(i => i.WriteoffFrom == WriteoffFrom.Warehouse
		        && i.WarehouseOperation?.ExpenseWarehouse == warehouse && stockPosition.Equals(i.StockPosition));
	        if(existing == null) {
		        Entity.AddItem(stockPosition, warehouse, 1, new[] { barcode });
		        return;
	        }
	        if(!existing.CanEditAmount) {
		        existing.AddBarcode(barcode);
		        return;
	        }
	        interactive.ShowMessage(ImportanceLevel.Warning,
		        $"{barcode.Title}: по этой позиции в документе уже есть списание без штрихкода, добавить со штрихкодом нельзя.");
        }

        private void LoadWarehouseBarcodes() {
	        var warehouseItems = Entity.Items.Where(i => i.WriteoffFrom == WriteoffFrom.Warehouse && i.WarehouseOperation?.Id > 0).ToList();
	        if(!warehouseItems.Any())
		        return;

	        var warehouseOperationIds = warehouseItems.Select(i => i.WarehouseOperation.Id).ToArray();
	        var barcodeOperations = UoW.Session.QueryOver<BarcodeOperation>()
		        .WhereRestrictionOn(x => x.WarehouseOperation.Id).IsIn(warehouseOperationIds)
		        .List();

	        foreach(var item in warehouseItems)
		        item.WarehouseBarcodeOperations = barcodeOperations.Where(bo => bo.WarehouseOperation.Id == item.WarehouseOperation.Id).ToList();
        }

        #endregion

        public void FillMaxAmount(DateTime? date = null) {
	        var itemsWh = Entity.Items.Where(i => i.WriteoffFrom == WriteoffFrom.Warehouse).ToList();
	        if(itemsWh.Any()) {
		        // для всех списаний со склада
		        var nomenclatures = itemsWh.Select(i => i.Nomenclature);
		        stockBalanceModel.OnDate = date;
		        stockBalanceModel.ExcludeOperations = itemsWh.Select(i => i.WarehouseOperation);
		        stockBalanceModel.AddNomenclatures(nomenclatures);
		        foreach(var item in itemsWh)
			        item.MaxAmount = stockBalanceModel.GetAmount(item.StockPosition);
	        }
	        
	        var itemsEmp = Entity.Items.Where(i => i.WriteoffFrom == WriteoffFrom.Employee).ToList();
	        if(itemsEmp.Any()) {
		        // для всех списаний с сотрудника
		        var operations = itemsEmp
			        .Select(i => i.EmployeeWriteoffOperation)
			        .Select(o => o.IssuedOperation)
			        .ToArray();
		        var writtenOff = employeeIssueModel.CalculateWrittenOff(operations, UoW, date);

		        foreach(var item in itemsEmp) {
			        var operation = item.EmployeeWriteoffOperation.IssuedOperation;
					item.MaxAmount = operation.Issued -
						(writtenOff.ContainsKey(operation.Id) ? writtenOff[operation.Id] : 0);
		        }
	        }
	        var itemsDutyNorm = Entity.Items.Where(i=>i.WriteoffFrom==WriteoffFrom.DutyNorm).ToList();
	        if(itemsDutyNorm.Any()) {
		        // для всех списаний с дежурной нормы
		        var operations = itemsDutyNorm
			        .Select(i=>i.DutyNormWriteOffOperation)
			        .Select(o=>o.IssuedOperation)
			        .ToArray();
		        var writtenOff=dutyNormRepository.CalculateWrittenOff(operations, UoW, date);
		        foreach(var item in itemsDutyNorm) {
			        var operation = item.DutyNormWriteOffOperation.IssuedOperation;
			        item.MaxAmount = operation.Issued - (writtenOff.ContainsKey(operation.Id) ? writtenOff[operation.Id] : 0);
		        }
	        }
	        
        }
        public void DeleteItem(WriteoffItem item) {
            Entity.RemoveItem(item);
            CalculateTotal();
        }
        #endregion

        #region Members
        public void DeleteMember(Leader member) {
	        Entity.RemoveMember(member);
        }
        public void AddMembers() {
	        var selectPage = NavigationManager.OpenViewModel<LeadersJournalViewModel>(this, OpenPageOptions.AsSlave);
	        selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
	        selectPage.ViewModel.OnSelectResult += MemberOnSelectResult;
        }
        void MemberOnSelectResult(object sender, JournalSelectedEventArgs e) {
	        var members = UoW.GetById<Leader>(e.SelectedObjects.Select(x => x.GetId()));
	        foreach(var member in members)
		        Entity.AddMember(member);
        }
        #endregion
        
        #region Save and print
        public override bool Save() {
            logger.Info ("Запись документа...");
            
            FillMaxAmount(Entity.Date);
            Entity.UpdateOperations(UoW);
            if (Entity.Id == 0)
                Entity.CreationDate = DateTime.Now;
            
            if(AutoDocNumber)
	            Entity.DocNumber = null;

            if(!base.Save()) {
	            logger.Info("Не Ок.");
	            return false;
            }
            
            var employeeOperations = Entity.Items.Where(w => w.WriteoffFrom == WriteoffFrom.Employee)
	            .Select(w => w.EmployeeWriteoffOperation)
	            .Where(w => w.ProtectionTools != null)
	            .ToArray();
            if(employeeOperations.Any()) {
                logger.Info("Обновляем записи о выданной одежде в карточке сотрудника...");
                employeeIssueModel.UpdateNextIssue(employeeOperations, changeLog: (operation, s) => logger.Debug(s));
                UoW.Commit();
            }
            logger.Info ("Ok");
            return true;
        }
        
        public void Print() {
	        if(UoW.HasChanges && !interactive.Question("Перед печатью документ будет сохранён. Продолжить?"))
		        return;
	        if (!Save())
		        return;
			
	        var reportInfo = new ReportInfo {
		        Title = String.Format("Акт списания №{0}", Entity.DocNumber ?? Entity.Id.ToString()),
		        Identifier = "Documents.WriteOffSheet",
		        Parameters = new Dictionary<string, object> {
			        { "id",  Entity.Id },
			        {"printPromo", FeaturesService.Available(WorkwearFeature.PrintPromo)},
		        }
	        };
	        NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
        }
        #endregion
    }
}
