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
				Entity.AddItem(protectionTools);
			}
		}

		public void RemoveItem(DutyNormItem item) {
			Entity.Items.Remove(item);
		}
		#endregion
	}
}
