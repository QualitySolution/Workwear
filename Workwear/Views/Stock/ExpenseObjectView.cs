using Gamma.Binding.Converters;
using QS.Views.Dialog;
using Workwear.Domain.Stock.Documents;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class ExpenseObjectView : EntityDialogViewBase<ExpenseObjectViewModel, Expense>
	{
		public ExpenseObjectView(ExpenseObjectViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			expensedocitemobjectview1.ViewModel = ViewModel.DocItemsObjectViewModel;

			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();

			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser.Name, w => w.LabelProp).InitializeFromSource();

			ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			ycomboOperation.ItemsEnum = typeof(ExpenseOperations);
			ycomboOperation.SelectedItem = ExpenseOperations.Object;
			ycomboOperation.Sensitive = false;

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			entityWarehouseExpense.ViewModel = ViewModel.WarehouseExpenceViewModel;
			entitySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

		}
	}
}
