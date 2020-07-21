using System;
using System.Collections.Generic;
using System.Data.Bindings.Utilities;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseDocItemEmployeeView : Gtk.Bin
	{
		private enum ColumnTags
		{
			FacilityPlace,
			BuhDoc
		}

		public ExpenseDocItemEmployeeView()
		{
			this.Build();
		}

		private ExpenseDocItemsEmployeeViewModel viewModel;

		public ExpenseDocItemsEmployeeViewModel ViewModel {
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

			ExpenseDoc_PropertyChanged(ViewModel.expenseEmployeeViewModel.Entity, new System.ComponentModel.PropertyChangedEventArgs(ViewModel.expenseEmployeeViewModel.Entity.GetPropertyName(x => x.Operation)));

			buttonAdd.Sensitive = ViewModel.Warehouse != null;

			ViewModel.expenseEmployeeViewModel.Entity.PropertyChanged += ExpenseDoc_PropertyChanged;

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
		void ExpenseDoc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var placeColumn = ytreeItems.ColumnsConfig.ConfiguredColumns.FirstOrDefault(x => ColumnTags.FacilityPlace.Equals(x.tag));
			var placeRenderer = placeColumn.ConfiguredRenderers.First() as ComboRendererMapping<ExpenseItem, SubdivisionPlace>;
			if(ViewModel.Subdivision != null) {
				placeRenderer.FillItems(ViewModel.Subdivision.Places);
			}
			else {
				placeRenderer.FillItems(new List<SubdivisionPlace>());
			}
		
			if(e.PropertyName == ViewModel.GetPropertyName(x => x.Operation)) {

				var buhDocColumn = ytreeItems.ColumnsConfig.GetColumnsByTag(ColumnTags.BuhDoc).First();
				buhDocColumn.Visible = ViewModel.Operation == ExpenseOperations.Employee;

				buttonFillBuhDoc.Visible = ViewModel.Operation == ExpenseOperations.Employee;
			}
			if(e.PropertyName == nameof(ViewModel.Warehouse))
				buttonAdd.Sensitive = ViewModel.Warehouse != null;
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

		void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			SetSum();
		}
	}

}
