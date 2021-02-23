using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using workwear.Tools.Import;
using workwear.ViewModels.Tools;

namespace workwear.Views.Tools
{
	public partial class EmployeesLoadView : DialogViewBase<EmployeesLoadViewModel>
	{
		public EmployeesLoadView(EmployeesLoadViewModel viewModel) : base(viewModel)
		{
			this.Build();

			#region Общее
			notebookSteps.ShowTabs = false;
			notebookSteps.Binding.AddBinding(viewModel, v => v.CurrentStep, w => w.CurrentPage).InitializeFromSource();

			treeviewRows.EnableGridLines = TreeViewGridLines.Both;
			treeviewRows.Binding.AddBinding(viewModel, v => v.DisplayRows, w => w.ItemsDataSource);

			viewModel.PropertyChanged += ViewModel_PropertyChanged;
			#endregion

			#region Шаг 1
			filechooser.Binding.AddBinding(viewModel, v => v.FileName, w => w.Filename);

			FileFilter Filter = new FileFilter();
			Filter.AddPattern("*.xls");
			Filter.AddPattern("*.xlsx");
			Filter.Name = "Excel";
			filechooser.AddFilter(Filter);

			comboSheets.SetRenderTextFunc<ImportedSheet>(x => x.Title);
			comboSheets.Binding.AddSource(viewModel)
				.AddBinding(v => v.Sheets, w => w.ItemsList)
				.AddBinding(v => v.SelectedSheet, w => w.SelectedItem);

			buttonLoad.Binding.AddBinding(viewModel, v => v.SensetiveLoadButton, w => w.Sensitive).InitializeFromSource();
			#endregion
			#region Шаг 2
			spinTitleRow.Binding.AddBinding(viewModel, v => v.HeaderRow, w => w.ValueAsInt).InitializeFromSource();
			#endregion
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Columns))
				RefreshColumns();
		}


		protected void OnButtonLoadClicked(object sender, System.EventArgs e)
		{
			ViewModel.SecondStep();
		}

		private void RefreshColumns()
		{
			var config = ColumnsConfigFactory.Create<SheetRow>();
			for(int i = 0; i < ViewModel.Columns.Count; i++) {
				int col = i;
				config.AddColumn(ViewModel.Columns[i].Title).HeaderAlignment(0.5f).Resizable()
					.AddTextRenderer(x => x.CellValue(col));
			}
			config.AddColumn(String.Empty);

			treeviewRows.ColumnsConfig = config.Finish();
		}
	}
}
