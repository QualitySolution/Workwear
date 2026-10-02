using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Views.Dialog;
using Workwear.Domain.Regulations;
using Workwear.Models.Regulations;
using Workwear.ViewModels.Regulations;

namespace Workwear.Views.Regulations {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EtnImportView : DialogViewBase<EtnImportViewModel> {
		public EtnImportView(EtnImportViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg() {
			ylabelHeadComment.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.HeadComment, w => w.LabelProp)
				.InitializeFromSource();
			ylabelSummary.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.Summary, w => w.LabelProp)
				.InitializeFromSource();

			yentrySearch.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.SearchText, w => w.Text)
				.InitializeFromSource();
			buttonClear.Clicked += (sender, e) => ViewModel.ClearSearch();

			ycheckIncludeAdditional.Label = "Добавить СИЗ по результатам оценки профрисков (гр. 8-9 приказа)";
			ycheckIncludeAdditional.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.IncludeAdditional, w => w.Active)
				.AddBinding(v => v.VisibleIncludeAdditional, w => w.Visible)
				.InitializeFromSource();

			ConfigureSubjects();
			ConfigureItems();
			ConfigurePaned();

			ybuttonAdd.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.SensitiveAdd, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonAdd.Clicked += (sender, e) => ViewModel.Accept();
			ybuttonCancel.Clicked += (sender, e) => ViewModel.Cancel();
		}

		private bool itemsPaneShown;

		private void ConfigurePaned() {
			((Gtk.Paned.PanedChild)vpaned1[GtkScrolledWindow]).Resize = true;

			vpaned1.SizeAllocated += VpanedSizeAllocated;
			ViewModel.Rows.CollectionChanged += (sender, e) => ShowItemsPane();
		}

		private void VpanedSizeAllocated(object o, Gtk.SizeAllocatedArgs args) {
			if(itemsPaneShown)
				return;
			var height = args.Allocation.Height;
			if(height > 1 && vpaned1.Position < height - 8)
				vpaned1.Position = height;
		}

		private void ShowItemsPane() {
			if(itemsPaneShown || ViewModel.Rows.Count == 0)
				return;
			itemsPaneShown = true;

			var height = vpaned1.Allocation.Height;
			if(height > 1)
				vpaned1.Position = height * 55 / 100;
		}

		private void ConfigureSubjects() {
			ytreeSubjects.ColumnsConfig = FluentColumnsConfig<EtnSubjectNode>.Create()
				.AddColumn("☑").AddToggleRenderer(x => x.Selected).Editing()
					.AddSetter((c, n) => c.Visible = n.Selectable)
				.AddColumn("№").AddReadOnlyTextRenderer(x => x.Code).SearchHighlight()
				.AddColumn("Наименование").Resizable()
					.AddReadOnlyTextRenderer(x => x.NameWithNote).WrapWidth(600).SearchHighlight()
				.RowCells()
					.AddSetter<Gtk.CellRendererText>((c, n) => c.Weight = n.Selected ? 600 : 400)
				.Finish();

			RenewSubjectsModel();
			ViewModel.TreeFilterChanged += (sender, e) => RenewSubjectsModel();
			ViewModel.SubjectsSelectionChanged += (sender, e) => ytreeSubjects.QueueDraw();
		}

		private void RenewSubjectsModel() {
			ytreeSubjects.SearchHighlightText = ViewModel.SearchText?.Trim();
			ytreeSubjects.YTreeModel = new Gamma.Binding.RecursiveTreeModel<EtnSubjectNode>(
				ViewModel.VisibleRoots, x => x.Parent, x => x.Children);
			if(String.IsNullOrWhiteSpace(ViewModel.SearchText))
				ytreeSubjects.CollapseAll();
			else
				ytreeSubjects.ExpandAll();
		}

		private void ConfigureItems() {
			ytreeItems.ColumnsConfig = FluentColumnsConfig<EtnImportPlanRow>.Create()
				.AddColumn("Пункт").AddReadOnlyTextRenderer(x => x.SourceName).WrapWidth(400)
				.AddColumn("Номенклатуры нормы (варианты)")
					.AddComboRenderer(x => x.Variant).WrapWidth(800)
						.SetDisplayFunc(x => x.Name)
						.SetDisplayListFunc(x => x.Name)
						.DynamicFillListFunc(x => x.Variants)
						.AddSetter((c, n) => c.Text = n.HasChoice ? n.Variant.Name : n.Name)
						.AddSetter((c, n) => c.WrapMode = Pango.WrapMode.WordChar)
						.AddSetter((c, n) => c.Editable = n.HasChoice)
						.AddSetter((c, n) => c.Foreground = n.HasChoice ? "blue" : null)
				.AddColumn("Количество")
					.AddNumericRenderer(x => x.Amount, OnAmountEdited).Editing()
						.Adjustment(new Gtk.Adjustment(1, 0, 100000, 1, 10, 10)).WidthChars(9)
				.AddColumn("Период")
					.AddNumericRenderer(x => x.PeriodCount, OnPeriodCountEdited).Editing()
						.Adjustment(new Gtk.Adjustment(1, 0, 100, 1, 10, 10)).WidthChars(6)
						.AddSetter((c, n) => c.Visible = n.PeriodType != NormPeriodType.Wearout)
				.AddColumn("Тип периода")
					.AddComboRenderer(x => x.PeriodType)
						.SetDisplayFunc(FormatPeriodType)
						.SetDisplayListFunc(FormatPeriodType)
						.FillItems(new NormPeriodType?[] { NormPeriodType.Year, NormPeriodType.Month, NormPeriodType.Wearout }, "—")
						.Editing()
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment).Editable().WrapWidth(200)
				.RowCells()
					//Жирным - строки, где от пользователя ещё требуется действие: в приложении 2 дофига строк без определённого срока выдачи.
					.AddSetter<Gtk.CellRendererText>((c, n) => c.Weight = n.IsComplete ? 400 : 600)
				.Finish();
			ytreeItems.Columns[0].MinWidth = ytreeItems.Columns[0].MaxWidth = 400;
			ytreeItems.Columns[1].MinWidth = ytreeItems.Columns[1].MaxWidth = 800;
			ytreeItems.ItemsDataSource = ViewModel.Rows;
		}

		private static string FormatPeriodType(NormPeriodType? value) => value.HasValue ? value.Value.GetEnumTitle() : "—";

		private void OnAmountEdited(object o, Gtk.EditedArgs args) => SetNullableInt(args, (x, v) => x.Amount = v);
		private void OnPeriodCountEdited(object o, Gtk.EditedArgs args) => SetNullableInt(args, (x, v) => x.PeriodCount = v);

		private void SetNullableInt(Gtk.EditedArgs args, Action<EtnImportPlanRow, int?> setValue) {
			if(!ytreeItems.YTreeModel.Adapter.GetIter(out var iter, new Gtk.TreePath(args.Path)))
				return;
			if(!(ytreeItems.YTreeModel.NodeFromIter(iter) is EtnImportPlanRow row))
				return;

			if(String.IsNullOrWhiteSpace(args.NewText)) {
				setValue(row, null);
				return;
			}
			if(int.TryParse(args.NewText, out var newValue))
				setValue(row, newValue);
		}
	}
}
