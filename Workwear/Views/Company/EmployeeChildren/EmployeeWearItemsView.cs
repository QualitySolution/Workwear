using System;
using System.Linq;
using Gamma.Utilities;
using Gdk;
using Gtk;
using QS.Utilities;
using QSWidgetLib;
using Workwear.Domain.Company;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeWearItemsView : Gtk.Bin
	{
		public EmployeeWearItemsView()
		{
			this.Build();
		}

		private EmployeeWearItemsViewModel viewModel;

		public EmployeeWearItemsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				viewModel.PropertyChanged += ViewModel_PropertyChanged;
				ConfigureTable();
			}
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ObservableWorkwearItems)) {
				ytreeWorkwear.ItemsDataSource = ViewModel.ObservableWorkwearItems;
			}
		}

		void ytreeWorkwear_Selection_Changed(object sender, EventArgs e)
		{
			buttonTimeLine.Sensitive = ytreeWorkwear.Selection.CountSelectedRows() > 0;
			buttonManualIssueDate.Sensitive = ytreeWorkwear.Selection.CountSelectedRows() > 0;
		}

		#region private
		Pixbuf handIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Workwear.icon.rows.нand.png");
		Pixbuf infoIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Workwear.icon.rows.info.png");

		void ConfigureTable()
		{
			ytreeWorkwear.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<EmployeeCardItem>()
				.AddColumn("ТОН").AddTextRenderer(node => node.TonText)
				.AddColumn("Тип выдачи").Visible(ViewModel.FeaturesService.Available(WorkwearFeature.CollectiveExpense))
					.AddTextRenderer(x => x.ProtectionTools.Type.IssueType.GetEnumTitle())
				.AddColumn("Наименование").AddTextRenderer(node => node.ProtectionTools.Name).WrapWidth(700)
				.AddColumn("По норме").AddTextRenderer(node => node.AmountByNormText)
				.AddColumn("Срок службы").AddTextRenderer(node => node.NormLifeText)
				.AddColumn("Послед. получения")
					.AddPixbufRenderer(node => node.LastIssued(DateTime.Today).Any(x => x.item.IssueOperation.ManualOperation) ? handIcon : null) //FIXME пока так определяем ручную операцию. Когда будут типы операций надо переделать.
					.AddTextRenderer(node => node.LastIssuedText, useMarkup: true)
				.AddColumn("Числится").AddTextRenderer(node => node.AmountText)
					.AddSetter((w, node) => w.Foreground = node.AmountColor)
				.AddColumn("След. получение")
					.ToolTipText(node => node.NextIssueAnnotation)
					.AddTextRenderer(node => $"{node.NextIssue:d}")
					.AddSetter((w, node) => w.Foreground = node.NextIssueColor(ViewModel.BaseParameters))
					.AddPixbufRenderer(node => String.IsNullOrEmpty(node.NextIssueAnnotation) ? null : infoIcon)
				.AddColumn("Просрочка").AddTextRenderer(
					node => node.NextIssue.HasValue && node.NextIssue.Value < DateTime.Today
					? NumberToTextRus.FormatCase((int)(DateTime.Today - node.NextIssue.Value).TotalDays, "{0} день", "{0} дня", "{0} дней")
					: String.Empty)
				.AddColumn("На складе").AddTextRenderer(node => node.InStockText)
				 .AddSetter((w, node) => w.Foreground = node.InStockState.GetEnumColor())
				.AddColumn("Подходящая номенклатура").AddTextRenderer(node => node.MatchedNomenclatureShortText)
				.AddSetter((w, node) => w.Foreground = node.InStockState.GetEnumColor())
				.Finish();
			ytreeWorkwear.Selection.Changed += ytreeWorkwear_Selection_Changed;
			ytreeWorkwear.ButtonReleaseEvent += YtreeWorkwear_ButtonReleaseEvent;
		}
		#endregion

		#region Кнопки
		protected void OnButtonGiveWearByNormClicked(object sender, EventArgs e) {
			buttonGiveWearByNorm.Sensitive = false;
			ViewModel.GiveWearByNorm();
			buttonGiveWearByNorm.Sensitive = true;
		}

		protected void OnButtonReturnWearClicked(object sender, EventArgs e) {
			buttonReturnWear1.Sensitive = false;
			viewModel.ReturnWear();
			buttonReturnWear1.Sensitive = true;
		}

		protected void OnButtonTimeLineClicked(object sender, EventArgs e)
		{
			ViewModel.OpenTimeLine(ytreeWorkwear.GetSelectedObject<EmployeeCardItem>());
		}

		protected void OnButtonWriteOffWearClicked(object sender, EventArgs e) {
			buttonWriteOffWear1.Sensitive = false;
			ViewModel.WriteOffWear();
			buttonWriteOffWear1.Sensitive = true;
		}

		protected void OnButtonRefreshWorkwearItemsClicked(object sender, EventArgs e)
		{
			ViewModel.UpdateWorkwearItems();
		}

		protected void OnButtonManualIssueDateClicked(object sender, EventArgs e)
		{
			ViewModel.SetIssueDateManual(ytreeWorkwear.GetSelectedObject<EmployeeCardItem>());
		}
		#endregion

		#region PopupMenu
		void YtreeWorkwear_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeWorkwear.GetSelectedObject<EmployeeCardItem>();
				
				var itemOpenActiveNorm = new MenuItemId<EmployeeCardItem>("Открыть активную норму");
				itemOpenActiveNorm.ID = selected;
				itemOpenActiveNorm.Sensitive = selected?.ActiveNormItem != null;
				itemOpenActiveNorm.Activated += (sender, e) => viewModel.OpenActiveNorm(((MenuItemId<EmployeeCardItem>)sender).ID);
				menu.Add(itemOpenActiveNorm);

				var itemOpenProtectionTools = new MenuItemId<EmployeeCardItem>("Открыть номенклатуру нормы");
				itemOpenProtectionTools.ID = selected;
				itemOpenProtectionTools.Sensitive = selected?.ProtectionTools != null;
				itemOpenProtectionTools.Activated += (sender, e) => viewModel.OpenProtectionTools(((MenuItemId<EmployeeCardItem>)sender).ID);
				menu.Add(itemOpenProtectionTools);

				var itemOpenLastIssue = new MenuItemId<EmployeeCardItem>("Открыть документ с последней выдачей");
				itemOpenLastIssue.ID = selected;
				itemOpenLastIssue.Sensitive = selected?.LastIssueOperation != null;
				itemOpenLastIssue.Activated += (sender, e) => viewModel.OpenLastIssue(((MenuItemId<EmployeeCardItem>)sender).ID);
				menu.Add(itemOpenLastIssue);

				menu.Add(new SeparatorMenuItem());

				var itemRecalculateLastIssue = new MenuItemId<EmployeeCardItem>("Пересчитать сроки носки последней выдаче");
				itemRecalculateLastIssue.ID = selected;
				itemRecalculateLastIssue.Sensitive = selected?.LastIssueOperation != null;
				itemRecalculateLastIssue.Activated += (sender, e) => viewModel.RecalculateLastIssue(((MenuItemId<EmployeeCardItem>)sender).ID);
				menu.Add(itemRecalculateLastIssue);

				menu.ShowAll();
				menu.Popup();
			}
		}
		#endregion
	}
}
