using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using workwear.Domain.Company;
using workwear.ViewModels.Tools;

namespace workwear.Tools.Import
{
	public class SheetRow
	{
		private readonly IRow cells;

		public SheetRow(IRow cells)
		{
			this.cells = cells;
		}

		public string CellValue(int col)
		{
			var cell = cells.GetCell(col);

			if(cell != null) {
				// TODO: you can add more cell types capatibility, e. g. formula
				switch(cell.CellType) {
					case NPOI.SS.UserModel.CellType.Numeric:
						return cell.NumericCellValue.ToString();
					case NPOI.SS.UserModel.CellType.String:
						return cell.StringCellValue;
				}
			}
			return null;
		}

		public string CellBackgroundColor(int col)
		{
			if(Skiped)
				return EmployeesLoadViewModel.ColorOfSkiped;

			if(ChangedColumns.Any(x => x.Index == col)) {
				if(Employees.Any())
					return EmployeesLoadViewModel.ColorOfChanged;
				else
					return EmployeesLoadViewModel.ColorOfNew;
			}
			return null;
		}

		#region Сопоставление с сотрудниками

		public List<EmployeeCard> Employees = new List<EmployeeCard>();

		public bool Skiped;

		public List<ImportedColumn> ChangedColumns = new List<ImportedColumn>();

		#endregion
	}
}
