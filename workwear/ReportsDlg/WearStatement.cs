using System;
using QSProjectsLib;

namespace workwear
{
	public partial class WearStatement : Gtk.Dialog
	{
		public WearStatement()
		{
			this.Build();
			ComboWorks.ComboFillReference(comboObject, "objects", ComboWorks.ListMode.OnlyItems);
			comboObject.Active = 0;
		}

		protected void OnButtonOkClicked(object sender, EventArgs e)
		{
			string param = String.Format("id={0}", ComboWorks.GetActiveId(comboObject)) ;
			ViewReportExt.Run("WearStatement", param);
		}
	}
}

