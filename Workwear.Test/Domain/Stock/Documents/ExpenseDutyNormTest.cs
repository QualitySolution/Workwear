using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools;

namespace Workwear.Test.Domain.Stock.Documents
{
	[TestFixture]
	public class ExpenseDutyNormTest
	{
		private static readonly DateTime DocumentDate = new DateTime(2026, 8, 31);

		[Test(Description = "Строка с количеством, но без номенклатуры не даёт сохранить документ, иначе падаем на записи складской операции.")]
		public void Validate_ItemWithAmountAndWithoutNomenclature_ReturnsError()
		{
			var document = new ExpenseDutyNorm {
				Warehouse = new Warehouse(),
				DutyNorm = new DutyNorm(),
				Date = DocumentDate
			};
			document.Items.Add(new ExpenseDutyNormItem {
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = new Nomenclature(),
					ProtectionTools = new ProtectionTools(),
					Issued = 1
				}
			});
			var item = new ExpenseDutyNormItem {
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = new Nomenclature(),
					ProtectionTools = new ProtectionTools(),
					Issued = 1
				}
			};
			document.Items.Add(item);

			var validationContext = new ValidationContext(document, null, new Dictionary<object, object> {
				{ nameof(BaseParameters), Substitute.For<BaseParameters>() }
			});
			var errorsWithNomenclature = document.Validate(validationContext).ToList();

			item.Nomenclature = null;

			var errors = document.Validate(validationContext).ToList();

			Assert.That(errors, Has.Count.EqualTo(errorsWithNomenclature.Count + 1));
		}

		[Test(Description = "Строка без номенклатуры и без количества сохранению не мешает, она будет удалена при записи.")]
		public void Validate_ItemWithoutNomenclatureAndWithoutAmount_ReturnsNoErrors()
		{
			var document = new ExpenseDutyNorm {
				Warehouse = new Warehouse(),
				DutyNorm = new DutyNorm(),
				Date = DocumentDate
			};
			document.Items.Add(new ExpenseDutyNormItem {
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = new Nomenclature(),
					ProtectionTools = new ProtectionTools(),
					Issued = 1
				}
			});
			document.Items.Add(new ExpenseDutyNormItem {
				Document = document,
				Operation = new DutyNormIssueOperation {
					ProtectionTools = new ProtectionTools(),
					Issued = 0
				}
			});

			var errors = document.Validate(new ValidationContext(document, null, new Dictionary<object, object> {
				{ nameof(BaseParameters), Substitute.For<BaseParameters>() }
			})).ToList();

			Assert.That(errors, Is.Empty);
		}

		[Test(Description = "В сохранённом документе количество в строке нельзя обнулить, иначе от строки останутся операции в базе.")]
		public void Validate_SavedItemWithZeroAmount_ReturnsError()
		{
			var nomenclature = new Nomenclature();
			var document = new ExpenseDutyNorm {
				Warehouse = new Warehouse(),
				DutyNorm = new DutyNorm(),
				Date = DocumentDate
			};
			document.Items.Add(new ExpenseDutyNormItem {
				Id = 1,
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = nomenclature,
					ProtectionTools = new ProtectionTools(),
					Issued = 1
				}
			});
			var item = new ExpenseDutyNormItem {
				Id = 2,
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = nomenclature,
					ProtectionTools = new ProtectionTools(),
					Issued = 1
				}
			};
			document.Items.Add(item);

			var validationContext = new ValidationContext(document, null, new Dictionary<object, object> {
				{ nameof(BaseParameters), Substitute.For<BaseParameters>() }
			});
			var errorsWithAmount = document.Validate(validationContext).ToList();

			item.Amount = 0;

			var errors = document.Validate(validationContext).ToList();

			Assert.That(errors, Has.Count.EqualTo(errorsWithAmount.Count + 1));
		}

		[Test(Description = "Новая строка с нулевым количеством сохранению не мешает, она будет удалена при записи.")]
		public void Validate_NewItemWithZeroAmount_ReturnsNoErrors()
		{
			var nomenclature = new Nomenclature();
			var document = new ExpenseDutyNorm {
				Warehouse = new Warehouse(),
				DutyNorm = new DutyNorm(),
				Date = DocumentDate
			};
			document.Items.Add(new ExpenseDutyNormItem {
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = nomenclature,
					ProtectionTools = new ProtectionTools(),
					Issued = 1
				}
			});
			document.Items.Add(new ExpenseDutyNormItem {
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = nomenclature,
					ProtectionTools = new ProtectionTools(),
					Issued = 0
				}
			});

			var errors = document.Validate(new ValidationContext(document, null, new Dictionary<object, object> {
				{ nameof(BaseParameters), Substitute.For<BaseParameters>() }
			})).ToList();

			Assert.That(errors, Is.Empty);
		}

		[Test(Description = "Удаление строки документа удаляет и связанную с ней строку ведомости.")]
		public void RemoveItem_ItemInIssuanceSheet_RemovesIssuanceSheetItem()
		{
			var document = new ExpenseDutyNorm {
				Warehouse = new Warehouse(),
				DutyNorm = new DutyNorm(),
				Date = DocumentDate,
				ResponsibleEmployee = new EmployeeCard()
			};
			var item = new ExpenseDutyNormItem {
				Document = document,
				Operation = new DutyNormIssueOperation {
					Nomenclature = new Nomenclature(),
					ProtectionTools = new ProtectionTools(),
					Issued = 1
				}
			};
			document.Items.Add(item);
			document.IssuanceSheet = new IssuanceSheet { ExpenseDutyNorm = document, Date = DocumentDate };
			document.UpdateIssuanceSheet();
			Assert.That(document.IssuanceSheet.Items, Has.Count.EqualTo(1));

			document.RemoveItem(item);

			Assert.That(document.IssuanceSheet.Items, Is.Empty);
		}
	}
}
