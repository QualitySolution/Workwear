using System;
using QS.DomainModel.Entity;

namespace workwear.Models.Import {
	public class ExcelValueTarget : PropertyChangedBase
	{
		public ExcelValueTarget(ExcelColumn column, int level) {
			Column = column ?? throw new ArgumentNullException(nameof(column));
			Level = level;
		}

		public ExcelColumn Column { get; }
		
		public int Level { get; }
		
		private DataType dataType;
		public DataType DataType {
			get => dataType;
			set {
				SetField(ref dataType, value);
			}
		}
	}
}
