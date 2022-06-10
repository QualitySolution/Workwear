using System;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Models.Import;
using workwear.Tools.Nhibernate;

namespace workwear.ViewModels.Import
{
	public class ExcelImportViewModel : UowDialogViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static readonly string ColorOfNew = "Pale Turquoise";
		public static readonly string ColorOfChanged = "Pale Green";
		public static readonly string ColorOfError = "Pink";
		public static readonly string ColorOfNotFound = "Yellow";
		public static readonly string ColorOfSkipped = "Orchid";

		public ExcelImportViewModel(
			IImportModel importModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			IInteractiveMessage interactiveMessage, 
			ProgressInterceptor progressInterceptor, 
			IValidator validator = null) : base(unitOfWorkFactory, navigation, validator)
		{
			ImportModel = importModel ?? throw new ArgumentNullException(nameof(importModel));
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

		public void SecondStep() {
			CurrentStep = 1;
			LoadSheet();
			ImportModel.AutoSetupColumns(ProgressStep);
		}

		#endregion

		#region Шаг 2

		public bool SensitiveThirdStepButton => ImportModel.CanMatch;
		public int RowsCount => ImportModel.SheetRowCount;
		#endregion

		#region Шаг 3

		public void ThirdStep() {
			CurrentStep = 2;
			ImportModel.MatchAndChanged(ProgressStep, UoW);
			SensitiveSaveButton = ImportModel.CanSave;
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
		#endregion
		#endregion
		#region Сохранение
		public new void Save() {
			sensitiveSaveButton = false;
			progressInterceptor.PrepareStatement += (sender, e) => ProgressStep.Add();
			var toSave = ImportModel.MakeToSave(ProgressStep, UoW);
			ProgressStep.Start(toSave.Count, text: "Сохранение");
			foreach(var item in toSave) {
				UoW.TrySave(item);
			}
			UoW.Commit();
			logger.Debug($"Объектов сохранено: {toSave.Count} Шагов сохранения: {ProgressStep.Value}");
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
			ProgressStep.Start(sh.LastRowNum, text: "Читаем лист...");

			var maxColumns = 0;
			for(var i = 0; i <= sh.LastRowNum; i++) {
				ProgressStep.Add();
				if(sh.GetRow(i) == null)
					continue;
				ImportModel.AddRow(sh.GetRow(i));
				maxColumns = Math.Max(sh.GetRow(i).Cells.Count, maxColumns);
			}
			ImportModel.MaxSourceColumns = maxColumns;
			OnPropertyChanged(nameof(RowsCount));
			ProgressStep.Close();
			logger.Info($"Прочитано {maxColumns} колонок и {sh.LastRowNum} строк");
		}
		#endregion
	}
}
