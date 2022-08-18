using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.ViewModels;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public abstract class ImportModelBase<TDataTypeEnum, TSheetRow> : PropertyChangedBase
		where TDataTypeEnum : Enum
		where TSheetRow : SheetRowBase<TSheetRow>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Type DataTypeEnum => typeof(TDataTypeEnum);

		#region Типы данных
		public virtual bool CanMatch => HasRequiredDataTypes(ImportedDataTypes.Select(x => x.DataType.Data).OfType<TDataTypeEnum>());

		protected virtual bool HasRequiredDataTypes(IEnumerable<TDataTypeEnum> dataTypes) => RequiredDataTypes.All(dataTypes.Contains);

		protected abstract TDataTypeEnum[] RequiredDataTypes { get; }
		#endregion


		private readonly IDataParser dataParser;

		protected ImportModelBase(IDataParser dataParser, Type countersEnum, ViewModelBase matchSettingsViewModel = null)
		{
			this.dataParser = dataParser;
			this.MatchSettingsViewModel = matchSettingsViewModel;
			CountersViewModel = new CountersViewModel(countersEnum);
		}

		public virtual void Init(IUnitOfWork uow)
		{
			
		}
		
		public ViewModelBase MatchSettingsViewModel { get; }
		
		public CountersViewModel CountersViewModel { get; }

		#region Колонки

		public List<ExcelColumn> Columns { get; } = new List<ExcelColumn>();

		public IEnumerable<ExcelValueTarget> ImportedDataTypes => Columns
			.SelectMany(x => x.DataTypeByLevels)
			.Where(x => x.DataType != null && !x.DataType.IsUnknown);
		public IEnumerable<DataType> DataTypes => dataParser.SupportDataTypes;

		public ExcelValueTarget GetColumnForDataType(object data) {
			return Columns.SelectMany(x => x.DataTypeByLevels).FirstOrDefault(x => data.Equals(x.DataType?.Data));
		}

		private int columnsCount;
		/// <summary>
		/// При установке значения будут пересозданы колонки
		/// </summary>
		public virtual int ColumnsCount {
			get => columnsCount;
			set {
				columnsCount = value;
				RecreateColumns(columnsCount, LevelsCount);
				RefreshColumnsTitle();
				OnPropertyChanged();
			}
		}
		
		public int LevelsCount { get; set; }

		private void RecreateColumns(int columnsCount, int levels)
		{
			Columns.Clear();
			for(int icol = 0; icol < columnsCount; icol++) {
				var column = new ExcelColumn(icol, levels);
				column.PropertyChanged += Column_PropertyChanged;
				Columns.Add(column);
			}
		}

		void Column_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ExcelColumn.DataTypeByLevels))
				OnPropertyChanged(nameof(CanMatch));
		}

		private void RefreshColumnsTitle()
		{
			if(HeaderRow <= 0) {
				for(int i = 0; i < ColumnsCount; i++)
					Columns[i].Title = CellReference.ConvertNumToColString(i);
			}
			else {
				var row = XlsRows[HeaderRow - 1];
				for(int i = 0; i < ColumnsCount; i++)
					Columns[i].Title = row.CellValue(i);
			}
			OnPropertyChanged(nameof(Columns));
		}

		public virtual void AutoSetupColumns(IProgressBarDisplayable progress)
		{
			logger.Info("Ищем заголовочную строку...");
			progress.Start(Math.Min(100, XlsRows.Count), text:"Определение типов данных...");
			var bestMath = new DataType[ColumnsCount];
			int bestColumns = 0;
			int bestHeaderRow = 0;
			int maxLevelAfterBest = 0;
			bool maxLevelDiscovering = false;
			TSheetRow bestRow = null;
			int rowNum = 0;
			foreach(var row in XlsRows) {
				progress.Add();
				var types = new DataType[ColumnsCount];
				rowNum++;
				string lastValue = null;
				for(int i = 0; i < ColumnsCount; i++) {
					var value = row.CellStringValue(i, null)?.ToLower() ?? String.Empty;
					types[i] = value != lastValue ? dataParser.DetectDataType(value) : null;
					lastValue = value;
				}
				if(bestColumns < types.Where(x => x != null).Distinct().Count()) {
					bestMath = types;
					bestRow = row;
					bestColumns = types.Where(x => x!= null).Distinct().Count();
					bestHeaderRow = rowNum;
					maxLevelDiscovering = true;
					maxLevelAfterBest = bestRow.RowLevel;
				}
				
				if(maxLevelDiscovering) {
					if(row.RowLevel <= bestRow.RowLevel && maxLevelAfterBest > bestRow.RowLevel)
						maxLevelDiscovering = false; //Отключаем поиск, явно вышли за уровень заголовка.
					else 
						maxLevelAfterBest = Math.Max(row.RowLevel, maxLevelAfterBest);
				}
				//Мало вероятно что в нормальном файле заголовочная строка располагаться ниже 100-ой строки.
				//Поэтому прерываем проверку, так как если не нашли все подходящие поля, на больших файлах будем проверять очень долго.
				//Пользователь быстрее в ручную укажет типы данных.
				if(rowNum > 100)
					break;
			}

			progress.Add();
			for (int i = 0; i < ColumnsCount; i++) {
				Columns[i].DataTypeByLevels[maxLevelAfterBest].DataType = bestMath[i] != null ? bestMath[i] : DataTypes.First();
			}

			logger.Debug($"Найдено соответсвие в {bestColumns} заголовков в строке {bestHeaderRow}");
			HeaderRow = bestHeaderRow;
			progress.Close();
			logger.Info("Ок");
		}
		#endregion

		#region Строки

		private int headerRow = -1;
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

		public void AddRow(IRow[] cells)
		{
			TSheetRow row = (TSheetRow)Activator.CreateInstance(typeof(TSheetRow), new object[] {cells });
			row.PropertyChanged += RowOnPropertyChanged;
			XlsRows.Add(row);
		}

		protected virtual void RowOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			
		}

		#endregion
		
		private IDictionary<int, ICell[]> mergedCells;
		public IDictionary<int, ICell[]> MergedCells {
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
