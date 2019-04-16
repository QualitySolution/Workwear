using System;
using QS.Dialog.Gtk;
using QSOrmProject;
using workwear.Domain.Organization;
using workwear.Representations.Organization;

namespace workwear.Dialogs.Organization
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeVacationsView : WidgetOnEntityDialogBase<EmployeeCard>
	{
		public EmployeeVacationsView()
		{
			this.Build();
		}

		public bool VacationsLoaded { get; private set; }

		public void UpdateList()
		{
			ConfigureIfNeed();
			treeviewVacations.RepresentationModel.UpdateNodes();
			VacationsLoaded = true;
		}

		private bool configured;
		private void ConfigureIfNeed()
		{
			if(configured)
				return;

			var vm = new EmployeeVacationsVM(EntityDialog.UoW) {
				Employee = RootEntity
			};
			treeviewVacations.RepresentationModel = vm;
			treeviewVacations.Selection.Changed += Vacation_Selection_Changed;
			configured = true;
		}

		void Vacation_Selection_Changed(object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive
				= treeviewVacations.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			if(UoW.IsNew) {
				if(CommonDialogs.SaveBeforeCreateSlaveEntity(typeof(EmployeeCard), typeof(EmployeeVacation))) {
					MyOrmDialog.UoW.Save();
				}
				else
					return;
			}
			OpenNewTab(new EmployeeVacationDlg(RootEntity));
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			OpenTab<EmployeeVacationDlg, int>(treeviewVacations.GetSelectedId());
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			QS.Deletion.DeleteHelper.DeleteEntity(typeof(EmployeeVacation), treeviewVacations.GetSelectedId());
		}
	}
}
