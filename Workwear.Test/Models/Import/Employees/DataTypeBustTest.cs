using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using Workwear.Measurements;
using Workwear.Models.Import.Employees.DataTypes;

namespace Workwear.Test.Models.Import.Employees {
	[TestFixture(TestOf = typeof(DataTypeBust))]
	public class DataTypeBustTest {

		[Test(Description = "Тестируем варианты обработки обхвата груди")]
		[TestCase("104", "52")] // Простое соответствие доступному размеру
		[TestCase("112", "56-58")] // 56 недоступен для карточки, поэтому выбран должен быть диапазон
		[TestCase("120", "60")] // Все варианты отключены поэтому выбирается точное соотвествие
		[TestCase("106", "52")] // Реальный баг, не находили размер по верхней границе
		public void ParseValue_Cases(string value, string resultName) {
			var uow = Substitute.For<IUnitOfWork>();
			var sizeType = new SizeType() {
				Id = 2,
				Name = "Размер одежды",
				UseInEmployee = true
			};
			var sizeService = Substitute.For<SizeService>();
			var sizes = GetSizes(sizeType);
			sizeService.GetSize(Arg.Any<IUnitOfWork>(), sizeType)
				.Returns(sizes);
			
			var dataType = new DataTypeBust(sizeService, sizeType);
			var size = dataType.ParseValue(uow, value);
			Assert.That(size.Name, Is.EqualTo(resultName));
		}

		List<Size> GetSizes(SizeType sizeType) {
			var size0 = new Size {
				Name = "52-54",
				SizeType = sizeType,
				ShowInEmployee = true
			};
			var size1 = new Size {
				Name = "52",
				SizeType = sizeType,
				ShowInEmployee = true
			};
			
			var size2 = new Size {
				Name = "56",
				SizeType = sizeType,
				ShowInEmployee = false
			};
			var size3 = new Size {
				Name = "56-58",
				SizeType = sizeType,
				ShowInEmployee = true
			};
			
			var size4 = new Size {
				Name = "60-62",
				SizeType = sizeType,
				ShowInEmployee = false
			};
			var size5 = new Size {
				Name = "60",
				SizeType = sizeType,
				ShowInEmployee = false
			};
			return new List<Size> { size0, size1, size2, size3, size4, size5 };
		}
	}
}
