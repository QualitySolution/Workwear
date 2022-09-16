using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Models.Import;
using Workwear.Tools.Nhibernate;

namespace workwear.ViewModels.Import
{
	public class ExcelImportViewModel : UowDialogViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static readonly string ColorOfNew = "Pale Turquoise";
		public static readonly string ColorOfChanged = "Pale Green";
		public static readonly string ColorOfError = "Pink";
		public static readonly string ColorOfAmbiguous = "Khaki";
		public static readonly string ColorOfNotFound = "Yellow";
		public static readonly string ColorOfSkipped = "Orchid";
		
		public static readonly string ColorOfWarning = "Orange Red";

		public ExcelImportViewModel(
			IImportModel importModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IInteractiveMessage interactiveMessage, 
			ProgressInterceptor progressInterceptor, 
			IValidator validator = null) : base(unitOfWorkFactory, navigation, validator, importModel.ImportName)
		{
			ImportModel = importModel ?? throw new ArgumentNullException(nameof(importModel));
			ImportModel.Init(UoW);
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.progressInterceptor = progressInterceptor;
			Title = importModel.ImportName;
			importModel.PropertyChanged += ImportModel_PropertyChanged;
		}

		#region Общее
		private int currentStep;
		public virtual int CurrentStep {
			get => currentStep;
			set => SetField(ref currentStep, value);
		}

		public IImportModel ImportModel { get; }

