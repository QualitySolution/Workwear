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

			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
			 	.InitializeFromSource();
			 ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser.Name, w => w.LabelProp)
			 	.InitializeFromSource();
			 ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			 ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			 entityWarehouseExpense.ViewModel = ViewModel.WarehouseExpenseEntryViewModel;
			 entityWarehouseReceipt.ViewModel = ViewModel.WarehouseReceiptEntryViewModel;

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
			 
			 ytreeReceiptItems.ColumnsConfig = ColumnsConfigFactory.Create<ComplectionResultItem>()
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
		}
		void AddSourceItems(object sender, EventArgs eventArgs) {
			ViewModel.AddSourceItems();
		}
		void AddResultItems(object sender, EventArgs eventArgs) {
			ViewModel.AddResultItems();
		}
		void DelSourceItems(object sender, EventArgs eventArgs) {
			Entity.ObservableSourceItems.Remove(ytreeExpenseItems.GetSelectedObject<CompletionSourceItem> ());
		}
		void DelResultItems(object sender, EventArgs eventArgs) {
			Entity.ObservableResultItems.Remove(ytreeReceiptItems.GetSelectedObject<ComplectionResultItem> ());
		}
	}
}
