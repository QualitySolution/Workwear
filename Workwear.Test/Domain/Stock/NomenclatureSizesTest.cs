using NUnit.Framework;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Test.Domain.Stock {
	[TestFixture(TestOf = typeof(NomenclatureSizes))]
	public class NomenclatureSizesTest {
		[Test(Description = "Если указан только размер, в заголовке отображается размер.")]
		public void Title_OnlyWearSize() {
			var item = new NomenclatureSizes {
				WearSize = new Size { Name = "52" }
			};

			Assert.That(item.Title, Is.EqualTo("52"));
		}

		[Test(Description = "Если указаны размер и рост, в заголовке отображается комбинация.")]
		public void Title_WearSizeAndHeight() {
			var item = new NomenclatureSizes {
				WearSize = new Size { Name = "52" },
				Height = new Size { Name = "170-176" }
			};

			Assert.That(item.Title, Is.EqualTo("52/170-176"));
		}

		[Test(Description = "Если указан только рост, в заголовке отображается рост.")]
		public void Title_OnlyHeight() {
			var item = new NomenclatureSizes {
				Height = new Size { Name = "170-176" }
			};

			Assert.That(item.Title, Is.EqualTo("170-176"));
		}

		[Test(Description = "Если размер и рост не указаны, заголовок остается пустым.")]
		public void Title_Empty() {
			var item = new NomenclatureSizes();

			Assert.That(item.Title, Is.Empty);
		}
	}
}
