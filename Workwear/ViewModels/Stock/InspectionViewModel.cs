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
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Stock {
	public class InspectionViewModel : EntityDialogViewModelBase<Inspection> {
		public InspectionViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, 
			IUserService userService,
			IInteractiveService interactive,
			ILifetimeScope autofacScope,
			EmployeeCard employee = null, 
			IValidator validator = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator) {
			this.interactive = interactive;
			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			if (employee != null)
				Employee = UoW.GetById<EmployeeCard>(employee.Id);
			var entryBuilder = new CommonEEVMBuilderFactory<Inspection>(this, Entity, UoW, navigation) {
				AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope))
			};
			ResponsibleDirectorPersonEntryViewModel = entryBuilder.ForProperty(x => x.Director)
				.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				.UseViewModelDialog<LeadersViewModel>()
				.Finish();
			ResponsibleChairmanPersonEntryViewModel = entryBuilder.ForProperty(x => x.Chairman)
				.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				.UseViewModelDialog<LeadersViewModel>()
				.Finish();
		}
		
		private IInteractiveService interactive;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public EntityEntryViewModel<Leader> ResponsibleDirectorPersonEntryViewModel { get; set; }
		public EntityEntryViewModel<Leader> ResponsibleChairmanPersonEntryViewModel { get; set; }
		public EmployeeCard Employee { get;}
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}

		public void DeleteMember(InspectionMember member) {
			Entity.RemoveMember(member);
		}
		public void AddMembers()
		{
			var selectPage = NavigationManager.OpenViewModel<LeadersJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += MemberOnSelectResult;
		}
		void MemberOnSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var members = UoW.GetById<Leader>(e.SelectedObjects.Select(x => x.GetId()));
			foreach(var member in members) {
				Entity.AddMember(member);
			}
		}

		public void DeleteItem(InspectionItem item) {
			Entity.RemoveItem(item);
			//CalculateTotal();
		}
		
		public void AddItems() {
			
			var selectJournal = 
				NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel, EmployeeCard>(
					this,
					Employee,
					OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.DateSensitive = false;
			selectJournal.ViewModel.Filter.Date = Entity.Date;
			selectJournal.ViewModel.Filter.EmployeeSensitive = Employee == null;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadItems;
		}
		
		private void LoadItems(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var selectedNodes = e.GetSelectedObjects<EmployeeBalanceJournalNode>();
			var operations = 
				UoW.GetById<EmployeeIssueOperation>(selectedNodes.Select(x => x.Id));
			foreach (var node in selectedNodes) 
				Entity.AddItem(operations.FirstOrDefault(o => o.Id == node.Id), node.Percentage);
			//CalculateTotal(null, null);
		}
		
		public override bool Save() {
			logger.Info ("Запись документа...");

			foreach(var item in Entity.Items) {
				item.NewOperationIssue.FixedOperation = true;
				item.NewOperationIssue.StartOfUse = Entity.Date;
				item.OperationIssue.EmployeeOperationIssueOnWriteOff = item.NewOperationIssue;
				if(item.WriteOffDateAfter != null)
					item.NewOperationIssue.UseAutoWriteoff = true;
				else 
					item.NewOperationIssue.UseAutoWriteoff = false;
				UoW.Save(item.NewOperationIssue);
			}

			if(!base.Save()) {
				logger.Info("Не Ок.");
				return false;
			}

			logger.Info ("Ok");
			return true;
		}
		
		public void OpenEmployee(InspectionItem item) {
			NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(item.Employee.Id));
		}
		
		private void CalculateTotal() {
			Total = "";
			throw new System.NotImplementedException();
		}
		
		public void Print() {
			if(UoW.HasChanges && !interactive.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = String.Format("Документ переоценки №{0}", Entity.Id),
				Identifier = "InspectionSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
	}
}
