using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities.Numeric;
using QS.Utilities.Text;
using workwear.Domain.Company;
using Workwear.Domain.Sizes;
using Workwear.Measurements;
using workwear.Models.Company;
using workwear.Models.Import.Employee.DataTypes;
using workwear.Repository.Company;
using workwear.ViewModels.Import;

namespace workwear.Models.Import.Employee
{
	public class DataParserEmployee : DataParserBase
	{
		private readonly PersonNames personNames;
		private readonly IUserService userService;
		private readonly SizeService sizeService;
		private readonly PhoneFormatter phoneFormatter;

		public DataParserEmployee(
			PersonNames personNames,
			SizeService sizeService,
			PhoneFormatter phoneFormatter, 
			IUserService userService = null)
		{
			this.personNames = personNames ?? throw new ArgumentNullException(nameof(personNames));
			this.phoneFormatter = phoneFormatter ?? throw new ArgumentException(nameof(phoneFormatter));
			this.userService = userService;
			this.sizeService = sizeService;
			

		}

		#region Размеры
		public void CreateDatatypes(IUnitOfWork uow, SettingsMatchEmployeesViewModel settings) {
			SupportDataTypes.Add(new DataTypeFio(personNames));
			SupportDataTypes.Add(new DataTypeSimpleString(
				DataTypeEmployee.CardKey,
				x => x.CardKey,
				new []{
					"CARD_KEY",
					"card",
					"uid"
				}
			));
			SupportDataTypes.Add(new DataTypeFirstName(personNames));
			SupportDataTypes.Add(new DataTypeSimpleString(
				DataTypeEmployee.LastName,
				x => x.LastName,
				new []{
					"LAST_NAME",
					"фамилия",
					"LAST NAME"
				}
			));
			SupportDataTypes.Add(new DataTypeSimpleString(
				DataTypeEmployee.Patronymic,
				x => x.Patronymic,
				new []{
					"SECOND_NAME",
					"SECOND NAME",
					"Patronymic",
					"Отчество"
				}
			));
			SupportDataTypes.Add(new DataTypeSex());
			SupportDataTypes.Add(new DataTypePersonalNumber(settings));
			SupportDataTypes.Add(new DataTypePhone(phoneFormatter));
			SupportDataTypes.Add(new DataTypeSimpleDate(
				DataTypeEmployee.HireDate,
				x => x.HireDate,
				new []{
					"Дата приема",
					"Дата приёма",
					"Принят"
				}
			));
			SupportDataTypes.Add(new DataTypeSimpleDate(
				DataTypeEmployee.DismissDate,
				x => x.DismissDate,
				new []{
					"Дата увольнения",
					"Уволен"
				}
			));
			SupportDataTypes.Add(new DataTypeSimpleDate(
				DataTypeEmployee.BirthDate,
				x => x.BirthDate,
				new []{
					"Дата рождения",
					"День рождения",
					"BirthDay"
				}
			));
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
			
			var sizeTypes = sizeService.GetSizeType(uow, true);
			foreach (var sizeType in sizeTypes)
			{
				var datatype = new DataTypeEmployeeSize(sizeService, sizeType);
				SupportDataTypes.Add(datatype);
			}
		}
		
		#endregion

