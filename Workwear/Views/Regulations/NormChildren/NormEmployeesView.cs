using System;
using Gamma.ColumnConfig;
using QS.Views;
using Workwear.ViewModels.Regulations.NormChildren;

namespace Workwear.Views.Regulations.NormChildren {
	public partial class NormEmployeesView : ViewBase<NormEmployeesViewModel> {
		public NormEmployeesView(NormEmployeesViewModel viewModel) : base(viewModel) {
			this.Build();

			tvEmployees.ColumnsConfig = FluentColumnsConfig<EmployeeNode>.Create()
				.AddColumn("Номер").AddReadOnlyTextRenderer(node => node.CardNumberText)
				.AddColumn("Табельный №").AddReadOnlyTextRenderer(node => node.PersonnelNumber)
				.AddColumn("Ф.И.О.").AddReadOnlyTextRenderer(node => node.FIO)
				.AddColumn("Должность").AddReadOnlyTextRenderer(node => node.Post)
				.AddColumn("Подразделение").AddReadOnlyTextRenderer(node => node.Subdivision)
				.AddColumn("Отдел").AddReadOnlyTextRenderer(node => node.Department)
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Dismiss ? "grey" : null)
				.Finish();
			tvEmployees.Selection.Mode = Gtk.SelectionMode.Multiple;
			tvEmployees.Binding.AddBinding(ViewModel, vm => vm.Employees, w => w.ItemsDataSource).InitializeFromSource();
			tvEmployees.Selection.Changed += (sender, e) => buttonRemove.Sensitive = tvEmployees.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e) {
			ViewModel.Add();
		}

		protected void OnButtonRemoveClicked(object sender, EventArgs e) {
			ViewModel.Remove(tvEmployees.GetSelectedObjects<EmployeeNode>());
		}
	}
}
