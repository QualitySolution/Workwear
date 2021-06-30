using System;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Stock;

namespace WorkwearTest.Stock
{
	[TestFixture(TestOf = typeof(Writeoff))]
	public class WriteoffTest
	{
		[Test(Description = "Мы должны иметь возможность изменять процент износа.")]
		public void UpdateOperations_WarehouseOperation_CanChangeWearPercentTest()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var nomeclature = Substitute.For<Nomenclature>();

			var warehouse = Substitute.For<Warehouse>();

			var position = new StockPosition(nomeclature, null, null, 0.2m);

			var writeoff = new Writeoff();
			writeoff.Date = new DateTime(2019, 1, 15);
			writeoff.AddItem(position, warehouse, 2);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(0.2m));
			//Меняем значение процента износа
			writeoff.Items[0].WearPercent = 2;

			//Выполняем
			writeoff.UpdateOperations(uow);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(2));
		}

		[Test(Description = "Мы должны иметь возможность изменять процент износа.")]
		public void UpdateOperations_SubdivisionWriteoffOperation_CanChangeWearPercentTest()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var nomeclature = Substitute.For<Nomenclature>();

			var subdivision = Substitute.For<Subdivision>();

			var issueOperation = new SubdivisionIssueOperation {
				OperationTime = new DateTime(2019, 1, 1),
				StartOfUse = new DateTime(2019, 1, 1),
				Issued = 10,
				Nomenclature = nomeclature,
				WearPercent = 0,
				ExpiryOn = new DateTime(2019, 1, 15),
				Subdivision = subdivision
			};

			var writeoff = new Writeoff();
			writeoff.Date = new DateTime(2019, 1, 15);
			writeoff.AddItem(issueOperation, 2);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(1));
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

			var nomeclature = Substitute.For<Nomenclature>();

			var employee = Substitute.For<EmployeeCard>();

			var issueOperation = new EmployeeIssueOperation {
				OperationTime = new DateTime(2019, 1, 1),
				StartOfUse = new DateTime(2019, 1, 1),
				Issued = 10,
				Nomenclature = nomeclature,
				WearPercent = 0,
				ExpiryByNorm = new DateTime(2019, 1, 15),
				Employee = employee
			};

			var writeoff = new Writeoff();
			writeoff.Date = new DateTime(2019, 1, 15);
			writeoff.AddItem(issueOperation, 2);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(1));
			//Меняем значение процента износа
			writeoff.Items[0].WearPercent = 2;

			//Выполняем
			writeoff.UpdateOperations(uow);

			Assert.That(writeoff.Items[0].WearPercent, Is.EqualTo(2));
		}
	}
}
