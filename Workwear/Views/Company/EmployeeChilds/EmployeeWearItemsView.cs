﻿using System;
using Gamma.Utilities;
using Gtk;
using QS.Utilities;
using QSWidgetLib;
using workwear.Domain.Company;
using workwear.ViewModels.Company.EmployeeChilds;

namespace workwear.Views.Company.EmployeeChilds
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeWearItemsView : Gtk.Bin
	{
		public EmployeeWearItemsView()
		{
			this.Build();

			ytreeWorkwear.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<EmployeeCardItem>()
				.AddColumn("ТОН").AddTextRenderer(node => node.TonText)
				.AddColumn("Наименование").AddTextRenderer(node => node.ProtectionTools.Name)
				.AddColumn("По норме").AddTextRenderer(node => node.AmountByNormText)
				.AddColumn("Срок службы").AddTextRenderer(node => node.NormLifeText)
				.AddColumn("Дата получения").AddTextRenderer(node => String.Format("{0:d}", node.LastIssue))
				.AddColumn("Получено").AddTextRenderer(node => node.AmountText)
					.AddSetter((w, node) => w.Foreground = node.AmountColor)
				.AddColumn("След. получение").AddTextRenderer(node => String.Format("{0:d}", node.NextIssue))
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

		private EmployeeWearItemsViewModel viewModel;

		public EmployeeWearItemsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				viewModel.PropertyChanged += ViewModel_PropertyChanged;
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
		}

		#region Кнопки
		protected void OnButtonGiveWearByNormClicked(object sender, EventArgs e)
		{
			ViewModel.GiveWearByNorm();
		}

		protected void OnButtonReturnWearClicked(object sender, EventArgs e)
		{
			viewModel.ReturnWear();
		}

		protected void OnButtonTimeLineClicked(object sender, EventArgs e)
		{
			ViewModel.OpenTimeLine(ytreeWorkwear.GetSelectedObject<EmployeeCardItem>());
		}

		protected void OnButtonWriteOffWearClicked(object sender, EventArgs e)
		{
			ViewModel.WriteOffWear();
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
				var item = new MenuItemId<EmployeeCardItem>("Открыть номеклатуру нормы");
				item.ID = selected;
				item.Sensitive = selected.ProtectionTools != null;
				if(selected == null)
					item.Sensitive = false;
				else
					item.Activated += (sender, e) => viewModel.OpenProtectionTools(((MenuItemId<EmployeeCardItem>)sender).ID);
				menu.Add(item);
				menu.ShowAll();
				menu.Popup();
			}
		}
		#endregion
	}
}
