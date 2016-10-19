using System;
using Gamma.Utilities;
using Gtk;
using NLog;
using QSOrmProject;
using workwear.Domain.Stock;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeDocItemsView : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private Income incomeDoc;

		public Income IncomeDoc {
			get {return incomeDoc;}
			set {
				if (incomeDoc == value)
					return;
				incomeDoc = value;
				ytreeItems.ItemsDataSource = incomeDoc.ObservableItems;
				IncomeDoc.PropertyChanged += IncomeDoc_PropertyChanged;
				IncomeDoc_PropertyChanged (null, new System.ComponentModel.PropertyChangedEventArgs(IncomeDoc.GetPropertyName (d => d.Operation)));
			}
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
		}

		public IncomeDocItemsView()
		{
			this.Build();

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<IncomeItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn ("Размер").AddTextRenderer (e => e.Nomenclature.Size)
				.AddColumn ("Рост").AddTextRenderer (e => e.Nomenclature.WearGrowth)
				.AddColumn ("Состояние").AddNumericRenderer (e => e.LifePercent, new MultiplierToPercentConverter()).Editing (new Adjustment(0,0,100,1,10,0))
				.AddTextRenderer (e => "%")
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1))
				.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn ("Стоимость").AddNumericRenderer (e => e.Cost).Editing (new Adjustment(0,0,100000000,100,1000,0)).Digits (2)
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
				var selectFromEmployeeDlg = new ReferenceRepresentation (new ViewModel.EmployeeBalanceVM (IncomeDoc.EmployeeCard));
				selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
				selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;

				var dialog = new OneWidgetDialog (selectFromEmployeeDlg);
				dialog.Show ();
				dialog.Run ();
				dialog.Destroy ();
			}

			if(IncomeDoc.Operation == IncomeOperations.Object)
			{
				var selectFromObjectDlg = new ReferenceRepresentation (new ViewModel.ObjectBalanceVM (IncomeDoc.Facility));
				selectFromObjectDlg.Mode = OrmReferenceMode.MultiSelect;
				selectFromObjectDlg.ObjectSelected += SelectFromObjectDlg_ObjectSelected;;

				var dialog = new OneWidgetDialog (selectFromObjectDlg);
				dialog.Show ();
				dialog.Run ();
				dialog.Destroy ();
			}

			if(IncomeDoc.Operation == IncomeOperations.Enter)
			{
				var selectNomenclatureDlg = new OrmReference (typeof(Nomenclature));
				selectNomenclatureDlg.Mode = OrmReferenceMode.Select;
				selectNomenclatureDlg.ObjectSelected += SelectNomenclatureDlg_ObjectSelected;

				var dialog = new OneWidgetDialog (selectNomenclatureDlg);
				dialog.Show ();
				dialog.Run ();
				dialog.Destroy ();
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
				IncomeDoc.AddItem (MyOrmDialog.UoW.GetById<ExpenseItem> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectNomenclatureDlg_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			IncomeDoc.AddItem (e.Subject as Nomenclature);
			CalculateTotal();
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			IncomeDoc.RemoveItem (ytreeItems.GetSelectedObject<IncomeItem> ());
			CalculateTotal();
		}

		private void CalculateTotal()
		{
			labelSum.Text = String.Format ("Количество: {0}", IncomeDoc.Items.Count);
		} 
	}
}

