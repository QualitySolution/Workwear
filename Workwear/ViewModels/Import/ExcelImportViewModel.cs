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
		public static readonly string ColorOfError = "Red";
		public static readonly string ColorOfSkiped = "Orchid";

		public ExcelImportViewModel(IImportModel importModel, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IInteractiveMessage interactiveMessage, ProgressInterceptor progressInterceptor, IValidator validator = null) : base(unitOfWorkFactory, navigation, validator)
		{
			ImportModel = importModel ?? throw new ArgumentNullException(nameof(importModel));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.progressInterceptor = progressInterceptor;
			Title = importModel.ImportName;
			CountersViewModel = new CountersViewModel(importModel.CountersEnum);
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
				OnPropertyChanged(nameof(SensetiveThirdStepButton));
		}
		#endregion

		#region private
		protected readonly IInteractiveMessage interactiveMessage;
		private readonly ProgressInterceptor progressInterceptor;
		protected IWorkbook wb;
		protected ISheet sh;
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
			ImportModel.AutoSetupColumns();
		}

		#endregion

		#region Шаг 2

		public bool SensetiveThirdStepButton => ImportModel.CanMatch;
		#endregion

		#region Шаг 3

		public void ThirdStep()
		{
			CurrentStep = 2;
			ImportModel.MatchAndChanged(UoW, CountersViewModel);
		}

		#region Статистика
		public CountersViewModel CountersViewModel;
		#endregion

		#region Свойства View
		public IProgressBarDisplayable ProgressStep;

		public bool SensetiveSaveButton => SaveNewItems || SaveChangedItems;
		#endregion

		#region Настройки
		private bool saveNewItems = true;
		[PropertyChangedAlso(nameof(SensetiveSaveButton))]
		public virtual bool SaveNewItems {
			get => saveNewItems;
			set => SetField(ref saveNewItems, value);
		}

		private bool saveChangedItems = true;
		[PropertyChangedAlso(nameof(SensetiveSaveButton))]
		public virtual bool SaveChangedItems {
			get => saveChangedItems;
			set => SetField(ref saveChangedItems, value);
		}
		#endregion
		#endregion

		#region Сохранение
		public new void Save()
		{
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

			int maxColumns = 0;
			int i = 0;
			while(sh.GetRow(i) != null) {
				ImportModel.AddRow(sh.GetRow(i));
				maxColumns = Math.Max(sh.GetRow(i).Cells.Count, maxColumns);
				i++;
			}
			ImportModel.MaxSourceColumns = maxColumns;

			logger.Info($"Прочитано {maxColumns} колонок и {i} строк");
		}
		#endregion
	}
}
