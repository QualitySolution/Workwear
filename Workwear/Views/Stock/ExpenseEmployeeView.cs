﻿using System;
using Gamma.Binding.Converters;
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
				.AddBinding(vm => vm.DocNumber, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource(); 

			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();

			ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			entityWarehouseExpense.ViewModel = ViewModel.WarehouseEntryViewModel;
			yentryEmployee.ViewModel = ViewModel.EmployeeCardEntryViewModel;

			enumPrint.ItemsEnum = typeof(IssuedSheetPrint);
			enumPrint.Binding.AddBinding(ViewModel, v => v.IssuanceSheetPrintVisible, w => w.Visible).InitializeFromSource();
			buttonIssuanceSheetOpen.Binding.AddBinding(ViewModel, v => v.IssuanceSheetOpenVisible, w => w.Visible).InitializeFromSource();
			buttonIssuanceSheetCreate.Binding.AddSource(ViewModel)
				.AddBinding(v => v.IssuanceSheetCreateVisible, w => w.Visible)
				.AddBinding(v => v.IssuanceSheetCreateSensitive, w => w.Sensitive)
				.InitializeFromSource();
		}

		protected void OnButtonIssuanceSheetCreateClicked(object sender, EventArgs e)
		{
			ViewModel.CreateIssuanceSheet();
		}
		protected void OnButtonIssuanceSheetOpenClicked(object sender, EventArgs e)
		{
			ViewModel.OpenIssuanceSheet();
		}

		protected void OnEnumPrintEnumItemClicked(object sender, EnumItemClickedEventArgs e)
		{
			ViewModel.PrintIssuanceSheet((IssuedSheetPrint)e.ItemEnum);
		}
	}
}
