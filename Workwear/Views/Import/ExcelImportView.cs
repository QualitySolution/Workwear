using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using QS.Views.Dialog;
using QS.Views.Resolve;
using QSWidgetLib;
using workwear.Models.Import;
using workwear.ViewModels.Import;

namespace workwear.Views.Import
{
	public partial class ExcelImportView : DialogViewBase<ExcelImportViewModel>
	{
		List<yLabel> columnsLabels = new List<yLabel>();
		List<yEnumComboBox> columnsTypeCombos = new List<yEnumComboBox>();

		public ExcelImportView(ExcelImportViewModel viewModel, IGtkViewResolver viewResolver) : base(viewModel)
		{
			this.Build();

			#region Общее
			notebookSteps.ShowTabs = false;
			notebookSteps.Binding.AddBinding(viewModel, v => v.CurrentStep, w => w.CurrentPage).InitializeFromSource();

			treeviewRows.EnableGridLines = TreeViewGridLines.Both;
			treeviewRows.Binding.AddBinding(viewModel.ImportModel, v => v.DisplayRows, w => w.ItemsDataSource);
			treeviewRows.ButtonReleaseEvent += TreeviewRows_ButtonReleaseEvent;

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
			spinTitleRow.Binding
				.AddBinding(ViewModel.ImportModel, v => v.HeaderRow, w => w.ValueAsInt)
				.AddBinding(ViewModel, v => v.RowsCount, w => w.Adjustment.Upper)
				.InitializeFromSource();
			buttonReadEmployees.Binding.AddBinding(viewModel, v => v.SensitiveThirdStepButton, w => w.Sensitive).InitializeFromSource();
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
			eventboxLegendaError.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfError));
			eventboxLegendaSkipRows.ModifyBg(StateType.Normal, ColorUtil.Create(ExcelImportViewModel.ColorOfSkipped));

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
					.ToolTipText(x => x.CellTooltip(col))
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
				if(ViewModel.ImportModel.HidedDataTypeEnumItems != null)
					combo.HiddenItems = ViewModel.ImportModel.HidedDataTypeEnumItems.ToArray();
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

		#region PopupMenu
		void TreeviewRows_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3 && ViewModel.CurrentStep == 2) {
				var menu = new Menu();
				var selected = treeviewRows.GetSelectedObject<ISheetRow>();
				var item = new MenuItemId<ISheetRow>(selected.UserSkipped ? "Загружать" : "Не загружать");
				item.ID = selected;
				item.Activated += Item_Activated;
				menu.Add(item);
				menu.ShowAll();
				menu.Popup();
			}
		}

		void Item_Activated(object sender, EventArgs e)
		{
			var item = (MenuItemId<ISheetRow>)sender;
			item.ID.UserSkipped = !item.ID.UserSkipped;
		}

		#endregion
	}
}
