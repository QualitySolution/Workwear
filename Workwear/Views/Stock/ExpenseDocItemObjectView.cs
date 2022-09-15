using System;
using Gtk;
using QS.DomainModel.Entity;
using QSWidgetLib;
using Workwear.Domain.Stock.Documents;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseDocItemObjectView : Gtk.Bin
	{
		private enum ColumnTags
		{
			FacilityPlace,
		}

		public ExpenseDocItemObjectView()
		{
			this.Build();
		}
		private ExpenseDocItemsObjectViewModel viewModel;

		public ExpenseDocItemsObjectViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				Configure();
			}
		}

		public void Configure()
		{
			CreateTable();
			ytreeItems.ItemsDataSource = ViewModel.ObservableItems;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;

			labelSum.Binding.AddBinding(ViewModel, v => v.Sum, w => w.LabelProp).InitializeFromSource();

			ViewModel.expenseObjectViewModel.Entity.PropertyChanged += ExpenseDoc_PropertyChanged;
			ViewModel.CalculateTotal();
		}

		void CreateTable()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ExpenseItem>()
				.AddColumn("Наименование").AddTextRenderer(e => e.Nomenclature.Name)
				.AddColumn("Процент износа").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer(e => e.Nomenclature.Type.Units.Name)
				.AddColumn("Расположение").Tag(ColumnTags.FacilityPlace).AddComboRenderer(e => e.SubdivisionPlace).Editing()
					.DynamicFillListFunc(x => ViewModel.Places)
					.SetDisplayFunc(x => DomainHelper.GetTitle(x))
				.AddColumn("")
				.Finish();
		}

		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeItems.GetSelectedObject<ExpenseItem>();
				var item = new MenuItemId<ExpenseItem>("Открыть номенклатуру");
				item.ID = selected;
				item.Sensitive = selected.Nomenclature != null;
				if(selected == null)
					item.Sensitive = false;
				else
					item.Activated += Item_Activated;
				menu.Add(item);
				menu.ShowAll();
				menu.Popup();
			}
		}

		void Item_Activated(object sender, EventArgs e)
		{
			var item = (sender as MenuItemId<ExpenseItem>).ID;
			viewModel.OpenNomenclature(item.Nomenclature);
		}
		#endregion

		void ExpenseDoc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.expenseObjectViewModel.Entity.Warehouse))
				buttonAdd.Sensitive = ViewModel.expenseObjectViewModel.Entity.Warehouse != null;
		}

		protected void OnButtonFillBuhDocClicked(object sender, EventArgs e)
		{
			ViewModel.FillBuhDoc();
		}

		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			viewModel.Delete(ytreeItems.GetSelectedObject<ExpenseItem>());
		}

		void YtreeItems_Selection_Changed(object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItem();
		}

		void AddNomenclature_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			ViewModel.AddNomenclature(sender, e);
		}
	}
}