		#region Обработка изменений
		public void FindChanges(
			IUnitOfWork uow, 
			IEnumerable<SheetRowEmployee> list, 
			ExcelValueTarget[] meaningfulColumns, 
			IProgressBarDisplayable progress)
		{
			progress.Start(list.Count(), text: "Поиск изменений");
			foreach(var row in list) {
				progress.Add();
				if(row.Skipped)
					continue;

				var employee = row.Employees.FirstOrDefault();

				if(employee == null) {
					employee = new EmployeeCard {
						Comment = "Импортирован из Excel",
						CreatedbyUser = userService?.GetCurrentUser(uow)
					};
					row.Employees.Add(employee);
				}

				foreach(var column in meaningfulColumns) {
					var datatype = (DataTypeEmployeeBase)column.DataType;
					datatype.CalculateChange(row, column, uow);
				}
				if(row.HasChanges)
					row.ToSave.Add(row.EditingEmployee);
			}
			progress.Close();
		}
		/*
		public void MakeChange(
			SettingsMatchEmployeesViewModel settings, 
			EmployeeCard employee, 
			SheetRowEmployee row, 
			ExcelColumn column, 
			ChangeType rowChange,
			IUnitOfWork uow)
		{
			var value = row.CellStringValue(column.Index);
			var dataType = column.DataTypeEnum;
			if(String.IsNullOrWhiteSpace(value)) {
				row.AddColumnChange(column, ChangeType.NotChanged);
				return;
			}

			switch(dataType) {
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
				default:
					throw new NotSupportedException($"Тип данных {dataType} не поддерживается.");
			}
		} */
		#endregion
		#region Сопоставление
		public void MatchByName(
			IUnitOfWork uow, 
			IEnumerable<SheetRowEmployee> list, 
			ImportModelEmployee model, 
			IProgressBarDisplayable progress)
		{
			progress.Start(2, text: "Сопоставление с существующими сотрудниками");
			var sizeWillSet = model.ImportedDataTypes.Any(x => x.DataType.Data is SizeType);
			var employeeRepository = new EmployeeRepository(uow);
			var query = employeeRepository.GetEmployeesByFIOs(list.Select(x => GetFIO(x, model)));
			if(sizeWillSet) //Если будем проставлять размеры, запрашиваем сразу имеющиеся размеры для ускорения...
				query.Fetch(SelectMode.Fetch, x => x.Sizes);
			var exists = query.List();
			progress.Add();
			foreach(var employee in exists) {
				var found = list.Where(x => EmployeeParse.CompareFio(employee, GetFIO(x, model))).ToArray();
				if(!found.Any())
					continue; //Так как в базе ищем без отчества, могут быть лишние.
				found.First().Employees.Add(employee);
			}

			progress.Add();
			//Пропускаем дубликаты имен в файле
			var groups = list.GroupBy(x => GetFIO(x, model).GetHash());
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

		public void MatchByNumber(
			IUnitOfWork uow, 
			IEnumerable<SheetRowEmployee> list, 
			ImportModelEmployee model, 
			SettingsMatchEmployeesViewModel settings, 
			IProgressBarDisplayable progress)
		{
			progress.Start(2, text: "Сопоставление с существующими сотрудниками");
			var numberColumn = model.GetColumnForDataType(DataTypeEmployee.PersonnelNumber);
			var numbers = list.Select(x => EmployeeParse.GetPersonalNumber(settings, x, numberColumn))
							.Where(x => !String.IsNullOrWhiteSpace(x))
							.Distinct().ToArray();
			
			var sizeWillSet = model.ImportedDataTypes.Any(x => x.DataType.Data is SizeType);
			var query = uow.Session.QueryOver<EmployeeCard>();
			if(sizeWillSet) //Если будем проставлять размеры, запрашиваем сразу имеющиеся размеры для ускорения...
				query.Fetch(SelectMode.Fetch, x => x.Sizes);
			var exists = query
				.Where(x => x.PersonnelNumber.IsIn(numbers))
				.List();

			progress.Add();
			foreach(var employee in exists) {
				var found = list.Where(x => 
					EmployeeParse.GetPersonalNumber(settings, x, numberColumn) == employee.PersonnelNumber).ToArray();
				found.First().Employees.Add(employee);
			}

			//Пропускаем дубликаты Табельных номеров в файле
			progress.Add();
			var groups = list.GroupBy(x => EmployeeParse.GetPersonalNumber(settings, x, numberColumn));
			foreach(var group in groups) {
				if(String.IsNullOrWhiteSpace(group.Key)) {
					//Если табельного номера нет проверяем по FIO
					MatchByName(uow, group, model, progress);
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
			ImportModelEmployee model, 
			IProgressBarDisplayable progress)
		{
			progress.Start(3, text: "Загружаем подразделения");
			var subdivisionColumn = model.GetColumnForDataType(DataTypeEmployee.Subdivision);
			if(subdivisionColumn != null) {
				var subdivisionNames = list.Select(x => x.CellStringValue(subdivisionColumn)).Distinct().ToArray();
				UsedSubdivisions.AddRange(uow.Session.QueryOver<Subdivision>()
					.Where(x => x.Name.IsIn(subdivisionNames))
					.List());
			}
			progress.Add(text: "Загружаем отделы");
			var departmentColumn = model.GetColumnForDataType(DataTypeEmployee.Department);
			if(departmentColumn != null) {
				var departmentNames = list.Select(x => x.CellStringValue(departmentColumn)).Distinct().ToArray();
				UsedDepartment.AddRange(uow.Session.QueryOver<Department>()
					.Where(x => x.Name.IsIn(departmentNames))
					.List());
			}
			progress.Add(text: "Загружаем должности");
			var postColumn = model.GetColumnForDataType(DataTypeEmployee.Post);
			if(postColumn != null) {
				var postNames = list.Select(x => x.CellStringValue(postColumn)).Distinct().ToArray();
				UsedPosts.AddRange( uow.Session.QueryOver<Post>()
					.Where(x => x.Name.IsIn(postNames))
					.List());
			}
			progress.Close();
		}
		#endregion
		#region Helpers
		public FIO GetFIO(SheetRowEmployee row, ImportModelEmployee model) {
			var fio = new FIO();
			var lastnameColumn = model.GetColumnForDataType(DataTypeEmployee.LastName);
			var firstNameColumn = model.GetColumnForDataType(DataTypeEmployee.FirstName);
			var patronymicColumn = model.GetColumnForDataType(DataTypeEmployee.Patronymic);
			var fioColumn = model.GetColumnForDataType(DataTypeEmployee.Fio);
			if(fioColumn != null)
				row.CellStringValue(fioColumn)?.SplitFullName(out fio.LastName, out fio.FirstName, out fio.Patronymic);
			if(lastnameColumn != null)
				fio.LastName = row.CellStringValue(lastnameColumn);
			if(firstNameColumn != null)
				fio.FirstName = row.CellStringValue(firstNameColumn);
			if(patronymicColumn != null)
				fio.Patronymic = row.CellStringValue(patronymicColumn);
			return fio;
		}
		#endregion
	}
}
