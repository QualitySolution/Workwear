using System;
using System.Collections.Generic;
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
	[TestFixture(TestOf = typeof(ProtectionToolsForecastingModel))]
	public class ProtectionToolsForecastingModelTest : WarehouseForecastingModelTestBase {
		[Test(Description = "В прогнозе по номенклатуре нормы размер строки остаётся размером сотрудника.")]
		public void MakeForecastingItems_EmployeeSizeSuitableForNomenclatureSize_KeepsEmployeeSize() {
			var sizeType = new SizeType { Id = 1, Name = "Размер перчаток", CategorySizeType = CategorySizeType.Size };
			var employeeSize10 = new Size { Id = 10, Name = "10", SizeType = sizeType, ShowInEmployee = true };
			var nomenclatureSize10Plus = new Size { Id = 1010, Name = "10+", SizeType = sizeType, ShowInNomenclature = true };
			employeeSize10.SuitableSizes.Add(nomenclatureSize10Plus);

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

			var stockBalance = new StockBalanceModel();
			SetStockBalances(stockBalance, new StockBalance(new StockPosition(nomenclature, 0, nomenclatureSize10Plus, null, null), 96));

			var columnsModel = Substitute.For<IForecastColumnsModel>();
			columnsModel.ForecastColumns.Returns(new[] {
				new ForecastColumn {
					StartDate = new DateTime(2026, 6, 15),
					EndDate = new DateTime(2026, 6, 21)
				}
			});

			var sizeService = new SizeService();
			SetCachedSizes(sizeService, employeeSize10, nomenclatureSize10Plus);
			var model = new ProtectionToolsForecastingModel(stockBalance, columnsModel, sizeService);
			var futureIssues = new List<FutureIssue> {
				new TestFutureIssue(protectionTools, employeeSize10)
			};

			var result = model.MakeForecastingItems(Substitute.For<IProgressBarDisplayable>(), futureIssues);

			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].Size, Is.SameAs(employeeSize10));
			Assert.That(result[0].InStock, Is.EqualTo(96));
			Assert.That(result[0].Forecast[0], Is.EqualTo(1));
		}
	}
}
