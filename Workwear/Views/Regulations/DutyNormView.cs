using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Regulations;

namespace Workwear.Views.Regulations {
	public partial class DutyNormView : EntityDialogViewBase<DutyNormViewModel, DutyNorm> {
		public DutyNormView(DutyNormViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureBase();
			ConfigureMainInfo();
			ConfigureDialog();
			ConfigureHistoty();
			tabs.Binding.AddBinding(ViewModel, v => v.CurrentTab, w => w.CurrentPage).InitializeFromSource();
			CommonButtonSubscription();
		}
		//Общее
		private void ConfigureBase() {
			enumSaveAndPrint.ItemsEnum = typeof(DutyNormSheetPrint);
			ycheckArchival.Binding.AddBinding(Entity, e => e.Archival, w=> w.Active).InitializeFromSource();
			buttonSaveAndGive.Binding.AddFuncBinding(Entity, e => !e.Archival, w => w.Sensitive).InitializeFromSource();
		}

		#region Вкладка Основное
		private void ConfigureMainInfo() { 
			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new Gamma.Binding.Converters.IdToStringConverter())
				.InitializeFromSource();
			datefrom.Binding.AddBinding(Entity, e => e.DateFrom, w => w.DateOrNull).InitializeFromSource();
			dateto.Binding.AddBinding(Entity, e => e.DateTo, w => w.DateOrNull).InitializeFromSource();
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			entitySubdivision.ViewModel = ViewModel.SubdivisionEntryViewModel;
			entityResponsibleEmployee.ViewModel = ViewModel.EmployeeCardEntryViewModel;
			entityResponsibleLeader.ViewModel = ViewModel.LeaderEntryViewModel;
		}
		#endregion
		
		#region Вкладка Норма (список РТ и баланс)
		private void ConfigureDialog() { 
			ytreeItems.ColumnsConfig = FluentColumnsConfig<DutyNormItem>.Create()
				.AddColumn("ИД").AddTextRenderer(p => p.ProtectionTools.Id.ToString())
				.AddColumn("Наименование").Resizable()
					.AddTextRenderer(i  => i.ProtectionTools != null ? i.ProtectionTools.Name : null).WrapWidth(700)
				.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount).WidthChars(5).Editing().Adjustment(new Gtk.Adjustment(1, 1, 65535, 1, 10, 10))
					.AddTextRenderer(i => i.AmountUnitText(i.Amount))
				.AddColumn("Период").Resizable()
					.AddNumericRenderer(i => i.PeriodCount).WidthChars(5).Editing().Adjustment(new Gtk.Adjustment(1, 1, 100, 1, 10, 10))
						.AddSetter((c, n) => c.Visible = n.NormPeriod != DutyNormPeriodType.Wearout)
					.AddEnumRenderer(i => i.NormPeriod).Editing()
					.AddSetter((c, n) => {
						c.Text = n.PeriodText;
						c.WidthChars = 8;
						c.Xalign = 1.0f; })
				.AddColumn("Числится").Resizable()
					.AddTextRenderer(i => i.Issued(DateTime.Now).ToString())
					.AddSetter((w, i) => w.Foreground = i.AmountColor)
					.AddTextRenderer(i => i.AmountUnitText(i.Issued(DateTime.Now)))
				.AddColumn("След. получение").Resizable()
					.AddTextRenderer(i => $"{i.NextIssue:d}")
					.AddSetter((w, i) => w.Foreground = i.NextIssueColor)
				.AddColumn("Пункт норм").AddTextRenderer(x => x.NormParagraph).Editable()
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment).Editable()
				.RowCells().AddSetter<CellRendererText>((c,x) => c.Foreground = x.ProtectionTools.Archival ? "gray" : null)
				.Finish ();
			ytreeItems.Binding
				.AddBinding(ViewModel, v => v.SelectedItem, w => w.SelectedRow)
				.AddBinding(Entity, e => e.Items, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeItems.ButtonReleaseEvent += TreeItems_ButtonReleaseEvent;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		}
		// Контекстное меню
		private void TreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeItems.GetSelectedObject<DutyNormItem>();
				
				var menuItem = new MenuItem("Открыть номенклатуру нормы");
				menuItem.Sensitive = selected != null;
				menuItem.Activated += (sender, e) => ViewModel.OpenProtectionTools(selected);
				menu.Add(menuItem);
				
				menuItem = new MenuItem("Открыть последнюю выдачу");
				menuItem.Sensitive = selected != null;
				menuItem.Activated += (sender, e) => ViewModel.OpenLastDocument(selected);
				menu.Add(menuItem);
				
				menu.ShowAll();
				menu.Popup();
			}
		}
		#endregion
		
		#region Вкладка История выдач
		private void ConfigureHistoty() { 
			ytreeviewHistory.ColumnsConfig = FluentColumnsConfig<DutyNormIssueOperation>.Create()
				.AddColumn("Дата").AddReadOnlyTextRenderer(x => x.OperationTime.ToShortDateString())
				.AddColumn("Номенклатура").Resizable()
					.AddTextRenderer(e => e.Nomenclature != null ? e.Nomenclature.Name : "").WrapWidth(1000)
				.AddColumn("Номенклатура нормы").Resizable()
					.AddTextRenderer(e => e.ProtectionTools != null ? e.ProtectionTools.Name : "").WrapWidth(1000)
				.AddColumn("% износа").AddTextRenderer(e => e.WearPercent.ToString("P0"))
				.AddColumn("Получено").AddNumericRenderer(e => e.Issued)
				.AddColumn("Дата автосписания").AddReadOnlyTextRenderer(x => x.AutoWriteoffDate?.ToShortDateString() ?? "")
				.Finish();
			ytreeviewHistory.ItemsDataSource = ViewModel.Operations;
		}
		#endregion
		
		#region Обработчики
		void YtreeItems_Selection_Changed (object sender, EventArgs e) {
			buttonRemoveItem.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}
		
		protected void OnButtonColorsLegendClicked(object sender, EventArgs e) => 
			ViewModel.ShowLegend();
		protected void OnButtonAddItemClicked (object sender, EventArgs e) =>
			ViewModel.AddItem();
		protected void OnButtonRemoveItemClicked (object sender, EventArgs e) =>
			ViewModel.RemoveItem(ytreeItems.GetSelectedObject<DutyNormItem>());
		protected void OnEnumSaveAndPrintEnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e) =>
			ViewModel.SaveAndPrint((DutyNormSheetPrint)e.ItemEnum);
		protected void OnButtonSaveAndGiveClicked(object sender, EventArgs e) =>
			ViewModel.AddExpense();

		#endregion
	}
}
