using System;
using Gamma.Binding.Converters;
using NLog;
using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
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
			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();

			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser.Name, w => w.LabelProp).InitializeFromSource();

			ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			ycomboOperation.ItemsEnum = typeof(ExpenseOperations);
			ycomboOperation.SelectedItem = ExpenseOperations.Employee;
			ycomboOperation.Sensitive = false;

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			entityWarehouseExpense.ViewModel = ViewModel.WarehouseEntryViewModel;
			yentryEmployee.ViewModel = ViewModel.EmployeeCardEntryViewModel;

			Entity.PropertyChanged += Entity_PropertyChanged;
			IssuanceSheetSensetive();
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Employee))
				IssuanceSheetSensetive();
		}

		private void IssuanceSheetSensetive()
		{
			buttonIssuanceSheetCreate.Sensitive = Entity.Employee != null;
			buttonIssuanceSheetCreate.Visible = Entity.IssuanceSheet == null;
			buttonIssuanceSheetOpen.Visible = buttonIssuanceSheetPrint.Visible = Entity.IssuanceSheet != null;
		}
		protected void OnButtonIssuanceSheetCreateClicked(object sender, EventArgs e)
		{
			ViewModel.CreateIssuenceSheet();
			IssuanceSheetSensetive();
		}
		protected void OnButtonIssuanceSheetOpenClicked(object sender, EventArgs e)
		{
			ViewModel.OpenIssuenceSheet();
		}
		protected void OnButtonIssuanceSheetPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintIssuenceSheet();
		}
	}
}
