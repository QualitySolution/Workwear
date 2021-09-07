using System;
using System.Linq;
using NUnit.Framework;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Repository.Operations;

namespace WorkwearTest.Integration.Operations
{
	[TestFixture(TestOf = typeof(EmployeeIssueRepository), Description = "Репозитория информации о выдачах сотруднику")]
	public class EmployeeIssueRepositoryTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что запрос не захватывает операции до даты выдачи.")]
		[Category("Integrated")]
		public void GetOperationsTouchDates_GetNotOperationBeforeAndAfterSelectedDateTest()
		{

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);

				//Операция без номеклатуры
				var opBefore = new EmployeeIssueOperation();
				opBefore.OperationTime = new DateTime(2018, 1, 1, 14, 0, 0);
				opBefore.AutoWriteoffDate = new DateTime(2020, 1, 1);
				opBefore.Employee = employee;
				opBefore.Nomenclature = nomenclature;
				opBefore.ProtectionTools = protectionTools;
				opBefore.Issued = 1;
				uow.Save(opBefore);

				var opInRange = new EmployeeIssueOperation();
				opInRange.OperationTime = new DateTime(2019, 1, 1, 13, 0, 0);
				opInRange.AutoWriteoffDate = new DateTime(2021, 1, 1);
				opInRange.Employee = employee;
				opInRange.Nomenclature = nomenclature;
				opInRange.ProtectionTools = protectionTools;
				opInRange.Issued = 1;
				uow.Save(opInRange);

				var opAfter = new EmployeeIssueOperation();
				opAfter.OperationTime = new DateTime(2021, 1, 1, 13, 0, 0);
				opAfter.AutoWriteoffDate = new DateTime(2021, 5, 1);
				opAfter.Employee = employee;
				opAfter.Nomenclature = nomenclature;
				opAfter.ProtectionTools = protectionTools;
				opAfter.Issued = 1;
				uow.Save(opInRange);

				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.GetOperationsByDates(new EmployeeCard[] { employee }, new DateTime(2018, 12, 30), new DateTime(2020, 1, 1));
				Assert.That(result.Count, Is.EqualTo(1));
				Assert.That(result.First().OperationTime, Is.EqualTo(new DateTime(2019, 1, 1, 13, 0, 0)));
			}
		}
		
		[Test(Description = "Проверяем что можем захватить операцию если период установлен одиним днем.")]
		[Category("Integrated")]
		public void GetOperationsTouchDates_OneDateRangeTest()
		{

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);
				
				var opBefore = new EmployeeIssueOperation();
				opBefore.OperationTime = new DateTime(2018, 12, 31, 14, 0, 0);
				opBefore.AutoWriteoffDate = new DateTime(2020, 1, 1);
				opBefore.Employee = employee;
				opBefore.Nomenclature = nomenclature;
				opBefore.ProtectionTools = protectionTools;
				opBefore.Issued = 1;
				uow.Save(opBefore);

				var opInRange = new EmployeeIssueOperation();
				opInRange.OperationTime = new DateTime(2019, 1, 1, 0, 0, 0);
				opInRange.AutoWriteoffDate = new DateTime(2021, 1, 1);
				opInRange.Employee = employee;
				opInRange.Nomenclature = nomenclature;
				opInRange.ProtectionTools = protectionTools;
				opInRange.Issued = 1;
				uow.Save(opInRange);

				var opInRange2 = new EmployeeIssueOperation();
				opInRange2.OperationTime = new DateTime(2019, 1, 1, 13, 0, 0);
				opInRange2.AutoWriteoffDate = new DateTime(2021, 2, 1);
				opInRange2.Employee = employee;
				opInRange2.Nomenclature = nomenclature;
				opInRange2.ProtectionTools = protectionTools;
				opInRange2.Issued = 2;
				uow.Save(opInRange2);
				
				var opAfter = new EmployeeIssueOperation();
				opAfter.OperationTime = new DateTime(2019, 1, 2, 13, 0, 0);
				opAfter.AutoWriteoffDate = new DateTime(2021, 5, 1);
				opAfter.Employee = employee;
				opAfter.Nomenclature = nomenclature;
				opAfter.ProtectionTools = protectionTools;
				opAfter.Issued = 1;
				uow.Save(opInRange);

				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.GetOperationsByDates(new EmployeeCard[] { employee }, new DateTime(2019, 1, 1), new DateTime(2019, 1, 1));
				Assert.That(result.Any(x => x.OperationTime == new DateTime(2019, 1, 1, 0, 0, 0)), Is.True);
				Assert.That(result.Any(x => x.OperationTime == new DateTime(2019, 1, 1, 13, 0, 0)), Is.True);
				Assert.That(result.Count, Is.EqualTo(2));
			}
		}
	}
}
