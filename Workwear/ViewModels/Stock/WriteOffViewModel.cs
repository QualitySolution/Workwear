using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NLog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Models.Operations;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock
{
    public class WriteOffViewModel : EntityDialogViewModelBase<Writeoff>
    {
	    private readonly EmployeeIssueModel issueModel;
	    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public SizeService SizeService { get; }
        public EmployeeCard Employee { get;}
        public Subdivision Subdivision { get;}
        public Warehouse CurWarehouse { get; set; }
        public FeaturesService FeaturesService { get; }
        public IList<Owner> Owners { get; }

        public WriteOffViewModel(
            IEntityUoWBuilder uowBuilder, 
            IUnitOfWorkFactory unitOfWorkFactory,
            UnitOfWorkProvider unitOfWorkProvider,
            INavigationManager navigation,
            SizeService sizeService,
            FeaturesService featuresService,
            EmployeeIssueModel issueModel,
            EmployeeCard employee = null,
            Subdivision subdivision = null,
            IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
	        this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
	        FeaturesService = featuresService;
            SizeService = sizeService;
            NavigationManager = navigation;
            Entity.ObservableItems.PropertyChanged += CalculateTotal;
            CalculateTotal(null, null);
            if (employee != null)
                Employee = UoW.GetById<EmployeeCard>(employee.Id);
            else if (subdivision != null)
                Subdivision = UoW.GetById<Subdivision>(subdivision.Id);
            Owners = UoW.GetAll<Owner>().ToList();
        }
        
        #region ViewProperty
        private string total;
        public string Total {
            get => total;
            set => SetField(ref total, value);
        }
        private bool delSensitive;
        public bool DelSensitive {
            get => delSensitive;
            set => SetField(ref delSensitive, value);
        }
        private bool fillBuhDocSensitive;
        public bool FillBuhDocSensitive {
            get => fillBuhDocSensitive;
            set => SetField(ref fillBuhDocSensitive, value);
        }
        #endregion

        private void CalculateTotal(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            Total = $"Позиций в документе: {Entity.Items.Count}  " +
                    $"Количество единиц: {Entity.Items.Sum(x => x.Amount)}";
            FillBuhDocSensitive = Entity.Items.Count > 0;
        }

        #region Items
        public void AddFromStock() {
            var selectJournal = 
                NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
            if(CurWarehouse != null) {
                selectJournal.ViewModel.Filter.Warehouse = CurWarehouse;
            }
            selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
            selectJournal.ViewModel.Filter.CanChoiseAmount = true;
            selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
        }
        private void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
            var selectVM = sender as StockBalanceJournalViewModel;
            foreach (var node in e.GetSelectedObjects<StockBalanceJournalNode>())
                Entity.AddItem(node.GetStockPosition(UoW), selectVM.Filter.Warehouse, node.AddAmount);
            CalculateTotal(null, null);
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
            selectJournal.ViewModel.Filter.CanChoiseAmount = true;
            selectJournal.ViewModel.OnSelectResult += SelectFromEmployee_Selected;
        }
        private void SelectFromEmployee_Selected(object sender, JournalSelectedEventArgs e)
        {
            var operations = 
                UoW.GetById<EmployeeIssueOperation>(e.GetSelectedObjects<EmployeeBalanceJournalNode>()
                .Select(x => x.Id));
            foreach (var operation in operations) 
                Entity.AddItem(operation, e.GetSelectedObjects<EmployeeBalanceJournalNode>().First(x => x.Id == operation.Id).AddAmount);
            CalculateTotal(null, null);
        }

        public void AddFromObject() {
            var selectJournal =
                NavigationManager.OpenViewModel<SubdivisionBalanceJournalViewModel, Subdivision>(
                    this,
                    Subdivision,
                    OpenPageOptions.AsSlave);
            selectJournal.ViewModel.Filter.DateSensitive = false;
            selectJournal.ViewModel.Filter.SubdivisionSensitive = Subdivision == null;
            selectJournal.ViewModel.Filter.Date = Entity.Date;
            selectJournal.ViewModel.Filter.CanChoiseAmount = true;
            selectJournal.ViewModel.OnSelectResult += SelectFromobject_ObjectSelected;
        }
        private void SelectFromobject_ObjectSelected(object sender, JournalSelectedEventArgs e) {
            var operations = 
                UoW.GetById<SubdivisionIssueOperation>(e.GetSelectedObjects<SubdivisionBalanceJournalNode>()
                    .Select(x => x.Id));
            foreach (var operation in operations) 
	            Entity.AddItem(operation, e.GetSelectedObjects<SubdivisionBalanceJournalNode>().First(x => x.Id == operation.Id).AddAmount);
            CalculateTotal(null, null);
        }
        public void DeleteItem(WriteoffItem item) {
            Entity.RemoveItem(item);
            CalculateTotal(null, null);
        }
        #endregion
        public void FillBuhDoc() {
            var docText = String.Empty;
            if (Entity.Items.Any(x => x.WriteoffFrom == WriteoffFrom.Employee)) 
                docText = Entity.Items.First(x => x.WriteoffFrom == WriteoffFrom.Employee).BuhDocument;
            var dlg = NavigationManager.OpenViewModel<BuhDocumentViewModel, String>(this, docText);
            dlg.ViewModel.SetChangeAndCloseEvent += SetBuhDoc;
        }
        private void SetBuhDoc(object sender, BuhDocEventArgs args) =>
            Entity.ObservableItems
                .Where(x => x.WriteoffFrom == WriteoffFrom.Employee)
                .ToList().ForEach(x => x.BuhDocument = args.Value);

        #region Save
        public override bool Save() {
            logger.Info ("Запись документа...");
            
            Entity.UpdateOperations(UoW);
            if (Entity.Id == 0)
                Entity.CreationDate = DateTime.Now;

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
                issueModel.UpdateNextIssue(employeeOperations, changeLog: (operation, s) => logger.Debug(s));
                UoW.Commit();
            }
            logger.Info ("Ok");
            return true;
        }
        #endregion
    }
}
