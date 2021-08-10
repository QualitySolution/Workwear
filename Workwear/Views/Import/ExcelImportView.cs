using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using QS.Views.Dialog;
using workwear.Models.Import;
using workwear.ViewModels.Import;

namespace workwear.Views.Import
{
	public partial class ExcelImportView : DialogViewBase<ExcelImportViewModel>
	{
		List<yLabel> columnsLabels = new List<yLabel>();
		List<yEnumComboBox> columnsTypeCombos = new List<yEnumComboBox>();

		public ExcelImportView(ExcelImportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			#region Общее
			notebookSteps.ShowTabs = false;
			notebookSteps.Binding.AddBinding(viewModel, v => v.CurrentStep, w => w.CurrentPage).InitializeFromSource();

			treeviewRows.EnableGridLines = TreeViewGridLines.Both;
			treeviewRows.Binding.AddBinding(viewModel.ImportModel, v => v.DisplayRows, w => w.ItemsDataSource);

			viewModel.ImportModel.PropertyChanged += IImportModel_PropertyChanged;
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

			buttonLoad.Binding.AddBinding(viewModel, v => v.SensitiveSecondStepButton, w => w.Sensitive).InitializeFromSource();
			#endregion
			#region Шаг 2
			spinTitleRow.Binding.AddBinding(viewModel.ImportModel, v => v.HeaderRow, w => w.ValueAsInt).InitializeFromSource();
			buttonReadEmployees.Binding.AddBinding(viewModel, v => v.SensitiveThirdStepButton, w => w.Sensitive).InitializeFromSource();
			labelColumnRecomendations.LabelProp = ViewModel.ImportModel.DataColunmsRecomendations;
			#endregion
			#region Шаг 3
			var countersView = new CountersView(ViewModel.CountersViewModel);
			vboxCounters.PackStart(countersView, false, true, 2);
			countersView.Visible = true;

			eventboxLegendaNew.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfNew));
			eventboxLegendaChanged.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfChanged));
			eventboxLegendaNotFound.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfNotFound));
			eventboxLegendaError.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfError));
			eventboxLegendaSkipRows.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfSkiped));

			buttonSave.Binding.AddBinding(ViewModel, v => v.SensitiveSaveButton, w => w.Sensitive).InitializeFromSource();
			ViewModel.ProgressStep = progressTotal;
			#endregion
		}

		void IImportModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(IImportModel.DisplayColumns))
				RefreshTableColumns();
			if(e.PropertyName == nameof(IImportModel.MaxSourceColumns))
				RefreshColumnsWidgets();
		}

		protected void OnButtonLoadClicked(object sender, System.EventArgs e)
		{
			ViewModel.SecondStep();
		}

		private void RefreshTableColumns()
		{
			var config = ColumnsConfigFactory.Create<ISheetRow>();
			for(int i = 0; i < ViewModel.ImportModel.DisplayColumns.Count; i++) {
				int col = i;
				config.AddColumn(ViewModel.ImportModel.DisplayColumns[i].Title).HeaderAlignment(0.5f).Resizable()
					.AddTextRenderer(x => x.CellValue(col))
					.AddSetter((c, x) => c.Background = x.CellBackgroundColor(col));
			}
			config.AddColumn(String.Empty);

			treeviewRows.ColumnsConfig = config.Finish();
		}

		public void RefreshColumnsWidgets()
		{
			foreach(var label in columnsLabels) 
				tableColumns.Remove(label);
			foreach(var combo in columnsTypeCombos)
				tableColumns.Remove(combo);
			tableColumns.NRows = (uint)ViewModel.ImportModel.MaxSourceColumns;
			columnsLabels.Clear();
			columnsTypeCombos.Clear();
			uint nrow = 0;
			foreach(var column in ViewModel.ImportModel.DisplayColumns) {
				var label = new yLabel();
				label.Xalign = 1;
				label.Binding.AddBinding(column, nameof(IDataColumn.Title), w => w.LabelProp).InitializeFromSource();
				columnsLabels.Add(label);
				tableColumns.Attach(label, 0, 1, nrow, nrow + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
				var combo = new yEnumComboBox();
				combo.ItemsEnum = ViewModel.ImportModel.DataTypeEnum;
				combo.Binding.AddBinding(column, nameof(IDataColumn.DataType), w => w.SelectedItem).InitializeFromSource();
				columnsTypeCombos.Add(combo);
				tableColumns.Attach(combo, 1, 2, nrow, nrow + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
				nrow++;
			}
			tableColumns.ShowAll();
		}

		protected void OnButtonReadEmployeesClicked(object sender, EventArgs e)
		{
			ViewModel.ThirdStep();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			ViewModel.Save();
		}
	}
}
