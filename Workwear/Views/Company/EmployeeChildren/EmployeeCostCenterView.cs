using System;
using Gtk;
using QSOrmProject;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeCostCenterView : Gtk.Bin {
		public EmployeeCostCenterView() {
			Build();
		}

		private EmployeeCostCentersViewModel viewModel;
		public EmployeeCostCentersViewModel ViewModel {
			get => viewModel; 
			set {
				viewModel = value;
				CreateTable();
			}
		}

		private void CreateTable()
		{
			ytreeCostCenter.CreateFluentColumnsConfig<EmployeeCostCenter>()
				.AddColumn("Место возникновения затрат").AddTextRenderer(e => e.CostCenter.Name)
				.AddColumn("Доля затрат").AddNumericRenderer (e => e.Percent, new MultiplierToPercentConverter())
					.Editing (new Adjustment(0,0,100,1,10,0)).WidthChars(4).Digits(0)
					.AddTextRenderer (e => "%", expand:false)
				.AddColumn("") //Заглушка, чтобы не расширялось
				.Finish();
			ytreeCostCenter.Binding.AddBinding(ViewModel, v => v.ObservableCostCenters, w => w.ItemsDataSource);
		}
		
		protected void OnButtonAddClicked(object sender, EventArgs e) {
			viewModel.AddItem();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e) {
			viewModel.DeleteItem(ytreeCostCenter.GetSelectedObject<EmployeeCostCenter>());
		}
	}
}
