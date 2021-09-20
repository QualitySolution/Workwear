using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Company;
using workwear.Domain.Regulations;

namespace workwear.Dialogs.Issuance
{
	public partial class EmployeeIssueGraphDlg : SingleUowTabBase
	{
		EmployeeCard Employee;
		ProtectionTools ProtectionTools;

		public EmployeeIssueGraphDlg(EmployeeCard employee, ProtectionTools protectionTools)
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Employee = employee;
			ProtectionTools = protectionTools;
			ConfigureDlg();
			Fill(employee, protectionTools);
		}

		public override string TabName { get => $"Хронология {Employee.ShortName} - {ProtectionTools.Name}"; }

		private void ConfigureDlg()
		{
			ytreeviewGraph.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<GraphInterval>()
				.AddColumn("C даты").AddTextRenderer(node => node.StartDate.ToShortDateString())
				.AddColumn("Выдано").AddNumericRenderer(node => node.Issued)
				.AddColumn("Списано").AddNumericRenderer(node => node.WriteOff)
				.AddColumn("Числится").AddNumericRenderer(node => node.CurrentCount)
				.AddColumn("Корректировка").AddTextRenderer(x => x.Reset ? "Да" : String.Empty)
				.Finish();
			ShowAll();
		}

		public void Fill(EmployeeCard employee, ProtectionTools itemsType)
		{
			var graph = IssueGraph.MakeIssueGraph(UoW, employee, itemsType);

			ytreeviewGraph.ItemsDataSource = graph.Intervals;
		}
	}
}
