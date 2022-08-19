
using QS.DomainModel.UoW;

namespace workwear.Models.Import.Employees.DataTypes {
	public abstract class DataTypeEmployeeBase : DataType {
		protected DataTypeEmployeeBase(object data = null, int? order = null) : base(data, order)
		{
		}

		public abstract void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow);
	}
}
