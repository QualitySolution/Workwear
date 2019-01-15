using QS.Dialog.Gtk;
using workwear.Domain.Organization;
using workwear.Representations;

namespace workwear.Dialogs.Organization
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeCardListedItemsView : WidgetOnEntityDialogBase<EmployeeCard>
	{
		public EmployeeCardListedItemsView()
		{
			this.Build();
		}

		public void UpdateList()
		{
			ConfigureIfNeed();
			treeviewListedItems.RepresentationModel.UpdateNodes();
		}

		private bool configured;
		private void ConfigureIfNeed()
		{
			if(configured)
				return;

			var vm = new EmployeeBalanceVM(EntityDialog.UoW);
			vm.Employee = RootEntity;
			treeviewListedItems.RepresentationModel = vm;
			configured = true;
		}
	}
}
