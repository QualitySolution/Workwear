namespace Workwear.Models.Import.Norms.DataTypes {
	public abstract class DataTypeNormBase : DataType {

		protected DataTypeNormBase(object data = null, int? order = null) : base(data, order)
		{
		}
		public abstract void CalculateChange(SheetRowNorm row, ExcelValueTarget target);
	}
}
