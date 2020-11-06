using System;
using QS.Dialog.Gtk;
using QS.Tdi;
using QSProjectsLib;
using workwear.Tools;

namespace workwear.Dialogs.DataBase
{
	public partial class DataBaseSettingsDlg : TdiTabBase, ITdiDialog
	{
		public bool HasChanges => ycheckAutoWriteoff.Active != oldYcheckAutoWriteoff || checkEmployeeSizeRanges.Active != oldYcheckAutoWriteoff
			|| !spbutAheadOfShedule.Value.Equals(oldSpbutAheadOfShedule);

		bool oldYcheckAutoWriteoff, oldCheckEmployeeSizeRanges;
		int oldSpbutAheadOfShedule;
		private readonly BaseParameters baseParameters;

		public DataBaseSettingsDlg(BaseParameters baseParameters)
		{
			this.Build();

			TabName = "Настройки учёта";
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));

			ycheckAutoWriteoff.Active = oldYcheckAutoWriteoff = baseParameters.DefaultAutoWriteoff;
			checkEmployeeSizeRanges.Active = oldCheckEmployeeSizeRanges = baseParameters.EmployeeSizeRanges;
			spbutAheadOfShedule.Value = oldSpbutAheadOfShedule = baseParameters.ColDayAheadOfShedule;
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
			baseParameters.DefaultAutoWriteoff = ycheckAutoWriteoff.Active;

			baseParameters.EmployeeSizeRanges = checkEmployeeSizeRanges.Active;

			baseParameters.ColDayAheadOfShedule = spbutAheadOfShedule.ValueAsInt;
			return true;
		}

		public void SaveAndClose()
		{
			Save();
			OnCloseTab(false);
		}
	}
}
