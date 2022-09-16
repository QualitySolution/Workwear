﻿using System;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace workwear.Views.Company.EmployeeChildren
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

		protected void OnButtonGiveWearClicked(object sender, EventArgs e)
		{
			viewModel.GiveWear();
		}

		protected void OnButtonReturnWearClicked(object sender, EventArgs e)
		{
			ViewModel.ReturnWear();
		}

		protected void OnButtonWriteOffWearClicked(object sender, EventArgs e)
		{
			ViewModel.WriteOffWear();
		}
	}
}
