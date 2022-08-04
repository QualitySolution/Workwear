using System;
using workwear.Domain.Company;

namespace workwear.Models.Import
{
	public static class EmployeeParse
	{
		public static string ConvertPersonnelNumber(string cellValue)
		{
			if(int.TryParse(cellValue, out int number))
				return number.ToString();
			else
				return cellValue;
		}
		
		public static string GetPersonalNumber(IMatchEmployeesSettings settings, ISheetRow row, int columnIndex) {
			var original = settings.ConvertPersonnelNumber ? 
				ConvertPersonnelNumber(row.CellStringValue(columnIndex)) : row.CellStringValue(columnIndex);
			return original?.Trim();
		}
		
		public static bool CompareFio(EmployeeCard employee, FIO fio)
		{
			return String.Equals(fio.LastName, employee.LastName, StringComparison.CurrentCultureIgnoreCase)
			       && String.Equals(fio.FirstName, employee.FirstName, StringComparison.CurrentCultureIgnoreCase)
			       && (fio.Patronymic == null || String.Equals(fio.Patronymic, employee.Patronymic, StringComparison.CurrentCultureIgnoreCase));
		}
	}
}
