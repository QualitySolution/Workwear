using System;
using Gtk;
using QSTelemetry;
using workwear.ViewModel;

public partial class MainWindow : Gtk.Window
{

	void PrepareCards ()
	{
		treeviewEmployees.RepresentationModel = new EmployeesVM();
		treeviewEmployees.Selection.Changed += TreeviewEmployees_Selection_Changed;
	}

	void TreeviewEmployees_Selection_Changed (object sender, EventArgs e)
	{
		bool isSelect = treeviewEmployees.Selection.CountSelectedRows () == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	void UpdateCards ()
	{
		MainTelemetry.AddCount("RefreshEmployeeCard");
        logger.Info ("Обновляем таблицу Карточек...");

		treeviewEmployees.RepresentationModel.UpdateNodes();

		logger.Info ("Ok");
	}

	protected void OnButtonCardsSearchClearClicked (object sender, EventArgs e)
	{
		entryCardsSearch.Text = "";
	}

	protected void OnEntryCardsSearchChanged (object sender, EventArgs e)
	{
		treeviewEmployees.RepresentationModel.SearchString = entryCardsSearch.Text;
	}

	protected void OnCheckOnlyActualClicked (object sender, EventArgs e)
	{
		(treeviewEmployees.RepresentationModel as EmployeesVM).OnlyWorking = checkCardsOnlyActual.Active;
	}

	protected void OnTreeviewEmployeesRowActivated(object o, RowActivatedArgs args)
	{
		buttonEdit.Click ();
	}
}