		void ImportModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ImportModel.CanMatch))
				OnPropertyChanged(nameof(SensitiveThirdStepButton));
		}

		public void SelectionChanged(ISheetRow[] rows) {
			ButtonIgnoreSensitive = rows.Length > 0;
			ButtonIgnoreTitle = AreMostIgnored(rows) ? "Загружать строку" : "Не загружать строку";
		}
		#endregion

		#region private
		protected readonly IInteractiveMessage interactiveMessage;
		private readonly ProgressInterceptor progressInterceptor;
		protected IWorkbook wb;
		protected ISheet sh;
		#endregion

		#region Шаг 1

		public bool SensitiveSecondStepButton => SelectedSheet != null;

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
		[PropertyChangedAlso(nameof(SensitiveSecondStepButton))]
		public virtual ImportedSheet SelectedSheet {
			get => selectedSheet;
			set => SetField(ref selectedSheet, value);
		}

		public void BackToFirstStep()
		{
			CurrentStep = 0;
		}

		#endregion

		#region Шаг 2

		public void SecondStep()
		{
			CurrentStep = 1;
			LoadSheet();
			ImportModel.AutoSetupColumns(ProgressStep);
		}

		public void BackToSecondStep()
		{
			CurrentStep = 1;
			ImportModel.DisplayRows.ForEach(x => x.ProgramSkipped = false);
			ImportModel.CleanMatch();
			RowActionsShow = false;
		}

		public bool SensitiveThirdStepButton => ImportModel.CanMatch;
		public int RowsCount => ImportModel.SheetRowCount;
		#endregion

		#region Шаг 3

		public void ThirdStep() {
			CurrentStep = 2;
			ImportModel.MatchAndChanged(ProgressStep, UoW);
			SensitiveSaveButton = ImportModel.CanSave;
			RowActionsShow = true;
		}

		public void ToggleIgnoreRows(ISheetRow[] rows) {
			var lastValue = AreMostIgnored(rows);
			foreach(var row in rows)
				row.UserSkipped = !lastValue;
			SelectionChanged(rows);
		}

		public bool AreMostIgnored(ISheetRow[] rows) {
			return rows.Count(x => x.UserSkipped) > rows.Length / 2;
		}

		#region Статистика
		public CountersViewModel CountersViewModel => ImportModel.CountersViewModel;
		#endregion

		#region Свойства View
		public IProgressBarDisplayable ProgressStep;

		private bool sensitiveSaveButton;
		public virtual bool SensitiveSaveButton {
			get => sensitiveSaveButton;
			set => SetField(ref sensitiveSaveButton, value);
		}

		private bool rowActionsShow;
		public virtual bool RowActionsShow {
			get => rowActionsShow;
			set => SetField(ref rowActionsShow, value);
		}

		private string buttonIgnoreTitle;
		public virtual string ButtonIgnoreTitle {
			get => buttonIgnoreTitle;
			set => SetField(ref buttonIgnoreTitle, value);
		}

		private bool buttonIgnoreSensitive;
		public virtual bool ButtonIgnoreSensitive {
			get => buttonIgnoreSensitive;
			set => SetField(ref buttonIgnoreSensitive, value);
		}
		#endregion
		#endregion
		#region Сохранение
		public new void Save() {
			var start = DateTime.Now;
			sensitiveSaveButton = false;
			progressInterceptor.PrepareStatement += (sender, e) => ProgressStep.Add();
			var toSave = ImportModel.MakeToSave(ProgressStep, UoW);
			ProgressStep.Start(toSave.Count, text: "Сохранение");
			foreach(var item in toSave) {
				UoW.TrySave(item);
			}
			UoW.Commit();
			logger.Debug($"Объектов сохранено: {toSave.Count} Шагов сохранения: {ProgressStep.Value} Время: {(DateTime.Now-start).TotalSeconds} сек.");
			ProgressStep.Close();
			Close(false, CloseSource.Save);
		}
		#endregion
		#region private Methods

		private void LoadFile() {
			try {
				using(var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read)) {
					if(FileName.EndsWith(".xls", StringComparison.InvariantCultureIgnoreCase))
						wb = new HSSFWorkbook(fs);
					else
						wb = new XSSFWorkbook(fs);
				}
			}
			catch(IOException ex) when(ex.HResult == -2147024864) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, 
					"Указанный файл уже открыт в другом приложении. Оно заблокировало доступ к файлу.");
				return;
			}

			Sheets = new List<ImportedSheet>();
			for(var s = 0; s < wb.NumberOfSheets; s++) {
				var sheet = new ImportedSheet {
					Number = s,
					Title = wb.GetSheetName(s)
				};
				Sheets.Add(sheet);
			}
			OnPropertyChanged(nameof(Sheets));
		}

		private void LoadSheet() {
			sh = wb.GetSheet(SelectedSheet.Title);
			ProgressStep.Start(sh.LastRowNum + sh.NumMergedRegions, text: "Читаем лист...");

			var maxColumnsIndex = 0;
			var maxLevels = 0;
			var levelsStack = new Stack<IRow>();
			for(var i = 0; i <= sh.LastRowNum; i++) {
				ProgressStep.Add();
				var row = sh.GetRow(i);
				if(row == null)
					continue;
				maxLevels = Math.Max(row.OutlineLevel, maxLevels);
				RowStackHelper.NewRow(levelsStack, row);
				ImportModel.AddRow(levelsStack.ToArray());
				//Здесь проверено экспериментально по всем файлам в тестах LastCellNum = количеству виртуальны ячеек(колонок).
				//То есть последний индекс ячеки +1. Не знаю зачем так сделано. https://github.com/dotnetcore/NPOI/issues/84
				//При отсутствии ячеек значение LastCellNum и FirstCellNum = -1
				maxColumnsIndex = Math.Max(row.LastCellNum, maxColumnsIndex);  
			}
			ImportModel.LevelsCount = maxLevels + 1;
			ImportModel.ColumnsCount = maxColumnsIndex;
			OnPropertyChanged(nameof(RowsCount));
			ProgressStep.Update("Обработка объединенных ячеек...");
			var merged = new Dictionary<int, ICell[]>();
			for(var i = 0; i <= sh.LastRowNum; i++) {
				if(sh.GetRow(i) == null)
					continue;
				merged[sh.GetRow(i).RowNum] = new ICell[maxColumnsIndex];
			}
			// Loop through all merge regions in this sheet.
			for (int i = 0; i < sh.NumMergedRegions; i++)
			{
				ProgressStep.Add();
				var mergeRegion = sh.GetMergedRegion(i);
				// Find the top-most and left-most cell in this region.
				var firstRegionCell = sh.GetRow(mergeRegion.FirstRow).GetCell(mergeRegion.FirstColumn);
				for (int row = mergeRegion.FirstRow; row <= mergeRegion.LastRow; row++) {
					for (int col = mergeRegion.FirstColumn; col <= mergeRegion.LastColumn; col++) {
						merged[row][col] = firstRegionCell;
					}
				}
			}
			ProgressStep.Add();
			ImportModel.MergedCells = merged;
			ProgressStep.Close();
			logger.Info($"Прочитано {maxColumnsIndex} колонок и {sh.LastRowNum} строк");
			if(maxLevels > 0)
				logger.Info($"Страница имеет группировку с {maxLevels + 1} уровнями");
		}
		#endregion
	}
}
