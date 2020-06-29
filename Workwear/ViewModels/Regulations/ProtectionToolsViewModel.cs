using System;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;

namespace workwear.ViewModels.Regulations
{
	public class ProtectionToolsViewModel : EntityDialogViewModelBase<ProtectionTools>
	{
		public ProtectionToolsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
		}

		#region Действия View

		public void AddAnalog()
		{
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Analog_OnSelectResult;
		}

		void Analog_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var toolsNode in e.SelectedObjects) {
				var tools = UoW.GetById<ProtectionTools>(toolsNode.GetId());
				Entity.AddAnalog(tools);
			}
		}

		public void RemoveAnalog(ProtectionTools[] tools)
		{
			foreach(var item in tools) {
				Entity.RemoveAnalog(item);
			}
		}

		#endregion
	}
}
