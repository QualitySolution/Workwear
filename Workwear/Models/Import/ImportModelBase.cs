using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.ViewModels;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public abstract class ImportModelBase<TDataTypeEnum, TSheetRow> : PropertyChangedBase
		where TDataTypeEnum : Enum
		where TSheetRow : SheetRowBase<TDataTypeEnum>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Type DataTypeEnum => typeof(TDataTypeEnum);

		#region Типы данных
		public virtual bool CanMatch => HasRequiredDataTypes(Columns.Select(x => x.DataType));

		protected virtual bool HasRequiredDataTypes(IEnumerable<TDataTypeEnum> dataTypes) => RequiredDataTypes.All(dataTypes.Contains);

		protected abstract TDataTypeEnum[] RequiredDataTypes { get; }
		#endregion


		private readonly IDataParser<TDataTypeEnum> dataParser;

		protected ImportModelBase(IDataParser<TDataTypeEnum> dataParser, Type countersEnum, ViewModelBase matchSettingsViewModel = null)
		{
			this.dataParser = dataParser;
			this.MatchSettingsViewModel = matchSettingsViewModel;
			CountersViewModel = new CountersViewModel(countersEnum);
		}

		public ViewModelBase MatchSettingsViewModel { get; }
		
		public CountersViewModel CountersViewModel { get; }

		#region Колонки
		public List<ImportedColumn<TDataTypeEnum>> Columns = new List<ImportedColumn<TDataTypeEnum>>();

		public IList<IDataColumn> DisplayColumns => Columns.Cast<IDataColumn>().ToList();

		public IDataColumn AddColumn(int index)
		{
			var column = new ImportedColumn<TDataTypeEnum>(index);
			Columns.Add(column);
			return column;
		}

		private int maxSourceColumns;
		/// <summary>
		/// При установке значения будут пересозданы колонки
		/// </summary>
		public virtual int MaxSourceColumns {
			get => maxSourceColumns;
			set {
				maxSourceColumns = value;
				RecreateColumns(maxSourceColumns);
				OnPropertyChanged();
			}
		}

		private void RecreateColumns(int columnsCount)
		{
			Columns.Clear();
			for(int icol = 0; icol < columnsCount; icol++) {
				var column = new ImportedColumn<TDataTypeEnum>(icol);
				column.PropertyChanged += Column_PropertyChanged;
				Columns.Add(column);
			}
		}

		void Column_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ImportedColumn<TDataTypeEnum>.DataType))
				OnPropertyChanged(nameof(CanMatch));
		}

		private void RefreshColumnsTitle()
		{
			if(HeaderRow == 0) {
				for(int i = 0; i < MaxSourceColumns; i++)
					Columns[i].Title = CellReference.ConvertNumToColString(i);
			}
			else {
				var row = XlsRows[HeaderRow - 1];
				for(int i = 0; i < MaxSourceColumns; i++)
					Columns[i].Title = row.CellValue(i);
			}
			OnPropertyChanged(nameof(Columns));
			OnPropertyChanged(nameof(DisplayColumns));
		}

		public void AutoSetupColumns(IProgressBarDisplayable progress)
		{
			logger.Info("Ищем заголовочную строку...");
			progress.Start(XlsRows.Count, text:"Определение типов данных...");
			var bestMath = new TDataTypeEnum[MaxSourceColumns];
			int bestColumns = 0;
			int bestHeaderRow = 0;
			SheetRowBase<TDataTypeEnum> bestRow = null;
			int rowNum = 0;
			foreach(var row in XlsRows) {
				progress.Add();
				var types = new TDataTypeEnum[MaxSourceColumns];
				rowNum++;
				for(int i = 0; i < MaxSourceColumns; i++) {
					var value = row.CellStringValue(i)?.ToLower() ?? String.Empty;
					types[i] = dataParser.DetectDataType(value);
				}
				if(bestColumns < types.Where(x => !default(TDataTypeEnum).Equals(x)).Distinct().Count()) {
					bestMath = types;
					bestRow = row;
					bestColumns = types.Where(x => !default(TDataTypeEnum).Equals(x)).Distinct().Count();
					bestHeaderRow = rowNum;
					if(HasRequiredDataTypes(bestMath))
						break;	
				}
			}

			progress.Add();
			for(int i = 0; i < MaxSourceColumns; i++)
				Columns[i].DataType = bestMath[i];

			logger.Debug($"Найдено соответсвие в {bestColumns} заголовков в строке {bestHeaderRow}");
			HeaderRow = bestHeaderRow;
			progress.Close();
			logger.Info("Ок");
		}
		#endregion

		#region Строки

		private int headerRow;
		public int HeaderRow {
			get => headerRow;
			set {
				if(SetField(ref headerRow, value)) {
					RefreshColumnsTitle();
					OnPropertyChanged(nameof(DisplayRows));
				}
			}
		}

		public int SheetRowCount => XlsRows.Count;

		public List<TSheetRow> XlsRows = new List<TSheetRow>();

		public IEnumerable<TSheetRow> UsedRows => XlsRows.Skip(HeaderRow);

		public List<ISheetRow> DisplayRows => UsedRows.Cast<ISheetRow>().ToList();

		public void AddRow(IRow cells)
		{
			TSheetRow row = (TSheetRow)Activator.CreateInstance(typeof(TSheetRow), new object[] {cells });
			row.PropertyChanged += RowOnPropertyChanged;
			XlsRows.Add(row);
		}

		protected virtual void RowOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			
		}

		#endregion
		
		private ICell[,] mergedCells;
		public ICell[,] MergedCells {
			get => mergedCells;
			set {
				mergedCells = value;
				foreach (var row in XlsRows) {
					row.MergedCells = value;
				}
			}
		}
	}
}
