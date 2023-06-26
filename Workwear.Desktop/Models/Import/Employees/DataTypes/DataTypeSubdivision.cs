using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.ViewModels.Import;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeSubdivision : DataTypeEmployeeBase {
		private readonly DataParserEmployee dataParserEmployee;
		private readonly SettingsMatchEmployeesViewModel settings;

		public DataTypeSubdivision(DataParserEmployee dataParserEmployee, SettingsMatchEmployeesViewModel settings)
		{
			this.dataParserEmployee = dataParserEmployee ?? throw new ArgumentNullException(nameof(dataParserEmployee));
			this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
			ColumnNameKeywords.Add("подразделение");
			Data = DataTypeEmployee.Subdivision;
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowEmployee row, string value) {
			if(String.IsNullOrWhiteSpace(value))
				return new ChangeState(ChangeType.NotChanged);
			
			var subdivisionNames = value.Split(new[] { settings.SubdivisionLevelEnable ? settings.SubdivisionLevelSeparator : ""}, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.ToArray();

			if(settings.SubdivisionLevelEnable && settings.SubdivisionLevelReverse)
				subdivisionNames = subdivisionNames.Reverse().ToArray();

			if(EqualsSubdivision(subdivisionNames, row.EditingEmployee.Subdivision)) {
				return new ChangeState(ChangeType.NotChanged);
			}
			var subdivision = GetOrCreateSubdivision(subdivisionNames, dataParserEmployee.UsedSubdivisions, null);
			row.EditingEmployee.Subdivision = subdivision;
			if(subdivision.Id == 0)
				return new ChangeState(ChangeType.NewEntity, willCreatedValues: new[] { "Подразделение:" + subdivision.Name });
			return new ChangeState(ChangeType.ChangeValue, oldValue: row.EditingEmployee.Subdivision?.Name);
		}

		internal static bool EqualsSubdivision(string[] names, Subdivision subdivision) {
			if(names.Length == 0 || subdivision == null)
				return false;
			if(!String.Equals(subdivision.Name, names.Last(), StringComparison.CurrentCultureIgnoreCase))
				return false;
			if(names.Length > 1)
				return EqualsSubdivision(names.Take(names.Length - 1).ToArray(), subdivision.ParentSubdivision);
			return subdivision.ParentSubdivision == null;
		}
		
		internal static Subdivision GetOrCreateSubdivision(string[] names, List<Subdivision> subdivisions, Subdivision parent) {
			var name = names.First();
			var subdivision = subdivisions.FirstOrDefault(x => x.ParentSubdivision == parent 
			                                                                          && String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
			if(subdivision == null) {
				subdivision = new Subdivision {
					Name = name, 
					ParentSubdivision = parent
				};
				subdivisions.Add(subdivision);
			}

			if(names.Length > 1)
				return GetOrCreateSubdivision(names.Skip(1).ToArray(), subdivisions, subdivision);
			else
				return subdivision;
		}
		#endregion
	}
}
