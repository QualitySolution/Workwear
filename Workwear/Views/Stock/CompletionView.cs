using System;
using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;
using Gamma.GtkWidgets;
using Gtk;
using QSOrmProject;
using workwear.Measurements;
using Workwear.Measurements;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace workwear.Views.Stock
{
	public partial class CompletionView : EntityDialogViewBase<CompletionViewModel, Completion>
	{
		public CompletionView(CompletionViewModel viewModel): base(viewModel)
		{
			this.Build();
			ConfigureDlg ();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			buttonAddExpenseNomenclature.Clicked += AddSourceItems;
			buttonAddReceiptNomenclature.Clicked += AddResultItems;
			buttonDelExpenseNomenclature.Clicked += DelSourceItems;
			buttonDelReceiptNomenclature.Clicked += DelResultItems;
			ytreeExpenseItems.Selection.Changed += ytreeExpenseItems_Selection_Changed;
			ytreeReceiptItems.Selection.Changed += ytreeReceiptItems_Selection_Changed;
			buttonDelExpenseNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanDelItemSource, b => b.Sensitive)
				.InitializeFromSource();
			buttonDelReceiptNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanDelItemResult, b => b.Sensitive)
				.InitializeFromSource();

			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
			 	.InitializeFromSource();
			 ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
			 	.InitializeFromSource();
			 ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			 ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			 
			 entityWarehouseExpense.ViewModel = ViewModel.WarehouseExpenseEntryViewModel;
			 entityWarehouseReceipt.ViewModel = ViewModel.WarehouseReceiptEntryViewModel;

			 entityWarehouseExpense.Visible = entityWarehouseReceipt.Visible = labelResult.Visible = labelSource.Visible = ViewModel.ShowWarehouses;
			 #region TreeSource
			 ytreeExpenseItems.ColumnsConfig = ColumnsConfigFactory.Create<CompletionSourceItem>()
				 .AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				 .AddColumn("Размер").MinWidth(60)
				 .AddComboRenderer(x => x.Size)
				 .DynamicFillListFunc(x => SizeHelper.GetSizesListByStdCode(x.Nomenclature.SizeStd, SizeUse.HumanOnly))
				 .AddSetter((c, n) => c.Editable = n.Nomenclature.SizeStd != null)
				 .AddColumn("Рост").MinWidth(70)
				 .AddComboRenderer(x => x.Growth)
				 .FillItems(SizeHelper.GetGrowthList(SizeUse.HumanOnly))
				 .AddSetter((c, n) => c.Editable = n.Nomenclature.Type.WearCategory.HasValue && SizeHelper.HasGrowthStandart(n.Nomenclature.Type.WearCategory.Value))
				 .AddColumn ("Процент износа").AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter()).Editing(new Adjustment(0, 0, 999, 1, 10, 0)).WidthChars(6).Digits(0)
				 .AddTextRenderer(e => "%", expand: false)
				 .AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
				 .AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				 .Finish ();
			 ytreeExpenseItems.ItemsDataSource = Entity.ObservableSourceItems;
			 #endregion
			 #region TreeResult
			 ytreeReceiptItems.ColumnsConfig = ColumnsConfigFactory.Create<CompletionResultItem>()
				 .AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				 .AddColumn("Размер").MinWidth(60)
				 .AddComboRenderer(x => x.Size)
				 .DynamicFillListFunc(x => SizeHelper.GetSizesListByStdCode(x.Nomenclature.SizeStd, SizeUse.HumanOnly))
				 .AddSetter((c, n) => c.Editable = n.Nomenclature.SizeStd != null)
				 .AddColumn("Рост").MinWidth(70)
				 .AddComboRenderer(x => x.Growth)
				 .FillItems(SizeHelper.GetGrowthList(SizeUse.HumanOnly))
				 .AddSetter((c, n) => c.Editable = n.Nomenclature.Type.WearCategory.HasValue && SizeHelper.HasGrowthStandart(n.Nomenclature.Type.WearCategory.Value))
				 .AddColumn ("Процент износа").AddNumericRenderer (e => e.WearPercent, new MultiplierToPercentConverter()).Editing (new Adjustment(0,0,999,1,10,0)).WidthChars(6).Digits(0)
				 .AddTextRenderer (e => "%", expand: false)
				 .AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(8)
				 .AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				 .Finish ();
			 ytreeReceiptItems.ItemsDataSource = Entity.ObservableResultItems;
			 #endregion
		}
		
		void ytreeExpenseItems_Selection_Changed(object sender, EventArgs e)
		{
			buttonDelExpenseNomenclature.Sensitive = ytreeExpenseItems.Selection.CountSelectedRows() > 0;
		}
		
		void ytreeReceiptItems_Selection_Changed(object sender, EventArgs e)
		{
			buttonDelReceiptNomenclature.Sensitive = ytreeReceiptItems.Selection.CountSelectedRows() > 0;
		}
		void AddSourceItems(object sender, EventArgs eventArgs) {
			ViewModel.AddSourceItems();
		}
		void AddResultItems(object sender, EventArgs eventArgs) {
			ViewModel.AddResultItems();
		}
		void DelSourceItems(object sender, EventArgs eventArgs) {
			ViewModel.DelSourceItems(ytreeExpenseItems.GetSelectedObject<CompletionSourceItem> ());
		}
		void DelResultItems(object sender, EventArgs eventArgs) {
			ViewModel.DelResultItems(ytreeReceiptItems.GetSelectedObject<CompletionResultItem> ());
		}
	}
}
