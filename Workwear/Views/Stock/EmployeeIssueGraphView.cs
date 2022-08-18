﻿using System;
using QS.Views.Dialog;
using workwear.Domain.Operations.Graph;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class EmployeeIssueGraphView : DialogViewBase<EmployeeIssueGraphViewModel>
	{	
		public EmployeeIssueGraphView(EmployeeIssueGraphViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}
		private void ConfigureDlg()
		{
			ytreeviewGraph.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<GraphInterval>()
				.AddColumn("C даты").AddTextRenderer(node => node.StartDate.ToShortDateString())
				.AddColumn("Выдано").AddNumericRenderer(node => node.Issued)
				.AddColumn("Списано").AddNumericRenderer(node => node.WriteOff)
				.AddColumn("Числится").AddNumericRenderer(node => node.CurrentCount)
				.AddColumn("Корректировка").AddTextRenderer(x => x.Reset ? "Да" : String.Empty)
				.Finish();
			
			ytreeviewGraph.Binding
				.AddBinding(ViewModel, vm => vm.Intervals, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
	}
}