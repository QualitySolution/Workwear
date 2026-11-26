using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Analytics;
using Workwear.Models.Analytics.WarehouseForecasting;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.Test.Models.Analytics.WarehouseForecasting {
	[TestFixture(TestOf = typeof(NomenclatureForecastingModel))]
	public class NomenclatureForecastingModelTest : WarehouseForecastingModelTestBase {
		[Test(Description = "Потребность по размеру сотрудника схлопывается в строку подходящего складского размера.")]
		public void MakeForecastingItems_EmployeeSizesSuitableForNomenclatureSize_CollapsedByNomenclatureSize() {
			var sizeType = new SizeType { Id = 1, Name = "Размер перчаток", CategorySizeType = CategorySizeType.Size };
			var employeeSize10 = new Size { Id = 10, Name = "10", SizeType = sizeType, ShowInEmployee = true };
			var employeeSize11 = new Size { Id = 11, Name = "11", SizeType = sizeType, ShowInEmployee = true };
			var nomenclatureSize10Plus = new Size { Id = 1010, Name = "10+", SizeType = sizeType, ShowInNomenclature = true };
			employeeSize10.SuitableSizes.Add(nomenclatureSize10Plus);
			employeeSize11.SuitableSizes.Add(nomenclatureSize10Plus);

			var itemsType = new ItemsType { Id = 1, Name = "Перчатки", SizeType = sizeType };
			var nomenclature = new Nomenclature {
				Id = 1,
				Name = "Перчатки Блеск CG-941",
				Type = itemsType,
				Sex = ClothesSex.Universal
			};
			nomenclature.NomenclatureSizes.Add(new NomenclatureSizes {
				Nomenclature = nomenclature,
				WearSize = nomenclatureSize10Plus
			});

			var protectionTools = new ProtectionTools {
				Id = 1,
				Name = "Перчатки для защиты от кислот и щелочей Блеск CG-941",
				Type = itemsType,
				SupplyType = SupplyType.Unisex,
				SupplyNomenclatureUnisex = nomenclature
			};
			protectionTools.ProtectionToolsNomenclatures = new ObservableList<ProtectionToolsNomenclature> {
				new ProtectionToolsNomenclature { ProtectionTools = protectionTools, Nomenclature = nomenclature }
			};

			var stockBalance = Substitute.For<StockBalanceModel>();
			stockBalance.Balances.Returns(new[] {
				new StockBalance(new StockPosition(nomenclature, 0, nomenclatureSize10Plus, null, null), 96)
			});

			var columnsModel = Substitute.For<IForecastColumnsModel>();
			columnsModel.ForecastColumns.Returns(new[] {
				new ForecastColumn {
					StartDate = new DateTime(2026, 6, 15),
					EndDate = new DateTime(2026, 6, 21)
				}
			});

			var sizeService = new SizeService();
			SetCachedSizes(sizeService, employeeSize10, employeeSize11, nomenclatureSize10Plus);
			var model = new NomenclatureForecastingModel(stockBalance, columnsModel, sizeService);
			var futureIssues = new List<FutureIssue> {
				new TestFutureIssue(protectionTools, employeeSize10),
				new TestFutureIssue(protectionTools, employeeSize11)
			};

			var result = model.MakeForecastingItems(Substitute.For<IProgressBarDisplayable>(), futureIssues);

			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].Size, Is.SameAs(nomenclatureSize10Plus));
			Assert.That(result[0].InStock, Is.EqualTo(96));
			Assert.That(result[0].Forecast[0], Is.EqualTo(2));
			Assert.That(result[0].WithoutDebt, Is.EqualTo(94));
		}

		[Test(Description = "Если в размерах номенклатуры нет подходящего размера, показываем размер сотрудника.")]
		public void MakeForecastingItems_EmployeeSizeNotSuitableForNomenclatureSizes_KeepsEmployeeSize() {
			var sizeType = new SizeType { Id = 1, Name = "Размер перчаток", CategorySizeType = CategorySizeType.Size };
			var employeeSize10 = new Size { Id = 10, Name = "10", SizeType = sizeType, ShowInEmployee = true };
			var nomenclatureSize7Minus = new Size { Id = 1007, Name = "7-", SizeType = sizeType, ShowInNomenclature = true };

			var itemsType = new ItemsType { Id = 1, Name = "Перчатки", SizeType = sizeType };
			var nomenclature = new Nomenclature {
				Id = 1,
				Name = "Перчатки Блеск CG-941",
				Type = itemsType,
				Sex = ClothesSex.Universal
			};
			nomenclature.NomenclatureSizes.Add(new NomenclatureSizes {
				Nomenclature = nomenclature,
				WearSize = nomenclatureSize7Minus
			});

			var protectionTools = new ProtectionTools {
				Id = 1,
				Name = "Перчатки для защиты от кислот и щелочей Блеск CG-941",
				Type = itemsType,
				SupplyType = SupplyType.Unisex,
				SupplyNomenclatureUnisex = nomenclature
			};
			protectionTools.ProtectionToolsNomenclatures = new ObservableList<ProtectionToolsNomenclature> {
				new ProtectionToolsNomenclature { ProtectionTools = protectionTools, Nomenclature = nomenclature }
			};

			var stockBalance = Substitute.For<StockBalanceModel>();
			stockBalance.Balances.Returns(new[] {
				new StockBalance(new StockPosition(nomenclature, 0, nomenclatureSize7Minus, null, null), 96)
			});

			var columnsModel = Substitute.For<IForecastColumnsModel>();
			columnsModel.ForecastColumns.Returns(new[] {
				new ForecastColumn {
					StartDate = new DateTime(2026, 6, 15),
					EndDate = new DateTime(2026, 6, 21)
				}
			});

			var sizeService = new SizeService();
			SetCachedSizes(sizeService, employeeSize10, nomenclatureSize7Minus);
			var model = new NomenclatureForecastingModel(stockBalance, columnsModel, sizeService);
			var futureIssues = new List<FutureIssue> {
				new TestFutureIssue(protectionTools, employeeSize10)
			};

			var result = model.MakeForecastingItems(Substitute.For<IProgressBarDisplayable>(), futureIssues);

			Assert.That(result, Has.Count.EqualTo(2));
			var forecastItem = result.Single(x => x.Forecast[0] == 1);
			Assert.That(forecastItem.Size, Is.SameAs(employeeSize10));
			Assert.That(forecastItem.InStock, Is.EqualTo(0));
			Assert.That(forecastItem.SizeMatchStatus, Is.EqualTo(SizeMatchStatus.Red));
		}
	}
}
