using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using QS.DomainModel.Entity;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public abstract class SheetRowBase<TDataTypeEnum> : PropertyChangedBase, ISheetRow
		where TDataTypeEnum : System.Enum
	{
		private readonly IRow cells;
		
		public ICell[,] MergedCells;

		public SheetRowBase(IRow cells)
		{
			this.cells = cells;
		}

		#region Получение значений ячейки
		readonly Dictionary<int, string> changedValues = new Dictionary<int, string>();
		/// <summary>
		/// Получает значение ячейки видимое пользователю.
		/// </summary>
		public string CellValue(int col)
		{
			//Используем кешированные значения, так как графика для отрисовки очень часто дергает этот метод.
			if(changedValues.ContainsKey(col))
				return changedValues[col];

			string value;
			var cell = GetCellForValue(col);
			if(cell?.CellType == CellType.Blank)
				value = null;
			if(cell?.CellType == CellType.Error && cell.ErrorCellValue == 0)
				value = null;
			value = cell?.ToString();
			changedValues[col] = value;
			return value;
		}

		public string CellStringValue(int col)
		{
			var cell = GetCellForValue(col);

			if(cell != null) {
				// TODO: you can add more cell types compatibility, e. g. formula
				switch(cell.CellType) {
					case NPOI.SS.UserModel.CellType.Numeric:
						return cell.NumericCellValue.ToString();
					case NPOI.SS.UserModel.CellType.String:
						return cell.StringCellValue?.Trim();
				}
			}
			return null;
		}

		public int? CellIntValue(int col)
		{
			var cell = GetCellForValue(col);

			if(cell != null) {
				switch(cell.CellType) {
					case NPOI.SS.UserModel.CellType.Numeric:
						return Convert.ToInt32(cell.NumericCellValue);
					case NPOI.SS.UserModel.CellType.String:
						if(int.TryParse(cell.StringCellValue, out int value))
							return value;
						else
							return null;
				}
			}
			return null;
		}

		public DateTime? CellDateTimeValue(int col)
		{
			var cell = GetCellForValue(col);

			if(cell != null) {
				switch(cell.CellType) {
					case NPOI.SS.UserModel.CellType.Numeric:
						return cell.DateCellValue;
					case NPOI.SS.UserModel.CellType.String:
						if(DateTime.TryParse(cell.StringCellValue, out DateTime value))
							return value;
						else
							return null;
				}
			}
			return null;
		}
		
		private ICell GetCellForValue(int col)
		{
			var cell = MergedCells[cells.RowNum, col];
			if(cell != null) {
				cell = cells.GetCell(col);
			}
			return cell;
		}
		
		#endregion
		
		public string CellBackgroundColor(int col)
		{
			if (UserSkipped)
				return ExcelImportViewModel.ColorOfSkipped;
			
			var column = ChangedColumns.Keys.FirstOrDefault(x => x.Index == col);

			if(column != null) {
				switch(ChangedColumns[column].ChangeType) {
					case ChangeType.NewEntity : return ExcelImportViewModel.ColorOfNew;
					case ChangeType.ChangeValue : return ExcelImportViewModel.ColorOfChanged;
					case ChangeType.NotFound: return ExcelImportViewModel.ColorOfNotFound;
					case ChangeType.ParseError: return ExcelImportViewModel.ColorOfError;
					case ChangeType.NotChanged: break;
					default:
						throw new NotImplementedException("Не известный тип изменения");
				}
			}
			return ProgramSkipped ? ExcelImportViewModel.ColorOfSkipped : null;
		}
		
		public string CellForegroundColor(int col)
		{
			var column = ChangedColumns.Keys.FirstOrDefault(x => x.Index == col);

			if(column != null) {
				if(!String.IsNullOrEmpty(ChangedColumns[column].Warning) ) {
					return ExcelImportViewModel.ColorOfWarning;
				}
			}
			return null;
		}

		public bool Skipped => ProgramSkipped || UserSkipped;

		#region Работа с изменениями

		public Dictionary<ImportedColumn<TDataTypeEnum>, ChangeState> ChangedColumns = new Dictionary<ImportedColumn<TDataTypeEnum>, ChangeState>();

		public void AddColumnChange(ImportedColumn<TDataTypeEnum> column, ChangeType changeType, string oldValue = null)
		{
			ChangedColumns[column] = new ChangeState(changeType, oldValue);
		}

		public string CellTooltip(int col) {
			List<string> result = new List<string>();
			var column = ChangedColumns.Keys.FirstOrDefault(x => x.Index == col);
			if(column != null) {
				var change = ChangedColumns[column];
				if(change.Warning != null)
					result.Add($"Предупреждение: {change.Warning}");
				if(change.OldValue != null)
					result.Add($"Старое значение: {change.OldValue}");
				if(change.InterpretedValue != null)
					result.Add($"Обработанное значение: {change.InterpretedValue}");
				if(change.WillCreatedValues.Any())
					result.Add($"Будут созданы: " + String.Join(", ", change.WillCreatedValues.Select(x => $"[{x}]")));
				
				return result.Any() ? String.Join("\n", result) : null;
			}
			return null;
		}

		public bool HasChanges => !Skipped && ChangedColumns.Any(x => x.Value.ChangeType == ChangeType.ChangeValue || x.Value.ChangeType == ChangeType.NewEntity);

		public bool ProgramSkipped { get; set; }

		private bool userSkipped;

		public virtual bool UserSkipped {
			get => userSkipped;
			set => SetField(ref userSkipped, value);
		}
		#endregion
	}

	public interface ISheetRow
	{
		string CellValue(int col);
		string CellTooltip(int col);
		string CellBackgroundColor(int col);
		string CellForegroundColor(int col);

		bool ProgramSkipped { get; set; }
		bool UserSkipped { get; set; }
	}
}
