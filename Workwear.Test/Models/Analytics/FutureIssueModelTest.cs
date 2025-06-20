using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Models.Analytics;
using Workwear.Tools;

namespace Workwear.Test.Models.Analytics {
	[TestFixture(TestOf = typeof(FutureIssueModel))]
	public class FutureIssueModelTest {
		[Category("Real case")]
		[Test(Description = "Убеждаемся что конкретный случай расчета полностью просроченной частичной выдачи считается правильно.")]
		public void CalculateIssues_AllExpiredPartialIssue_Calculated()
		{
			// arrange
			var baseParameters = Substitute.For<BaseParameters>();
			var model = new FutureIssueModel(baseParameters);
			var protectionTools = new ProtectionTools {
				Nomenclatures = new ObservableList<Nomenclature>() {
					new Nomenclature()
				}
			};

			var norm = new Norm();
			
			var normItem = new NormItem {
				Norm = norm,
				ProtectionTools = protectionTools,
				Amount = 2,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 4
			};
			
			var operations = new List<IGraphIssueOperation>() {
				new EmployeeIssueOperation {
					OperationTime = new DateTime(2022, 10, 5),
					Issued = 1,
					AutoWriteoffDate = new DateTime(2024, 10, 5),
					ExpiryByNorm = new DateTime(2024, 10, 5)
				}
			};
			
			var employeeItems = new List<EmployeeCardItem> {
				new EmployeeCardItem {
					EmployeeCard = new EmployeeCard(),
					ActiveNormItem = normItem,
					ProtectionTools = protectionTools,
					NextIssue = new DateTime(2022, 10, 5),
					Graph = new IssueGraph(operations)
				}
			};

			// act
			var result = model.CalculateIssues(new DateTime(2024, 11, 5), new DateTime(2024, 12, 31), true, employeeItems);

			// assert
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].OperationDate, Is.EqualTo(new DateTime(2024, 11, 5)));
			Assert.That(result[0].DelayIssueDate, Is.EqualTo(new DateTime(2022, 10, 5)));
			Assert.That(result[0].Amount, Is.EqualTo(2));
		}
		
		[Test(Description = "Проверяем стандартное прогнозирование, выдачу перчаток каждый 3 месяца")]
		public void CalculateIssues_StandardForecast_Calculated()
		{
			// arrange
			var baseParameters = Substitute.For<BaseParameters>();
			var model = new FutureIssueModel(baseParameters);
			var protectionTools = new ProtectionTools {
				Nomenclatures = new ObservableList<Nomenclature>() {
					new Nomenclature()
				}
			};

			var norm = new Norm();
			
			var normItem = new NormItem {
				Norm = norm,
				ProtectionTools = protectionTools,
				Amount = 12,
				NormPeriod = NormPeriodType.Month,
				PeriodCount = 3
			};
			
			var operations = new List<IGraphIssueOperation>();
			
			var employeeItems = new List<EmployeeCardItem> {
				new EmployeeCardItem {
					EmployeeCard = new EmployeeCard(),
					ActiveNormItem = normItem,
					ProtectionTools = protectionTools,
					NextIssue = new DateTime(2024, 1, 5),
					Graph = new IssueGraph(operations)
				}
			};

			// act
			var result = model.CalculateIssues(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), false, employeeItems);

			// assert
			Assert.That(result.Count, Is.EqualTo(4));
			
			Assert.That(result[0].OperationDate, Is.EqualTo(new DateTime(2024, 1, 5)));
			Assert.That(result[0].Amount, Is.EqualTo(12));
			
			Assert.That(result[1].OperationDate, Is.EqualTo(new DateTime(2024, 4, 5)));
			Assert.That(result[1].Amount, Is.EqualTo(12));
			
			Assert.That(result[2].OperationDate, Is.EqualTo(new DateTime(2024, 7, 5)));
			Assert.That(result[2].Amount, Is.EqualTo(12));
			
			Assert.That(result[3].OperationDate, Is.EqualTo(new DateTime(2024, 10, 5)));
			Assert.That(result[3].Amount, Is.EqualTo(12));
		}
		
		[Category("Real case")]
		[Test(Description = "Убеждаемся что учитываем потребность вышедшего из отпуска в прогнозируемый период.")]
		public void CalculateIssues_End_vacation()
		{
			// arrange
			var baseParameters = Substitute.For<BaseParameters>();
			var model = new FutureIssueModel(baseParameters);

			EmployeeCard employee = new EmployeeCard();
			EmployeeVacation vacation = new EmployeeVacation() {
				BeginDate = new DateTime(2024, 6, 1),
				EndDate = new DateTime(2024, 11, 1),
				Employee = employee
			};
			employee.AddVacation(vacation);
			
			var protectionTools1 = new ProtectionTools { Nomenclatures = new ObservableList<Nomenclature>() { new Nomenclature() } };
			var protectionTools2 = new ProtectionTools { Nomenclatures = new ObservableList<Nomenclature>() { new Nomenclature() } };
			var protectionTools3 = new ProtectionTools { Nomenclatures = new ObservableList<Nomenclature>() { new Nomenclature() } };
			
			var norm = new Norm();
			
			var normItem1 = new NormItem {
				Norm = norm,
				ProtectionTools = protectionTools1,
				Amount = 2,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 4
			};
			var normItem2 = new NormItem {
				Norm = norm,
				ProtectionTools = protectionTools2,
				Amount = 2,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 1
			};
			var normItem3 = new NormItem {
				Norm = norm,
				ProtectionTools = protectionTools3,
				Amount = 2,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 4
			};
			
			var operations1 = new List<IGraphIssueOperation>() {
				new EmployeeIssueOperation { //Уже списано
					OperationTime = new DateTime(2020, 5, 1),
					Issued = 2,
					AutoWriteoffDate = new DateTime(2024, 5, 1),
					ExpiryByNorm = new DateTime(2024, 5, 1)
				}
			};
			var operations2 = new List<IGraphIssueOperation>() {
				new EmployeeIssueOperation { //Частичная
					OperationTime = new DateTime(2022, 5, 1),
					Issued = 1,
					AutoWriteoffDate = new DateTime(2026, 5, 1),
					ExpiryByNorm = new DateTime(2026, 5, 1)
				}
			};
			var operations3 = new List<IGraphIssueOperation>() {
				new EmployeeIssueOperation { //Возникнет если не учитываем переносы по отпуску
					OperationTime = new DateTime(2020, 11, 10),
					Issued = 2,
					AutoWriteoffDate = new DateTime(2024, 11, 10),
					ExpiryByNorm = new DateTime(2024, 11, 10)
				}
			};
			var employeeItems1 = new List<EmployeeCardItem> {
				new EmployeeCardItem {
					EmployeeCard = new EmployeeCard(),
					ActiveNormItem = normItem1,
					ProtectionTools = protectionTools1,
					NextIssue = new DateTime(2024, 5, 1),
					Graph = new IssueGraph(operations1)
				}
			};
			var employeeItems2 = new List<EmployeeCardItem> {
				new EmployeeCardItem {
					EmployeeCard = new EmployeeCard(),
					ActiveNormItem = normItem2,
					ProtectionTools = protectionTools2,
					NextIssue = new DateTime(2022, 5, 1),
					Graph = new IssueGraph(operations2)
				}
			};
			var employeeItems3 = new List<EmployeeCardItem> {
				new EmployeeCardItem {
					EmployeeCard = new EmployeeCard(),
					ActiveNormItem = normItem3,
					ProtectionTools = protectionTools3,
					NextIssue = new DateTime(2024, 11, 10),
					Graph = new IssueGraph(operations3)
				}
			};

			// act
			var result1 = model.CalculateIssues(new DateTime(2024, 10, 1), new DateTime(2024, 12, 31), true, employeeItems1);
			var result2 = model.CalculateIssues(new DateTime(2024, 10, 1), new DateTime(2024, 12, 31), true, employeeItems2);
			var result3 = model.CalculateIssues(new DateTime(2024, 10, 1), new DateTime(2024, 12, 31), true, employeeItems3);
			// assert
			Assert.That(result1.Count, Is.EqualTo(1));
			Assert.That(result1[0].OperationDate, Is.EqualTo(new DateTime(2024, 10, 1)));
			Assert.That(result1[0].Amount, Is.EqualTo(2));
			
			Assert.That(result2.Count, Is.EqualTo(1));
			Assert.That(result2[0].OperationDate, Is.EqualTo(new DateTime(2024, 10, 1)));
			Assert.That(result2[0].Amount, Is.EqualTo(1));
			
			Assert.That(result3.Count, Is.EqualTo(1));
			Assert.That(result3[0].OperationDate, Is.EqualTo(new DateTime(2024, 11, 10)));
			Assert.That(result3[0].Amount, Is.EqualTo(2));
		}
		
		[Category("Real case")]
		[Test(Description = "Убеждаемся что если начало прогнозируемого периода по условиям нормы не попадает в период выдачи. Мы правильно показываем долги.")]
		public void CalculateIssues_NormConditionPeriod_Calculated()
		{
			// arrange
			var baseParameters = Substitute.For<BaseParameters>();
			var model = new FutureIssueModel(baseParameters);
			var protectionTools = new ProtectionTools {
				Nomenclatures = new ObservableList<Nomenclature>() {
					new Nomenclature()
				}
			};

			var norm = new Norm {
				DateFrom = new DateTime(2024, 12, 1)
			};
			
			var normCondition = new NormCondition {
				Name = "Зима",
				IssuanceStart = new DateTime(2000, 9, 1),
				IssuanceEnd = new DateTime(2000, 5, 1),
			};
			
			var normItem = new NormItem {
				Norm = norm,
				ProtectionTools = protectionTools,
				Amount = 2,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 4,
				NormCondition = normCondition
			};
			
			var employeeItems = new List<EmployeeCardItem> {
				new EmployeeCardItem {
					EmployeeCard = new EmployeeCard(),
					Created = new DateTime(2024, 12, 1),
					ActiveNormItem = normItem,
					ProtectionTools = protectionTools,
					NextIssue = new DateTime(2024, 12, 1),
					Graph = new IssueGraph()
				}
			};

			// Начало прогнозирования в июне, зимнюю одежду должны выдавать с сентября.
			var result = model.CalculateIssues(new DateTime(2025, 6, 18), new DateTime(2025, 12, 31), true, employeeItems);

			// Мы убеждаемся что не потеряли долг, так как у нас стоит перенос долга на дату начала периода.
			// Но это не попадает в период выдачи по норме, долг пропадал. Это не правильно.
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].OperationDate, Is.EqualTo(new DateTime(2025, 6, 18)));
			Assert.That(result[0].DelayIssueDate, Is.EqualTo(new DateTime(2024, 12, 1)));
			Assert.That(result[0].Amount, Is.EqualTo(2));
		}
	}
}
