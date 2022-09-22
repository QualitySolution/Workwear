using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.Models.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeFio : DataTypeEmployeeBase {
		private readonly PersonNames personNames;
		private int lenghtOfLastName, lenghtOfFirstName, lenghtOfPatronymic;

		public DataTypeFio(PersonNames personNames)
		{
			this.personNames = personNames ?? throw new ArgumentNullException(nameof(personNames));
			ColumnNameKeywords.AddRange(new []{				
				"фио",
				"ф.и.о.",
				"сотрудник",
				"наименование"//Встречается при выгрузке из 1C
			});
			ColumnNameRegExp = "фамилия.+имя.+отчество";
			Data = DataTypeEmployee.Fio;
			lenghtOfLastName = TextParser.GetMaxStringLenght<EmployeeCard>(nameof(EmployeeCard.LastName));
			lenghtOfFirstName = TextParser.GetMaxStringLenght<EmployeeCard>(nameof(EmployeeCard.FirstName));
			lenghtOfPatronymic = TextParser.GetMaxStringLenght<EmployeeCard>(nameof(EmployeeCard.Patronymic));
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowEmployee row, string value) {
			if(String.IsNullOrWhiteSpace(value))
				return new ChangeState(ChangeType.NotChanged);
			
			var employee = row.EditingEmployee;

			value.SplitFullName(out var lastName, out var firstName, out var patronymic);
			var lastDiff = !String.IsNullOrEmpty(lastName) && !EmployeeParse.CompareString(employee.LastName, lastName);
			var firstDiff = !String.IsNullOrEmpty(firstName) && !EmployeeParse.CompareString(employee.FirstName, firstName);
			var patronymicDiff = !String.IsNullOrEmpty(patronymic) && !EmployeeParse.CompareString(employee.Patronymic, patronymic);
			string oldValue = (lastDiff || firstDiff || patronymicDiff) ? employee.FullName : null;
			
			var lenghtErrors = new List<string>();
			if(lastDiff && lastName.Length > lenghtOfLastName) 
				lenghtErrors.Add($@"Длинна фамилии '{lastName}' больше максимальной {lenghtOfLastName}.");
			if(firstDiff && firstName.Length > lenghtOfFirstName) 
				lenghtErrors.Add($@"Длинна имени '{firstName}' больше максимальной {lenghtOfFirstName}.");
			if(patronymicDiff && patronymic.Length > lenghtOfPatronymic) 
				lenghtErrors.Add($@"Длинна отчества '{patronymic}' больше максимальной {lenghtOfPatronymic}.");
			
			if(lenghtErrors.Any())
				return new ChangeState(ChangeType.ParseError, error: String.Join("\n", lenghtErrors));

			if(!lastDiff && !firstDiff && !patronymicDiff)
				return new ChangeState(ChangeType.NotChanged);

			ChangeState state;
			if(row.EditingEmployee.Id == 0)
				state = new ChangeState(ChangeType.NewEntity);
			else
				state = new ChangeState(ChangeType.ChangeValue, oldValue: oldValue);
			
			if(lastDiff)
				row.AddSetValueAction(ValueSetOrder, () => employee.LastName = lastName);
			if(firstDiff) {
				row.AddSetValueAction(ValueSetOrder, () => employee.FirstName = firstName);
				if(employee.Sex == Sex.None) {
					var detectedSex = personNames.GetSexByName(firstName);
					if(detectedSex != Sex.None) {
						row.AddSetValueAction(ValueSetOrder, () => employee.Sex = detectedSex);
						state.AddCreatedValues(detectedSex.GetEnumTitle());
					}
				}
			}
			if(patronymicDiff)
				row.AddSetValueAction(ValueSetOrder, () => employee.Patronymic = patronymic);

			return state;
		}
		#endregion
	}
}
