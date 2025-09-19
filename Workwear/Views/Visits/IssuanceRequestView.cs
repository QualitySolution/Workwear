using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.ViewModels.Visits;

namespace Workwear.Views.Visits {
	public partial class IssuanceRequestView : EntityDialogViewBase<IssuanceRequestViewModel, IssuanceRequest> {
		public IssuanceRequestView(IssuanceRequestViewModel viewModel): base(viewModel) {
			this.Build();
			MakeMenu();
			ConfigureMainInfo();
			ConfigureEmployeesList();
			ConfigureCollectiveExpenseList();
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
			
			ytreeviewEmployees.Selection.Mode = SelectionMode.Multiple;
			ytreeviewEmployees.ButtonReleaseEvent += TreeItems_ButtonReleaseEvent;
			ytreeviewEmployees.Selection.Changed += Employee_Selection_Changed;
		}
		
		#region PopupMenu

		private void TreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeviewEmployees.GetSelectedObjects<EmployeeCard>().FirstOrDefault();
				if(selected == null) return;
				var menuItem = new MenuItem("Открыть сотрудника");
				menuItem.Activated += (sender, e) => ViewModel.OpenEmployee(selected);
				menu.Add(menuItem);
				menu.ShowAll();
				menu.Popup();
			}
		}

		#endregion
		protected void OnButtonRemoveItemClicked(object sender, EventArgs e) {
			ViewModel.RemoveEmployees(ytreeviewEmployees.GetSelectedObjects<EmployeeCard>());
		}
		#endregion

		#region Вкладка Выдачи

		private void ConfigureCollectiveExpenseList() {
			yspeccomboboxWarehouse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Warehouses, w => w.ItemsList)
				.AddBinding(vm => vm.SelectWarehouse, w => w.SelectedItem)
				.InitializeFromSource();
			ytreeviewExpense.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CollectiveExpenses, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeviewExpense.ColumnsConfig = FluentColumnsConfig<CollectiveExpense>.Create()
				.AddColumn("Номер").AddTextRenderer(c => c.DocNumber ?? c.Id.ToString())
				.AddColumn("Дата").AddTextRenderer(c => c.Date.ToShortDateString())
				.AddColumn("Ведомость").AddReadOnlyTextRenderer(c => c.IssuanceSheet?.DocNumber ?? c.IssuanceSheet?.Id.ToString())
				.AddColumn("Склад").AddTextRenderer(c => c.Warehouse.Name)
				.AddColumn("Автор").Resizable().AddReadOnlyTextRenderer(c => c.CreatedbyUser?.Name)
				.AddColumn("Дата создания").AddReadOnlyTextRenderer(c => c.CreationDate?.ToShortDateString())
				.AddColumn("Комментарий").Resizable().AddReadOnlyTextRenderer(c => c.Comment)
				.Finish();
			ytreeviewExpense.Selection.Mode = SelectionMode.Multiple;
			ytreeviewExpense.Selection.Changed += CollectiveExpense_Selection_Changed;
		}

		private void CollectiveExpense_Selection_Changed(object sender, EventArgs e) {
			buttonRemoveExpense.Sensitive = ytreeviewExpense.Selection.CountSelectedRows() > 0;
		}
		protected void OnButtonAddExpenseClicked(object sender, EventArgs e) {
			ViewModel.AddCollectiveExpense();
		}
		protected void OnButtonRemoveExpenseClicked(object sender, EventArgs e) {
			ViewModel.RemoveCollectiveExpense(ytreeviewExpense.GetSelectedObjects<CollectiveExpense>());
		}

		protected void OnButtonCreateExpenseClicked(object sender, EventArgs e) {
			ViewModel.CreateCollectiveExpense();
		}
		#endregion
		
	}
}
