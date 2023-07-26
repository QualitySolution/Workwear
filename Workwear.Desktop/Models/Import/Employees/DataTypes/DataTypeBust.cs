using System;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using Workwear.Tools.Sizes;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypeBust : DataTypeEmployeeSize {
		public DataTypeBust(SizeService sizeService, SizeType sizeType) : base(sizeService, sizeType) {
			ColumnNameRegExp = null;
			ColumnNameKeywords.Add("груди");
		}

		public override string Title => "Обхват груди";

		internal override Size ParseValue(IUnitOfWork uow, string value) {
			var sizeName = SizeParser.BustToSize(value);
			if(String.IsNullOrEmpty(sizeName))
				return null;

			var sizes = sizeService.GetSize(uow, sizeType);
			//Сначала ищем среди возможных для использования в сотруднике, точный размер.
			var size = sizes.Where(x => x.ShowInEmployee).FirstOrDefault(x => x.Name.Equals(sizeName));
			//Если не нашли пробует найти доступные но включая диапазоны
			if(size == null)
				size = sizes.Where(x => x.ShowInEmployee).FirstOrDefault(x => x.Name.Contains(sizeName));
			//Если тоже не получилось ищем в отключенных
			if(size == null)
				size = sizes.FirstOrDefault(x => x.Name.Equals(sizeName));
			return size;
		}
	}
}
