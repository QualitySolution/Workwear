using System;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using workwear.Models.Company;

namespace workwear.Models.Import.Employee.DataTypes {
	public class DataTypeFio : DataTypeEmployeeBase {
		private readonly PersonNames personNames;

		public DataTypeFio(PersonNames personNames)
		{
			this.personNames = personNames ?? throw new ArgumentNullException(nameof(personNames));
			ColumnNameKeywords.AddRange(new []{				
				"ФИО",
				"Ф.И.О.",
				"Фамилия Имя Отчество",
				"Сотрудник",
				"Наименование"//Встречается при выгрузке из 1C
			});
			Data = DataTypeEmployee.Fio;
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
			var lastDiff = !String.IsNullOrEmpty(lastName) && 
			               !String.Equals(employee.LastName, lastName, StringComparison.CurrentCultureIgnoreCase);
			var firstDiff = !String.IsNullOrEmpty(firstName) && 
			                !String.Equals(employee.FirstName, firstName, StringComparison.CurrentCultureIgnoreCase);
			var patronymicDiff = !String.IsNullOrEmpty(patronymic) && 
			                     !String.Equals(employee.Patronymic, patronymic, StringComparison.CurrentCultureIgnoreCase);
			string oldValue = (lastDiff || firstDiff || patronymicDiff) ? employee.FullName : null;
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
					var detectedSex = personNames.GetSexByName(employee.FirstName);
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
