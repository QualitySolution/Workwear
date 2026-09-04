using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Stock;
using Workwear.Tools.OverNorms;

namespace Workwear.Test.Integration.Stock {
	[TestFixture(TestOf = typeof(OverNorm), Description = "Типовая работа документа выдачи вне нормы")]
	public class OverNormIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase {
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Гостевая выдача по штрихкоду: путь OverNormViewModel.SelectEmployees → AddEmployees → AddWithBarcode → AddOrUpdateItem → Save().")]
		[Category("Integrated")]
		public void Save_GuestMode_FillsOperationAndReducesWarehouseStock() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse { Name = "Гостевой склад" };
				uow.Save(warehouse);

				var employee = new EmployeeCard { FirstName = "Пётр", LastName = "Гостев" };
				uow.Save(employee);

				var itemsType = new ItemsType { Name = "Куртки" };
				uow.Save(itemsType);

				var nomenclature = new Nomenclature { Name = "Куртка гостевая", Type = itemsType, UseBarcode = true };
				uow.Save(nomenclature);

				var barcode = new Barcode { Title = "000000000201", Nomenclature = nomenclature };
				uow.Save(barcode);

				var incomeWO = new WarehouseOperation {
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
					Amount = 3,
					OperationTime = new DateTime(2026, 8, 1)
				};
				uow.Save(incomeWO);
				uow.Commit();

				var model = new OverNormFactory().CreateModel(uow, OverNormType.Guest);
				var document = new OverNorm {
					Warehouse = warehouse,
					Type = OverNormType.Guest,
					Date = new DateTime(2026, 8, 20)
				};
				
				var item = document.AddItem(new OverNormOperation { Employee = employee });
				//по сути прямое заполнение param - эмитация выбранного пользователем пользователем
				var param = new OverNormParam(employee, nomenclature, 1, barcodes: new List<Barcode> { barcode });
				model.UseBarcodes = true;
				model.UpdateOperation(item, param);
				
////Пока не очевидно насколко оправдано и надёжно длля тестирования
				//эмитация OverNormViewModel.Save()
				foreach(OverNormItem docItem in document.Items) {
					docItem.OverNormOperation.WarehouseOperation.OperationTime = document.Date;
					uow.Save(docItem.OverNormOperation.WarehouseOperation);
					docItem.OverNormOperation.OperationTime = document.Date;
					uow.Save(docItem.OverNormOperation);
					foreach(var bo in docItem.OverNormOperation.BarcodeOperations)
						uow.Save(bo);
				}
				uow.Save(document);
				uow.Commit();

				Assert.That(document.Id, Is.GreaterThan(0));
				Assert.That(item.OverNormOperation.Type, Is.EqualTo(OverNormType.Guest));
				Assert.That(item.OverNormOperation.Employee.Id, Is.EqualTo(employee.Id));
				Assert.That(item.Barcodes.Select(x => x.Id), Is.EquivalentTo(new[] { barcode.Id }));

