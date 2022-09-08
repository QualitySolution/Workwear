using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using QSWidgetLib;
using workwear.ViewModels.Import;

namespace workwear.Views.Import 
{
	public partial class IncomeImportView : DialogViewBase<IncomeImportViewModel>
	{
		public IncomeImportView(IncomeImportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			#region SubscribeToEvent

			ybuttonFileChoose.Clicked += OnFileChooseClick;
			ViewModel.DocumentLoaded += ViewModelOnDocumentLoaded;
			ybuttonDownload.Clicked += OnDownloadClicked;
			ybuttonCancel.Clicked += YButtonCancelOnClicked;
			ybuttonCreateIncome.Clicked += YButtonSaveOnClicked;
			ybuttonCreateNomenclature.Clicked += YButtonCreateNomenclatureOnClicked;
			ybuttonSelectAll.Clicked += YButtonSelectAllOnClicked;

			#endregion

			ybuttonFileChoose.Binding
				.AddBinding(ViewModel, wm => wm.SelectFileVisible, w => w.Visible)
				.InitializeFromSource();
			
			ybuttonDownload.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.DocumentDownloadSensitive, w => w.Sensitive)
				.AddBinding(vm => vm.SelectFileVisible, w=> w.Visible)
				.InitializeFromSource();
			
			ytreeview1.ColumnsConfig = ColumnsConfigFactory.Create<DocumentViewModel>()
				.AddColumn("Номер").AddNumericRenderer(c => c.Number)
				.AddColumn("Документ").AddTextRenderer(c => c.DocumentType)
				.AddColumn("Загружать?").AddToggleRenderer(c => c.WantDownload)
				.Finish();
			
			ytreeview1.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentsViewModels, w=> w.ItemsDataSource)
				.InitializeFromSource();

			ylistcomboboxDocuments.Binding
				.AddBinding(ViewModel, vm => vm.DocumentHasBeenUploaded, w=> w.Visible)
				.InitializeFromSource();
			
			ybuttonCreateIncome.Binding
				.AddBinding(ViewModel, vm => vm.DocumentHasBeenUploaded, w => w.Visible)
				.InitializeFromSource();
			
			ybuttonCreateNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.DocumentHasBeenUploaded, w => w.Visible)
				.InitializeFromSource();
			
			entityWarehouseIncome.Binding
				.AddBinding(ViewModel, wm => wm.WarehouseSelectVisible, w => w.Visible)
				.InitializeFromSource();

			entityWarehouseIncome.ViewModel = ViewModel.EntryWarehouseViewModel;
			
			ybuttonSelectAll.Binding
				.AddSource(ViewModel)
				.AddBinding( vm => vm.SelectAllVisible, w=> w.Visible)
				.InitializeFromSource();
			
			ycheckbuttonOpenAfterSave.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.OpenSelectDocumentAfterSave, w => w.Active)
				.AddBinding(vm => vm.DocumentHasBeenUploaded, w => w.Visible)
				.InitializeFromSource();
		}

		#region TreeViewBuild

		private void TreeViewDocumentItemsBuild() 
		{
			ytreeview1.ColumnsConfig = ColumnsConfigFactory.Create<DocumentItemViewModel>()
				.AddColumn("Номенклатура").AddTextRenderer(i => i.NomenclatureName)
				.AddSetter(
					(c, n) => c.Foreground = ColorState(n.NomenclatureWarning))
				.AddColumn("Номенклатурный номер").AddTextRenderer(i => i.Article)
				.AddColumn("Размер").AddTextRenderer(i => i.Size)
					.AddSetter(
						(c, n) => c.Foreground = ColorState(n.SizeWarning))
				.AddColumn("Рост").AddTextRenderer(i => i.Height)
					.AddSetter(
						(c, n) => c.Foreground = ColorState(n.HeightWarning))
				.AddColumn("Количество").AddNumericRenderer(i => i.Amount)
					.Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(8)
				.AddColumn("Стоимость").AddNumericRenderer(i => i.Cost)
					.Editing (new Adjustment(0,0,100000000,100,1000,0)).Digits (2).WidthChars(12)
					.AddSetter((c, n) => c.Foreground = ColorState(n.CostWarning))
				.Finish();
			
			ytreeview1.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.SelectDocumentItemViewModels, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeview1.YTreeModel.EmitModelChanged();
			ytreeview1.ButtonReleaseEvent += TreeView1OnButtonReleaseEvent;
		}

		#endregion

		#region ViewEvents

		private void OnDownloadClicked(object sender, EventArgs e) {
			ViewModel.ParseDocuments();
			TreeViewDocumentItemsBuild();
			
			ylistcomboboxDocuments.SetRenderTextFunc<DocumentViewModel>(vm => vm.Title);
			ylistcomboboxDocuments.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentsViewModels, w => w.ItemsList)
				.AddBinding(vm => vm.SelectDocumentViewModel, w => w.SelectedItem)
				.InitializeFromSource();
		}

		private void ViewModelOnDocumentLoaded() => ytreeview1.YTreeModel.EmitModelChanged();
		private void YButtonCancelOnClicked(object sender, EventArgs e) => ViewModel.Cancel();
		private void YButtonSaveOnClicked(object sender, EventArgs e) => ViewModel.CreateIncome();
		private void YButtonCreateNomenclatureOnClicked(object sender, EventArgs e) => ViewModel.CreateNomenclature();
		private void YButtonSelectAllOnClicked(object sender, EventArgs e) {
			ViewModel.SelectAll();
			ytreeview1.YTreeModel.EmitModelChanged();
		}

		#endregion

		#region FileChooserDialog

		private void OnFileChooseClick(object sender, EventArgs e) {
			var fileChooserDialog = new FileChooserDialog("Open File", null, FileChooserAction.Open, 
				"Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
			if(fileChooserDialog.Run() == (int)ResponseType.Accept) 
				ViewModel.LoadDocument(fileChooserDialog.Filename);
			fileChooserDialog.Destroy();
		}
		private static string ColorState(bool state) => state ? "red" : null;

		#endregion

		#region Popup Menu

		private void TreeView1OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeview1.GetSelectedObject<DocumentItemViewModel>();
				
				var itemSetSize = new MenuItemId<DocumentItemViewModel>("Поставить размер");
				itemSetSize.ID = selected;
				itemSetSize.Sensitive = selected?.Item.Nomenclature?.Type.SizeType != null;
				itemSetSize.Activated += (sender, e) => 
					ViewModel.AddSize(((MenuItemId<DocumentItemViewModel>)sender).ID);
				menu.Add(itemSetSize);
				
				var itemSetHeight = new MenuItemId<DocumentItemViewModel>("Поставить рост");
				itemSetHeight.ID = selected;
				itemSetHeight.Sensitive = selected?.Item.Nomenclature?.Type.HeightType != null;
				itemSetHeight.Activated += (sender, e) => 
					ViewModel.AddHeight(((MenuItemId<DocumentItemViewModel>)sender).ID);
				menu.Add(itemSetHeight);
				
				menu.ShowAll();
				menu.Popup();
			}
		}

		#endregion
	}
}
