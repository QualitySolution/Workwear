using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools;

namespace Workwear.Test.Domain.Stock.Documents
{
	[TestFixture(TestOf = typeof(Writeoff))]
	public class WriteoffTest
	{
		[Test(Description = "Мы должны иметь возможность изменять процент износа.")]
		public void UpdateOperations_WarehouseOperation_CanChangeWearPercentTest()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var nomenclature = Substitute.For<Nomenclature>();

			var warehouse = Substitute.For<Warehouse>();

			var position = new StockPosition(nomenclature, 0.2m, new Size(), new Size(), null);

			var writeoff = new Writeoff {
				Date = new DateTime(2019, 1, 15)
			};
			writeoff.AddItem(position, warehouse, 2);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(0.2m));
			//Меняем значение процента износа
			writeoff.Items[0].WearPercent = 2;

			//Выполняем
			writeoff.UpdateOperations(uow);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(2));
		}

		[Test(Description = "Мы должны иметь возможность изменять процент износа.")]
		public void UpdateOperations_EmployeeIssueOperation_CanChangeWearPercentTest()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var nomenclature = Substitute.For<Nomenclature>();

			var employee = Substitute.For<EmployeeCard>();

			var issueOperation = new EmployeeIssueOperation {
				OperationTime = new DateTime(2019, 1, 1),
				StartOfUse = new DateTime(2019, 1, 1),
				Issued = 10,
				Nomenclature = nomenclature,
				WearPercent = 0,
				ExpiryByNorm = new DateTime(2019, 1, 15),
				Employee = employee
			};

			var writeoff = new Writeoff {
				Date = new DateTime(2019, 1, 15)
			};
			writeoff.AddItem(issueOperation, 2);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(1));
			//Меняем значение процента износа
			writeoff.Items[0].WearPercent = 2;

			//Выполняем
			writeoff.UpdateOperations(uow);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(2));
		}

		[Test(Description = "Операция списания с дежурной нормы должна ссылаться на правильную строку нормы")]
		public void UpdateOperations_DutyNormIssueOperation_ChildOperationInheritsDutyNormItem()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var protectionTools = new ProtectionTools();
			var dutyNormItem = new DutyNormItem { ProtectionTools = protectionTools, Amount = 2 };
			var issueOperation = new DutyNormIssueOperation {
				DutyNorm = new DutyNorm(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = protectionTools,
				DutyNormItem = dutyNormItem,
				Issued = 2
			};

			var writeoff = new Writeoff { Date = new DateTime(2019, 1, 15) };
			writeoff.AddItem(issueOperation, 1);

			writeoff.UpdateOperations(uow);

			Assert.That(writeoff.Items[0].DutyNormWriteOffOperation.DutyNormItem, Is.SameAs(dutyNormItem));
		}

		[Test(Description = "Списание с дежурной нормы должно уменьшать числящееся.")]
		public void UpdateOperations_DutyNormIssueOperation_DecreasesIssuedAmountAndUpdatesNextIssue()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var dutyNorm = new DutyNorm();
			var protectionTools = new ProtectionTools();
			var dutyNormItem = new DutyNormItem { DutyNorm = dutyNorm, ProtectionTools = protectionTools, Amount = 2 };
			var issueOperation = new DutyNormIssueOperation {
				DutyNorm = dutyNorm,
				Nomenclature = new Nomenclature(),
				ProtectionTools = protectionTools,
				DutyNormItem = dutyNormItem,
				OperationTime = new DateTime(2019, 1, 1),
				Issued = 2
			};

			dutyNormItem.Graph = new IssueGraph(new List<IGraphIssueOperation> { issueOperation });
			dutyNormItem.UpdateNextIssue();
			Assert.That(dutyNormItem.Issued(new DateTime(2019, 1, 1)), Is.EqualTo(2));
			Assert.That(dutyNormItem.NextIssue, Is.Null);

			var writeoff = new Writeoff { Date = new DateTime(2019, 1, 15) };
			writeoff.AddItem(issueOperation, 1);
			writeoff.UpdateOperations(uow);

			dutyNormItem.Graph = new IssueGraph(new List<IGraphIssueOperation> {
				issueOperation, writeoff.Items[0].DutyNormWriteOffOperation
			});
			dutyNormItem.UpdateNextIssue();

			Assert.That(dutyNormItem.Issued(new DateTime(2019, 1, 15)), Is.EqualTo(1),
				"Числящееся должно уменьшиться на списанное количество.");
			Assert.That(dutyNormItem.NextIssue, Is.EqualTo(new DateTime(2019, 1, 15)),
				"После списания выданного недостаточно для нормы - должна появиться дата следующей выдачи.");
		}

		[Test(Description = "При списании с сотрудника с выбранными штрихкодами в операцию списания попадают только выбранные коды, без привязки к складу.")]
		public void AddItem_EmployeeIssueWithSelectedBarcode_AddsOnlySelectedBarcodeWithoutWarehouseOperation()
		{
			var selectedBarcode = new Barcode { Title = "100001" };
			var otherBarcode = new Barcode { Title = "100002" };
			var issueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 2
			};
			var selectedBarcodeOperation = new BarcodeOperation { Barcode = selectedBarcode, EmployeeIssueOperation = issueOperation };
			var otherBarcodeOperation = new BarcodeOperation { Barcode = otherBarcode, EmployeeIssueOperation = issueOperation };
			issueOperation.BarcodeOperations.Add(selectedBarcodeOperation);
			issueOperation.BarcodeOperations.Add(otherBarcodeOperation);

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };

			writeoff.AddItem(issueOperation, 1, new[] { selectedBarcode });

			Assert.That(writeoff.Items[0].EmployeeWriteoffOperation.BarcodeOperations, Has.Count.EqualTo(1));
			Assert.That(writeoff.Items[0].EmployeeWriteoffOperation.BarcodeOperations.Single().Barcode, Is.SameAs(selectedBarcode));
			Assert.That(writeoff.Items[0].EmployeeWriteoffOperation.BarcodeOperations.Single().WarehouseOperation, Is.Null);
			Assert.That(writeoff.Items[0].BarcodesString, Is.EqualTo(selectedBarcode.Title));
			Assert.That(writeoff.Items[0].CanEditAmount, Is.False);
		}

		[Test(Description = "При списании прямо со склада с выбранными штрихкодами операция склада проставляется только как расход")]
		public void AddItem_WarehouseWithSelectedBarcode_AddsWarehouseBarcodeOperationWithExpenseOnly()
		{
			var barcode = new Barcode { Title = "200001" };
			var nomenclature = new Nomenclature();
			var warehouse = new Warehouse();
			var position = new StockPosition(nomenclature, 0, new Size(), new Size(), null);

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };

			writeoff.AddItem(position, warehouse, 1, new[] { barcode });

			Assert.That(writeoff.Items[0].WarehouseBarcodeOperations, Has.Count.EqualTo(1));
			var barcodeOperation = writeoff.Items[0].WarehouseBarcodeOperations.Single();
			Assert.That(barcodeOperation.Barcode, Is.SameAs(barcode));
			Assert.That(barcodeOperation.WarehouseOperation, Is.SameAs(writeoff.Items[0].WarehouseOperation));
			Assert.That(barcodeOperation.WarehouseOperation.ExpenseWarehouse, Is.SameAs(warehouse));
			Assert.That(barcodeOperation.WarehouseOperation.ReceiptWarehouse, Is.Null);
			Assert.That(writeoff.Items[0].BarcodesString, Is.EqualTo(barcode.Title));
			Assert.That(writeoff.Items[0].CanEditAmount, Is.False);
		}

		[Test(Description = "Если промаркировано меньше, чем запрошено к списанию со склада - непромаркированный остаток должен добавляться отдельной строкой, а не пропускаться как \"уже добавленная позиция\".")]
		public void AddItem_WarehouseSamePositionWithAndWithoutBarcodes_AddsTwoSeparateItems()
		{
			var barcode = new Barcode { Title = "200001" };
			var nomenclature = new Nomenclature();
			var warehouse = new Warehouse();
			var position = new StockPosition(nomenclature, 0, new Size(), new Size(), null);

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };

			writeoff.AddItem(position, warehouse, 1, new[] { barcode });
			writeoff.AddItem(position, warehouse, 3);

			Assert.That(writeoff.Items, Has.Count.EqualTo(2));
			Assert.That(writeoff.Items[0].CanEditAmount, Is.False);
			Assert.That(writeoff.Items[0].Amount, Is.EqualTo(1));
			Assert.That(writeoff.Items[1].CanEditAmount, Is.True);
			Assert.That(writeoff.Items[1].Amount, Is.EqualTo(3));
		}

		[Test(Description = "Повторный AddItem с другим штрихкодом для той же выдачи сотруднику должен добавлять отдельную строку списания, а не пропускаться как \"уже добавлено\".")]
		public void AddItem_EmployeeIssueTwiceWithDifferentBarcodes_AddsTwoSeparateItems()
		{
			var firstBarcode = new Barcode { Title = "100001" };
			var secondBarcode = new Barcode { Title = "100002" };
			var issueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 2
			};
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = firstBarcode, EmployeeIssueOperation = issueOperation });
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = secondBarcode, EmployeeIssueOperation = issueOperation });

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };
			writeoff.AddItem(issueOperation, 1, new[] { firstBarcode });
			writeoff.AddItem(issueOperation, 1, new[] { secondBarcode });

			Assert.That(writeoff.Items, Has.Count.EqualTo(2));
			Assert.That(writeoff.Items[0].BarcodesString, Is.EqualTo(firstBarcode.Title));
			Assert.That(writeoff.Items[1].BarcodesString, Is.EqualTo(secondBarcode.Title));
		}

		[Test(Description = "Повторное добавление той же самой непромаркированной выдачи сотруднику по прежнему пропускается.")]
		public void AddItem_EmployeeIssueTwiceWithoutBarcodes_SkipsDuplicate()
		{
			var issueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 2
			};

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };
			writeoff.AddItem(issueOperation, 2);
			writeoff.AddItem(issueOperation, 1);

			Assert.That(writeoff.Items, Has.Count.EqualTo(1));
			Assert.That(writeoff.Items[0].Amount, Is.EqualTo(2));
		}

		[Test(Description = "Повторный AddItem с другим штрихкодом для той же дежурной выдачи должен добавлять отдельную строку списания, а не пропускаться как \"уже добавлено\".")]
		public void AddItem_DutyNormIssueTwiceWithDifferentBarcodes_AddsTwoSeparateItems()
		{
			var firstBarcode = new Barcode { Title = "300001" };
			var secondBarcode = new Barcode { Title = "300002" };
			var issueOperation = new DutyNormIssueOperation {
				DutyNorm = new DutyNorm(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 2
			};
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = firstBarcode, DutyNormIssueOperation = issueOperation });
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = secondBarcode, DutyNormIssueOperation = issueOperation });

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };
			writeoff.AddItem(issueOperation, 1, new[] { firstBarcode });
			writeoff.AddItem(issueOperation, 1, new[] { secondBarcode });

			Assert.That(writeoff.Items, Has.Count.EqualTo(2));
			Assert.That(writeoff.Items[0].BarcodesString, Is.EqualTo(firstBarcode.Title));
			Assert.That(writeoff.Items[1].BarcodesString, Is.EqualTo(secondBarcode.Title));
		}

		[Test(Description = "Повторное добавление той же самой непромаркированной дежурной выдачи по прежнему пропускается.")]
		public void AddItem_DutyNormIssueTwiceWithoutBarcodes_SkipsDuplicate()
		{
			var issueOperation = new DutyNormIssueOperation {
				DutyNorm = new DutyNorm(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 2
			};

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };
			writeoff.AddItem(issueOperation, 2);
			writeoff.AddItem(issueOperation, 1);

			Assert.That(writeoff.Items, Has.Count.EqualTo(1));
			Assert.That(writeoff.Items[0].Amount, Is.EqualTo(2));
		}

		[Test(Description = "Повторное добавление той же самой непромаркированной позиции склада по прежнему пропускается.")]
		public void AddItem_WarehouseSamePositionTwiceWithoutBarcodes_SkipsDuplicate()
		{
			var nomenclature = new Nomenclature();
			var warehouse = new Warehouse();
			var position = new StockPosition(nomenclature, 0, new Size(), new Size(), null);

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };

			writeoff.AddItem(position, warehouse, 2);
			writeoff.AddItem(position, warehouse, 3);

			Assert.That(writeoff.Items, Has.Count.EqualTo(1));
			Assert.That(writeoff.Items[0].Amount, Is.EqualTo(2));
		}

		[Test(Description = "AddBarcode дозаписывает ещё один отсканированный код в уже существующую строку складского списания и увеличивает количество.")]
		public void AddBarcode_ExistingWarehouseWriteoffItem_AddsBarcodeAndIncreasesAmount()
		{
			var firstBarcode = new Barcode { Title = "200001" };
			var secondBarcode = new Barcode { Title = "200002" };
			var nomenclature = new Nomenclature();
			var warehouse = new Warehouse();
			var position = new StockPosition(nomenclature, 0, new Size(), new Size(), null);

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };
			writeoff.AddItem(position, warehouse, 1, new[] { firstBarcode });

			writeoff.Items[0].AddBarcode(secondBarcode);

			Assert.That(writeoff.Items[0].Amount, Is.EqualTo(2));
			Assert.That(writeoff.Items[0].WarehouseBarcodeOperations.Select(x => x.Barcode), Is.EquivalentTo(new[] { firstBarcode, secondBarcode }));
		}

		[Test(Description = "Документ списания не проходит валидацию, если количество строки не равно количеству выбранных штрихкодов.")]
		public void Validate_WriteoffItemWithBarcodesAndDifferentAmount_ReturnsValidationError()
		{
			var barcode = new Barcode { Title = "100001" };
			var issueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature { Name = "Куртка" },
				ProtectionTools = new ProtectionTools(),
				Issued = 1
			};
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = barcode, EmployeeIssueOperation = issueOperation });

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };
			writeoff.AddItem(issueOperation, 1, new[] { barcode });
			writeoff.Items[0].Amount = 2;
			writeoff.Items[0].MaxAmount = 2;

			var errors = writeoff.Validate(new ValidationContext(writeoff, new Dictionary<object, object> {
				{ nameof(BaseParameters), Substitute.For<BaseParameters>() }
			})).ToList();

			Assert.That(errors, Has.Some.Matches<ValidationResult>(x =>
				x.ErrorMessage.Contains("количество должно быть равно количеству выбранных штрихкодов")));
		}

		[Test(Description = "Документ списания не проходит валидацию, если одна и та же метка добавлена в двух разных строках документа.")]
		public void Validate_SameBarcodeInTwoItems_ReturnsValidationError() {
			var barcode = new Barcode { Title = "100001" };
			var firstIssueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature { Name = "Куртка" },
				ProtectionTools = new ProtectionTools(),
				Issued = 1
			};
			firstIssueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = barcode, EmployeeIssueOperation = firstIssueOperation });
			var secondIssueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature { Name = "Куртка" },
				ProtectionTools = new ProtectionTools(),
				Issued = 1
			};
			secondIssueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = barcode, EmployeeIssueOperation = secondIssueOperation });

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };
			writeoff.AddItem(firstIssueOperation, 1, new[] { barcode });
			writeoff.AddItem(secondIssueOperation, 1, new[] { barcode });
			foreach(var item in writeoff.Items)
				item.MaxAmount = 1;

			var errors = writeoff.Validate(new ValidationContext(writeoff, new Dictionary<object, object> {
				{ nameof(BaseParameters), Substitute.For<BaseParameters>() }
			})).ToList();

			Assert.That(errors, Has.Some.Matches<ValidationResult>(x =>
				x.ErrorMessage.Contains("несколько раз указана одна и та же метка")));
		}

		[Test(Description = "Документ списания без повторяющихся меток проходит эту проверку валидации.")]
		public void Validate_NoDuplicateBarcodes_DoesNotReturnDuplicateValidationError() {
			var firstBarcode = new Barcode { Title = "100001" };
			var secondBarcode = new Barcode { Title = "100002" };
			var issueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature { Name = "Куртка" },
				ProtectionTools = new ProtectionTools(),
				Issued = 2
			};
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = firstBarcode, EmployeeIssueOperation = issueOperation });
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = secondBarcode, EmployeeIssueOperation = issueOperation });

			var writeoff = new Writeoff { Date = new DateTime(2024, 1, 15) };
			writeoff.AddItem(issueOperation, 1, new[] { firstBarcode });
			writeoff.AddItem(issueOperation, 1, new[] { secondBarcode });
			foreach(var item in writeoff.Items)
				item.MaxAmount = 2;

			var errors = writeoff.Validate(new ValidationContext(writeoff, new Dictionary<object, object> {
				{ nameof(BaseParameters), Substitute.For<BaseParameters>() }
			})).ToList();

			Assert.That(errors, Has.None.Matches<ValidationResult>(x =>
				x.ErrorMessage.Contains("несколько раз указана одна и та же метка")));
		}
	}
}
