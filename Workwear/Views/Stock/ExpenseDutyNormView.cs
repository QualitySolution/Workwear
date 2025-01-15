using System;
using System.Linq;
using Gamma.Binding.Converters;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseDutyNormView : EntityDialogViewBase<ExpenseDutyNormViewModel, ExpenseDutyNorm> {
		public ExpenseDutyNormView(ExpenseDutyNormViewModel viewModel) : base(viewModel) {
			Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() {
			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();
			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			yentryNorm.ViewModel = ViewModel.DutyNormEntryViewModel;
			yentryWarehouseExpense.ViewModel = ViewModel.WarehouseEntryViewModel;
			yentryResponsible.ViewModel = ViewModel.ResponsibleEmployeeCardEntryViewModel;
			
			//butt
			
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ExpenseDutyNormItem> ()
				.AddColumn("Номенклатура нормы").Resizable().AddComboRenderer(x => x.ProtectionTools)
					.SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.ProtectionToolsListFromNorm.ToList())
					.Editing()
				.AddColumn("Номенклатура").Resizable()
					.AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : "")
					.WrapWidth(700)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.HeightType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.HeightType != null) 
//.AddColumn("Износ").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer(e => 
					e.Nomenclature != null && e.Nomenclature.Type != null && e.Nomenclature.Type.Units != null ? e.Nomenclature.Type.Units.Name : null)
				.Finish();
			ytreeItems.ItemsDataSource = Entity.Items;

		}

		protected void OnButtonAddClicked(object sender, EventArgs e) {
			ViewModel.AddItems();
		}

		protected void OnButtonDelClicked(object sender, EventArgs e) {
			foreach(var item in ytreeItems.GetSelectedObjects<ExpenseDutyNormItem>())
				ViewModel.DeleteItem(item);
		}
	}
}
