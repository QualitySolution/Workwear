using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using QSOrmProject;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
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
			buttonAddExpenseNomenclature.Clicked += (s,e) => 
				ViewModel.AddSourceItems();
			buttonAddReceiptNomenclature.Clicked += (s,e) => 
				ViewModel.AddResultItems();
			buttonDelExpenseNomenclature.Clicked += (s,e) => 
				ViewModel.DelSourceItems(ytreeExpenseItems.GetSelectedObject<CompletionSourceItem> ());;
			buttonDelReceiptNomenclature.Clicked += (s,e) => 
				ViewModel.DelResultItems(ytreeReceiptItems.GetSelectedObject<CompletionResultItem> ());
			buttonAddSizesReceiptNomenclature.Clicked += (s,e) => 
				ViewModel.AddSizesResultItems(ytreeReceiptItems.GetSelectedObject<CompletionResultItem> ());
			ytreeExpenseItems.Selection.Changed += (s,e) => 
				ViewModel.SelectedSourceItem = ytreeExpenseItems.GetSelectedObject<CompletionSourceItem>();
			ytreeReceiptItems.Selection.Changed += (s,e) =>
				ViewModel.SelectedResultItem = ytreeReceiptItems.GetSelectedObject<CompletionResultItem>();
			
			buttonDelExpenseNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.SensitiveDellSourceItemButton, b => b.Sensitive).InitializeFromSource();
			buttonDelReceiptNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.SensitiveDellResultItemButton, b => b.Sensitive).InitializeFromSource(); 
			buttonAddSizesReceiptNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.SensitiveAddSizesResultButton, b => b.Sensitive).InitializeFromSource();
			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocNumberText, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive).InitializeFromSource();
			checkAuto.Binding
				.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource(); 
			
			 ylabelCreatedBy.Binding
				 .AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			 ydateDoc.Binding
				 .AddBinding(ViewModel, vm => vm.DocumentDate, w => w.Date)
				 .AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive)
				 .AddBinding(ViewModel,vm => vm.CanChangeDocDate, w => w.IsEditable)
				 .InitializeFromSource();
			 ytextComment.Binding
				 .AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				 .AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			 
			 entityWarehouseExpense.ViewModel = ViewModel.WarehouseExpenseEntryViewModel;
			 entityWarehouseReceipt.ViewModel = ViewModel.WarehouseReceiptEntryViewModel;
			 entityWarehouseExpense.Visible = entityWarehouseReceipt.Visible = labelResult.Visible = labelSource.Visible = ViewModel.ShowWarehouses;
			 
			 #region TreeSource
			 buttonAddExpenseNomenclature.Binding
				 .AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			 ytreeExpenseItems.ColumnsConfig = ColumnsConfigFactory.Create<CompletionSourceItem>()
				 .AddColumn ("Наименование")
					.ToolTipText(x => $"ИД номенклатуры: {x.Nomenclature.Id}")
					.AddTextRenderer (e => e.Nomenclature.Name)
					.WrapWidth(500)
				 .AddColumn("Размер").MinWidth(60)
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.SizeType != null)
				 .AddColumn("Рост").MinWidth(70)
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.HeightType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.HeightType != null)
				 .AddColumn("Собственник")
					.Visible(ViewModel.featuresService.Available(WorkwearFeature.Owners))
					.AddComboRenderer(x => x.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.Owners, "Нет")
					.Editing(ViewModel.CanEdit)
				 .AddColumn ("Износ")
					.AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
				 .Editing(new Adjustment(0, 0, 999, 1, 10, 0), ViewModel.CanEdit).WidthChars(6).Digits(0)
					.AddTextRenderer(e => "%", expand: false)
				 .AddColumn ("Количество").AddNumericRenderer (e => e.Amount)
				 .Editing(new Adjustment(0, 0, 100000, 1, 10, 1), ViewModel.CanEdit).WidthChars(7)
					.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				 .Finish();
			 ytreeExpenseItems.ItemsDataSource = Entity.SourceItems;
			 ylabelAmountSource.Binding.AddBinding(ViewModel, vm => vm.SourceAmountText, w => w.LabelProp).InitializeFromSource();
			 #endregion
			 
			 #region TreeResult
			 buttonAddReceiptNomenclature.Binding
				 .AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			 ytreeReceiptItems.ColumnsConfig = ColumnsConfigFactory.Create<CompletionResultItem>()
				 .AddColumn("Наименование")
					.ToolTipText(x => $"ИД номенклатуры: {x.Nomenclature.Id}")
					.AddTextRenderer (e => e.Nomenclature.Name)
					.WrapWidth(500)
				 .AddColumn("Размер").MinWidth(60)
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.SizeType != null)
				 .AddColumn("Рост").MinWidth(70)
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.HeightType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.SizeType != null)
				 .AddColumn("Собственник")
					.Visible(ViewModel.featuresService.Available(WorkwearFeature.Owners))
					.AddComboRenderer(x => x.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.Owners, "Нет")
					.Editing(ViewModel.CanEdit)
				 .AddColumn ("Износ").AddNumericRenderer (e => e.WearPercent, new MultiplierToPercentConverter())
					.Editing (new Adjustment(0,0,999,1,10,0),ViewModel.CanEdit).WidthChars(6).Digits(0)
					.AddTextRenderer (e => "%", expand: false)
				 .AddColumn ("Количество").AddNumericRenderer (e => e.Amount)
					.Editing (new Adjustment(0, 0, 100000, 1, 10, 1),ViewModel.CanEdit).WidthChars(8)
					.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				 .Finish();
			 ytreeReceiptItems.ItemsDataSource = Entity.ResultItems;
			 ylabelAmountResult.Binding.AddBinding(ViewModel, vm => vm.ResultAmountText, w => w.LabelProp).InitializeFromSource();
			 #endregion
		}
		
		int lastHpanedWidth;
		protected void OnHpaned1SizeAllocated(object o, SizeAllocatedArgs args) {
			if(lastHpanedWidth != hpaned1.Allocation.Width) {
				lastHpanedWidth = hpaned1.Allocation.Width;
				hpaned1.Position = hpaned1.Allocation.Width / 2;
			}
		}
	}
}
