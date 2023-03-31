using System;
using System.Collections;
using System.Linq;
using NHibernate.Criterion;
using NLog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;

namespace Workwear.ViewModels.Stock {
	public class InspectionViewModel : EntityDialogViewModelBase<Inspection> {
		public InspectionViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, 
			IUserService userService,
			IValidator validator = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator) {
			if(UoW.IsNew)
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);

			DelSensitive = true;
			AddEmployeeSensitive = true;
		}
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		public bool DelSensitive { get; set; }
		public bool AddEmployeeSensitive { get; set; }
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}

		public void DeleteItem(InspectionItem item) {
			Entity.RemoveItem(item);
			//CalculateTotal();
		}

		public void AddItems() {
			
			var selectJournal = 
				NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel>(
					this,
					OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.DateSensitive = false;
			selectJournal.ViewModel.Filter.Date = Entity.Date;
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

			foreach(var item in Entity.Items) 
				item.OperationIssue.FixedOperation = true;

			if(!base.Save()) {
				logger.Info("Не Ок.");
				return false;
			}

			logger.Info ("Ok");
			return true;
		}
		
		private void CalculateTotal() {
			Total = "";
			throw new System.NotImplementedException();
		}
	}
}
