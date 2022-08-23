using System;
using System.Linq;
using NPOI.SS.Formula.Functions;
using QS.Utilities.Text;
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
		
		public static string GetPersonalNumber(IMatchEmployeesSettings settings, ISheetRow row, ExcelValueTarget column) {
			var original = settings.ConvertPersonnelNumber ? 
				ConvertPersonnelNumber(row.CellStringValue(column)) : row.CellStringValue(column);
			return original?.Trim();
		}
		
		public static bool CompareFio(EmployeeCard employee, FIO fio)
		{
			return CompareString(fio.LastName, employee.LastName)
			       && CompareString(fio.FirstName, employee.FirstName)
			       && (fio.Patronymic == null || CompareString(fio.Patronymic, employee.Patronymic));
		}
		
		public static bool CompareNameWithInitials(EmployeeCard employee, string nameWithInitials) {
			if(String.IsNullOrWhiteSpace(nameWithInitials))
				return false;
			nameWithInitials.SplitNameWithInitials(out var lastName, out var firstName, out var patronymic);
			return CompareString(lastName, employee.LastName)
			       && CompareString(firstName, employee.FirstName.FirstOrDefault().ToString())
			       && CompareString(patronymic, employee.Patronymic.FirstOrDefault().ToString());
		}

		public static bool CompareString(string text1, string text2) {
			return String.Equals(ReplaceYo(text1), ReplaceYo(text2), StringComparison.CurrentCultureIgnoreCase);
		}
		private static string ReplaceYo(string value) {
			return value?.Replace('ё', 'е').Replace('Ё', 'Е');
		}
	}
}
