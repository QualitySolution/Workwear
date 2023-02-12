using System;
using NSubstitute.Core;
using NUnit.Framework;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Test.Domain.Stock {
	public class StockPositionTest {
		[Test(Description = "Проверяем что правильно сравниваем позиции с разными экземплярами одного и того же")]
		public void StockPosition_EqualFullFilledCase() {
			var position1 = new StockPosition(
				new Nomenclature{Id = 5},
				0.5m,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			var position2 = new StockPosition(
				new Nomenclature{Id = 5},
				0.5m,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			Assert.That(position1.Equals(position2), Is.True);
			//Assert.That(position1 == position2, Is.True); Так не работает
			Assert.That(position1.GetHashCode(), Is.EqualTo(position2.GetHashCode()));
		}
		
		[Test(Description = "Проверяем что правильно сравниваем не полностью заполненные позиции")]
		public void StockPosition_EqualPartFilledCase() {
			var position1 = new StockPosition(
				new Nomenclature{Id = 5},
				0,
				null,
				new Size{Id = 20},
				null
			);
			
			var position2 = new StockPosition(
				new Nomenclature{Id = 5},
				0,
				null,
				new Size{Id = 20},
				null
			);
			
			Assert.That(position1.Equals(position2), Is.True);
			//Assert.That(position1 == position2, Is.True); Так не работает
			Assert.That(position1.GetHashCode(), Is.EqualTo(position2.GetHashCode()));
		}
		
		[Test(Description = "Проверяем что позиции без номенклатуры не считаем одинаковыми, так как это единственный обязательный параметр")]
		public void StockPosition_CantCreateWhenNomenclatureIsEmptyCase() {
			Assert.Catch<ArgumentNullException>(() =>
				new StockPosition(
					null,
					0.5m,
					new Size { Id = 15 },
					new Size { Id = 20 },
					new Owner { Id = 100 }
			));
		}
		
		[Test(Description = "Проверяем что правильно сравниваем позиции с разными номенклатурами")]
		public void StockPosition_NotEqualDiffNomenclatureCase() {
			var position1 = new StockPosition(
				new Nomenclature{Id = 50},
				0.5m,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			var position2 = new StockPosition(
				new Nomenclature{Id = 5},
				0.5m,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			Assert.That(position1.Equals(position2), Is.False);
			//Assert.That(position1 == position2, Is.True); Так не работает
			Assert.That(position1.GetHashCode(), Is.Not.EqualTo(position2.GetHashCode()));
		}
		
		[Test(Description = "Проверяем что правильно сравниваем позиции с разными номенклатурами")]
		public void StockPosition_NotEqualDiffSizeCase() {
			var position1 = new StockPosition(
				new Nomenclature{Id = 5},
				0.5m,
				new Size{Id = 150},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			var position2 = new StockPosition(
				new Nomenclature{Id = 5},
				0.5m,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			Assert.That(position1.Equals(position2), Is.False);
			//Assert.That(position1 == position2, Is.True); Так не работает
			Assert.That(position1.GetHashCode(), Is.Not.EqualTo(position2.GetHashCode()));
		}
		
		[Test(Description = "Проверяем что правильно сравниваем позиции с разными номенклатурами")]
		public void StockPosition_NotEqualDiffHeightCase() {
			var position1 = new StockPosition(
				new Nomenclature{Id = 5},
				0.5m,
				new Size{Id = 15},
				new Size{Id = 201},
				new Owner{Id = 100}
			);
			
			var position2 = new StockPosition(
				new Nomenclature{Id = 5},
				0.5m,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			Assert.That(position1.Equals(position2), Is.False);
			//Assert.That(position1 == position2, Is.True); Так не работает
			Assert.That(position1.GetHashCode(), Is.Not.EqualTo(position2.GetHashCode()));
		}
		
		[Test(Description = "Проверяем что правильно сравниваем позиции с разными номенклатурами")]
		public void StockPosition_NotEqualDiffWearPercentCase() {
			var position1 = new StockPosition(
				new Nomenclature{Id = 5},
				0,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			var position2 = new StockPosition(
				new Nomenclature{Id = 5},
				0.5m,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			Assert.That(position1.Equals(position2), Is.False);
			//Assert.That(position1 == position2, Is.True); Так не работает
			Assert.That(position1.GetHashCode(), Is.Not.EqualTo(position2.GetHashCode()));
		}
		
		[Test(Description = "Проверяем что правильно сравниваем позиции с разными номенклатурами")]
		public void StockPosition_NotEqualDiffOwnerCase() {
			var position1 = new StockPosition(
				new Nomenclature{Id = 5},
				1,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 1}
			);
			
			var position2 = new StockPosition(
				new Nomenclature{Id = 5},
				1,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			Assert.That(position1.Equals(position2), Is.False);
			//Assert.That(position1 == position2, Is.True); Так не работает
			Assert.That(position1.GetHashCode(), Is.Not.EqualTo(position2.GetHashCode()));
		}
		
		[Test(Description = "Проверяем что правильно сравниваем позиции с разными номенклатурами")]
		public void StockPosition_NotEqualOneOwnerIsEmptyCase() {
			var position1 = new StockPosition(
				new Nomenclature{Id = 5},
				1,
				new Size{Id = 15},
				new Size{Id = 20},
				null
			);
			
			var position2 = new StockPosition(
				new Nomenclature{Id = 5},
				1,
				new Size{Id = 15},
				new Size{Id = 20},
				new Owner{Id = 100}
			);
			
			Assert.That(position1.Equals(position2), Is.False);
			//Assert.That(position1 == position2, Is.True); Так не работает
			Assert.That(position1.GetHashCode(), Is.Not.EqualTo(position2.GetHashCode()));
		}
	}
}
