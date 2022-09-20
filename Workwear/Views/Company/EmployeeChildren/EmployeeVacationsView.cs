using System;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeVacationsView : Gtk.Bin
	{
		private EmployeeVacationsViewModel viewModel;

		public EmployeeVacationsView()
		{
			this.Build();
			treeviewVacations.Selection.Changed += Vacation_Selection_Changed;
			treeviewVacations.RowActivated += TreeviewVacations_RowActivated;
		}

		public EmployeeVacationsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				viewModel.PropertyChanged += ViewModel_PropertyChanged;
			}
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.EmployeeVacationsVM)) {
				treeviewVacations.RepresentationModel = ViewModel.EmployeeVacationsVM;
			}
		}

		void Vacation_Selection_Changed(object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive
				= treeviewVacations.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			viewModel.AddItem();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			viewModel.EditItem(treeviewVacations.GetSelectedId());
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			viewModel.DeleteItem(treeviewVacations.GetSelectedId());
		}

		void TreeviewVacations_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			viewModel.DoubleClick(treeviewVacations.GetSelectedId());
		}

	}
}
