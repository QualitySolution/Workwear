using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;
using NHibernate.Engine;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.Measurements;
using workwear.Models.Company;
using workwear.Repository.Company;
using workwear.ViewModels.Import;
using Workwear.Domain.Company;
using Workwear.Measurements;

namespace workwear.Models.Import
{
	public class DataParserEmployee : DataParserBase<DataTypeEmployee>
	{
		private readonly PersonNames personNames;
		private readonly SizeService sizeService;
		private readonly IUserService userService;

		public DataParserEmployee(
			PersonNames personNames,
			SizeService sizeService,
			IUserService userService = null)
		{
			AddColumnName(DataTypeEmployee.Fio,
				"ФИО",
				"Ф.И.О.",
				"Фамилия Имя Отчество",
				"Наименование"//Встречается при выгрузке из 1C
				);
			AddColumnName(DataTypeEmployee.CardKey,
				"CARD_KEY",
				"card",
				"uid"
				);
			AddColumnName(DataTypeEmployee.FirstName,
				"FIRST_NAME",
				"имя",
				"FIRST NAME"
				);
			AddColumnName(DataTypeEmployee.LastName,
				"LAST_NAME",
				"фамилия",
				"LAST NAME"
				);
			AddColumnName(DataTypeEmployee.Patronymic,
				"SECOND_NAME",
				"SECOND NAME",
				"Patronymic",
				"Отчество"
				);
			AddColumnName(DataTypeEmployee.Sex,
				"Sex",
				"Gender",
				"Пол"
				);
			AddColumnName(DataTypeEmployee.PersonnelNumber,
				"TN",
				"Табельный",
				"Таб. №",
				"Таб."//Если такой вариант будет пересекаться с другими полями его можно удалить.
				);
			AddColumnName(DataTypeEmployee.HireDate,
				"Дата приема",
				"Дата приёма",
				"Принят"
				);
			AddColumnName(DataTypeEmployee.DismissDate,
				"Дата увольнения",
				"Уволен"
			);
			AddColumnName(DataTypeEmployee.Subdivision,
				"Подразделение"
				);
			AddColumnName(DataTypeEmployee.Department,
				"Отдел",
				"Бригада",
				"Бригады"
				);
			AddColumnName(DataTypeEmployee.Post,
				"Должность"
				);
			AddColumnName(DataTypeEmployee.Growth,
				"Рост"
				);
			AddColumnName(DataTypeEmployee.ShoesSize,
				"Обувь"
				);
			//Разместил былиже к концу чтобы слово "размер", срабатывало только в том случае если другого не нашли.
			AddColumnName(DataTypeEmployee.WearSize,
				"Размер",
				"Одежда"
				);
			this.personNames = personNames ?? throw new ArgumentNullException(nameof(personNames));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.userService = userService;
		}

		private void AddColumnName(DataTypeEmployee type, params string[] names)
		{
			foreach(var name in names)
				ColumnNames.Add(name.ToLower(), type);
		}

		#region Обработка изменений
		public void FindChanges(
			IUnitOfWork uow, 
			IEnumerable<SheetRowEmployee> list, 
			ImportedColumn<DataTypeEmployee>[] meaningfulColumns, 
			IProgressBarDisplayable progress, 
			SettingsMatchEmployeesViewModel settings)
		{
			progress.Start(list.Count(), text: "Поиск изменений");
			foreach(var row in list) {
				progress.Add();
				if(row.Skipped)
					continue;

				var employee = row.Employees.FirstOrDefault();
				var rowChange = ChangeType.ChangeValue;

				if(employee == null) {
					employee = new EmployeeCard {
						Comment = "Импортирован из Excel",
						CreatedbyUser = userService?.GetCurrentUser(uow)
					};
					row.Employees.Add(employee);
					rowChange = ChangeType.NewEntity;
				}

				foreach(var column in meaningfulColumns) {
					MakeChange(settings, employee, row, column, rowChange);
				}
			}
			progress.Close();
		}

