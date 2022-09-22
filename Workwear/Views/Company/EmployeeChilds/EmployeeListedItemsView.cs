using System;
using workwear.ViewModels.Company.EmployeeChilds;

namespace workwear.Views.Company.EmployeeChilds
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeListedItemsView : Gtk.Bin
	{
		private EmployeeListedItemsViewModel viewModel;

		public EmployeeListedItemsView()
		{
			this.Build();
		}

		public EmployeeListedItemsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				viewModel.PropertyChanged += ViewModel_PropertyChanged;
				buttonReturnWear.Sensitive = ViewModel.SensetiveButtonReturn;
				buttonWriteOffWear.Sensitive = ViewModel.SensetiveButtonWriteoff;
				buttonGiveWear.Sensitive = ViewModel.SensetiveButtonGiveWear;
			}
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(viewModel.EmployeeBalanceVM))
				treeviewListedItems.RepresentationModel = viewModel.EmployeeBalanceVM;
		}

		protected void OnButtonGiveWearClicked(object sender, EventArgs e) {
			buttonGiveWear.Sensitive = false;
			viewModel.GiveWear();
			buttonGiveWear.Sensitive = ViewModel.SensetiveButtonGiveWear;
		}

		protected void OnButtonReturnWearClicked(object sender, EventArgs e) {
			buttonReturnWear.Sensitive = false;
			ViewModel.ReturnWear();
			buttonReturnWear.Sensitive = ViewModel.SensetiveButtonReturn;
		}

		protected void OnButtonWriteOffWearClicked(object sender, EventArgs e) {
			buttonWriteOffWear.Sensitive = false;
			ViewModel.WriteOffWear();
			buttonWriteOffWear.Sensitive = ViewModel.SensetiveButtonWriteoff;
		}
	}
}
