﻿using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace Workwear.Test.Integration.Stock
{
	[TestFixture(TestOf = typeof(Expense), Description = "Выдача сотруднику")]
	public class ExpenseEmployeeIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}
		[SetUp]
		public void TestSetup() { }
		[TearDown]
		public void TestTearDown() { }
		[Test(Description = "Корректно обрабатываем выдачу одной номенклатуры несколько раз за день. Реальный баг.")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateOperations_IssuingMultipleRows_TwoNomenclatureSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var sizeType = new SizeType();
				var heightType = new SizeType();
				uow.Save(sizeType);
				uow.Save(heightType);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры",
					SizeType = sizeType
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);
				
				var size = new Size{SizeType = sizeType};
				var height = new Size {SizeType = heightType};
				uow.Save(size);
				uow.Save(height);

				var position1 = new StockPosition(nomenclature, 0, size, height, null);

				var nomenclature2 = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature2);

				var position2 = new StockPosition(nomenclature2, 0, size, height, null);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				protectionTools.AddNomenclature(nomenclature2);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var employeeSize = new EmployeeSize {Size = size, SizeType = sizeType, Employee = employee};
				var employeeHeight = new EmployeeSize {Size = height, SizeType = heightType, Employee = employee};
				
				employee.Sizes.Add(employeeSize);
				employee.Sizes.Add(employeeHeight);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature2, ask);
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow);
				uow.Save(income);

				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22),
					IssueDate = new DateTime(2018, 10, 22)
				};
				var expenseItem1 = expense.AddItem(position1, 1);
				expenseItem1.EmployeeCardItem = employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools);
				expenseItem1.ProtectionTools = protectionTools;
				
				var expenseItem2 = expense.AddItem(position2, 1);
				expenseItem2.EmployeeCardItem = employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools);
				expenseItem2.ProtectionTools = protectionTools;

				var baseParameters = Substitute.For<BaseParameters>();
				baseParameters.DefaultAutoWriteoff.Returns(true);
				baseParameters.ColDayAheadOfShedule.Returns(0);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				expense.UpdateEmployeeWearItems();

				//Тут ожидаем предложение перенести дату использование второй номенклатуры на год.
				ask.ReceivedWithAnyArgs().Question(String.Empty);

				Assert.That(employee.WorkwearItems[0].NextIssue,
					Is.EqualTo(new DateTime(2020, 10, 22))
				);
			}
		}

		[Test(Description = "Не увеличивать дату при повторных вызовах. Реальный баг.")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateOperations_DoNotIncreaseDateWhenRepeatedCallsTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.DefaultAutoWriteoff.Returns(true);
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);
				
				var sizeType = new SizeType();
				var heightType = new SizeType();
				uow.Save(sizeType);
				uow.Save(heightType);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры",
					SizeType = sizeType,
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);
				
				var size = new Size{SizeType = sizeType};
				var height = new Size {SizeType = heightType};
				uow.Save(size);
				uow.Save(height);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var position1 = new StockPosition(nomenclature, 0, size, height, null);
				var position2 = new StockPosition(nomenclature, 0, size,height, null);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow);
				uow.Save(income);

				var income2 = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2018, 1, 1)
				};
				var incomeItem2 = income2.AddItem(nomenclature, ask);
				incomeItem2.WearSize = size;
				incomeItem2.Amount = 5;
				income2.UpdateOperations(uow);
				uow.Save(income2);

				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22),
					IssueDate = new DateTime(2018, 10, 22),
				};
				var expenseItem1 = expense.AddItem(position1, 1);
				expenseItem1.EmployeeCardItem = employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools);
				expenseItem1.ProtectionTools = protectionTools;
				
				var expenseItem2 = expense.AddItem(position2, 1);
				expenseItem2.EmployeeCardItem = employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools);
				expenseItem2.ProtectionTools = protectionTools;

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask); //Здесь 2020 
				uow.Save(expense);
				uow.Commit();

				expense.UpdateOperations(uow, baseParameters, ask); //Здесь 2022(неправильно)
				uow.Save(expense);
				uow.Commit();

				expense.UpdateOperations(uow, baseParameters, ask); //Здесь 2024(неправильно)
				uow.Save(expense);
				uow.Commit();

				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
				employee.UpdateNextIssue(expense.Items.Select(x => x.ProtectionTools).ToArray());

				//Тут ожидаем предложение перенести дату использование второй номенклатуры на год.
				ask.ReceivedWithAnyArgs().Question(String.Empty);

				Assert.That(employee.WorkwearItems[0].NextIssue,
					Is.EqualTo(new DateTime(2020, 10, 22))
				);
			}
		}

		[Test(Description = "Убеждаемся что корректно рассчитываем дату следующей выдачи при норме в 1 месяц. " +
		                    "При разных id. Реальный баг был в том что проверялись id не тех сущностей, " +
		                    "но вы тестах id одинаковые, поэтому тесты работали..")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateEmployeeWearItems_NextIssueDiffIdsTest()
		{
			NewSessionWithSameDB();
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);
				
				var sizeType = new SizeType();
				var heightType = new SizeType();
				uow.Save(sizeType);
				uow.Save(heightType);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры",
					SizeType = sizeType
				};
				uow.Save(nomenclatureType);
				

				//Поднимаем id номенклатуры до 2.
				uow.Save(new Nomenclature());

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);
				
				var size = new Size{SizeType = sizeType};
				var height = new Size {SizeType = heightType};
				uow.Save(size);
				uow.Save(height);

				var position1 = new StockPosition(nomenclature, 0, size, height, null);

				//Поднимаем id сиза до 3.
				uow.Save(new ProtectionTools { Name = "Id = 1" });
				uow.Save(new ProtectionTools { Name = "Id = 2" });

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow);
				uow.Save(income);

				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22),
					IssueDate = new DateTime(2018, 10, 22)
				};
				var expenseItem1 = expense.AddItem(position1, 1);
				expenseItem1.EmployeeCardItem = employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools);
				expenseItem1.ProtectionTools = protectionTools;

				var baseParameters = Substitute.For<BaseParameters>();
				baseParameters.ColDayAheadOfShedule.Returns(0);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				expense.UpdateEmployeeWearItems();
				uow.Commit();

				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot()) {
					var employeeTest = uow2.GetById<EmployeeCard>(employee.Id);
					Assert.That(employeeTest.WorkwearItems[0].NextIssue, Is.EqualTo(new DateTime(2018, 11, 22)));
				}
			}
		}

		[Test(Description = "Убеждаемся что на созданную выдачу можем добавить ведомость.")]
		[Category("Integrated")]
		public void IssueAndCreateIssuanceSheetTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);
				
				var sizeType = new SizeType();
				var heightType = new SizeType();
				uow.Save(sizeType);
				uow.Save(heightType);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры",
					SizeType = sizeType,
					HeightType = heightType
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);
				
				var size = new Size{SizeType = sizeType};
				var height = new Size {SizeType = heightType};
				uow.Save(size);
				uow.Save(height);

				var position1 = new StockPosition(nomenclature, 0, size, height, null);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow);
				uow.Save(income);

				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22),
					IssueDate = new DateTime(2018, 10, 22),
				};
				var expenseItem = expense.AddItem(position1, 1);

				expense.CreateIssuanceSheet(null, null, null);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				expense.UpdateIssuanceSheet();
				uow.Save(expense.IssuanceSheet);
				uow.Save(expense);
				uow.Commit();
				expense.UpdateEmployeeWearItems();
				uow.Commit();

				Assert.That(expense.IssuanceSheet, Is.Not.Null);
				var issuanceItem = expense.IssuanceSheet.Items.First();
				Assert.That(issuanceItem.ExpenseItem, Is.EqualTo(expenseItem));
			}
		}
	}
}
