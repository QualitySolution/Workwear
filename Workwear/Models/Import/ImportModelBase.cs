using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using QS.DomainModel.Entity;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public abstract class ImportModelBase<TDataTypeEnum, TSheetRow> : PropertyChangedBase
		where TDataTypeEnum : Enum
		where TSheetRow : SheetRowBase<TDataTypeEnum>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Type DataTypeEnum => typeof(TDataTypeEnum);

		public abstract bool CanMatch { get; }

		private readonly IDataParser<TDataTypeEnum> dataParser;

		protected ImportModelBase(IDataParser<TDataTypeEnum> dataParser)
		{
			this.dataParser = dataParser;
		}

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
		/// При установке зачения будут пересозданы колонки
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

		public void AutoSetupColumns()
		{
			logger.Info("Ищем заголовочную строку...");

			var bestMath = new TDataTypeEnum[MaxSourceColumns];
			int bestColumns = 0;
			int bestHeaderRow = 0;
			SheetRowBase<TDataTypeEnum> bestRow = null;
			int rowNum = 0;
			foreach(var row in XlsRows) {
				var types = new TDataTypeEnum[MaxSourceColumns];
				rowNum++;
				for(int i = 0; i < MaxSourceColumns; i++) {
					var value = row.CellValue(i)?.ToLower() ?? String.Empty;
					types[i] = dataParser.DetectDataType(value);
				}
				if(bestColumns < types.Count(x => !default(TDataTypeEnum).Equals(x))) {
					bestMath = types;
					bestRow = row;
					bestColumns = types.Count(x => !default(TDataTypeEnum).Equals(x));
					bestHeaderRow = rowNum;
				}
				if(bestColumns >= 3)
					break;
			}

			for(int i = 0; i < MaxSourceColumns; i++)
				Columns[i].DataType = bestMath[i];

			logger.Debug($"Найдено соответсвие в {bestColumns} заголовков в строке {bestHeaderRow}");
			HeaderRow = bestHeaderRow;
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

		public List<TSheetRow> XlsRows = new List<TSheetRow>();

		public IEnumerable<TSheetRow> UsedRows => XlsRows.Skip(HeaderRow);

		public List<ISheetRow> DisplayRows => UsedRows.Cast<ISheetRow>().ToList();

		public void AddRow(IRow cells)
		{
			TSheetRow row = (TSheetRow)Activator.CreateInstance(typeof(TSheetRow), new object[] {cells });
			XlsRows.Add(row);
		}

		#endregion

		#region Данные



		#endregion
	}
}
