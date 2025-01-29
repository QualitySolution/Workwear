using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Regulations {
	public class DutyNormViewModel : EntityDialogViewModelBase<DutyNorm>{
		
		public DutyNormViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null) 
			: base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider){

			currentTab = Entity.Id == 0 ? 0 : 1;
//711			
//Пока обновление при открытии нормы			
			Entity.UpdateNextIssues(UoW);
		}

		#region Свойства

		public List<RegulationDoc> RegulationDocs { get; set; }
		
		private DutyNormItem selectedItem;
		public virtual DutyNormItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}

		private int currentTab;
		public virtual int CurrentTab {
			get => currentTab;
			set {
				SetField(ref currentTab, value);
			}
		}

		 public virtual IList<DutyNormIssueOperation> Operations => UoW.Session.QueryOver<DutyNormIssueOperation>()
			 .Where(o => o.DutyNorm.Id == Entity.Id).List();

		 #endregion
		
		#region Действия View
		public void AddItem() {
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Protection_OnSelectResult;
		}

		void Protection_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			foreach(var protectionNode in e.SelectedObjects) {
				var protectionTools = UoW.GetById<ProtectionTools>(protectionNode.GetId());
				var item = Entity.AddItem(protectionTools);
				item.UpdateNextIssue(UoW);
			}
		}

		public void RemoveItem(DutyNormItem item) {
			Entity.Items.Remove(item); 
		}

		public void AddExpense() {
			if(!Save())
				return;

			var vm = NavigationManager.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder, DutyNorm>(this, EntityUoWBuilder.ForCreate(), Entity);
		}
		
		public void OpenProtectionTools(DutyNormItem dutyNormItem) {
			NavigationManager.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(dutyNormItem.ProtectionTools.Id));
		}
		#endregion
//711		
		/*public override bool Save() {
			if (SelectedStartMonth != null)
				Entity.IssuanceStart = new DateTime(2001, SelectedStartMonth.Value.Month, StartDay ?? 1);
			else
				Entity.IssuanceStart = null;
			if (SelectedEndMonth != null)
				Entity.IssuanceEnd = new DateTime(2001, SelectedEndMonth.Value.Month, EndDay ?? 1);
			else
				Entity.IssuanceEnd = null;

			if (!Validate()) return false;
			UoW.Save(Entity); return true;
		}*/
	}
}
