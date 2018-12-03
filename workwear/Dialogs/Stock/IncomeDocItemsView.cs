using System;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QSOrmProject;
using workwear.Domain.Operations;
using workwear.Domain.Stock;

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
				|| e.PropertyName == IncomeDoc.GetPropertyName (d => d.Facility))
			{
				buttonAdd.Sensitive = (IncomeDoc.Operation == IncomeOperations.Return && IncomeDoc.EmployeeCard != null) 
					|| (IncomeDoc.Operation == IncomeOperations.Object && IncomeDoc.Facility != null) 
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
				.AddColumn ("Размер").AddTextRenderer (e => e.Nomenclature.Size)
				.AddColumn ("Рост").AddTextRenderer (e => e.Nomenclature.WearGrowth)
				.AddColumn ("Состояние").AddNumericRenderer (e => e.LifePercent, new MultiplierToPercentConverter()).Editing (new Adjustment(0,0,100,1,10,0)).WidthChars(6)
				.AddTextRenderer (e => "%")
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(8)
				.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn ("Стоимость").AddNumericRenderer (e => e.Cost).Editing (new Adjustment(0,0,100000000,100,1000,0)).Digits (2).WidthChars(12)
				.AddColumn("Сумма").AddNumericRenderer(x => x.Total).Digits(2)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument).Editable()
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if(IncomeDoc.Operation == IncomeOperations.Return)
			{
				var selectFromEmployeeDlg = new ReferenceRepresentation (new ViewModel.EmployeeBalanceVM (IncomeDoc.EmployeeCard), 
				                                                         $"Выданное {IncomeDoc.EmployeeCard.ShortName}");
				selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
				selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;

				OpenSlaveTab(selectFromEmployeeDlg);
			}

			if(IncomeDoc.Operation == IncomeOperations.Object)
			{
				var selectFromObjectDlg = new ReferenceRepresentation (new ViewModel.ObjectBalanceVM (IncomeDoc.Facility),
				                                                       $"Выданное на {IncomeDoc.Facility.Name}");
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
				IncomeDoc.AddItem (MyOrmDialog.UoW.GetById<ExpenseItem> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.EmployeeBalanceVMNode> ())
			{
				IncomeDoc.AddItem (UoW, MyOrmDialog.UoW.GetById<EmployeeIssueOperation> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectNomenclatureDlg_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			e.GetEntities<Nomenclature>().ToList().ForEach(IncomeDoc.AddItem);
			CalculateTotal();
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			IncomeDoc.RemoveItem (ytreeItems.GetSelectedObject<IncomeItem> ());
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
	}
}

