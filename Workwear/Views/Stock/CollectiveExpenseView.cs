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
			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocNumber, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource();
			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			entityWarehouseExpense.ViewModel = ViewModel.WarehouseEntryViewModel;
			entityTransferAgent.ViewModel = ViewModel.TransferAgentEntryViewModel;
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
