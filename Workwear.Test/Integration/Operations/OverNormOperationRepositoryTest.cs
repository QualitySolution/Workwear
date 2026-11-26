using System;
using System.Linq;
using NUnit.Framework;
using QS.Testing.DB;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Operations;

namespace Workwear.Test.Integration.Operations {
	[TestFixture(TestOf = typeof(OverNormOperationRepository))]
	public class OverNormOperationRepositoryTest : InMemoryDBGlobalConfigTestFixtureBase {
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "После возврата одного штрихкода из выдачи вне нормы остальные штрихкоды той же операции остаются действующими для заявок.")]
		[Category("Integrated")]
		public void GetActualIssuedOperations_PartialBarcodeReturn_ReturnsOperationForNotReturnedBarcodeClaim()
		{
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse { Name = "Склад" };
				var employee = new EmployeeCard { FirstName = "Иван", LastName = "Иванов" };
				var nomenclatureType = new ItemsType { Name = "Одежда" };
				var nomenclature = new Nomenclature {
					Name = "Куртка",
					Type = nomenclatureType,
					UseBarcode = true
				};
				var returnedBarcode = new Barcode {
					Title = "000000000001",
					Nomenclature = nomenclature
				};
				var activeBarcode = new Barcode {
					Title = "000000000002",
					Nomenclature = nomenclature
				};
				var activeClaim = new ServiceClaim {
					Employee = employee,
					Barcode = activeBarcode,
					IsClosed = false
				};

				uow.Save(warehouse);
				uow.Save(employee);
				uow.Save(nomenclatureType);
				uow.Save(nomenclature);
				uow.Save(returnedBarcode);
				uow.Save(activeBarcode);
				uow.Save(activeClaim);

				var warehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					OperationTime = new DateTime(2026, 7, 1),
					Amount = 2,
					Nomenclature = nomenclature
				};
				uow.Save(warehouseOperation);

				var issuedOperation = new OverNormOperation {
					Type = OverNormType.Guest,
					Employee = employee,
					Nomenclature = nomenclature,
					OperationTime = warehouseOperation.OperationTime,
					WarehouseOperation = warehouseOperation
				};
				var returnedBarcodeOperation = new BarcodeOperation {
					Barcode = returnedBarcode,
					OverNormOperation = issuedOperation,
					WarehouseOperation = warehouseOperation
				};
				var activeBarcodeOperation = new BarcodeOperation {
					Barcode = activeBarcode,
					OverNormOperation = issuedOperation,
					WarehouseOperation = warehouseOperation
				};
				issuedOperation.BarcodeOperations.Add(returnedBarcodeOperation);
				issuedOperation.BarcodeOperations.Add(activeBarcodeOperation);
				returnedBarcode.BarcodeOperations.Add(returnedBarcodeOperation);
				activeBarcode.BarcodeOperations.Add(activeBarcodeOperation);
				uow.Save(issuedOperation);
				uow.Commit();

				var returnDocument = new Return {
					Warehouse = warehouse,
					Date = new DateTime(2026, 7, 5)
				};
				returnDocument.AddItem(issuedOperation, 1, barcodes: new[] { returnedBarcode });
				returnDocument.UpdateOperations(uow);
				uow.Save(returnDocument);
				uow.Commit();

				var result = new OverNormOperationRepository(uow)
					.GetActualIssuedOperations(new[] { activeClaim }, uow);

				Assert.That(result.Keys, Is.EquivalentTo(new[] { activeClaim.Id }));
				Assert.That(result[activeClaim.Id].Id, Is.EqualTo(issuedOperation.Id));
				Assert.That(result[activeClaim.Id].BarcodeOperations.Select(x => x.Barcode.Id), Does.Contain(activeBarcode.Id));
			}
		}
	}
}
