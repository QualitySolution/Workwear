using System;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Visits;
using Workwear.ViewModels.Visits;

namespace Workwear.Views.Visits {
	public partial class IssuanceRequestView : EntityDialogViewBase<IssuanceRequestViewModel, IssuanceRequest> {
		public IssuanceRequestView(IssuanceRequestViewModel viewModel): base(viewModel) {
			this.Build();
			ConfigureMainInfo();
			ConfigureEmployeesList();
			MakeMenu();
			ytreeviewEmployees.Selection.Changed += Employee_Selection_Changed;
			CommonButtonSubscription();
		}

		#region Вкладка Основное

		private void ConfigureMainInfo() {
			ylabelId.Binding.AddBinding(ViewModel, vm => vm.Id, w => w.LabelProp).InitializeFromSource();
			ylabelUser.Binding.AddFuncBinding(ViewModel, vm => vm.CreatedByUser != null ? vm.CreatedByUser.Name : null, w => w.LabelProp).InitializeFromSource();
			receiptDate.Binding.AddBinding(ViewModel, vm => vm.ReceiptDate, w => w.DateOrNull).InitializeFromSource();
			enumStatus.ItemsEnum = typeof(IssuanceRequestStatus);
			enumStatus.Binding.AddBinding(ViewModel, vm => vm.Status, w => w.SelectedItem).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.Comment, w => w.Buffer.Text).InitializeFromSource();
		}

		#endregion

		#region Вкладка Сотрудники

		private void MakeMenu() {
			var addMenu = new Menu();
			var item = new yMenuItem("Сотрудников");
			item.Activated += (sender, e) => ViewModel.AddEmployees();
			addMenu.Add(item);
			item = new yMenuItem("Подразделения");
			item.Activated += (sender, e) => ViewModel.AddSubdivisions();
			addMenu.Add(item);
			item = new yMenuItem("Отделы");
			item.Activated += (sender, e) => ViewModel.AddDepartments();
			addMenu.Add(item);
			item = new yMenuItem("Группы");
			item.Activated += (sender, e) => ViewModel.AddGroups();
			addMenu.Add(item);
			buttonAdd.Menu = addMenu;
			addMenu.ShowAll();
		}

		private void Employee_Selection_Changed(object sender, EventArgs e) {
			buttonRemove.Sensitive = ytreeviewEmployees.Selection.CountSelectedRows() > 0;
		}
		
		private void ConfigureEmployeesList() {
			ytreeviewEmployees.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Employees, w => w.ItemsDataSource)
				.InitializeFromSource();
			
			ytreeviewEmployees.ColumnsConfig = FluentColumnsConfig<EmployeeCard>.Create()
				.AddColumn("Табель").AddTextRenderer(e => e.PersonnelNumber)
				.AddColumn("ФИО").Resizable().AddTextRenderer(e => e.FullName)
				.AddColumn("Должность").AddReadOnlyTextRenderer(e => e.Post?.Name)
				.AddColumn("Подразделение").AddReadOnlyTextRenderer(e => e.Subdivision?.Name)
				.AddColumn("Отдел").AddReadOnlyTextRenderer(e => e.Department?.Name)
				.Finish();
		}
		#endregion
		protected void OnButtonRemoveItemClicked(object sender, System.EventArgs e) {
			ViewModel.RemoveEmployee(ytreeviewEmployees.GetSelectedObject<EmployeeCard>());
		}

		
	}
}
