using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using QSOrmProject;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace Workwear.Views.Stock
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
			 ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser.Name, w => w.LabelProp)
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
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				 .AddColumn("Рост").MinWidth(70)
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.HeightType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.HeightType != null)
				 .AddColumn("Собственики")
					.Visible(ViewModel.featuresService.Available(WorkwearFeature.Owners))
					.AddComboRenderer(x => x.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.Owners, "отменить")
					.Editing()
				 .AddColumn ("Процент износа")
					.AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
				 .Editing(new Adjustment(0, 0, 999, 1, 10, 0)).WidthChars(6).Digits(0)
					.AddTextRenderer(e => "%", expand: false)
				 .AddColumn ("Количество").AddNumericRenderer (e => e.Amount)
				 .Editing(new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
					.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				 .Finish();
			 ytreeExpenseItems.ItemsDataSource = Entity.ObservableSourceItems;
			 #endregion
			 #region TreeResult
			 ytreeReceiptItems.ColumnsConfig = ColumnsConfigFactory.Create<CompletionResultItem>()
				 .AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				 .AddColumn("Размер").MinWidth(60)
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				 .AddColumn("Рост").MinWidth(70)
				 .AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.HeightType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				 .AddColumn("Собственики")
					.Visible(ViewModel.featuresService.Available(WorkwearFeature.Owners))
					.AddComboRenderer(x => x.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.Owners, "отменить")
					.Editing()
				 .AddColumn ("Процент износа").AddNumericRenderer (e => e.WearPercent, new MultiplierToPercentConverter()).Editing (new Adjustment(0,0,999,1,10,0)).WidthChars(6).Digits(0)
				 .AddTextRenderer (e => "%", expand: false)
				 .AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(8)
				 .AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				 .Finish();
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
