using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using QS.DomainModel.Entity;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public abstract class SheetRowBase<TRow> : PropertyChangedBase, ISheetRow
		where TRow : SheetRowBase<TRow>
	{
		private readonly IRow[] cells;
		
		public IDictionary<int, ICell[]>  MergedCells;

		public SheetRowBase(IRow[] cells)
		{
			this.cells = cells;
		}

		#region Получение значений ячейки
		readonly Dictionary<int, string> cachedValues = new Dictionary<int, string>();
		/// <summary>
		/// Получает значение ячейки видимое пользователю.
		/// </summary>
		public string CellValue(int col)
		{
			//Используем кешированные значения, так как графика для отрисовки очень часто дергает этот метод.
			if(cachedValues.ContainsKey(col))
				return cachedValues[col];

			string value;
			var cell = GetCellForValue(col, cells.Length - 1);
			if(cell?.CellType == CellType.Blank)
				value = null;
			if(cell?.CellType == CellType.Error && cell.ErrorCellValue == 0)
				value = null;
			value = cell?.ToString();
			cachedValues[col] = value;
			return value;
		}
		
		public string CellStringValue(ExcelValueTarget valueTarget) {
			return CellStringValue(valueTarget.Column.Index, valueTarget.Level);
		}

		public string CellStringValue(int col, int? level)
		{
			var cell = GetCellForValue(col, level);

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

		public int? CellIntValue(ExcelValueTarget valueTarget) {
			return CellIntValue(valueTarget.Column.Index, valueTarget.Level);
		}
		
		public int? CellIntValue(int col, int? level)
		{
			var cell = GetCellForValue(col, level);

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

		public DateTime? CellDateTimeValue(ExcelValueTarget valueTarget) {
			return CellDateTimeValue(valueTarget.Column.Index, valueTarget.Level);
		}
		
		public DateTime? CellDateTimeValue(int col, int? level)
		{
			var cell = GetCellForValue(col, level);

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
		
		private ICell GetCellForValue(int col, int? level) {
			var row = level.HasValue ? cells[level.Value] : cells.Last();
			return MergedCells[row.RowNum][col] ?? row.GetCell(col);
		}
		
		#endregion

		public int RowLevel => cells.Last().OutlineLevel;

		#region Цвет ячейки
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
		#endregion

		public bool Skipped => ProgramSkipped || UserSkipped;

		#region Работа с изменениями

		public Dictionary<ExcelColumn, ChangeState> ChangedColumns = new Dictionary<ExcelColumn, ChangeState>();

		public void AddColumnChange(ExcelValueTarget column, ChangeType changeType, string oldValue = null) {
			AddColumnChange(column, new ChangeState(changeType, oldValue));
		}

		public void AddColumnChange(ExcelValueTarget column, ChangeState state) {
			if(column.Level + 1 == cells.Length)
				ChangedColumns[column.Column] = state;
		}

		public string CellTooltip(int col) {
			List<string> result = new List<string>();
			var column = ChangedColumns.Keys.FirstOrDefault(x => x.Index == col);
			if(column != null) {
				var change = ChangedColumns[column];
				if(change.Error != null)
					result.Add($"Ошибка: {change.Error}");
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

		#region Сохранение
		/// <summary>
		/// В коллекции действия имеют порядок применения чтобы процесс обработки данных был контролируемый.
		/// Это надо для того чтобы при наличие 2 полей с похожими данными заполнялись в правильном порядке.
		/// Например чтобы отдельное поле с фамилией могло перезаписать значение фамилии поученной из общего поля ФИО.
		/// </summary>
		public List<SetValueAction> SetValueActions = new List<SetValueAction>();

		public void AddSetValueAction(int order, Action action) {
			SetValueActions.Add(new SetValueAction(order, action));
		}

		public readonly List<object> ToSave = new List<object>();

		public virtual IEnumerable<object> PrepareToSave() {
			foreach(var action in SetValueActions.OrderBy(x => x.Order)) {
				action.Action();
			}
			return ToSave;
		}
		#endregion
	}
}
