using System;
using NLog;
using QS.Views.Dialog;
using QS.Widgets;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock
{
	public partial class ExpenseEmployeeView : EntityDialogViewBase<ExpenseEmployeeViewModel, Expense>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ExpenseEmployeeView(ExpenseEmployeeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			expensedocitememployeeview1.ViewModel = ViewModel.DocItemsEmployeeViewModel;
			
			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocNumberText, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding
				.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource(); 
			
			ylabelCreatedBy.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			ydateDoc.Binding
				.AddBinding(ViewModel, vm => vm.DocumentDate, w => w.Date)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel,vm => vm.CanChangeDocDate, w => w.IsEditable)
				.InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ydateIssueDate.Binding
				.AddBinding(Entity, e => e.IssueDate, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditIssueDate, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonSetIssue.Binding
				.AddBinding(ViewModel, vm => vm.CanEditIssueDate, w => w.Sensitive).InitializeFromSource();
			entityWarehouseExpense.ViewModel = ViewModel.WarehouseEntryViewModel;
			entityWarehouseExpense.Binding
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			yentryEmployee.ViewModel = ViewModel.EmployeeCardEntryViewModel;
			yentryEmployee.Binding
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			enumPrint.ItemsEnum = typeof(IssuedSheetPrint);
			enumPrint.Binding
				.AddBinding(ViewModel, v => v.IssuanceSheetPrintVisible, w => w.Visible).InitializeFromSource();
			buttonIssuanceSheetOpen.Binding
				.AddBinding(ViewModel, v => v.IssuanceSheetOpenVisible, w => w.Visible).InitializeFromSource();
			buttonIssuanceSheetCreate.Binding.AddSource(ViewModel)
				.AddBinding(v => v.IssuanceSheetCreateVisible, w => w.Visible)
				.AddBinding(v => v.CanCreateIssuanceSheet, w => w.Sensitive)
				.InitializeFromSource();
		}

		protected void OnButtonIssuanceSheetCreateClicked(object sender, EventArgs e) => ViewModel.CreateIssuanceSheet();
		protected void OnButtonIssuanceSheetOpenClicked(object sender, EventArgs e) => ViewModel.OpenIssuanceSheet();
		protected void OnEnumPrintEnumItemClicked(object sender, EnumItemClickedEventArgs e) => ViewModel.PrintIssuanceSheet((IssuedSheetPrint)e.ItemEnum);
		protected void OnYbuttonSetIssueClicked(object sender, EventArgs e) => ViewModel.SetIssue();
	}
}
