using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using workwear.ViewModels.Tools;

namespace workwear.Models.Import
{
	public abstract class SheetRowBase<TDataTypeEnum>
		where TDataTypeEnum : System.Enum
	{
		private readonly IRow cells;

		public SheetRowBase(IRow cells)
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
				return ExcelLoadViewModelBase<TDataTypeEnum>.ColorOfSkiped;

			if(ChangedColumns.Any(x => x.Index == col)) {
				if(NeedCreateItem)
					return ExcelLoadViewModelBase<TDataTypeEnum>.ColorOfNew;
				else
					return ExcelLoadViewModelBase<TDataTypeEnum>.ColorOfChanged;
			}
			return null;
		}

		protected abstract bool NeedCreateItem {get;}

		public bool Skiped;

		public List<ImportedColumn<TDataTypeEnum>> ChangedColumns = new List<ImportedColumn<TDataTypeEnum>>();
	}
}
