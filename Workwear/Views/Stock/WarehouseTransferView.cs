using System;
using System.Linq;
using Gamma.Binding.Converters;
using Gtk;
using QS.DomainModel.Entity;
using QS.Views.Dialog;
using QSWidgetLib;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class WarehouseTransferView : EntityDialogViewBase<WarehouseTransferViewModel, Transfer>
	{
		public WarehouseTransferView(WarehouseTransferViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			datepicker.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			entryNumber.Binding.AddBinding(Entity, e => e.Id, w => w.Text, new IdToStringConverter()).InitializeFromSource();
			entryUser.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? DomainHelper.GetTitle(e.CreatedbyUser) : String.Empty, w => w.Text).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer).InitializeFromSource();
			entityentryWarehouseFrom.ViewModel = ViewModel.WarehouseFromEntryViewModel;
			entityentryWarehouseTo.ViewModel = ViewModel.WarehouseToEntryViewModel;

			table.CreateFluentColumnsConfig<TransferItem>()
			.AddColumn("Наименование").Tag("Name").AddTextRenderer(x => x.Nomenclature!= null ? x.Nomenclature.Name : String.Empty)
			.AddColumn("Размер").AddTextRenderer(x => x.WarehouseOperation.Size)
			.AddColumn("Рост").AddTextRenderer(x => x.WarehouseOperation.Growth)
			.AddColumn("Процент износа").AddTextRenderer(x => x.WarehouseOperation.WearPercent.ToString("P0"))
			.AddColumn("Количество").Tag("Count")
				.AddNumericRenderer(x => x.Amount, false).Editing(true).Adjustment(new Adjustment(1, 0, 100000, 1, 10, 10)).WidthChars(8)
				.AddTextRenderer(x => x.Nomenclature != null && x.Nomenclature.Type.Units != null ? x.Nomenclature.Type.Units.Name : String.Empty,  false)
			.AddColumn("")
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = GetRowColor(n))
			.Finish();

			table.Selection.Changed += Selection_Changed;
			table.Selection.Mode = SelectionMode.Multiple;
			table.ItemsDataSource = ViewModel.Entity.ObservableItems;
			table.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;

			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			buttonAddItem.Sensitive = ViewModel.CanAddItem;
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

		void Item_Activated(object sender, EventArgs e)
		{
			var item = (sender as MenuItemId<TransferItem>).ID;
			ViewModel.OpenNomenclature(item.Nomenclature);
		}
		#endregion

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			buttonAddItem.Sensitive = ViewModel.CanAddItem;
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveItem.Sensitive =  table.Selection.CountSelectedRows() > 0 ;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItems();
		}

		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			var items = table.GetSelectedObjects<TransferItem>();
			ViewModel.RemoveItems(items);
		}

		private string GetRowColor(TransferItem item)
		{
			if(!ViewModel.ValidateNomenclature(item))
				return "red";
			return null;
		}
	}
}
