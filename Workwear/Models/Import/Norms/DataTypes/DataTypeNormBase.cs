namespace workwear.Models.Import.Norms.DataTypes {
	public abstract class DataTypeNormBase : DataType {

		public abstract void CalculateChange(SheetRowNorm row, ExcelValueTarget target);
	}
}
