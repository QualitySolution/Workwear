using System;
using System.Linq;
using NUnit.Framework;
using workwear.Measurements;

namespace WorkwearTest.Measurements
{
	[TestFixture(TestOf = typeof(SizeHelper))]
	public class SizeMatchTest
	{
		[Test(Description = "Проверяем что находим через соответсвие свой же размер, если размер один и некому больше не соответствует.")]
		public void MatchSize_AppropriatedSelfAloneSizeTest()
		{
			var sizeLists = SizeHelper.MatchSize(SizeStandartWomenWear.Rus, "38", SizeUsePlace.Сlothes);
			Assert.That(sizeLists.Any(x => x.Size == "38" && x.StandardCode == SizeHelper.GetSizeStdCode(SizeStandartWomenWear.Rus)));
		}

		[Test(Description = "Проверяем что при указани диапазона вернутся отдельные размеры.")]
		public void MatchSize_WhenMatchbyRangeFindSingleSizeTest()
		{
			var sizeLists = SizeHelper.MatchSize(SizeStandartWomenWear.Rus, "48-50", SizeUsePlace.Сlothes);
			Assert.That(sizeLists.Any(x => x.Size == "48-50" && x.StandardCode == SizeHelper.GetSizeStdCode(SizeStandartWomenWear.Rus)), "Нет 48-50");
			Assert.That(sizeLists.Any(x => x.Size == "50" && x.StandardCode == SizeHelper.GetSizeStdCode(SizeStandartWomenWear.Rus)), "Нет 50");
			Assert.That(sizeLists.Any(x => x.Size == "48" && x.StandardCode == SizeHelper.GetSizeStdCode(SizeStandartWomenWear.Rus)), "Нет 48");
		}
	}
}
