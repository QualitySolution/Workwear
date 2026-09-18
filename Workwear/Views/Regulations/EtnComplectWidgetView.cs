using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Views;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Regulations;

namespace Workwear.Views.Regulations {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EtnComplectWidgetView : ViewBase<EtnComplectWidgetViewModel> {
		public EtnComplectWidgetView(EtnComplectWidgetViewModel viewModel) : base(viewModel) {
			this.Build();

			ylabelHeadTitle.Binding.AddBinding(ViewModel, v => v.HeadTitle, w => w.LabelProp).InitializeFromSource();
			ylabelHeadComment.Binding.AddBinding(ViewModel, v => v.HeadComment, w => w.LabelProp).InitializeFromSource();

			ytreeItems.Selection.Mode = ViewModel.AllowMultipleSelection ? Gtk.SelectionMode.Multiple : Gtk.SelectionMode.Single;
			ytreeItems.ColumnsConfig = FluentColumnsConfig<EtnComplectItemNode>.Create()
				.AddColumn("Название").AddReadOnlyTextRenderer(x => x.Name).WrapWidth(400)
				.AddColumn("Количество")
					.AddNumericRenderer(x => x.Amount, OnAmountEdited).Editing().Adjustment(new Gtk.Adjustment(1, 0, 100000, 1, 10, 10)).WidthChars(9)
				.AddColumn("Период")
					.AddNumericRenderer(x => x.PeriodCount, OnPeriodCountEdited).Editing().Adjustment(new Gtk.Adjustment(1, 0, 100, 1, 10, 10)).WidthChars(6)
						.AddSetter((c, n) => c.Visible = n.PeriodType != NormPeriodType.Wearout)
				.AddColumn("Тип периода")
					.AddComboRenderer(x => x.PeriodType)
						.SetDisplayFunc(FormatPeriodType)
						.SetDisplayListFunc(FormatPeriodType)
						.FillItems(new NormPeriodType?[] { NormPeriodType.Year, NormPeriodType.Month, NormPeriodType.Wearout }, "—")
						.Editing()
				.AddColumn("Комментарий").AddReadOnlyTextRenderer(x => x.Comment).WrapWidth(400)
				.Finish();
			ytreeItems.ItemsDataSource = ViewModel.Items;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ybuttonAddSelected.Sensitive = false;

			ybuttonAddSelected.Clicked += (sender, e) => ViewModel.AddSelected(ytreeItems.GetSelectedObjects<EtnComplectItemNode>());
			ybuttonCancel.Clicked += (sender, e) => ViewModel.Cancel();
		}

		void YtreeItems_Selection_Changed(object sender, EventArgs e) {
			ybuttonAddSelected.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		string FormatPeriodType(NormPeriodType? value) => value.HasValue ? value.Value.GetEnumTitle() : "—";

		//Ручные обработчики редактирования: количество и период c поддержкой nullable<int>
		void OnAmountEdited(object o, Gtk.EditedArgs args) => SetNullableInt(args, (x, v) => x.Amount = v);
		void OnPeriodCountEdited(object o, Gtk.EditedArgs args) => SetNullableInt(args, (x, v) => x.PeriodCount = v);

		void SetNullableInt(Gtk.EditedArgs args, Action<EtnComplectItemNode, int?> setValue) {
			if(!ytreeItems.YTreeModel.Adapter.GetIter(out var iter, new Gtk.TreePath(args.Path)))
				return;
			if(!(ytreeItems.YTreeModel.NodeFromIter(iter) is EtnComplectItemNode node))
				return;

			if(String.IsNullOrWhiteSpace(args.NewText)) {
				setValue(node, null);
				return;
			}
			if(int.TryParse(args.NewText, out var newValue))
				setValue(node, newValue);
		}
	}
}
