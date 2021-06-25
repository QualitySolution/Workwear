using System;
using System.Data.Bindings.Utilities;
using System.Linq;
using Gtk;
using QSWidgetLib;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseDocItemObjectView : Gtk.Bin
	{
		private enum ColumnTags
		{
			FacilityPlace,
			BuhDoc
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

			ExpenseDoc_PropertyChanged(ViewModel.expenseObjectViewModel.Entity, new System.ComponentModel.PropertyChangedEventArgs(ViewModel.expenseObjectViewModel.Entity.GetPropertyName(x => x.Operation)));
			if(ViewModel.expenseObjectViewModel.Entity.Operation == ExpenseOperations.Object)
				ExpenseDoc_PropertyChanged(ViewModel.expenseObjectViewModel.Entity, new System.ComponentModel.PropertyChangedEventArgs(ViewModel.expenseObjectViewModel.Entity.GetPropertyName(x => x.Subdivision)));

			ViewModel.expenseObjectViewModel.Entity.PropertyChanged += ExpenseDoc_PropertyChanged;
			ViewModel.PropertyChanged += PropertyChanged;
			ViewModel.CalculateTotal();

		}

		void CreateTable()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ExpenseItem>()
				.AddColumn("Наименование").AddTextRenderer(e => e.Nomenclature.Name)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.Size)
					.DynamicFillListFunc(x => SizeHelper.GetSizesListByStdCode(x.Nomenclature.SizeStd, SizeUse.HumanOnly))
					.AddSetter((c, n) => c.Editable = n.Nomenclature.SizeStd != null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.WearGrowth)
					.DynamicFillListFunc(x => SizeHelper.GetSizesListByStdCode(x.Nomenclature.WearGrowthStd, SizeUse.HumanOnly))
					.AddSetter((c, n) => c.Editable = n.Nomenclature.WearGrowthStd != null)
				.AddColumn("Процент износа").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer(e => e.Nomenclature.Type.Units.Name)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument).Editable()
				.AddColumn("Расположение").Tag(ColumnTags.FacilityPlace).AddComboRenderer(e => e.SubdivisionPlace).Editing()
					.SetDisplayFunc(x => (x as SubdivisionPlace) != null ? (x as SubdivisionPlace).Name : String.Empty)
				.AddColumn("")
				.Finish();
		}

		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeItems.GetSelectedObject<ExpenseItem>();
				var item = new MenuItemId<ExpenseItem>("Открыть номеклатуру");
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
			if(e.PropertyName == ViewModel.expenseObjectViewModel.Entity.GetPropertyName(x => x.Operation)) {
				var placeColumn = ytreeItems.ColumnsConfig.GetColumnsByTag(ColumnTags.FacilityPlace).First();
				placeColumn.Visible = ViewModel.expenseObjectViewModel.Entity.Operation == ExpenseOperations.Object;

				var buhDocColumn = ytreeItems.ColumnsConfig.GetColumnsByTag(ColumnTags.BuhDoc).First();
				buhDocColumn.Visible = ViewModel.expenseObjectViewModel.Entity.Operation == ExpenseOperations.Employee;

				buttonFillBuhDoc.Visible = ViewModel.expenseObjectViewModel.Entity.Operation == ExpenseOperations.Employee;
			}
			if(e.PropertyName == nameof(ViewModel.expenseObjectViewModel.Entity.Warehouse))
				buttonAdd.Sensitive = ViewModel.expenseObjectViewModel.Entity.Warehouse != null;
		}

		void SetSum()
		{
			labelSum.Markup = viewModel.Sum;
			buttonFillBuhDoc.Sensitive = ViewModel.SensetiveFillBuhDoc;
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
		void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			SetSum();
		}
	}
}
