﻿using System;
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

			#region SubscribeToEvent

			ybuttonFileChoose.Clicked += OnFileChooseClick;
			ViewModel.DocumentLoaded += ViewModelOnDocumentLoaded;
			ybuttonDownload.Clicked += OnDownloadClicked;
			ybuttonCancel.Clicked += YButtonCancelOnClicked;
			ybuttonCreateIncome.Clicked += YButtonSaveOnClicked;
			ybuttonCreateNomenclature.Clicked += YButtonCreateNomenclatureOnClicked;

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
		}

		#region TreeViewBuild

		private void TreeViewDocumentItemsBuild() 
		{
			ytreeview1.ColumnsConfig = ColumnsConfigFactory.Create<DocumentItemViewModel>()
				.AddColumn("Номенклатура").AddTextRenderer(i => i.Nomenclature)
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
				.AddColumn("Стоимость").AddNumericRenderer(i => i.Cost)
					.AddSetter((c, n) => c.Foreground = ColorState(n.CostWarning))
				.Finish();
			
			ytreeview1.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.SelectDocumentItems, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeview1.YTreeModel.EmitModelChanged();
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
				.AddBinding(vm => vm.SelectDocument, w => w.SelectedItem)
				.InitializeFromSource();
		}

		private void ViewModelOnDocumentLoaded() {
			ytreeview1.YTreeModel.EmitModelChanged();
		}
		
		private void YButtonCancelOnClicked(object sender, EventArgs e) => ViewModel.Cancel();
		private void YButtonSaveOnClicked(object sender, EventArgs e) => ViewModel.CreateIncome();
		private void YButtonCreateNomenclatureOnClicked(object sender, EventArgs e) => ViewModel.CreateNomenclature();

		#endregion

		#region FileChooserDialog

		private void OnFileChooseClick(object sender, EventArgs e) {
			var param = new object[] { "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept };
			var fileChooserDialog = new FileChooserDialog("Open File", null, FileChooserAction.Open, param);
			if(fileChooserDialog.Run() == (int)ResponseType.Accept) {
				ViewModel.LoadDocument(fileChooserDialog.Filename);
			}
			fileChooserDialog.Destroy();
		}

		private static string ColorState(bool state) => state ? "red" : null;

		#endregion
	}
}