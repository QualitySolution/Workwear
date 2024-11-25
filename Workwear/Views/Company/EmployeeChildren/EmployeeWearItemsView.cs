using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gdk;
using Gtk;
using QS.Utilities;
using QSWidgetLib;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
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
				MakeManualIssueMenu();
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
			ViewModel.SelectedWorkwearItem = ytreeWorkwear.GetSelectedObject<EmployeeCardItem>();
			buttonTimeLine.Sensitive = ytreeWorkwear.Selection.CountSelectedRows() > 0;
		}

		#region private
		Pixbuf handIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Workwear.icon.rows.нand.png");
		Pixbuf infoIcon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Workwear.icon.rows.info.png");

		void ConfigureTable()
		{
			ytreeWorkwear.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<EmployeeCardItem>()
				.AddColumn("ТОН").AddTextRenderer(item => item.TonText)
				.AddColumn("Тип выдачи").Visible(ViewModel.FeaturesService.Available(WorkwearFeature.CollectiveExpense))
					.AddTextRenderer(item => item.ProtectionTools.Type.IssueType.GetEnumTitle())
				.AddColumn("Наименование").Resizable().AddTextRenderer(item => item.ProtectionTools.Name).WrapWidth(700)
				.AddColumn("По норме").AddTextRenderer(item => item.AmountByNormText)
				.AddColumn("Срок службы").AddTextRenderer(item => item.NormLifeText)
				.AddColumn("Послед. получения")
					.AddPixbufRenderer(node => node.LastIssued(DateTime.Today, ViewModel.BaseParameters).Any(x => ((EmployeeIssueOperation)x.item.IssueOperation).ManualOperation) ? handIcon : null)
					.AddTextRenderer(node => MakeLastIssuedText(node), useMarkup: true)
				.AddColumn("Числится").AddTextRenderer(node => node.AmountText)
					.AddSetter((w, node) => w.Foreground = node.AmountColor)
				.AddColumn("След. получение")
					.ToolTipText(item => item.NextIssueAnnotation)
					.AddTextRenderer(item => $"{item.NextIssue:d}")
					.AddSetter((w, item) => w.Foreground = item.NextIssueColor(ViewModel.BaseParameters))
					.AddPixbufRenderer(item => String.IsNullOrEmpty(item.NextIssueAnnotation) ? null : infoIcon)
				.AddColumn("Просрочка").AddTextRenderer(
					item => item.NextIssue.HasValue && item.NextIssue.Value < DateTime.Today
					? NumberToTextRus.FormatCase((int)(DateTime.Today - item.NextIssue.Value).TotalDays, "{0} день", "{0} дня", "{0} дней")
					: String.Empty)
				.AddColumn("На складе").AddTextRenderer(item => item.InStockText)
				.AddSetter((w, item) => w.Foreground = item.InStockState.GetEnumColor())
				.AddColumn("Подходящая номенклатура").AddTextRenderer(item => item.MatchedNomenclatureShortText)
				.AddSetter((w, item) => w.Foreground = item.InStockState.GetEnumColor())
				.Finish();
			ytreeWorkwear.Selection.Changed += ytreeWorkwear_Selection_Changed;
			ytreeWorkwear.ButtonReleaseEvent += YtreeWorkwear_ButtonReleaseEvent;
		}

		private void MakeManualIssueMenu() {
			var menu = new Menu();
			var itemManualIssueRow = new yMenuItem("Для выбранной строки");
			itemManualIssueRow.Binding.AddBinding(ViewModel, v => v.SensitiveManualIssueOnRow, w => w.Sensitive).InitializeFromSource();
			itemManualIssueRow.Activated += (sender, e) => ViewModel.SetIssueDateManual(ytreeWorkwear.GetSelectedObject<EmployeeCardItem>());
			menu.Add(itemManualIssueRow);
			var itemManualIssueNew = new MenuItem("Для другой номенклатуры нормы");
			itemManualIssueNew.Activated += (sender, e) => ViewModel.SetIssueDateManual();
			menu.Add(itemManualIssueNew);
			menu.ShowAll();
			buttonManualIssueDate.Menu = menu;
		}

		private string MakeLastIssuedText(EmployeeCardItem item) => String.Join("\n", 
			item.LastIssued(DateTime.Today, ViewModel.BaseParameters)
				.Select(x => $"{FormatOfLastIssue(x.date.Date)} - {x.amount}{ShowIfExist(x.removed)}"));
		private string ShowIfExist(int removed) => removed > 0 ? $"(-{removed})" : "";

		private string FormatOfLastIssue(DateTime issueDate) {
			if(issueDate.Date < DateTime.Today)
				return $"{issueDate:d}";
			var color = issueDate.Date == DateTime.Today ? "darkgreen" : "darkviolet";
			return $"<span foreground=\"{color}\">{issueDate:d}</span>";
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
				itemOpenLastIssue.Sensitive = selected?.LastIssueOperation(DateTime.Today, ViewModel.BaseParameters) != null;
				itemOpenLastIssue.Activated += (sender, e) => viewModel.OpenLastIssue(((MenuItemId<EmployeeCardItem>)sender).ID);
				menu.Add(itemOpenLastIssue);

				menu.Add(new SeparatorMenuItem());

				var itemRecalculateLastIssue = new MenuItemId<EmployeeCardItem>("Пересчитать сроки носки последней выдаче");
				itemRecalculateLastIssue.ID = selected;
				itemRecalculateLastIssue.Sensitive = selected?.LastIssueOperation(DateTime.Today, ViewModel.BaseParameters) != null;
				itemRecalculateLastIssue.Activated += (sender, e) => viewModel.RecalculateLastIssue(((MenuItemId<EmployeeCardItem>)sender).ID);
				menu.Add(itemRecalculateLastIssue);

				menu.ShowAll();
				menu.Popup();
			}
		}
		#endregion
	}
}
