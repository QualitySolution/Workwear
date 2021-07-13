using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NPOI.SS.UserModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.Models.Import;

namespace workwear.ViewModels.Tools
{
	public class EmployeesLoadViewModel : ExcelLoadViewModelBase<DataTypeEmployee>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public EmployeesLoadViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IInteractiveMessage interactiveMessage, DataParserEmployee dataParser) : base(unitOfWorkFactory, navigation, interactiveMessage, dataParser)
		{
			Title = "Загрузка сотрудников";
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
		}

		private readonly DataParserEmployee dataParser;

		#region Шаг 2

		public override bool SensetiveThirdStepButton => Columns.Any(x => x.DataType == DataTypeEmployee.Fio) 
			|| (Columns.Any(x => x.DataType == DataTypeEmployee.LastName) && Columns.Any(x => x.DataType == DataTypeEmployee.FirstName));

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

		public new List<SheetRowEmployee> DisplayRows => base.DisplayRows.Cast<SheetRowEmployee>().ToList();

		protected override SheetRowBase<DataTypeEmployee> CreateXlsRow(IRow cells)
		{
			return new SheetRowEmployee(cells);
		}

		public void ThirdStep()
		{
			CurrentStep = 2;
			if(Columns.Any(x => x.DataType == DataTypeEmployee.PersonnelNumber))
				MatchByNumber();
			else
				MatchByName(DisplayRows);

			FindChanges();
			OnPropertyChanged(nameof(DisplayRows));
		}

		private void MatchByName(List<SheetRowEmployee> list)
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

		private bool СompareFio(SheetRowEmployee x, EmployeeCard employee)
		{
			var fio = GetFIO(x);
			return String.Equals(fio.LastName, employee.LastName, StringComparison.CurrentCultureIgnoreCase)
				&& String.Equals(fio.FirstName, employee.FirstName, StringComparison.CurrentCultureIgnoreCase)
				&& (fio.Patronymic == null || String.Equals(fio.Patronymic, employee.Patronymic, StringComparison.CurrentCultureIgnoreCase));
		}

		private void MatchByNumber()
		{
			var list = DisplayRows;
			var numberColumn = GetColumn(DataTypeEmployee.PersonnelNumber);
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
			var meaningfulColumns = Columns.Where(x => x.DataType != DataTypeEmployee.Unknown).ToArray();
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
				var employee = dataParser.PrepareToSave(UoW, row);
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

		#region Helpers

		public FIO GetFIO(SheetRowEmployee row)
		{
			var fio = new FIO();
			var lastnameColumn = GetColumn(DataTypeEmployee.LastName);
			var firstNameColumn = GetColumn(DataTypeEmployee.FirstName);
			var patronymicColumn = GetColumn(DataTypeEmployee.Patronymic);
			var fioColumn = GetColumn(DataTypeEmployee.Fio);
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
