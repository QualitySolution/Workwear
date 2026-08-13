using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Test.Domain.Stock.Documents {
	[TestFixture(TestOf = typeof(Return))]
	public class ReturnTest {
		[Test(Description = "Повторный AddItem с другим штрихкодом для той же выдачи должен добавлять отдельную строку, а не пропускаться как \"уже добавлено\" - иначе при возврате нескольких меток по одной операции теряются.")]
		public void AddItem_EmployeeIssueTwiceWithDifferentBarcodes_AddsTwoSeparateItems() {
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

			var document = new Return();
			document.AddItem(issueOperation, 1, barcodes: new[] { firstBarcode });
			document.AddItem(issueOperation, 1, barcodes: new[] { secondBarcode });

			Assert.That(document.Items, Has.Count.EqualTo(2));
			Assert.That(document.Items[0].BarcodesString, Is.EqualTo(firstBarcode.Title));
			Assert.That(document.Items[1].BarcodesString, Is.EqualTo(secondBarcode.Title));
			Assert.That(document.Items[0].Amount, Is.EqualTo(1));
			Assert.That(document.Items[1].Amount, Is.EqualTo(1));
		}

		[Test(Description = "Повторный AddItem без штрихкодов для той же выдачи по прежнему пропускается как дубликат.")]
		public void AddItem_EmployeeIssueTwiceWithoutBarcodes_SkipsDuplicate() {
			var issueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 2
			};

			var document = new Return();
			document.AddItem(issueOperation, 2);
			document.AddItem(issueOperation, 1);

			Assert.That(document.Items, Has.Count.EqualTo(1));
			Assert.That(document.Items[0].Amount, Is.EqualTo(2));
		}

		[Test(Description = "Повторный AddItem с другим штрихкодом для той же дежурной выдачи должен добавлять отдельную строку, а не пропускаться как \"уже добавлено\".")]
		public void AddItem_DutyNormIssueTwiceWithDifferentBarcodes_AddsTwoSeparateItems() {
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

			var document = new Return();
			document.AddItem(issueOperation, 1, barcodes: new[] { firstBarcode });
			document.AddItem(issueOperation, 1, barcodes: new[] { secondBarcode });

			Assert.That(document.Items, Has.Count.EqualTo(2));
			Assert.That(document.Items[0].BarcodesString, Is.EqualTo(firstBarcode.Title));
			Assert.That(document.Items[1].BarcodesString, Is.EqualTo(secondBarcode.Title));
			Assert.That(document.Items[0].Amount, Is.EqualTo(1));
			Assert.That(document.Items[1].Amount, Is.EqualTo(1));
		}

		[Test(Description = "Повторное добавление той же самой непромаркированной дежурной выдачи по прежнему пропускается.")]
		public void AddItem_DutyNormIssueTwiceWithoutBarcodes_SkipsDuplicate() {
			var issueOperation = new DutyNormIssueOperation {
				DutyNorm = new DutyNorm(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 2
			};

			var document = new Return();
			document.AddItem(issueOperation, 2);
			document.AddItem(issueOperation, 1);

			Assert.That(document.Items, Has.Count.EqualTo(1));
			Assert.That(document.Items[0].Amount, Is.EqualTo(2));
		}

		[Test(Description = "Повторный AddItem с другим штрихкодом для той же выдачи вне нормы должен добавлять отдельную строку, а не пропускаться как \"уже добавлено\".")]
		public void AddItem_OverNormTwiceWithDifferentBarcodes_AddsTwoSeparateItems() {
			var firstBarcode = new Barcode { Title = "400001" };
			var secondBarcode = new Barcode { Title = "400002" };
			var nomenclature = new Nomenclature { Name = "Куртка" };
			var size = new Size { Name = "52" };
			var height = new Size { Name = "182" };
			var issueOperation = new OverNormOperation {
				Employee = new EmployeeCard(),
				Nomenclature = nomenclature,
				WearSize = size,
				Height = height,
				WarehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = new Warehouse(),
					Amount = 2,
					StockPosition = new StockPosition(nomenclature, 0, size, height, null)
				}
			};
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = firstBarcode, OverNormOperation = issueOperation });
			issueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = secondBarcode, OverNormOperation = issueOperation });

			var document = new Return { Warehouse = new Warehouse() };
			document.AddItem(issueOperation, 1, barcodes: new[] { firstBarcode });
			document.AddItem(issueOperation, 1, barcodes: new[] { secondBarcode });

			Assert.That(document.Items, Has.Count.EqualTo(2));
			Assert.That(document.Items[0].BarcodesString, Is.EqualTo(firstBarcode.Title));
			Assert.That(document.Items[1].BarcodesString, Is.EqualTo(secondBarcode.Title));
			Assert.That(document.Items[0].Amount, Is.EqualTo(1));
			Assert.That(document.Items[1].Amount, Is.EqualTo(1));
		}

		[Test(Description = "Повторное добавление той же самой непромаркированной выдачи вне нормы по прежнему пропускается.")]
		public void AddItem_OverNormTwiceWithoutBarcodes_SkipsDuplicate() {
			var nomenclature = new Nomenclature { Name = "Куртка" };
			var issueOperation = new OverNormOperation {
				Employee = new EmployeeCard(),
				Nomenclature = nomenclature,
				WarehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = new Warehouse(),
					Amount = 2,
					StockPosition = new StockPosition(nomenclature, 0, null, null, null)
				}
			};

			var document = new Return { Warehouse = new Warehouse() };
			document.AddItem(issueOperation, 2);
			document.AddItem(issueOperation, 1);

			Assert.That(document.Items, Has.Count.EqualTo(1));
			Assert.That(document.Items[0].Amount, Is.EqualTo(2));
		}

		[Test(Description = "Документ возврата не проходит валидацию, если одна и та же метка добавлена в двух разных строках документа.")]
		public void Validate_SameBarcodeInTwoItems_ReturnsValidationError() {
			var barcode = new Barcode { Title = "100001" };
			var firstIssueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 1
			};
			firstIssueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = barcode, EmployeeIssueOperation = firstIssueOperation });
			var secondIssueOperation = new EmployeeIssueOperation {
				Employee = new EmployeeCard(),
				Nomenclature = new Nomenclature(),
				ProtectionTools = new ProtectionTools(),
				Issued = 1
			};
			secondIssueOperation.BarcodeOperations.Add(new BarcodeOperation { Barcode = barcode, EmployeeIssueOperation = secondIssueOperation });

			var document = new Return();
			document.AddItem(firstIssueOperation, 1, barcodes: new[] { barcode });
			document.AddItem(secondIssueOperation, 1, barcodes: new[] { barcode });
			foreach(var item in document.Items)
				item.MaxAmount = 1;

			var errors = document.Validate(new ValidationContext(document)).ToList();

			Assert.That(errors, Has.Some.Matches<ValidationResult>(x =>
				x.ErrorMessage.Contains("несколько раз указана одна и та же метка")));
		}

		[Test(Description = "Документ возврата без повторяющихся меток проходит эту проверку валидации.")]
		public void Validate_NoDuplicateBarcodes_DoesNotReturnDuplicateValidationError() {
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

			var document = new Return();
			document.AddItem(issueOperation, 1, barcodes: new[] { firstBarcode });
			document.AddItem(issueOperation, 1, barcodes: new[] { secondBarcode });
			foreach(var item in document.Items)
				item.MaxAmount = 2;

			var errors = document.Validate(new ValidationContext(document)).ToList();

			Assert.That(errors, Has.None.Matches<ValidationResult>(x =>
				x.ErrorMessage.Contains("несколько раз указана одна и та же метка")));
		}
	}
}
