using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Tools.Import;

namespace workwear.ViewModels.Tools
{
	public class EmployeesLoadViewModel : UowDialogViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static readonly string ColorOfNew = "Pale Turquoise";
		public static readonly string ColorOfChanged = "Pale Green";
		public static readonly string ColorOfSkiped = "Orchid";

		public EmployeesLoadViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IInteractiveMessage interactiveMessage, DataParser dataParser) : base(unitOfWorkFactory, navigation)
		{
			Title = "Загрузка сотрудников";
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
		}

		private int currentStep;
		public virtual int CurrentStep {
			get => currentStep;
			set => SetField(ref currentStep, value);
		}

		#region private
		IWorkbook wb;
		ISheet sh;
		#endregion

		#region Шаг 1

		public bool SensetiveSecondStepButton => SelectedSheet != null;

		private string fileName;
		private readonly IInteractiveMessage interactiveMessage;
		private readonly DataParser dataParser;

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

		public List<ImportedColumn> Columns = new List<ImportedColumn>();

		public ImportedColumn GetColumn(DataType dataType) => Columns.FirstOrDefault(x => x.DataType == dataType);

		private int maxSourceColumns;
		public virtual int MaxSourceColumns {
			get => maxSourceColumns;
			set => SetField(ref maxSourceColumns, value);
		}

		private List<SheetRow> xlsRows;
		public virtual List<SheetRow> XlsRows {
			get => xlsRows;
			set => SetField(ref xlsRows, value);
		}

		public List<SheetRow> DisplayRows => XlsRows.Skip(HeaderRow).ToList();

		public bool SensetiveThirdStepButton => Columns.Any(x => x.DataType == DataType.Fio) 
			|| (Columns.Any(x => x.DataType == DataType.LastName) && Columns.Any(x => x.DataType == DataType.FirstName));

		#endregion

		#region Шаг 3
		#region Статистика
		private int countSkipRows;
		public virtual int CountSkipRows {
			get => countSkipRows;
			set => SetField(ref countSkipRows, value);
		}

		private int countMultiMatch;
		public virtual int CountMultiMatch {
			get => countMultiMatch;
			set => SetField(ref countMultiMatch, value);
		}

		private int countNewEmployees;
		public virtual int CountNewEmployees {
			get => countNewEmployees;
			set => SetField(ref countNewEmployees, value);
		}

		private int countChangedEmployees;
		public virtual int CountChangedEmployees {
			get => countChangedEmployees;
			set => SetField(ref countChangedEmployees, value);
		}

		private int countNoChangesEmployees;
		public virtual int CountNoChangesEmployees {
			get => countNoChangesEmployees;
			set => SetField(ref countNoChangesEmployees, value);
		}
		#endregion

		#region Свойства View
		public IProgressBarDisplayable ProgressStep3;

		public bool SensetiveSaveButton => SaveNewEmployees || SaveChangedEmployees;
		#endregion

		#region Настройки
		private bool saveNewEmployees = true;
		[PropertyChangedAlso(nameof(SensetiveSaveButton))]
		public virtual bool SaveNewEmployees {
			get => saveNewEmployees;
			set => SetField(ref saveNewEmployees, value);
		}

		private bool saveChangedEmployees = true;
		[PropertyChangedAlso(nameof(SensetiveSaveButton))]
		public virtual bool SaveChangedEmployees {
			get => saveChangedEmployees;
			set => SetField(ref saveChangedEmployees, value);
		}
		#endregion

		public void ThirdStep()
		{
			CurrentStep = 2;
			if(Columns.Any(x => x.DataType == DataType.PersonnelNumber))
				MatchByNumber();
			else
				MatchByName(DisplayRows);

			FindChanges();
			OnPropertyChanged(nameof(DisplayRows));
		}

		private void MatchByName(List<SheetRow> list)
		{
			var searchValues = list.Select(GetFIO)
				.Where(fio => !String.IsNullOrEmpty(fio.LastName) && !String.IsNullOrEmpty(fio.FirstName))
				.Select(fio => (fio.LastName + "|" + fio.FirstName).ToUpper())
				.Distinct().ToArray();
			var exists = UoW.Session.QueryOver<EmployeeCard>()
				.Where(Restrictions.In(
				Projections.SqlFunction(
							  "upper", NHibernateUtil.String,
							  Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
							  	NHibernateUtil.String,
							  	Projections.Constant(""),
								Projections.Property<EmployeeCard>(x => x.LastName),
								Projections.Constant("|"),
								Projections.Property<EmployeeCard>(x => x.FirstName)
							   )),
						   searchValues))
				.List();

			foreach(var employee in exists) {
				var found = list.Where(x => СompareFio(x, employee)).ToArray();
				if(!found.Any())
					continue; //Так как из базе ищем без отчества, могуть быть лишние.
				found.First().Employees.Add(employee);
				if(found.First().Employees.Count == 2)
					CountMultiMatch++;
			}
			//Пропускаем дубликаты имен в файле
			var groups = list.GroupBy(x => GetFIO(x).GetHash());
			foreach(var group in groups) {
				if(String.IsNullOrWhiteSpace(group.Key)) {
					group.First().Skiped = true;
					CountSkipRows++;
				}

				foreach(var item in group.Skip(1)) {
					item.Skiped = true;
					CountSkipRows++;
				}
			}
		}

		private bool СompareFio(SheetRow x, EmployeeCard employee)
		{
			var fio = GetFIO(x);
			return String.Equals(fio.LastName, employee.LastName, StringComparison.CurrentCultureIgnoreCase)
				&& String.Equals(fio.FirstName, employee.FirstName, StringComparison.CurrentCultureIgnoreCase)
				&& (fio.Patronymic == null || String.Equals(fio.Patronymic, employee.Patronymic, StringComparison.CurrentCultureIgnoreCase));
		}

		private void MatchByNumber()
		{
			var list = DisplayRows;
			var numberColumn = GetColumn(DataType.PersonnelNumber);
			var numbers = list.Select(x => x.CellValue(numberColumn.Index))
							.Where(x => !String.IsNullOrWhiteSpace(x))
							.Distinct().ToArray();
			var exists = UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PersonnelNumber.IsIn(numbers))
				.List();

			foreach(var employee in exists) {
				var found = list.Where(x => x.CellValue(numberColumn.Index) == employee.PersonnelNumber).ToArray();
				found.First().Employees.Add(employee);
				if(found.First().Employees.Count == 2)
					CountMultiMatch++;
			}

			//Пропускаем дубликаты Табельных номеров в файле
			var groups = list.GroupBy(x => x.CellValue(numberColumn.Index));
			foreach(var group in groups) {
				if(String.IsNullOrWhiteSpace(group.Key)) {
					//Если табельного номера нет проверяем по FIO
					MatchByName(group.ToList());
				}

				foreach(var item in group.Skip(1)) {
					item.Skiped = true;
					CountSkipRows++;
				}
			}
		}

		private void FindChanges()
		{
			var meaningfulColumns = Columns.Where(x => x.DataType != DataType.Unknown).ToArray();
			foreach(var row in DisplayRows) {
				if(row.Skiped)
					continue;
				if(!row.Employees.Any()) {
					CountNewEmployees++;
					row.ChangedColumns.AddRange(meaningfulColumns);
					continue;
				}
				var employee = row.Employees.First();
				foreach(var column in meaningfulColumns) {
					if(dataParser.IsDiff(employee, column.DataType, row.CellValue(column.Index)))
						row.ChangedColumns.Add(column);
				}
				if(row.ChangedColumns.Any())
					CountChangedEmployees++;
				else
					CountNoChangesEmployees++;
			}
		}

		public new void Save()
		{
			int i = 0;
			var toSave = DisplayRows.Where(x => (SaveNewEmployees && !x.Employees.Any()) 
					|| (SaveChangedEmployees && x.Employees.Any() && x.ChangedColumns.Any()))
				.ToList();
			logger.Info($"Новых: {toSave.Count(x => !x.Employees.Any())} Измененых: {toSave.Count(x => x.Employees.Any())} Всего: {toSave.Count}");
			ProgressStep3.Start(toSave.Count);
			foreach(var row in toSave) {
				var employee = dataParser.PrepareToSave(row);
				UoW.Save(employee);
				i++;
				if(i % 50 == 0) {
					UoW.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / toSave.Count:P}]... ");
				}
				ProgressStep3.Add();
			}
			UoW.Commit();
			ProgressStep3.Close();
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

		private void AutoSetupColumns()
		{
			logger.Info("Ищем заголовочную строку...");

			var bestMath = new DataType[MaxSourceColumns];
			int bestColumns = 0;
			int bestHeaderRow = 0;
			SheetRow bestRow = null;
			int rowNum = 0;
			foreach(var row in XlsRows) {
				var types = new DataType[MaxSourceColumns];
				rowNum++;
				for(int i = 0; i < MaxSourceColumns; i++) {
					var value = row.CellValue(i)?.ToLower() ?? String.Empty;
					var foundKey = dataParser.ColumnNames.Keys.FirstOrDefault(pattern => value.Contains(pattern));
					if(foundKey != null)
						types[i] = dataParser.ColumnNames[foundKey];
				}
				if(bestColumns < types.Count(x => x != DataType.Unknown)) {
					bestMath = types;
					bestRow = row;
					bestColumns = types.Count(x => x != DataType.Unknown);
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

		private void LoadSheet()
		{
			logger.Info("Читаем лист...");
			sh = wb.GetSheet(SelectedSheet.Title);
			XlsRows = new List<SheetRow>();

			int maxColumns = 0;
			int i = 0;
			while(sh.GetRow(i) != null) {
				xlsRows.Add(new SheetRow(sh.GetRow(i)));
				maxColumns = Math.Max(sh.GetRow(i).Cells.Count, maxColumns);
				i++;
			}
			Columns.Clear();
			for(int icol = 0; icol < maxColumns; icol++) {
				var column = new ImportedColumn(icol);
				column.PropertyChanged += Column_PropertyChanged;;
				Columns.Add(column);
			}
			MaxSourceColumns = maxColumns;

			logger.Info($"Прочитано {maxColumns} колонок и {i} строк");
		}

		void Column_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ImportedColumn.DataType))
				OnPropertyChanged(nameof(SensetiveThirdStepButton));
		}

		#endregion

		#region Helpers

		public FIO GetFIO(SheetRow row)
		{
			var fio = new FIO();
			var lastnameColumn = GetColumn(DataType.LastName);
			var firstNameColumn = GetColumn(DataType.FirstName);
			var patronymicColumn = GetColumn(DataType.Patronymic);
			var fioColumn = GetColumn(DataType.Fio);
			if(fioColumn != null)
				row.CellValue(fioColumn.Index).SplitFullName(out fio.LastName, out fio.FirstName, out fio.Patronymic);
			if(lastnameColumn != null)
				fio.LastName = row.CellValue(lastnameColumn.Index);
			if(firstNameColumn != null)
				fio.FirstName = row.CellValue(firstNameColumn.Index);
			if(patronymicColumn != null)
				fio.Patronymic = row.CellValue(patronymicColumn.Index);
			return fio;
		}

		#endregion
	}
}
