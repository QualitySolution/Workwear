using System;
using QS.Views.Dialog;
using workwear.ViewModels.Import;

namespace workwear.Views.Import 
{
	public partial class IncomeImportView : DialogViewBase<IncomeImportViewModel>
	{
		public IncomeImportView(IncomeImportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ybuttonLoad.Clicked += OnLoadClick;
		}

		private void OnLoadClick(object sender, EventArgs e) {
			var param = new object[] { "Cancel", Gtk.ResponseType.Cancel, "Open", Gtk.ResponseType.Accept };
			var fileChooserDialog = new Gtk.FileChooserDialog("Open File", null, Gtk.FileChooserAction.Open, param);
			if(fileChooserDialog.Run() == (int)Gtk.ResponseType.Accept) {
				ViewModel.LoadDocument(fileChooserDialog.Filename);
			}
			fileChooserDialog.Destroy();
		}
	}
}
