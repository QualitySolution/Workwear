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
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Repository.Regulations;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Company;

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

            if(UoW.IsNew) {
	            Entity.CreatedbyUser = userService.GetCurrentUser();
	            logger.Info($"Создание Нового документа Списания");
            } else 
	            AutoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
            
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
        
        private string total;
        public string Total {
            get => total;
            set => SetField(ref total, value);
        }
        private bool delSensitive;
        public bool DelSensitive {
            get => CanEdit && delSensitive;
            set => SetField(ref delSensitive, value);
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
	        foreach (var node in e.GetSelectedObjects<StockBalanceJournalNode>())
		        Entity.AddItem(node.GetStockPosition(UoW), selectVM.Filter.Warehouse, 
					addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : node.Amount));
	        CalculateTotal();
        }

        public void AddFromEmployee() {
            var selectJournal = 
                NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel, EmployeeCard>(
                    this,
                    Employee,
                    OpenPageOptions.AsSlave);
            selectJournal.ViewModel.Filter.DateSensitive = false;
            selectJournal.ViewModel.Filter.CheckShowWriteoffVisible = false;
            selectJournal.ViewModel.Filter.SubdivisionSensitive =  Employee == null;
            selectJournal.ViewModel.Filter.EmployeeSensitive = Employee == null;
            selectJournal.ViewModel.Filter.Date = Entity.Date;
            selectJournal.ViewModel.Filter.CanChooseAmount = true;
            selectJournal.ViewModel.OnSelectResult += SelectFromEmployee_Selected;
        }
        private void SelectFromEmployee_Selected(object sender, JournalSelectedEventArgs e)
        {
            var operations = UoW.GetById<EmployeeIssueOperation>(e.GetSelectedObjects<EmployeeBalanceJournalNode>().Select(x => x.Id));
            var addedAmount = ((EmployeeBalanceJournalViewModel)sender).Filter.AddAmount;
            var balance = e.GetSelectedObjects<EmployeeBalanceJournalNode>().ToDictionary(k => k.Id, v => v.Balance);

            foreach (var operation in operations)
	            Entity.AddItem(operation, addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : balance[operation.Id]));
            
            CalculateTotal();
        }

        public void AddFromDutyNorm() 
        {
	        var selectJournal = NavigationManager.OpenViewModel<DutyNormBalanceJournalViewModel, DutyNorm>(
		        this,
		        DutyNorm,
		        OpenPageOptions.AsSlave);
	        selectJournal.ViewModel.Filter.DateSensitive = false;
	        selectJournal.ViewModel.Filter.SubdivisionSensitive =  DutyNorm == null;
	        selectJournal.ViewModel.Filter.DutyNormSensitive = DutyNorm == null;
	        selectJournal.ViewModel.Filter.Date = Entity.Date;
	        selectJournal.ViewModel.OnSelectResult += SelectFromDutyNorm_Selected;
        }

        private void SelectFromDutyNorm_Selected(object sender, JournalSelectedEventArgs e) 
        {
	        var operations=UoW.GetById<DutyNormIssueOperation>(e.GetSelectedObjects<DutyNormBalanceJournalNode>().Select(x => x.Id));
	        var balance = e.GetSelectedObjects<DutyNormBalanceJournalNode>().ToDictionary(k => k.Id, v => v.Balance);
	        foreach(var operation in operations)
		        Entity.AddItem(operation, balance[operation.Id]);
	        CalculateTotal();
        }
        
        public void FillMaxAmount(DateTime? date = null) {
	        var itemsWh = Entity.Items.Where(i => i.WriteoffFrom == WriteoffFrom.Warehouse).ToList();
	        if(itemsWh.Any()) {
		        // для всех списаний со склада
		        var nomenclatures = itemsWh.Select(i => i.Nomenclature);
		        stockBalanceModel.OnDate = date;
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
