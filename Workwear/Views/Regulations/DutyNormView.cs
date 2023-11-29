using System;
using Gamma.ColumnConfig;
using QS.Utilities.Text;
using QS.Views.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Regulations;

namespace Workwear.Views.Regulations {
	public partial class DutyNormView : EntityDialogViewBase<DutyNormViewModel, DutyNorm> {
		public DutyNormView(DutyNormViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureMainInfo();
			ConfigureDialog();
			ConfigureHistoty();
			tabs.Binding.AddBinding(ViewModel, v => v.CurrentTab, w => w.CurrentPage).InitializeFromSource();
			CommonButtonSubscription();
		}
		//Вкладка Основное
		private void ConfigureMainInfo() { 
			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new Gamma.Binding.Converters.IdToStringConverter())
				.InitializeFromSource();
			ycomboAnnex.SetRenderTextFunc<RegulationDocAnnex>(x => StringManipulationHelper.EllipsizeMiddle(x.Title, 160));
			yentryRegulationDoc.SetRenderTextFunc<RegulationDoc>(x => StringManipulationHelper.EllipsizeMiddle(x.Title, 160));
			yentryRegulationDoc.ItemsList = ViewModel.RegulationDocs;
			yentryRegulationDoc.WidthRequest = 1; //Минимальное не нулевое значение, чтобы элемент не участвовал в расчёте мин. ширины окна
			ycomboAnnex.WidthRequest = 1;
			yentryRegulationDoc.Binding.AddBinding(Entity, e => e.Document, w => w.SelectedItem).InitializeFromSource();
			ycomboAnnex.Binding.AddBinding(Entity, e => e.Annex, w => w.SelectedItem).InitializeFromSource();
			datefrom.Binding.AddBinding(Entity, e => e.DateFrom, w => w.DateOrNull).InitializeFromSource();
			dateto.Binding.AddBinding(Entity, e => e.DateTo, w => w.DateOrNull).InitializeFromSource();
			yentryTonParagraph.Binding.AddBinding(Entity, e => e.TONParagraph, w => w.Text).InitializeFromSource();
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
		}
		//Вкладка Норма (список РТ и баланс)
		private void ConfigureDialog() { 
			ytreeItems.ColumnsConfig = FluentColumnsConfig<DutyNormItem>.Create()
				.AddColumn("ИД").AddTextRenderer(p => p.ProtectionTools.Id.ToString())
				.AddColumn("Наименование").Resizable()
					.AddTextRenderer(i  => i.ProtectionTools != null ? i.ProtectionTools.Name : null).WrapWidth(700)
				.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount).WidthChars(5).Editing().Adjustment(new Gtk.Adjustment(1, 1, 65535, 1, 10, 10))
					.AddTextRenderer(i => i.AmountUnitText(i.Amount))
				.AddColumn("Период").Resizable()
					.AddNumericRenderer(i => i.PeriodCount).WidthChars(3).Editing().Adjustment(new Gtk.Adjustment(1, 1, 100, 1, 10, 10))
						.AddSetter((c, n) => c.Visible = n.NormPeriod != NormPeriodType.Wearout && n.NormPeriod != NormPeriodType.Duty)
					.AddEnumRenderer(i => i.NormPeriod)
						.AddSetter((c,n) => c.Text = n.PeriodText )
						.Editing()
				.AddColumn("Числится").Resizable()
					.AddTextRenderer(i => i.Issued(DateTime.Now).ToString())
					.AddSetter((w, i) => w.Foreground = i.AmountColor)
					.AddTextRenderer(i => i.AmountUnitText(i.Issued(DateTime.Now)))
				.AddColumn("След. получение").Resizable()
					.AddTextRenderer(i => $"{i.NextIssue:d}")
					.AddSetter((w, i) => w.Foreground = i.NextIssueColor)
				.AddColumn("Пункт норм").AddTextRenderer(x => x.NormParagraph).Editable()
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment).Editable()
				.Finish ();
			ytreeItems.ItemsDataSource = Entity.Items;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.Binding.AddSource(ViewModel).AddBinding(v => v.SelectedItem, w => w.SelectedRow);
		}
		//Вкладка История выдач
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
				.Finish ();
			ytreeviewHistory.ItemsDataSource = ViewModel.Operations;
		}
		
		void YtreeItems_Selection_Changed (object sender, EventArgs e) {
			buttonRemoveItem.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}
		
		protected void OnButtonAddItemClicked (object sender, EventArgs e) {
			ViewModel.AddItem();
		}

		protected void OnButtonRemoveItemClicked (object sender, EventArgs e) {
			ViewModel.RemoveItem(ytreeItems.GetSelectedObject<DutyNormItem>());
		}
	}
}
