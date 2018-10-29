using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NLog;
using QSOrmProject;
using workwear.Domain.Organization;
using workwear.Domain.Stock;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseDocItemsView : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private enum ColumnTags
		{
			FacilityPlace,
			BuhDoc
		}

	private Expense expenceDoc;

		public Expense ExpenceDoc {
			get {return expenceDoc;}
			set {
				if (expenceDoc == value)
					return;
				if(expenceDoc != null)
				{
					expenceDoc.PropertyChanged -= ExpenceDoc_PropertyChanged;
				}
				expenceDoc = value;
				if(expenceDoc != null)
				{
					expenceDoc.PropertyChanged += ExpenceDoc_PropertyChanged;
				}
				ytreeItems.ItemsDataSource = expenceDoc.ObservableItems;
				expenceDoc.ObservableItems.ListContentChanged += ExpenceDoc_ObservableItems_ListContentChanged;
				ExpenceDoc_PropertyChanged(expenceDoc, new System.ComponentModel.PropertyChangedEventArgs(expenceDoc.GetPropertyName(x => x.Operation)));
				if(ExpenceDoc.Operation == ExpenseOperations.Object)
					ExpenceDoc_PropertyChanged(expenceDoc, new System.ComponentModel.PropertyChangedEventArgs(expenceDoc.GetPropertyName(x => x.Facility)));
				CalculateTotal();
			}
		}

		void ExpenceDoc_ObservableItems_ListContentChanged (object sender, EventArgs e)
		{
			CalculateTotal();
		}

		void ExpenceDoc_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == ExpenceDoc.GetPropertyName(x => x.Facility))
			{
				var placeColumn = ytreeItems.ColumnsConfig.ConfiguredColumns.FirstOrDefault(x => ColumnTags.FacilityPlace.Equals(x.tag));
				var placeRenderer = placeColumn.ConfiguredRenderers.First() as ComboRendererMapping<ExpenseItem, FacilityPlace>;
				if(ExpenceDoc.Facility != null)
				{
					placeRenderer.FillItems(ExpenceDoc.Facility.Places);
				}
				else
				{
					placeRenderer.FillItems(new List<FacilityPlace> ());
				}
			}

			if(e.PropertyName == ExpenceDoc.GetPropertyName(x => x.Operation))
			{
				var placeColumn = ytreeItems.ColumnsConfig.GetColumnsByTag(ColumnTags.FacilityPlace).First();
				placeColumn.Visible = ExpenceDoc.Operation == ExpenseOperations.Object;

				var buhDocColumn = ytreeItems.ColumnsConfig.GetColumnsByTag(ColumnTags.BuhDoc).First();
				buhDocColumn.Visible = ExpenceDoc.Operation == ExpenseOperations.Employee;
			}
		}

		public ExpenseDocItemsView()
		{
			this.Build();

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ExpenseItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn ("Размер").AddTextRenderer (e => e.Nomenclature.Size)
				.AddColumn ("Рост").AddTextRenderer (e => e.Nomenclature.WearGrowth)
				.AddColumn ("Состояние").AddTextRenderer (e => (e.IncomeOn.LifePercent).ToString ("P0"))
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument).Editable()
				.AddColumn ("Расположение").Tag(ColumnTags.FacilityPlace).AddComboRenderer (e => e.FacilityPlace).Editing()
					.SetDisplayFunc(x => (x as FacilityPlace) != null ? (x as FacilityPlace).Name : String.Empty)
				.AddColumn("")
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			var selectDlg = new ReferenceRepresentation (new ViewModel.StockBalanceVM (MyOrmDialog.UoW,
				ExpenceDoc.Operation == ExpenseOperations.Employee ? ViewModel.StockBalanceVMMode.DisplayAll : ViewModel.StockBalanceVMMode.OnlyProperties,
			                                                                           ViewModel.StockBalanceVMGroupBy.IncomeItem
			),
			     "Остатки на складе");
			selectDlg.Mode = OrmReferenceMode.MultiSelect;
			selectDlg.ObjectSelected += SelectDlg_ObjectSelected;

			OpenSlaveTab(selectDlg);
		}

		void SelectDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.StockBalanceVMNode> ())
			{
				ExpenceDoc.AddItem (MyOrmDialog.UoW.GetById<IncomeItem> (node.Id));
			}
			CalculateTotal();
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			ExpenceDoc.RemoveItem (ytreeItems.GetSelectedObject<ExpenseItem> ());
			CalculateTotal();
		}

		private void CalculateTotal()
		{
			labelSum.Markup = String.Format ("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>", 
				ExpenceDoc.Items.Count,
				ExpenceDoc.Items.Sum(x => x.Amount)
			);
		} 
	}
}

