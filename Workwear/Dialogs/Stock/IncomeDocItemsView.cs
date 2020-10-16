using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QSOrmProject;
using QSWidgetLib;
using workwear.Domain.Operations;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.Representations.Organization;
using workwear.ViewModels.Stock.Widgets;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeDocItemsView : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private enum ColumnTags
		{
			BuhDoc
		}

		private Income incomeDoc;

		public Income IncomeDoc {
			get {return incomeDoc;}
			set {
				if (incomeDoc == value)
					return;
				incomeDoc = value;
				ytreeItems.ItemsDataSource = incomeDoc.ObservableItems;
				incomeDoc.ObservableItems.ListContentChanged += IncomeDoc_ObservableItems_ListContentChanged;
				IncomeDoc.PropertyChanged += IncomeDoc_PropertyChanged;
				IncomeDoc_PropertyChanged (null, new System.ComponentModel.PropertyChangedEventArgs(IncomeDoc.GetPropertyName (d => d.Operation)));
				CalculateTotal();

				IncomeDoc.Items.ToList().ForEach(item => item.PropertyChanged += Item_PropertyChanged);
			}
		}

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(IncomeItem.BuhDocument)) {
				(MyEntityDialog as EntityDialogBase<Income>).HasChanges = true;
			}
		}

		void IncomeDoc_ObservableItems_ListContentChanged (object sender, EventArgs e)
		{
			CalculateTotal();
		}

		void IncomeDoc_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == IncomeDoc.GetPropertyName (d => d.Operation) 
				|| e.PropertyName == IncomeDoc.GetPropertyName (d => d.EmployeeCard)
				|| e.PropertyName == IncomeDoc.GetPropertyName (d => d.Subdivision))
			{
				buttonAdd.Sensitive = (IncomeDoc.Operation == IncomeOperations.Return && IncomeDoc.EmployeeCard != null) 
					|| (IncomeDoc.Operation == IncomeOperations.Object && IncomeDoc.Subdivision != null) 
					|| IncomeDoc.Operation == IncomeOperations.Enter;
			}

			if (e.PropertyName == IncomeDoc.GetPropertyName(x => x.Operation))
			{
				var buhDocColumn = ytreeItems.ColumnsConfig.GetColumnsByTag(ColumnTags.BuhDoc).First();
				buhDocColumn.Visible = IncomeDoc.Operation == IncomeOperations.Return;
				buttonFillBuhDoc.Visible = IncomeDoc.Operation == IncomeOperations.Return;
			}
		}

		public IncomeDocItemsView()
		{
			this.Build();

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<IncomeItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn("Сертификат").AddTextRenderer(e => e.Certificate).Editable()
				.AddColumn("Размер")
					.AddComboRenderer(x => x.Size)
					.DynamicFillListFunc(x => SizeHelper.GetSizesListByStdCode(x.Nomenclature.SizeStd, SizeUse.HumanOnly))
					.AddSetter((c, n) => c.Editable = n.Nomenclature.SizeStd != null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.WearGrowth)
					.DynamicFillListFunc(x => SizeHelper.GetSizesListByStdCode(x.Nomenclature.WearGrowthStd, SizeUse.HumanOnly))
					.AddSetter((c, n) => c.Editable = n.Nomenclature.WearGrowthStd != null)
				.AddColumn ("Процент износа").AddNumericRenderer (e => e.WearPercent, new MultiplierToPercentConverter()).Editing (new Adjustment(0,0,100,1,10,0)).WidthChars(6).Digits(0)
				.AddTextRenderer (e => "%", expand: false)
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(8)
				.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn ("Стоимость").AddNumericRenderer (e => e.Cost).Editing (new Adjustment(0,0,100000000,100,1000,0)).Digits (2).WidthChars(12)
				.AddColumn("Сумма").AddNumericRenderer(x => x.Total).Digits(2)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument).Editable()
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
		}

		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeItems.GetSelectedObject<IncomeItem>();
				var item = new MenuItemId<IncomeItem>("Открыть номеклатуру");
				item.ID = selected;
				item.Activated += Item_Activated;
				menu.Add(item);
				menu.ShowAll();
				menu.Popup();
			}
		}

		void Item_Activated(object sender, EventArgs e)
		{
			var item = (sender as MenuItemId<IncomeItem>).ID;
			OpenTab<NomenclatureDlg, int>(item.Nomenclature.Id);
		}

		#endregion
		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
			if(ytreeItems.GetSelectedObject<IncomeItem>() != null) {
				var obj = ytreeItems.GetSelectedObject<IncomeItem>();
				var clothesType = obj.Nomenclature.Type.WearCategory;
				if(clothesType != null)
					buttonAddSizes.Sensitive = obj.Nomenclature.Type.Category == Domain.Regulations.ItemTypeCategory.wear
						&& ytreeItems.Selection.CountSelectedRows() == 1
						&& (clothesType != СlothesType.PPE)
						&& SizeHelper.HasСlothesSizeStd(clothesType.Value);
				else
					buttonAddSizes.Sensitive = false;
			}
		}
		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if(IncomeDoc.Operation == IncomeOperations.Return)
			{
				var vm = new EmployeeBalanceVM(UoW);
				vm.Employee = IncomeDoc.EmployeeCard;
				var selectFromEmployeeDlg = new ReferenceRepresentation (vm, $"Выданное {IncomeDoc.EmployeeCard.ShortName}");
				selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
				selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;

				OpenSlaveTab(selectFromEmployeeDlg);
			}

			if(IncomeDoc.Operation == IncomeOperations.Object)
			{
				var selectFromObjectDlg = new ReferenceRepresentation (new ViewModel.ObjectBalanceVM (IncomeDoc.Subdivision),
				                                                       $"Выданное на {IncomeDoc.Subdivision.Name}");
				selectFromObjectDlg.Mode = OrmReferenceMode.MultiSelect;
				selectFromObjectDlg.ObjectSelected += SelectFromObjectDlg_ObjectSelected;;

				OpenSlaveTab(selectFromObjectDlg);
			}

			if(IncomeDoc.Operation == IncomeOperations.Enter)
			{
				var selectNomenclatureDlg = new OrmReference (typeof(Nomenclature));
				selectNomenclatureDlg.Mode = OrmReferenceMode.MultiSelect;
				selectNomenclatureDlg.ObjectSelected += SelectNomenclatureDlg_ObjectSelected;

				OpenSlaveTab(selectNomenclatureDlg);
			}
		}

		void SelectFromObjectDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.ObjectBalanceVMNode> ())
			{
				IncomeDoc.AddItem (MyOrmDialog.UoW.GetById<SubdivisionIssueOperation> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<EmployeeBalanceVMNode> ())
			{
				IncomeDoc.AddItem (MyOrmDialog.UoW.GetById<EmployeeIssueOperation> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectNomenclatureDlg_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			e.GetEntities<Nomenclature>().ToList().ForEach(n => IncomeDoc.AddItem(n));
			CalculateTotal();
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			IncomeDoc.RemoveItem (ytreeItems.GetSelectedObject<IncomeItem> ());
			buttonAddSizes.Sensitive = false;
			CalculateTotal();
		}

		private void CalculateTotal()
		{
			labelSum.Markup = String.Format ("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>  Сумма: <u>{2:C}</u>", 
				IncomeDoc.Items.Count,
				IncomeDoc.Items.Sum(x => x.Amount),
				IncomeDoc.Items.Sum(x => x.Total)
			);
			buttonFillBuhDoc.Sensitive = IncomeDoc.Items.Count > 0;
		}

		protected void OnButtonFillBuhDocClicked(object sender, EventArgs e)
		{
			using (var dlg = new Dialog("Введите бухгалтерский документ", MainClass.MainWin, DialogFlags.Modal))
			{
				var docEntry = new Entry(80);
				if (incomeDoc.Items.Count > 0)
					docEntry.Text = incomeDoc.Items.First().BuhDocument;
				docEntry.TooltipText = "Бухгалтерский документ по которому была произведена выдача. Отобразится вместо подписи сотрудника в карточке.";
				docEntry.ActivatesDefault = true;
				dlg.VBox.Add(docEntry);
				dlg.AddButton("Заменить", ResponseType.Ok);
				dlg.AddButton("Отмена", ResponseType.Cancel);
				dlg.DefaultResponse = ResponseType.Ok;
				dlg.ShowAll();
				if (dlg.Run() == (int)ResponseType.Ok)
				{
					incomeDoc.ObservableItems.ToList().ForEach(x => x.BuhDocument = docEntry.Text);
				}
				dlg.Destroy();
			}
		}

		protected void OnButtonAddSizesCliked(object sender, EventArgs e)
		{
			var item = ytreeItems.GetSelectedObject<IncomeItem>();
			Dictionary<string, List<string>> excludedSizeDictionary = null;
			if(item.Nomenclature.Type.WearCategory == СlothesType.Gloves ||
					 item.Nomenclature.Type.WearCategory == СlothesType.Headgear ||
					item.Nomenclature.Type.WearCategory == СlothesType.Shoes ||
					  item.Nomenclature.Type.WearCategory == СlothesType.WinterShoes) {

				excludedSizeDictionary = new Dictionary<string, List<string>>();
				excludedSizeDictionary.Add("", new List<string>(IncomeDoc.Items.Where(i => item != null && i.Nomenclature.Id == item.Nomenclature.Id).Select(i => i.Size)));
			}
			else if(item.Nomenclature.Type.WearCategory == СlothesType.Wear)
				excludedSizeDictionary = IncomeDoc.Items
							.Where(i => item != null && i.Nomenclature.Id == item.Nomenclature.Id)
							.GroupBy(x => x.WearGrowth)
							.Where(g => g.Key != null)
							.ToDictionary(g => g.Key, g => new List<string>(g.Select(x => x.Size)));
				if(item == null)
					return;
				if(item.Nomenclature == null)
					return;
				QS.Navigation.IPage<SizeWidgetViewModel> page = null;
				if(excludedSizeDictionary.Count > 0)
					page = MainClass.MainWin.NavigationManager.OpenViewModel<SizeWidgetViewModel, Nomenclature, Dictionary<string, List<string>>>
					(null, item.Nomenclature, excludedSizeDictionary);
				else
					page = MainClass.MainWin.NavigationManager.OpenViewModel<SizeWidgetViewModel, Nomenclature>
					(null, item.Nomenclature);
				page.ViewModel.AddedSizes += SelectWearSize_SizeSelected;

		}

		void SelectWearSize_SizeSelected(object sender , AddedSizesEventArgs e)
		{
			var item = ytreeItems.GetSelectedObject<IncomeItem>();
			if(item.Amount <= 0)
				IncomeDoc.RemoveItem(item);
			e.SizesWithAmount.ToList().ForEach(i => IncomeDoc.AddItem(e.Source, e.Growth, i.Key, i.Value));
			CalculateTotal();
		}
	}
}

