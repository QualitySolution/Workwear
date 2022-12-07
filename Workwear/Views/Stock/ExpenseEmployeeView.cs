using System;
using Gamma.Binding.Converters;
using NLog;
using QS.Views.Dialog;
using workwear.Domain.Statements;
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

			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();

			ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			ycomboOperation.ItemsEnum = typeof(ExpenseOperations);
			ycomboOperation.SelectedItem = ExpenseOperations.Employee;
			ycomboOperation.Sensitive = false;

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			entityWarehouseExpense.ViewModel = ViewModel.WarehouseEntryViewModel;
			yentryEmployee.ViewModel = ViewModel.EmployeeCardEntryViewModel;

			enumPrint.ItemsEnum = typeof(IssuedSheetPrint);

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
			buttonIssuanceSheetOpen.Visible = enumPrint.Visible = Entity.IssuanceSheet != null;
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

		protected void OnEnumPrintEnumItemClicked(object sender, QSOrmProject.EnumItemClickedEventArgs e)
		{
			ViewModel.PrintIssuenceSheet((IssuedSheetPrint)e.ItemEnum);
		}
	}
}
