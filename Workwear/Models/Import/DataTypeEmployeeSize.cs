using System;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using Workwear.Measurements;

namespace workwear.Models.Import {
	public class DataTypeEmployeeSize : DataType {
		private readonly SizeService sizeService;
		private readonly SizeType sizeType;

		public DataTypeEmployeeSize(SizeService sizeService, SizeType sizeType) : base(sizeType) {
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.sizeType = sizeType ?? throw new ArgumentNullException(nameof(sizeType));
			ColumnNameKeywords.Add(sizeType.Name.ToLower());
		}

		public Size ParseValue(IUnitOfWork uow, string value) {
			var size = sizeService
				.GetSize(uow, sizeType)
				.FirstOrDefault(x => 
					x.Name.Trim().ToLower().Equals(value)
					|| value.Equals(x.AlternativeName?.Trim().ToLower()));

			if(size == null && sizeType.Id == 1) {
				//Дополнительно конвертируем рост.
				var interpreted = SizeParser.HeightToGOST(value);
				size = sizeService
					.GetSize(uow, sizeType)
					.FirstOrDefault(x => x.Name.Trim().ToLower().Equals(interpreted));
			}

			return size;
		}
	}
}
