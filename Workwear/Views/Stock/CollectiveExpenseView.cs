using System;
using Gamma.Binding.Converters;
using NLog;
using QS.Views.Dialog;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock
{
	public partial class CollectiveExpenseView : EntityDialogViewBase<CollectiveExpenseViewModel, CollectiveExpense>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public CollectiveExpenseView(CollectiveExpenseViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			expensedocitememployeeview1.ViewModel = ViewModel.CollectiveExpenseItemsViewModel;
			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();

			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser.Name, w => w.LabelProp).InitializeFromSource();

			ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			entityWarehouseExpense.ViewModel = ViewModel.WarehouseEntryViewModel;

			enumPrint.ItemsEnum = typeof(IssuedSheetPrint);

			IssuanceSheetSensetive();
		}

		private void IssuanceSheetSensetive()
		{
			buttonIssuanceSheetCreate.Visible = Entity.IssuanceSheet == null;
			buttonIssuanceSheetOpen.Visible = enumPrint.Visible = Entity.IssuanceSheet != null;
		}

		protected void OnButtonIssuanceSheetCreateClicked(object sender, EventArgs e)
		{
			ViewModel.CreateIssuanceSheet();
			IssuanceSheetSensetive();
		}

		protected void OnButtonIssuanceSheetOpenClicked(object sender, EventArgs e)
		{
			ViewModel.OpenIssuanceSheet();
		}

		protected void OnEnumPrintEnumItemClicked(object sender, QSOrmProject.EnumItemClickedEventArgs e)
		{
			ViewModel.PrintIssuanceSheet((IssuedSheetPrint)e.ItemEnum);
		}
	}
}
