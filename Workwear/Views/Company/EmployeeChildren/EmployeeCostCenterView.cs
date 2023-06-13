using System;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeCostCenterView : Gtk.Bin {
		public EmployeeCostCenterView() {
			this.Build();
		}

		private EmployeeCostCenterViewModel viewModel;

		public EmployeeCostCenterViewModel ViewModel {
			get => viewModel;
			set => viewModel = value;
		}
	}
}
