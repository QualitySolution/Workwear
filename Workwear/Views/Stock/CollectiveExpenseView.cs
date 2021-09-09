using System;
using System.Linq;
using Gamma.Binding.Converters;
using NLog;
using QS.Views.Dialog;
using workwear.Domain.Statements;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
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
			buttonIssuanceSheetCreate.Sensitive = Entity.Items.Any();
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
