using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Views.Dialog;
using QS.Views.Resolve;
using QS.Widgets.GtkUI;
using QSWidgetLib;
using Workwear.Models.Import;
using Workwear.ViewModels.Import;

namespace Workwear.Views.Import {
	public partial class ExcelImportView : DialogViewBase<ExcelImportViewModel>
	{
		List<yLabel> columnsLabels = new List<yLabel>();
		List<SpecialListComboBox> columnsTypeCombos = new List<SpecialListComboBox>();

		public ExcelImportView(ExcelImportViewModel viewModel, IGtkViewResolver viewResolver) : base(viewModel)
		{
			Build();

			#region Общее
			notebookSteps.ShowTabs = false;
			notebookSteps.Binding
				.AddBinding(viewModel, v => v.CurrentStep, w => w.CurrentPage)
				.InitializeFromSource();

			treeviewRows.EnableGridLines = TreeViewGridLines.Both;
			treeviewRows.Selection.Mode = SelectionMode.Multiple;
			treeviewRows.Selection.Changed += Selection_Changed;
			treeviewRows.Binding.AddBinding(viewModel.ImportModel, v => v.DisplayRows, w => w.ItemsDataSource);
			treeviewRows.ButtonReleaseEvent += TreeviewRows_ButtonReleaseEvent;

			viewModel.ImportModel.PropertyChanged += IImportModel_PropertyChanged;
			#endregion

			#region Шаг 1
			filechooser.Binding.AddBinding(viewModel, v => v.FileName, w => w.Filename);

			var Filter = new FileFilter();
			Filter.AddPattern("*.xls");
			Filter.AddPattern("*.xlsx");
			Filter.AddMimeType("application/vnd.ms-excel");
			Filter.AddMimeType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			Filter.Name = "Excel";
			filechooser.AddFilter(Filter);

			comboSheets.SetRenderTextFunc<ImportedSheet>(x => x.Title);
			comboSheets.Binding.AddSource(viewModel)
				.AddBinding(v => v.Sheets, w => w.ItemsList)
				.AddBinding(v => v.SelectedSheet, w => w.SelectedItem);

			buttonLoad.Binding
				.AddBinding(viewModel, v => v.SensitiveSecondStepButton, w => w.Sensitive)
				.InitializeFromSource();
			#endregion
			#region Шаг 2
			spinTitleRow.Binding
				.AddBinding(ViewModel.ImportModel, v => v.HeaderRow, w => w.ValueAsInt)
				.AddBinding(ViewModel, v => v.RowsCount, w => w.Adjustment.Upper)
				.InitializeFromSource();
			buttonReadEmployees.Binding
				.AddBinding(viewModel, v => v.SensitiveThirdStepButton, w => w.Sensitive)
				.InitializeFromSource();
			labelColumnRecomendations.LabelProp = ViewModel.ImportModel.DataColumnsRecommendations;
			if(viewModel.ImportModel.MatchSettingsViewModel != null) {
				var settingsView = viewResolver.Resolve(viewModel.ImportModel.MatchSettingsViewModel);
				tableMatchSettings.Attach(settingsView, 0, 2, 1, 2);
				settingsView.Show();
			}
			#endregion
			#region Шаг 3
			var countersView = new CountersView(ViewModel.CountersViewModel);
			vboxCounters.PackStart(countersView, false, true, 2);
			countersView.Visible = true;

			eventboxLegendaNew.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfNew));
			eventboxLegendaChanged.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfChanged));
			eventboxLegendaNotFound.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfNotFound));
			eventboxLegendaAmbiguous.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfAmbiguous));
			eventboxLegendaDublicate.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfDuplicate));
			eventboxLegendaError.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfError));
			eventboxLegendaSkipRows.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfSkipped));
			labelLegendaWarning.ModifyFg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfWarning));

			buttonSave.Binding
				.AddBinding(ViewModel, v => v.SensitiveSaveButton, w => w.Sensitive)
				.InitializeFromSource();

			hboxRowActions.Binding.AddBinding(ViewModel, v => v.RowActionsShow, w => w.Visible).InitializeFromSource();

			buttonIgnore.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.ButtonIgnoreSensitive, w => w.Sensitive)
				.AddBinding(v => v.ButtonIgnoreTitle, w => w.Label)
				.InitializeFromSource();
			ViewModel.ProgressStep = progressTotal;
			#endregion
		}

		void IImportModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
				case nameof(IImportModel.Columns):
					RefreshTableColumns();
					break;
				case nameof(IImportModel.ColumnsCount):
					RefreshColumnsWidgets();
					break;
			}
		}

		private void RefreshTableColumns() {
			var config = ColumnsConfigFactory.Create<ISheetRow>();
			for(var i = 0; i < ViewModel.ImportModel.Columns.Count; i++) {
				var col = i;
				config.AddColumn(ViewModel.ImportModel.Columns[i].Title).HeaderAlignment(0.5f).Resizable()
					.ToolTipText(x => x.CellTooltip(col))
					.AddTextRenderer(x => x.CellValue(col))
					.AddSetter((c, x) => c.Foreground = x.CellForegroundColor(col))
					.AddSetter((c, x) => c.Background = x.CellBackgroundColor(col));
			}
			config.AddColumn(String.Empty);

			treeviewRows.ColumnsConfig = config.Finish();
		}

		public void RefreshColumnsWidgets() {
			foreach(var label in columnsLabels) 
				tableColumns.Remove(label);
			foreach(var combo in columnsTypeCombos)
				tableColumns.Remove(combo);
			tableColumns.NRows = (uint)ViewModel.ImportModel.ColumnsCount + 1;
			tableColumns.NColumns = (uint)ViewModel.ImportModel.LevelsCount + 1;
			columnsLabels.Clear();
			columnsTypeCombos.Clear();
			uint nRow = 1;
			if(ViewModel.ImportModel.LevelsCount > 1) {
				for(uint col = 1; col <= ViewModel.ImportModel.LevelsCount; col++) {
					var label = new Label($"Уровень {col}");
					tableColumns
						.Attach(label, col, col + 1, 0, 1, 
							AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
				}
			}
			foreach(var column in ViewModel.ImportModel.Columns) {
				var label = new yLabel();
				label.Xalign = 1;
				label.Binding
					.AddBinding(column, v => v.Title, w => w.LabelProp)
					.InitializeFromSource();
				columnsLabels.Add(label);
				tableColumns
					.Attach(label, 0, 1, nRow, nRow + 1, 
						AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
				for(uint col = 0; col < ViewModel.ImportModel.LevelsCount; col++) {
					var groupLevel = column.DataTypeByLevels[col];
					var combo = new SpecialListComboBox {ItemsList = ViewModel.ImportModel.DataTypes};
					combo.SetRenderTextFunc<DataType>(x => x.Title);
					combo.Binding
						.AddBinding(groupLevel, c => c.DataType, w => w.SelectedItem)
						.InitializeFromSource();
					columnsTypeCombos.Add(combo);
					tableColumns
						.Attach(combo, col + 1, col + 2, nRow, nRow + 1, 
							AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
				}
				nRow++;
			}
			tableColumns.ShowAll();
		}

		void Selection_Changed(object sender, EventArgs e) {
			ViewModel.SelectionChanged(treeviewRows.GetSelectedObjects<ISheetRow>());
		}

		protected void OnButtonIgnoreClicked(object sender, EventArgs e) {
			ViewModel.ToggleIgnoreRows(treeviewRows.GetSelectedObjects<ISheetRow>());
		}

		#region StepButtons
		protected void OnButtonLoadClicked(object sender, EventArgs e)
		{
			ViewModel.SecondStep();
		}

		protected void OnButtonBackToSelectSheetClicked(object sender, EventArgs e)
		{
			ViewModel.BackToFirstStep();
		}

		protected void OnButtonReadEmployeesClicked(object sender, EventArgs e) {
			ViewModel.ThirdStep();
		}

		protected void OnButtonBackToDataTypesClicked(object sender, EventArgs e)
		{
			ViewModel.BackToSecondStep();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e) {
			ViewModel.Save();
		}
		#endregion
		#region PopupMenu
		void TreeviewRows_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if (args.Event.Button != 3 || ViewModel.CurrentStep != 2) return;
			var menu = new Menu();
			var selected = treeviewRows.GetSelectedObjects<ISheetRow>();
			var item = new MenuItemId<ISheetRow[]>(ViewModel.ButtonIgnoreTitle);
			item.ID = selected;
			item.Activated += Item_Activated;
			menu.Add(item);
			menu.ShowAll();
			menu.Popup();
			args.RetVal = true;
		}

		void Item_Activated(object sender, EventArgs e) {
			var item = (MenuItemId<ISheetRow[]>)sender;
			ViewModel.ToggleIgnoreRows(item.ID);
		}
		#endregion
	}
}
