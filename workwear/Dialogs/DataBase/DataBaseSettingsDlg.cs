using System;
using QSProjectsLib;
using QSSupportLib;
using QSTDI;
using workwear.Tools;

namespace workwear.Dialogs.DataBase
{
	public partial class DataBaseSettingsDlg : TdiTabBase
	{

		public DataBaseSettingsDlg()
		{
			this.Build();

			TabName = "Настройки учёта";

			ycheckAutoWriteoff.Active = BaseParameters.DefaultAutoWriteoff;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			MainSupport.BaseParameters.UpdateParameter(QSMain.ConnectionDB,
				BaseParameterNames.DefaultAutoWriteoff.ToString(),
				ycheckAutoWriteoff.Active.ToString());
			OnCloseTab(false);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}
