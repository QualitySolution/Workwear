using System;
using Gamma.GtkWidgets;
using Gtk;
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
			ViewModel.documentLoaded += ViewModelOnDocumentLoaded;
			ybuttonParse.Clicked += YButtonParseOnClicked;
			
			ylistcomboboxDocuments.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Documents, w => w.ItemsList)
				.AddBinding(vm => vm.DocumentListVisible, w=> w.Visible)
				.InitializeFromSource();
			
			ybuttonLoad.Binding
				.AddBinding(ViewModel, wm => wm.SelectFileVisible, w => w.Visible)
				.InitializeFromSource();
		}

		#region TreeViewBuild

		private void TreeViewDocumentBuild() 
		{
			ytreeview1.ColumnsConfig = ColumnsConfigFactory.Create<DocumentViewModel>()
				.AddColumn("Номер").AddNumericRenderer(c => c.Number)
				.AddColumn("Документ").AddTextRenderer(c => c.Title)
				.AddColumn("Загружать?").AddToggleRenderer(c => c.WantDownload)
				.Finish();
			ytreeview1.Binding
				.AddBinding(ViewModel, vm => vm.Documents, w=> w.ItemsDataSource)
				.InitializeFromSource();
			ytreeview1.YTreeModel.EmitModelChanged();
		}

		private void TreeViewDocumentItemsBuild() {
			ytreeview1.ColumnsConfig = ColumnsConfigFactory.Create<DocumentItemViewModel>()
				.AddColumn("Документ").AddTextRenderer(c => c.Title)
				.Finish();
			ytreeview1.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.SelectDocumentItems, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeview1.YTreeModel.EmitModelChanged();
		}
		
		#endregion

		#region ViewEvents

		private void YButtonParseOnClicked(object sender, EventArgs e) {
			ViewModel.ParseDocuments();
			TreeViewDocumentItemsBuild();
		}

		private void ViewModelOnDocumentLoaded() {
			TreeViewDocumentBuild();
		}

		#endregion

		#region FileChooserDialog

		private void OnLoadClick(object sender, EventArgs e) {
			var param = new object[] { "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept };
			var fileChooserDialog = new FileChooserDialog("Open File", null, FileChooserAction.Open, param);
			if(fileChooserDialog.Run() == (int)ResponseType.Accept) {
				ViewModel.LoadDocument(fileChooserDialog.Filename);
			}
			fileChooserDialog.Destroy();
		}

		#endregion
	}
}
