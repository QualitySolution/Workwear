using System;
using QS.Dialog.Gtk;
using QS.Tdi;
using QSProjectsLib;
using QSSupportLib;
using workwear.Tools;

namespace workwear.Dialogs.DataBase
{
	public partial class DataBaseSettingsDlg : TdiTabBase, ITdiDialog
	{
		public bool HasChanges => ycheckAutoWriteoff.Active != oldYcheckAutoWriteoff || checkEmployeeSizeRanges.Active != oldYcheckAutoWriteoff
			|| !spbutAheadOfShedule.Value.Equals(oldSpbutAheadOfShedule);

		bool oldYcheckAutoWriteoff, oldCheckEmployeeSizeRanges;
		int oldSpbutAheadOfShedule;

		public DataBaseSettingsDlg()
		{
			this.Build();

			TabName = "Настройки учёта";

			ycheckAutoWriteoff.Active = oldYcheckAutoWriteoff = BaseParameters.DefaultAutoWriteoff;
			checkEmployeeSizeRanges.Active = oldCheckEmployeeSizeRanges = BaseParameters.EmployeeSizeRanges;
			spbutAheadOfShedule.Value = oldSpbutAheadOfShedule = BaseParameters.ColDayAheadOfShedule;
		}

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			SaveAndClose();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(HasChanges);
		}

		public bool Save()
		{
			MainSupport.BaseParameters.UpdateParameter(QSMain.ConnectionDB,
				BaseParameterNames.DefaultAutoWriteoff.ToString(),
				ycheckAutoWriteoff.Active.ToString());

			MainSupport.BaseParameters.UpdateParameter(QSMain.ConnectionDB,
				BaseParameterNames.EmployeeSizeRanges.ToString(),
				checkEmployeeSizeRanges.Active.ToString());

			MainSupport.BaseParameters.UpdateParameter(QSMain.ConnectionDB,
				BaseParameterNames.ColDayAheadOfShedule.ToString(), spbutAheadOfShedule.Text);

			return true;

		}

		public void SaveAndClose()
		{
			Save();
			OnCloseTab(false);
		}
	}
}
