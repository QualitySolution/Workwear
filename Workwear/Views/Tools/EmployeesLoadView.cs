using System;
using System.Collections.Generic;
using Gamma.Binding.Converters;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using QS.Views.Dialog;
using workwear.Models.Import;
using workwear.ViewModels.Tools;

namespace workwear.Views.Tools
{
	public partial class EmployeesLoadView : DialogViewBase<EmployeesLoadViewModel>
	{
		List<yLabel> columnsLabels = new List<yLabel>();
		List<yEnumComboBox> columnsTypeCombos = new List<yEnumComboBox>();

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

			buttonLoad.Binding.AddBinding(viewModel, v => v.SensetiveSecondStepButton, w => w.Sensitive).InitializeFromSource();
			#endregion
			#region Шаг 2
			spinTitleRow.Binding.AddBinding(viewModel, v => v.HeaderRow, w => w.ValueAsInt).InitializeFromSource();
			buttonReadEmployees.Binding.AddBinding(viewModel, v => v.SensetiveThirdStepButton, w => w.Sensitive).InitializeFromSource();
			#endregion
			#region Шаг 3
			var converter = new NumbersToStringConverter();
			labelCountNewEmployees.Binding.AddBinding(viewModel, v => v.CountNewEmployees, w => w.LabelProp, converter).InitializeFromSource();
			labelCountChangedEmployees.Binding.AddBinding(viewModel, v => v.CountChangedEmployees, w => w.LabelProp, converter).InitializeFromSource();
			labelCountNoChangesEmployees.Binding.AddBinding(viewModel, v => v.CountNoChangesEmployees, w => w.LabelProp, converter).InitializeFromSource();
			labelCountMultiMatch.Binding.AddBinding(viewModel, v => v.CountMultiMatch, w => w.LabelProp, converter).InitializeFromSource();
			labelCountSkipRows.Binding.AddBinding(viewModel, v => v.CountSkipRows, w => w.LabelProp, converter).InitializeFromSource();

			eventboxCountNewEmployees.ModifyBg(StateType.Normal, ColorUtil.Create(EmployeesLoadViewModel.ColorOfNew));
			eventboxCountChangedEmployees.ModifyBg(StateType.Normal, ColorUtil.Create(EmployeesLoadViewModel.ColorOfChanged));
			eventboxCountSkipRows.ModifyBg(StateType.Normal, ColorUtil.Create(EmployeesLoadViewModel.ColorOfSkiped));

			buttonSave.Binding.AddBinding(ViewModel, v => v.SensetiveSaveButton, w => w.Sensitive).InitializeFromSource();
			checkSaveNewEmployees.Binding.AddBinding(ViewModel, v => v.SaveNewEmployees, w => w.Active).InitializeFromSource();
			checkSaveChangedEmployees.Binding.AddBinding(ViewModel, v => v.SaveChangedEmployees, w => w.Active).InitializeFromSource();
			ViewModel.ProgressStep3 = progressStep3;
			#endregion
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Columns))
				RefreshTableColumns();
			if(e.PropertyName == nameof(ViewModel.MaxSourceColumns))
				RefreshColumnsWidgets();
		}


		protected void OnButtonLoadClicked(object sender, System.EventArgs e)
		{
			ViewModel.SecondStep();
		}

		private void RefreshTableColumns()
		{
			var config = ColumnsConfigFactory.Create<SheetRowEmployee>();
			for(int i = 0; i < ViewModel.Columns.Count; i++) {
				int col = i;
				config.AddColumn(ViewModel.Columns[i].Title).HeaderAlignment(0.5f).Resizable()
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
			tableColumns.NRows = (uint)ViewModel.MaxSourceColumns + 1;
			columnsLabels.Clear();
			columnsTypeCombos.Clear();
			uint nrow = 0;
			foreach(var column in ViewModel.Columns) {
				nrow++;
				var label = new yLabel();
				label.Xalign = 1;
				label.Binding.AddBinding(column, c => c.Title, w => w.LabelProp).InitializeFromSource();
				columnsLabels.Add(label);
				tableColumns.Attach(label, 0, 1, nrow, nrow + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
				var combo = new yEnumComboBox();
				combo.ItemsEnum = typeof(DataTypeEmployee);
				combo.Binding.AddBinding(column, c => c.DataType, w => w.SelectedItem).InitializeFromSource();
				columnsTypeCombos.Add(combo);
				tableColumns.Attach(combo, 1, 2, nrow, nrow + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
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
