﻿using System;
using System.Linq;
using Gamma.Binding.Converters;
using Gtk;
using QS.Views.Dialog;
using QSWidgetLib;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock
{
	public partial class WarehouseTransferView : EntityDialogViewBase<WarehouseTransferViewModel, Transfer>
	{
		public WarehouseTransferView(WarehouseTransferViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}
		private void ConfigureDlg() {
			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocNumberText, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource(); 
			datepicker.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource(); 
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			entityentryOrganization.ViewModel = ViewModel.OrganizationEntryViewModel;
			entityentryWarehouseFrom.ViewModel = ViewModel.WarehouseFromEntryViewModel;
			entityentryWarehouseTo.ViewModel = ViewModel.WarehouseToEntryViewModel;

			table.CreateFluentColumnsConfig<TransferItem>()
			.AddColumn("Наименование").Tag("Name").AddReadOnlyTextRenderer(x => x.Nomenclature?.Name).WrapWidth(700)
			.AddColumn("Размер").AddReadOnlyTextRenderer(x => x.WarehouseOperation.WearSize?.Name)
			.AddColumn("Рост").AddReadOnlyTextRenderer(x => x.WarehouseOperation.Height?.Name)
			.AddColumn("Собственники")
				.Visible(ViewModel.FeaturesService.Available(WorkwearFeature.Owners))
				.AddComboRenderer(x => x.Owner)
				.SetDisplayFunc(x => x.Name)
				.FillItems(ViewModel.Owners, "Нет")
				.Editing()
			.AddColumn("Процент износа").AddTextRenderer(x => x.WarehouseOperation.WearPercent.ToString("P0"))
			.AddColumn("Количество").Tag("Count")
				.AddNumericRenderer(x => x.Amount, false)
					.Editing(true).Adjustment(new Adjustment(1, 0, 100000, 1, 10, 10)).WidthChars(8)
				.AddReadOnlyTextRenderer(x => x.Nomenclature?.Type?.Units?.Name,  false)
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = GetRowColor(n))
			.Finish();

			table.Selection.Changed += Selection_Changed;
			table.Selection.Mode = SelectionMode.Multiple;
			table.ItemsDataSource = ViewModel.Entity.Items;
			table.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;

			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			buttonAddItem.Sensitive = ViewModel.CanAddItem;
			ybuttonPrint.Clicked += OnButtonPrintClicked;
		}
		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var selected = table.GetSelectedObjects<TransferItem>().FirstOrDefault();
				if(selected == null)
					return;
				
				var menu = new Menu();
				var item = new MenuItemId<TransferItem>("Открыть номенклатуру");
				item.ID = selected;
				item.Sensitive = selected.Nomenclature != null;
				item.Activated += Item_Activated;
				menu.Add(item);
				menu.ShowAll();
				menu.Popup();
			}
		}
		private void Item_Activated(object sender, EventArgs e) {
			var item = (sender as MenuItemId<TransferItem>)?.ID;
			ViewModel.OpenNomenclature(item?.Nomenclature);
		}
		#endregion
		private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			buttonAddItem.Sensitive = ViewModel.CanAddItem;
		}
		private void Selection_Changed(object sender, EventArgs e) {
			buttonRemoveItem.Sensitive =  table.Selection.CountSelectedRows() > 0 ;
		}
		private void OnButtonAddClicked(object sender, EventArgs e) {
			ViewModel.AddItems();
		}
		private void OnButtonDelClicked(object sender, EventArgs e) {
			var items = table.GetSelectedObjects<TransferItem>();
			ViewModel.RemoveItems(items);
		}
		private string GetRowColor(TransferItem item) 
			=> !ViewModel.ValidateNomenclature(item) ? "red" : null;
		private void OnButtonPrintClicked(object sender, EventArgs e) => ViewModel.Print();
	}
}
