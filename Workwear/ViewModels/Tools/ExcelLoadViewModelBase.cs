using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Models.Import;

namespace workwear.ViewModels.Tools
{
	public abstract class ExcelLoadViewModelBase<TDataTypeEnum> : UowDialogViewModelBase
		where TDataTypeEnum : System.Enum
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static readonly string ColorOfNew = "Pale Turquoise";
		public static readonly string ColorOfChanged = "Pale Green";
		public static readonly string ColorOfSkiped = "Orchid";

		protected ExcelLoadViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IInteractiveMessage interactiveMessage, IDataParser<TDataTypeEnum> dataParser, IValidator validator = null) : base(unitOfWorkFactory, navigation, validator)
		{
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
		}

		private int currentStep;
		public virtual int CurrentStep {
			get => currentStep;
			set => SetField(ref currentStep, value);
		}

		#region private
		protected readonly IInteractiveMessage interactiveMessage;

		protected IWorkbook wb;
		protected ISheet sh;

		private readonly IDataParser<TDataTypeEnum> dataParser;
		#endregion

		#region Шаг 1

		public bool SensetiveSecondStepButton => SelectedSheet != null;

		private string fileName;

		public virtual string FileName {
			get => fileName;
			set {
				SetField(ref fileName, value);
				if(!String.IsNullOrWhiteSpace(FileName))
					LoadFile();
			}
		}

		public List<ImportedSheet> Sheets { get; set; } = new List<ImportedSheet>();
		private ImportedSheet selectedSheet;
		[PropertyChangedAlso(nameof(SensetiveSecondStepButton))]
		public virtual ImportedSheet SelectedSheet {
			get => selectedSheet;
			set => SetField(ref selectedSheet, value);
		}

		public void SecondStep()
		{
			CurrentStep = 1;
			LoadSheet();
			AutoSetupColumns();
			OnPropertyChanged(nameof(DisplayRows));
		}

		#endregion

		#region Шаг 2
		private int headerRow;
		public virtual int HeaderRow {
			get => headerRow;
			set {
				if(SetField(ref headerRow, value)) {
					RefreshColumnsTitle();
					OnPropertyChanged(nameof(DisplayRows));
				}
			}
		}

		public List<ImportedColumn<TDataTypeEnum>> Columns = new List<ImportedColumn<TDataTypeEnum>>();

		public ImportedColumn<TDataTypeEnum> GetColumn(TDataTypeEnum dataType) => Columns.FirstOrDefault(x => EqualityComparer<TDataTypeEnum>.Default.Equals(x.DataType, dataType));

		private int maxSourceColumns;
		public virtual int MaxSourceColumns {
			get => maxSourceColumns;
			set => SetField(ref maxSourceColumns, value);
		}

		private List<SheetRowBase<TDataTypeEnum>> xlsRows;
		public virtual List<SheetRowBase<TDataTypeEnum>> XlsRows {
			get => xlsRows;
			set => SetField(ref xlsRows, value);
		}

		public List<SheetRowBase<TDataTypeEnum>> DisplayRows => XlsRows.Skip(HeaderRow).ToList();

		protected abstract SheetRowBase<TDataTypeEnum> CreateXlsRow(IRow cells);
		public abstract bool SensetiveThirdStepButton { get; }
		#endregion

		#region private Methods

		private void LoadFile()
		{
			try {
				using(var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read)) {
					if(FileName.EndsWith(".xls", StringComparison.InvariantCultureIgnoreCase))
						wb = new HSSFWorkbook(fs);
					else
						wb = new XSSFWorkbook(fs);
				}
			}
			catch(IOException ex) when(ex.HResult == -2147024864) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "Указанный файл уже открыт в другом приложении. Оно заблокировало доступ к файлу.");
				return;
			}

			for(int s = 0; s < wb.NumberOfSheets; s++) {
				var sheet = new ImportedSheet {
					Number = s,
					Title = wb.GetSheetName(s)
				};
				Sheets.Add(sheet);
			}
			OnPropertyChanged(nameof(Sheets));
		}

		private void LoadSheet()
		{
			logger.Info("Читаем лист...");
			sh = wb.GetSheet(SelectedSheet.Title);
			XlsRows = new List<SheetRowBase<TDataTypeEnum>>();

			int maxColumns = 0;
			int i = 0;
			while(sh.GetRow(i) != null) {
				xlsRows.Add(CreateXlsRow(sh.GetRow(i)));
				maxColumns = Math.Max(sh.GetRow(i).Cells.Count, maxColumns);
				i++;
			}
			Columns.Clear();
			for(int icol = 0; icol < maxColumns; icol++) {
				var column = new ImportedColumn<TDataTypeEnum>(icol);
				column.PropertyChanged += Column_PropertyChanged; ;
				Columns.Add(column);
			}
			MaxSourceColumns = maxColumns;

			logger.Info($"Прочитано {maxColumns} колонок и {i} строк");
		}

		void Column_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ImportedColumn<TDataTypeEnum>.DataType))
				OnPropertyChanged(nameof(SensetiveThirdStepButton));
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
		}

		private void AutoSetupColumns()
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
	}
}
