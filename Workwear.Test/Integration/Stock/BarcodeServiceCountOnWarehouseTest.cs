using System;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Operations;
using Workwear.Tools;
using Workwear.Tools.Barcodes;

namespace Workwear.Test.Integration.Stock
{
	[TestFixture(TestOf = typeof(BarcodeService), Description = "Подсчёт промаркированного остатка на складе")]
	public class BarcodeServiceCountOnWarehouseTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Штрихкод, уже выданный сотруднику, не должен считаться промаркированным остатком на складе, " +
			"даже если у него в истории есть более ранняя операция приёмки на этот склад.")]
		[Category("Integrated")]
		public void CountBarcodesOnWarehouse_BarcodeIssuedToEmployee_NotCountedAsInStockAnymore()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			var barcodeService = new BarcodeService(baseParameters, new EmployeeIssueRepository());

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType { Name = "Тестовый тип номенклатуры" };
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature { Type = nomenclatureType, UseBarcode = true };
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools { Name = "СИЗ для тестирования" };
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);

				var position = new StockPosition(nomenclature, 0, null, null, null);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2020, 1, 1)
				};
				var incomeItem = income.AddItem(nomenclature, ask);
				incomeItem.Amount = 2;
				income.UpdateOperations(uow);
				uow.Save(income);
				uow.Commit();

				//Маркируем 1 единицу из поступивших - как это делает BarcodingViewModel.LoadFromWidget:
				//создаётся отдельная операция приёмки на тот же склад, к которой привязывается штрихкод.
				var markingReceipt = new WarehouseOperation {
					StockPosition = position,
					ReceiptWarehouse = warehouse,
					Amount = 1,
					OperationTime = new DateTime(2020, 1, 2)
				};
				uow.Save(markingReceipt);

				var barcode = new Barcode { Type = BarcodeTypes.EAN13, Title = "2000000000001", Nomenclature = nomenclature };
				uow.Save(barcode);
				var markingOperation = new BarcodeOperation { Barcode = barcode, WarehouseOperation = markingReceipt };
				uow.Save(markingOperation);
				uow.Commit();

				Assert.That(
					barcodeService.CountBarcodesOnWarehouse(uow, position, warehouse),
					Is.EqualTo(1),
					"Сразу после маркировки штрихкод должен считаться промаркированным остатком на складе.");

				//Выдаём промаркированную единицу сотруднику - штрихкод физически покидает склад.
				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2020, 1, 5),
					IssueDate = new DateTime(2020, 1, 5)
				};
				var expenseItem = expense.AddItem(position, 1);
				expense.UpdateOperations(uow, baseParameters, ask);
				expense.SaveOperations(uow);
				uow.Save(expense);

				var issueOperation = new BarcodeOperation { Barcode = barcode, EmployeeIssueOperation = expenseItem.EmployeeIssueOperation };
				uow.Save(issueOperation);
				uow.Commit();

				Assert.That(
					barcodeService.CountBarcodesOnWarehouse(uow, position, warehouse),
					Is.EqualTo(0),
					"Штрихкод уже выдан сотруднику - старая операция приёмки на склад не должна давать ложный остаток.");
			}
		}
	}
}