		public void MakeChange(
			SettingsMatchEmployeesViewModel settings, 
			EmployeeCard employee, 
			SheetRowEmployee row, 
			ImportedColumn<DataTypeEmployee> column, 
			ChangeType rowChange)
		{
			var value = row.CellStringValue(column.Index);
			var dataType = column.DataType;
			if(String.IsNullOrWhiteSpace(value)) {
				row.AddColumnChange(column, ChangeType.NotChanged);
				return;
			}

			switch(dataType) {
				case DataTypeEmployee.CardKey:
					row.ChangedColumns.Add(column, CompareString(employee.CardKey, value, rowChange));
					break;
				case DataTypeEmployee.PersonnelNumber:
					row.ChangedColumns.Add(column, CompareString(employee.PersonnelNumber, 
						(settings.ConvertPersonnelNumber ? EmployeeParse.ConvertPersonnelNumber(value) : value)?.Trim(), rowChange));
					break;
				case DataTypeEmployee.LastName:
					row.ChangedColumns.Add(column, CompareString(employee.LastName, value, rowChange));
					break;
				case DataTypeEmployee.FirstName:
					row.ChangedColumns.Add(column, CompareString(employee.FirstName, value, rowChange));
					break;
				case DataTypeEmployee.Patronymic:
					row.ChangedColumns.Add(column, CompareString(employee.Patronymic, value, rowChange));
					break;
				case DataTypeEmployee.Sex:
					//Первая М английская, вторая русская.
					if(value.StartsWith("M", StringComparison.CurrentCultureIgnoreCase) || 
					   value.StartsWith("М", StringComparison.CurrentCultureIgnoreCase)) {
						row.AddColumnChange(column, employee.Sex == Sex.M ? ChangeType.NotChanged : rowChange);
						break;
					}
					if(value.StartsWith("F", StringComparison.CurrentCultureIgnoreCase) || 
					   value.StartsWith("Ж", StringComparison.CurrentCultureIgnoreCase)) {
						row.AddColumnChange(column, employee.Sex == Sex.F ? ChangeType.NotChanged : rowChange);
						break;
					}
					row.AddColumnChange(column, ChangeType.ParseError);
					break;
				case DataTypeEmployee.Fio:
					value.SplitFullName(out var lastName, out var firstName, out var patronymic);
					var lastDiff = !String.IsNullOrEmpty(lastName) && 
					               !String.Equals(employee.LastName, lastName, StringComparison.CurrentCultureIgnoreCase);
					var firstDiff = !String.IsNullOrEmpty(firstName) && 
					                !String.Equals(employee.FirstName, firstName, StringComparison.CurrentCultureIgnoreCase);
					var patronymicDiff = !String.IsNullOrEmpty(patronymic) && 
					                     !String.Equals(employee.Patronymic, patronymic, StringComparison.CurrentCultureIgnoreCase);
					row.AddColumnChange(column, lastDiff || firstDiff || patronymicDiff ? 
						rowChange : ChangeType.NotChanged);
					break;
				case DataTypeEmployee.HireDate:
					row.ChangedColumns.Add(column, CompareDate(employee.HireDate, row.CellDateTimeValue(column.Index), rowChange));
					break;
				case DataTypeEmployee.DismissDate:
					row.ChangedColumns.Add(column, CompareDate(employee.DismissDate, row.CellDateTimeValue(column.Index), rowChange));
					break;
				case DataTypeEmployee.Subdivision:
					if(String.Equals(employee.Subdivision?.Name, value, StringComparison.CurrentCultureIgnoreCase)) {
						row.AddColumnChange(column, ChangeType.NotChanged);
						break;
					}

					var subdivision = UsedSubdivisions.FirstOrDefault(x =>
						String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase));
					if(subdivision == null) {
						subdivision = new Subdivision { Name = value };
						UsedSubdivisions.Add(subdivision);
					}
					row.AddColumnChange(column, subdivision.Id == 0 ? ChangeType.NewEntity : rowChange, employee.Subdivision?.Name);
					employee.Subdivision = subdivision;
					break;
				case DataTypeEmployee.Department:
					if(String.Equals(employee.Department?.Name, value, StringComparison.CurrentCultureIgnoreCase)) {
						row.AddColumnChange(column, ChangeType.NotChanged);
						break;
					}
					var department = UsedDepartment.FirstOrDefault(x =>
						String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase)
						&& (employee.Subdivision == null && x.Subdivision == null || 
						    DomainHelper.EqualDomainObjects(x.Subdivision, employee.Subdivision)));
					if(department == null) {
						department = new Department {
							Name = value,
							Subdivision = employee.Subdivision,
							Comments = "Создан при импорте сотрудников из Excel"
						};
						UsedDepartment.Add(department);
					}
					row.AddColumnChange(column, department.Id == 0 ? ChangeType.NewEntity : rowChange, employee.Department?.Name);
					employee.Department = department;
					break;
				case DataTypeEmployee.Post:
					if(String.Equals(employee.Post?.Name, value, StringComparison.CurrentCultureIgnoreCase)) {
						row.AddColumnChange(column, ChangeType.NotChanged);
						break;
					}
					var post = UsedPosts.FirstOrDefault(x =>
						String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase)
						&&(employee.Subdivision == null && x.Subdivision == null || 
						   DomainHelper.EqualDomainObjects(x.Subdivision, employee.Subdivision)));
					if(post == null) {
						post = new Post { 
							Name = value, 
							Subdivision = employee.Subdivision,
							Comments = "Создана при импорте сотрудников из Excel"
						};
						UsedPosts.Add(post);
					}
					row.AddColumnChange(column, post.Id == 0 ? ChangeType.NewEntity : rowChange, employee.Post?.Name);
					employee.Post = post;
					break;
				case DataTypeEmployee.Growth:
					row.ChangedColumns.Add(column, CompareString(employee.WearGrowth,
						row.CellStringValue(column.Index), rowChange));
					break;
				case DataTypeEmployee.WearSize:
					row.ChangedColumns.Add(column, CompareSize(employee.WearSize, 
						row.CellStringValue(column.Index), СlothesType.Wear, employee.Sex, rowChange));
					break;
				case DataTypeEmployee.ShoesSize:
					row.ChangedColumns.Add(column, CompareSize(employee.ShoesSize, 
						row.CellStringValue(column.Index), СlothesType.Shoes, employee.Sex, rowChange));
					break;

				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}

		private ChangeState CompareString(string fieldValue, string newValue, ChangeType rowChange) {
			var changeType = String.Equals(fieldValue, newValue, StringComparison.InvariantCultureIgnoreCase) ? 
				ChangeType.NotChanged : rowChange;
			return changeType == ChangeType.ChangeValue ? 
				new ChangeState(changeType, fieldValue) : new ChangeState(changeType);
		}

		private ChangeState CompareDate(DateTime? fieldValue, DateTime? newValue, ChangeType rowChange)
		{
			var changeType = fieldValue == newValue ? ChangeType.NotChanged : rowChange;
			if(changeType == ChangeType.ChangeValue)
				return new ChangeState(changeType, fieldValue?.ToShortDateString());
			return new ChangeState(changeType);
		}

		private ChangeState CompareSize(string fieldValue, string newValue, СlothesType сlothesType, Sex sex, ChangeType rowChange) {
			var changeType = String.Equals(fieldValue, newValue, StringComparison.InvariantCultureIgnoreCase) ? ChangeType.NotChanged : rowChange;
			if (changeType == ChangeType.NotChanged)
				return changeType == ChangeType.ChangeValue
					? new ChangeState(changeType, fieldValue)
					: new ChangeState(changeType);
			var standarts = SizeHelper.GetSizeStandartsEnum(сlothesType, sex);
			var sizes = sizeService.GetAllSizesForEmployee(standarts);
			if(!sizes.Any(x => String.Equals(x.Size, newValue, StringComparison.InvariantCultureIgnoreCase)))
				changeType = ChangeType.ParseError;

			return changeType == ChangeType.ChangeValue ? 
				new ChangeState(changeType, fieldValue) : new ChangeState(changeType);
		}
		#endregion
		#region Сопоставление
		public void MatchByName(
			IUnitOfWork uow, 
			IEnumerable<SheetRowEmployee> list, 
			List<ImportedColumn<DataTypeEmployee>> columns, 
			IProgressBarDisplayable progress)
		{
			progress.Start(2, text: "Сопоставление с существующими сотрудниками");
			var searchValues = list.Select(x => GetFIO(x, columns))
				.Where(fio => !String.IsNullOrEmpty(fio.LastName) && !String.IsNullOrEmpty(fio.FirstName))
				.Select(fio => (fio.LastName + "|" + fio.FirstName).ToUpper())
				.Distinct().ToArray();
			
			Console.WriteLine((uow.Session.SessionFactory as ISessionFactoryImplementor).Dialect);
			var exists = uow.Session.QueryOver<EmployeeCard>()
				.Where(Restrictions.In(
				Projections.SqlFunction(
							  "upper", NHibernateUtil.String,
							  (uow.Session.SessionFactory as ISessionFactoryImplementor).Dialect is SQLiteDialect //Данный диалект используется в тестах.
								  ? 
								  Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.String, "( ?1 || '|' || ?2)"),
									  NHibernateUtil.String,
									  Projections.Property<EmployeeCard>(x => x.LastName),
									  Projections.Property<EmployeeCard>(x => x.FirstName)
								  )
							: Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
							  	NHibernateUtil.String,
							  	Projections.Constant(""),
								Projections.Property<EmployeeCard>(x => x.LastName),
								Projections.Constant("|"),
								Projections.Property<EmployeeCard>(x => x.FirstName)
							    )),
						   searchValues)).List();
			progress.Add();
			foreach(var employee in exists) {
				var found = list.Where(x => СompareFio(x, employee, columns)).ToArray();
				if(!found.Any())
					continue; //Так как из базе ищем без отчества, могуть быть лишние.
				found.First().Employees.Add(employee);
			}

			progress.Add();
			//Пропускаем дубликаты имен в файле
			var groups = list.GroupBy(x => GetFIO(x, columns).GetHash());
			foreach(var group in groups) {
				if(String.IsNullOrWhiteSpace(group.Key)) {
					group.First().ProgramSkipped = true;
				}

				foreach(var item in group.Skip(1)) {
					item.ProgramSkipped = true;
				}
			}
			progress.Close();
		}

		private bool СompareFio(SheetRowEmployee x, EmployeeCard employee, List<ImportedColumn<DataTypeEmployee>> columns)
		{
			var fio = GetFIO(x, columns);
			return String.Equals(fio.LastName, employee.LastName, StringComparison.CurrentCultureIgnoreCase)
				&& String.Equals(fio.FirstName, employee.FirstName, StringComparison.CurrentCultureIgnoreCase)
				&& (fio.Patronymic == null || String.Equals(fio.Patronymic, employee.Patronymic, StringComparison.CurrentCultureIgnoreCase));
		}

		public void MatchByNumber(
			IUnitOfWork uow, 
			IEnumerable<SheetRowEmployee> list, 
			List<ImportedColumn<DataTypeEmployee>> columns, 
			SettingsMatchEmployeesViewModel settings, 
			IProgressBarDisplayable progress)
		{
			progress.Start(2, text: "Сопоставление с существующими сотрудниками");
			var numberColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.PersonnelNumber);
			var numbers = list.Select(x => GetPersonalNumber(settings, x, numberColumn.Index))
							.Where(x => !String.IsNullOrWhiteSpace(x))
							.Distinct().ToArray();
			var exists = uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PersonnelNumber.IsIn(numbers))
				.List();

			progress.Add();
			foreach(var employee in exists) {
				var found = list.Where(x => 
					GetPersonalNumber(settings, x, numberColumn.Index) == employee.PersonnelNumber).ToArray();
				found.First().Employees.Add(employee);
			}

			//Пропускаем дубликаты Табельных номеров в файле
			progress.Add();
			var groups = list.GroupBy(x => GetPersonalNumber(settings, x, numberColumn.Index));
			foreach(var group in groups) {
				if(String.IsNullOrWhiteSpace(group.Key)) {
					//Если табельного номера нет проверяем по FIO
					MatchByName(uow, group, columns, progress);
				}

				foreach(var item in group.Skip(1)) {
					item.ProgramSkipped = true;
				}
			}
			progress.Close();
		}
		#endregion
		#region Создание объектов
		public readonly List<Subdivision> UsedSubdivisions = new List<Subdivision>();
		public readonly List<Post> UsedPosts = new List<Post>();
		public readonly List<Department> UsedDepartment = new List<Department>();

		public void FillExistEntities(
			IUnitOfWork uow, 
			IEnumerable<SheetRowEmployee> list, 
			List<ImportedColumn<DataTypeEmployee>> columns, 
			IProgressBarDisplayable progress)
		{
			progress.Start(3, text: "Загружаем подразделения");
			var subdivisionColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Subdivision);
			if(subdivisionColumn != null) {
				var subdivisionNames = list.Select(x => x.CellStringValue(subdivisionColumn.Index)).Distinct().ToArray();
				UsedSubdivisions.AddRange(uow.Session.QueryOver<Subdivision>()
					.Where(x => x.Name.IsIn(subdivisionNames))
					.List());
			}
			progress.Add(text: "Загружаем отделы");
			var departmentColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Department);
			if(departmentColumn != null) {
				var departmentNames = list.Select(x => x.CellStringValue(departmentColumn.Index)).Distinct().ToArray();
				UsedDepartment.AddRange(uow.Session.QueryOver<Department>()
					.Where(x => x.Name.IsIn(departmentNames))
					.List());
			}
			progress.Add(text: "Загружаем должности");
			var postColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Post);
			if(postColumn != null) {
				var postNames = list.Select(x => x.CellStringValue(postColumn.Index)).Distinct().ToArray();
				UsedPosts.AddRange( uow.Session.QueryOver<Post>()
					.Where(x => x.Name.IsIn(postNames))
					.List());
			}
			progress.Close();
		}
		#endregion
		#region Сохранение данных
		public IEnumerable<object> PrepareToSave(IUnitOfWork uow, SettingsMatchEmployeesViewModel settings, SheetRowEmployee row) {
			var employee = row.Employees.FirstOrDefault() ?? new EmployeeCard();
			//Здесь колонки сортируются чтобы процесс обработки данных был в порядке следования описания типов в Enum
			//Это надо для того чтобы наличие 2 полей с похожими данными заполнялись правильно.
			//Например чтобы отдельное поле с фамилией могло перезаписать значение фамилии поученой из общего поля ФИО.
			foreach(var column in row.ChangedColumns.Keys.OrderBy(x => x.DataType)) {
				if(row.ChangedColumns[column].ChangeType == ChangeType.NewEntity || row.ChangedColumns[column].ChangeType == ChangeType.ChangeValue)
					SetValue(settings, uow, employee, row, column);
			}
			yield return employee;
		}

		private void SetValue(
			SettingsMatchEmployeesViewModel settings, 
			IUnitOfWork uow, 
			EmployeeCard employee, 
			SheetRowEmployee row, 
			ImportedColumn<DataTypeEmployee> column)
		{
			var value = row.CellStringValue(column.Index);
			var dataType = column.DataType;
			if(String.IsNullOrWhiteSpace(value))
				return;

			switch(dataType) {
				case DataTypeEmployee.CardKey:
					employee.CardKey = value;
					break;
				case DataTypeEmployee.PersonnelNumber:
					employee.PersonnelNumber = (settings.ConvertPersonnelNumber ? 
						EmployeeParse.ConvertPersonnelNumber(value) : value)?.Trim();
					break;
				case DataTypeEmployee.LastName:
					employee.LastName = value;
					break;
				case DataTypeEmployee.FirstName:
					employee.FirstName = value;
					if(employee.Sex == Sex.None) 
						employee.Sex = personNames.GetSexByName(employee.FirstName);
					break;
				case DataTypeEmployee.Patronymic:
					employee.Patronymic = value;
					break;
				case DataTypeEmployee.Sex:
					//Первая М английская, вторая русская.
					if(value.StartsWith("M", StringComparison.CurrentCultureIgnoreCase) || 
					   value.StartsWith("М", StringComparison.CurrentCultureIgnoreCase))
						employee.Sex = Sex.M;
					if(value.StartsWith("F", StringComparison.CurrentCultureIgnoreCase) || 
					   value.StartsWith("Ж", StringComparison.CurrentCultureIgnoreCase))
						employee.Sex = Sex.F;
					break;
				case DataTypeEmployee.Fio:
					value.SplitFullName(out var lastName, out var firstName, out var patronymic);
					if(!String.IsNullOrEmpty(lastName) && !String.Equals(employee.LastName, value, StringComparison.CurrentCultureIgnoreCase))
						employee.LastName = lastName;
					if(!String.IsNullOrEmpty(firstName) && !String.Equals(employee.FirstName, value, StringComparison.CurrentCultureIgnoreCase))
						employee.FirstName = firstName;
					if(!String.IsNullOrEmpty(patronymic) && !String.Equals(employee.Patronymic, value, StringComparison.CurrentCultureIgnoreCase))
						employee.Patronymic = patronymic;
					if(employee.Sex == Sex.None && !String.IsNullOrWhiteSpace(employee.FirstName)) 
						employee.Sex = personNames.GetSexByName(employee.FirstName);
					break;
				case DataTypeEmployee.HireDate:
					var hireDate = row.CellDateTimeValue(column.Index);
					if(hireDate != null)
					 	employee.HireDate = hireDate;
					break;
				case DataTypeEmployee.DismissDate:
					var dismissDate = row.CellDateTimeValue(column.Index);
					if(dismissDate != null)
						employee.DismissDate = dismissDate;
					break;

				case DataTypeEmployee.Subdivision:
				case DataTypeEmployee.Department:
				case DataTypeEmployee.Post:
					//Устанавливаем в MakeChange;
					break;

				case DataTypeEmployee.Growth:
					employee.WearGrowth = value;
					break;
				case DataTypeEmployee.WearSize:
					var standartsWear = SizeHelper.GetSizeStandartsEnum(СlothesType.Wear, employee.Sex);
					var sizesWear = sizeService.GetAllSizesForEmployee(standartsWear);
					var pairWear = sizesWear.First(x => String.Equals(x.Size, value, StringComparison.InvariantCultureIgnoreCase));
					employee.WearSize = pairWear.Size;
					employee.WearSizeStd = pairWear.StandardCode;
					break;
				case DataTypeEmployee.ShoesSize:
					var standartsShoes = SizeHelper.GetSizeStandartsEnum(СlothesType.Shoes, employee.Sex);
					var sizesShoes = sizeService.GetAllSizesForEmployee(standartsShoes);
					var pairShoes = sizesShoes.First(x => String.Equals(x.Size, value, StringComparison.InvariantCultureIgnoreCase));
					employee.ShoesSize = pairShoes.Size;
					employee.ShoesSizeStd = pairShoes.StandardCode;
					break;

				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion
		#region Helpers
		public FIO GetFIO(SheetRowEmployee row, List<ImportedColumn<DataTypeEmployee>> columns) {
			var fio = new FIO();
			var lastnameColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.LastName);
			var firstNameColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.FirstName);
			var patronymicColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Patronymic);
			var fioColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Fio);
			if(fioColumn != null)
				row.CellStringValue(fioColumn.Index)?.SplitFullName(out fio.LastName, out fio.FirstName, out fio.Patronymic);
			if(lastnameColumn != null)
				fio.LastName = row.CellStringValue(lastnameColumn.Index);
			if(firstNameColumn != null)
				fio.FirstName = row.CellStringValue(firstNameColumn.Index);
			if(patronymicColumn != null)
				fio.Patronymic = row.CellStringValue(patronymicColumn.Index);
			return fio;
		}
		public string GetPersonalNumber(SettingsMatchEmployeesViewModel settings, SheetRowEmployee row, int columnIndex) {
			var original = settings.ConvertPersonnelNumber ? 
				EmployeeParse.ConvertPersonnelNumber(row.CellStringValue(columnIndex)) : row.CellStringValue(columnIndex);
			return original?.Trim();
		}
		#endregion
	}
}
