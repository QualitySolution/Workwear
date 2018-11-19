using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;

namespace workwear.Dialogs.Issuance
{
	public partial class EmployeeIssueGraphDlg : SingleUowTabBase
	{
		EmployeeCard Employee;
		ItemsType ItemsType;

		public EmployeeIssueGraphDlg(EmployeeCard employee, ItemsType itemsType)
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Employee = employee;
			ItemsType = itemsType;
			ConfigureDlg();
			Fill(employee, itemsType);
		}

		public override string TabName { get => $"Хронология {Employee.ShortName} - {ItemsType.Name}"; }

		private void ConfigureDlg()
		{
			ytreeviewGraph.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<GraphInterval>()
				.AddColumn("C даты").AddTextRenderer(node => node.StartDate.ToShortDateString())
				.AddColumn("Выдано").AddNumericRenderer(node => node.Issued)
				.AddColumn("Списано").AddNumericRenderer(node => node.WriteOff)
				.AddColumn("Числится количество").AddNumericRenderer(node => node.CurrentCount)
				.Finish();
			ShowAll();
		}

		public void Fill(EmployeeCard employee, ItemsType itemsType)
		{
			var graph = IssueGraph.MakeIssueGraph(UoW, employee, itemsType);

			ytreeviewGraph.ItemsDataSource = graph.Intervals;
		}
	}
}
