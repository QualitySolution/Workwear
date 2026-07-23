using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Stock;
using Workwear.Tools;

namespace Workwear.Test.Domain.Stock.Documents {
	[TestFixture(TestOf = typeof(OverNorm))]
	public class OverNormTest {
		[Test(Description = "Документ выдачи вне нормы не должен содержать один штрихкод в нескольких строках.")]
		public void Validate_DuplicateBarcode_ReturnValidationError()
		{
			var barcode = new Barcode { Title = "100001" };
			var nomenclature = new Nomenclature();
			var document = new OverNorm {
				Warehouse = new Warehouse()
			};
			document.AddItem(CreateOperation(barcode, nomenclature));
			document.AddItem(CreateOperation(barcode, nomenclature));

			var errors = document.Validate(new ValidationContext(document)).ToList();

			Assert.That(errors.Select(x => x.ErrorMessage), Has.Some.Contains("несколько раз добавлены"));
			Assert.That(errors.Select(x => x.ErrorMessage), Has.Some.Contains(barcode.Title));
		}

		[Test(Description = "При включенной проверке остатков документ выдачи вне нормы должен проверять количество на складе.")]
		public void Validate_AmountGreaterThanStockBalance_ReturnValidationError()
		{
			var nomenclature = new Nomenclature { Name = "Куртка" };
			var warehouse = new Warehouse();
			var document = new OverNorm {
				Warehouse = warehouse,
				Date = new DateTime(2026, 7, 16)
			};
			document.AddItem(CreateOperation(null, nomenclature, 2));

			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.CheckBalances.Returns(true);
			var stockRepository = Substitute.For<StockRepository>();
			stockRepository.StockBalances(
					warehouse,
					Arg.Any<IEnumerable<Nomenclature>>(),
					document.Date,
					Arg.Any<IEnumerable<WarehouseOperation>>())
				.Returns(new List<StockBalanceDTO> {
					new StockBalanceDTO {
						Nomenclature = nomenclature,
						Amount = 1
					}
				});

			var errors = document.Validate(
				new ValidationContext(document, null, new Dictionary<object, object> {
					{ nameof(BaseParameters), baseParameters },
					{ nameof(StockRepository), stockRepository }
				})).ToList();

			Assert.That(errors.Select(x => x.ErrorMessage), Has.Some.Contains("Недостаточное количество"));
			Assert.That(errors.Select(x => x.ErrorMessage), Has.Some.Contains(nomenclature.Name));
		}

		private static OverNormOperation CreateOperation(Barcode barcode, Nomenclature nomenclature, int amount = 1)
		{
			var operation = new OverNormOperation {
				Employee = new EmployeeCard(),
				WarehouseOperation = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = amount
				}
			};
			if(barcode == null)
				return operation;

			var barcodeOperation = new BarcodeOperation {
				Barcode = barcode,
				OverNormOperation = operation
			};
			operation.BarcodeOperations.Add(barcodeOperation);
			barcode.BarcodeOperations.Add(barcodeOperation);
			return operation;
		}
	}
}