				var balance = new StockRepository(uow)
					.StockBalances(warehouse, new[] { nomenclature }, document.Date)
					.SingleOrDefault(x => x.NomenclatureId == nomenclature.Id);
				Assert.That(balance?.Amount ?? 0, Is.EqualTo(2), "После гостевой выдачи 1 шт. из 3 на складе должно остаться 2.");
			}
		}

		[Test(Description = "Разовая выдача без штрихкодов: путь OverNormViewModel.SelectEmployees → AddEmployees → AddNomenclature (ветка CanUseWithoutBarcodes) → Save().")]
		[Category("Integrated")]
		public void Save_SimpleMode_FillsOperationReducesStockAndForbidsWriteOff() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse { Name = "Склад ремонта" };
				uow.Save(warehouse);

				var employee = new EmployeeCard { FirstName = "Сергей", LastName = "Ремонтов" };
				uow.Save(employee);

				var itemsType = new ItemsType { Name = "Роба" };
				uow.Save(itemsType);

				var nomenclature = new Nomenclature { Name = "Роба ремонтная", Type = itemsType, UseBarcode = false };
				uow.Save(nomenclature);

				var income = new WarehouseOperation {
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
					Amount = 5,
					OperationTime = new DateTime(2026, 8, 1)
				};
				uow.Save(income);
				uow.Commit();

				var model = new OverNormFactory().CreateModel(uow, OverNormType.Simple);
				var document = new OverNorm {
					Warehouse = warehouse,
					Type = OverNormType.Simple,
					Date = new DateTime(2026, 8, 20)
				};
				var item = document.AddItem(new OverNormOperation { Employee = employee });

				var param = new OverNormParam(employee, nomenclature, 2);
				model.UseBarcodes = false;
				model.UpdateOperation(item, param);

				foreach(OverNormItem docItem in document.Items) {
					docItem.OverNormOperation.WarehouseOperation.OperationTime = document.Date;
					uow.Save(docItem.OverNormOperation.WarehouseOperation);
					docItem.OverNormOperation.OperationTime = document.Date;
					uow.Save(docItem.OverNormOperation);
				}
				uow.Save(document);
				uow.Commit();

				Assert.That(document.Id, Is.GreaterThan(0));
				Assert.That(item.OverNormOperation.Type, Is.EqualTo(OverNormType.Simple));
				Assert.That(item.Amount, Is.EqualTo(2));
				//Разовые операции принципиально не подлежат списанию — часть типового поведения этого режима.
				Assert.Throws<InvalidOperationException>(() => model.WriteOffOperation(item.OverNormOperation, warehouse));

				var balance = new StockRepository(uow)
					.StockBalances(warehouse, new[] { nomenclature }, document.Date)
					.SingleOrDefault(x => x.NomenclatureId == nomenclature.Id);
				Assert.That(balance?.Amount ?? 0, Is.EqualTo(3), "После разовой выдачи 2 шт. из 5 на складе должно остаться 3.");
			}
		}

		[Test(Description = "Подменная выдача: сотруднику временно выдаётся вещь из подменного фонда взамен уже выданной ему — путь OverNormViewModel.SelectEmployeeIssue → AddNomenclatureFromEmployee → AddWithBarcode → Save().")]
		[Category("Integrated")]
		public void Save_SubstituteMode_FillsOperationLinksIssueAndReducesWarehouseStock() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse { Name = "Подменный фонд" };
				uow.Save(warehouse);

				var employee = new EmployeeCard { FirstName = "Игорь", LastName = "Подменов" };
				uow.Save(employee);

				var itemsType = new ItemsType { Name = "Куртки" };
				uow.Save(itemsType);

				var nomenclature = new Nomenclature { Name = "Куртка утеплённая", Type = itemsType, UseBarcode = true };
				uow.Save(nomenclature);

				var substituteBarcode = new Barcode { Title = "000000000301", Nomenclature = nomenclature };
				uow.Save(substituteBarcode);

				//Вещь, уже числящаяся за сотрудником, которую подменяем на время ремонта/стирки его основной.
				var originalIssueOperation = new EmployeeIssueOperation {
					Employee = employee,
					Nomenclature = nomenclature,
					Issued = 1,
					Returned = 0
				};
				uow.Save(originalIssueOperation);

				var income = new WarehouseOperation {
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
					Amount = 2,
					OperationTime = new DateTime(2026, 8, 1)
				};
				uow.Save(income);
				uow.Commit();

				var model = new OverNormFactory().CreateModel(uow, OverNormType.Substitute);
				var document = new OverNorm {
					Warehouse = warehouse,
					Type = OverNormType.Substitute,
					Date = new DateTime(2026, 8, 20)
				};
				
				var item = document.AddItem(new OverNormOperation {
					Employee = employee,
					SubstitutedIssueOperation = originalIssueOperation
				});

				var param = new OverNormParam(
					employee,
					nomenclature,
					1,
					employeeIssueOperation: originalIssueOperation,
					barcodes: new List<Barcode> { substituteBarcode });
				model.UseBarcodes = true;
				model.UpdateOperation(item, param);

				foreach(OverNormItem docItem in document.Items) {
					docItem.OverNormOperation.WarehouseOperation.OperationTime = document.Date;
					uow.Save(docItem.OverNormOperation.WarehouseOperation);
					docItem.OverNormOperation.OperationTime = document.Date;
					uow.Save(docItem.OverNormOperation);
					foreach(var bo in docItem.OverNormOperation.BarcodeOperations)
						uow.Save(bo);
				}
				uow.Save(document);
				uow.Commit();

				Assert.That(document.Id, Is.GreaterThan(0));
				Assert.That(item.OverNormOperation.Type, Is.EqualTo(OverNormType.Substitute));
				Assert.That(item.OverNormOperation.SubstitutedIssueOperation.Id, Is.EqualTo(originalIssueOperation.Id));
				Assert.That(item.Barcodes.Select(x => x.Id), Is.EquivalentTo(new[] { substituteBarcode.Id }));

				var balance = new StockRepository(uow)
					.StockBalances(warehouse, new[] { nomenclature }, document.Date)
					.SingleOrDefault(x => x.NomenclatureId == nomenclature.Id);
				Assert.That(balance?.Amount ?? 0, Is.EqualTo(1), "После подменной выдачи 1 шт. из 2 на складе должна остаться 1.");
			}
		}
	}
}
