using System;
using System.Collections.Generic;
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
using Workwear.Measurements;
using Workwear.Tools;

namespace Workwear.Test.Domain.Company
{
	[TestFixture(TestOf = typeof(EmployeeCardItem))]
	public class EmployeeCardItemTests
	{
		#region UpdateNextIssue

		[Test(Description = "Проверяем учитывает ли расчет даты следующей выдачи, дату автосписания.")]
		public void UpdateNextIssue_AutoWriteoffCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation> { operation1 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItem();
			item.EmployeeCard = employee;
			item.Graph = graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 2, 1)));
		}

		[Test(Description = "Проверяем проставляет ли расчет следующей выдачи дату износа по норме " +
		                    "в том случае если авто списание последней выдачи отключено.")]
		public void UpdateNextIssue_NotWriteoffCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.ExpiryByNorm.Returns(new DateTime(2018, 3, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation> { operation1 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItem();
			item.EmployeeCard = employee;
			item.Graph = graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 3, 1)));
		}

		[Test(Description = "Проверяем что алгоритм не падает в случае если нет автосписания и нет даты износа по норме.(bug)")]
		[Category("real case")]
		public void UpdateNextIssue_WihtoutWriteoffAndWithoutExpiryByNormCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.ExpiryByNorm.Returns(x => null);
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation> { operation1 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItem();
			item.EmployeeCard = employee;
			item.Graph = graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.Null);
		}

		[Test(Description = "Тест проверяет корректную установку следующей выдачи в случает когда по норме положено 10, " +
		                    "выдали 10, потом списали 2. " +
		                    "Следующая выдача должна быть первой датой когда стало меньше нормы, то есть в день списания.")]
		public void UpdateNextIssue_FirstNotEnoughCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 3, 1));
			operation1.Issued.Returns(10);

			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.IssuedOperation.Returns(operation1);
			operation2.OperationTime.Returns(new DateTime(2018, 1, 15));
			operation2.Returned.Returns(2);

			var list = new List<EmployeeIssueOperation> { operation1, operation2 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItem();
			item.EmployeeCard = employee;
			item.Graph = graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 1, 15)));
		}

		[Test(Description = "Если выдачи этого типа сиз еще не было, " +
		                    "дату следующей выдачи должны устанавливать датой создания потребности.")]
		public void UpdateNextIssue_WithoutIssuesNextDateEqualCreateItemDate()
		{
			var uow = Substitute.For<IUnitOfWork>();
			var graph = Substitute.For<IssueGraph>();
			var employee = Substitute.For<EmployeeCard>();
			var normItem = Substitute.For<NormItem>();

			var item = new EmployeeCardItem();
			item.EmployeeCard = employee;
			item.Graph = graph;
			item.Created = new DateTime(2018, 1, 15);
			item.ActiveNormItem = normItem;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 1, 15)));
		}

		[Test(Description = "Проверяем что если дата создания строки с нормой, " +
		                    "допустим удалили норму и добавили, после даты износа выданного, " +
		                    "то следующая выдача не перескочит на дату создания новой строки.")]
		public void UpdateNextIssue_NotBreakNextIssueDateAfterRecreateItem()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2017, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2017, 10, 1));
			operation1.Issued.Returns(10);

			var uow = Substitute.For<IUnitOfWork>();
			var list = new List<EmployeeIssueOperation> { operation1 };
			var graph = new IssueGraph(list);
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItem();
			item.EmployeeCard = employee;
			item.Graph = graph;
			item.Created = new DateTime(2018, 1, 15);
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2017, 10, 1)));
		}

		[Test(Description = "Проверяем сдвигается ли дата следующей выдачи на первый день после отпуска.")]
		public void UpdateNextIssue_MoveDateToLeaveEndCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation> { operation1 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.
			var vacation = Substitute.For<EmployeeVacation>();
			vacation.Employee.Returns(employee);
			vacation.BeginDate.Returns(new DateTime(2018, 1, 15));
			vacation.EndDate.Returns(new DateTime(2018, 2, 15));
			employee.Vacations.Returns(new List<EmployeeVacation> { vacation });

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItem();
			item.EmployeeCard = employee;
			item.Graph = graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 2, 16)));
		}

		[Test(Description = "Проверяем что при сдвиге на конец отпуска, " +
		                    "обращаем внимание на случаи когда отпуск еще не начался.")]
		public void UpdateNextIssue_NotMoveDateWhenLeaveNotBeginCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation> { operation1 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.
			var vacation = Substitute.For<EmployeeVacation>();
			vacation.Employee.Returns(employee);
			vacation.BeginDate.Returns(new DateTime(2018, 2, 15));
			vacation.EndDate.Returns(new DateTime(2018, 3, 15));
			employee.Vacations.Returns(new List<EmployeeVacation> { vacation });

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItem();
			item.EmployeeCard = employee;
			item.Graph = graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 2, 1)));
		}
		#endregion

		#region CalculateRequiredIssue

		[Test(Description = "Не получали совсем, должны получить все")]
		public void CalculateRequiredIssue_NotReceivedCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 5
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Graph = new IssueGraph(new List<EmployeeIssueOperation>()),
				NextIssue = DateTime.Today.AddDays(30)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters, DateTime.Today), Is.EqualTo(5));
		}

		[Test(Description = "Все получили")]
		public void CalculateRequiredIssue_ReceivedFullyCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 4
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Graph = new IssueGraph(new List<EmployeeIssueOperation> {
					new EmployeeIssueOperation {
						OperationTime = DateTime.Today.AddDays(-30),
						StartOfUse = DateTime.Today.AddDays(-30),
						ExpiryByNorm = DateTime.Today.AddDays(30),
						AutoWriteoffDate = DateTime.Today.AddDays(30),
						Issued = 4
					}
				}),
				NextIssue = DateTime.Today.AddDays(30)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters, DateTime.Today), Is.EqualTo(0));
		}
		
		[Test(Description = "Получили частично")]
		public void CalculateRequiredIssue_ReceivedPartCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 4
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Graph = new IssueGraph(new List<EmployeeIssueOperation> {
					new EmployeeIssueOperation {
						OperationTime = DateTime.Today.AddDays(-30),
						StartOfUse = DateTime.Today.AddDays(-30),
						ExpiryByNorm = DateTime.Today.AddDays(30),
						AutoWriteoffDate = DateTime.Today.AddDays(30),
						Issued = 1
					}
				}),
				NextIssue = DateTime.Today.AddDays(30)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters, DateTime.Today), Is.EqualTo(3));
		}
		
		[Test(Description = "Получили до износа.")]
		public void CalculateRequiredIssue_NextIssueIsNullCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 2
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Graph = new IssueGraph(new List<EmployeeIssueOperation> {
					new EmployeeIssueOperation {
						OperationTime = DateTime.Today.AddMonths(-30),
						StartOfUse = DateTime.Today.AddMonths(-30),
						ExpiryByNorm = null,
						AutoWriteoffDate = null,
						UseAutoWriteoff = false,
						Issued = 2
					}
				}),
				NextIssue = null
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters, DateTime.Today), Is.EqualTo(0));
		}
		
		[Test(Description = "Проверяем учитывает ли расчет даты следующей выдачи дату автосписания.")]
		public void CalculateRequiredIssue_ReceiveExpiredCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 1
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Graph = new IssueGraph(new List<EmployeeIssueOperation> {
					new EmployeeIssueOperation {
						OperationTime = DateTime.Today.AddDays(-40),
						StartOfUse = DateTime.Today.AddDays(-40),
						ExpiryByNorm = DateTime.Today.AddDays(-10),
						AutoWriteoffDate = DateTime.Today.AddDays(-10),
						Issued = 1
					}
				}),
				NextIssue = DateTime.Today.AddDays(-10)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters, DateTime.Today), Is.EqualTo(1));
		}
		
		[Test(Description = "В настройках базы стоит возможность выдачи раньше на 30 дней.")]
		public void CalculateRequiredIssue_ReceiveBeforeExpiredCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(30);
			
			var norm = new NormItem {
				Amount = 1
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Graph = new IssueGraph(new List<EmployeeIssueOperation> {
					new EmployeeIssueOperation {
						OperationTime = DateTime.Today.AddMonths(-5),
						StartOfUse = DateTime.Today.AddMonths(-5),
						ExpiryByNorm = DateTime.Today.AddDays(-10),
						AutoWriteoffDate = DateTime.Today.AddDays(-10),
						Issued = 1
					}
				}),
				NextIssue = DateTime.Today.AddDays(-10)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters, DateTime.Today), Is.EqualTo(1));
		}
		
		[Test(Description = "Проверяем что не выдаем до начала периода выдачи, если не выдавали.")]
		public void CalculateRequiredIssue_NormCondition_NotIssueCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0); //Тут вопрос не ясен, должна ли эта настройка учитываться в случае периода выдачи.

			var normCondition = new NormCondition {
				Name = "Зима",
				IssuanceStart = new DateTime(2001, 10, 1),
				IssuanceEnd = new DateTime(2001, 4, 15)
			};
			
			var norm = new NormItem {
				Amount = 1,
				NormCondition = normCondition
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Graph = new IssueGraph(new List<EmployeeIssueOperation>()),
				NextIssue = new DateTime(2022, 5, 20)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 9, 20)), Is.EqualTo(0));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 5, 20)), Is.EqualTo(0));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 10, 1)), Is.EqualTo(1));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 10, 20)), Is.EqualTo(1));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2023, 4, 15)), Is.EqualTo(1));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2023, 4, 16)), Is.EqualTo(0));
		}
		
		[Test(Description = "Проверяем что не выдаем до начала периода выдачи, если выдавали но износилось ранее.")]
		public void CalculateRequiredIssue_NormCondition_IssuedBeforeCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);  //Тут вопрос не ясен, должна ли эта настройка учитываться в случае периода выдачи.
			
			var normCondition = new NormCondition {
				Name = "Зима",
				IssuanceStart = new DateTime(2001, 10, 1),
				IssuanceEnd = new DateTime(2001, 4, 15)
			};
			
			var norm = new NormItem {
				Amount = 1,
				NormCondition = normCondition
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Graph = new IssueGraph(new List<EmployeeIssueOperation> {
					new EmployeeIssueOperation {
						OperationTime = new DateTime(2021, 5, 20),
						StartOfUse = new DateTime(2021, 5, 20),
						ExpiryByNorm = new DateTime(2022, 5, 20),
						AutoWriteoffDate = new DateTime(2022, 5, 20),
						Issued = 1
					}
				}),
				NextIssue = new DateTime(2022, 5, 20)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 3, 20)), Is.EqualTo(0));//Еще действует старая выдача
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 9, 20)), Is.EqualTo(0));//Не положено
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 5, 20)), Is.EqualTo(0));//Не положено
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 10, 1)), Is.EqualTo(1));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2022, 10, 20)), Is.EqualTo(1));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2023, 4, 15)), Is.EqualTo(1));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2023, 4, 16)), Is.EqualTo(0));
			Assert.That(item.CalculateRequiredIssue(baseParameters, new DateTime(2024, 3, 10)), Is.EqualTo(1));
		}
		
		#endregion
		#region MatchStockPosition

		[Test(Description = "Проверяем случай при котором у складской позиции отсутствует рост, " +
		                    "значит эта номенклатура без роста и не надо сравнивать ее по росту с сотрудником.")]
		public void MatcheStockPosition_WithoutGrowthCase()
		{
			var heightType = new SizeType {CategorySizeType = CategorySizeType.Height};
			var sizeType = new SizeType {CategorySizeType = CategorySizeType.Size};
			var size = new Size {Name = "52", SizeType = sizeType};
			var itemType = Substitute.For<ItemsType>();
			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.Type.Returns(itemType);
			nomenclature.MatchingEmployeeSex(Sex.M).Returns(true);
			var protectionTools = Substitute.For<ProtectionTools>();
			protectionTools.MatchedNomenclatures.Returns(new[] { nomenclature });
			var employee = Substitute.For<EmployeeCard>();
			employee.Sex.Returns(Sex.M);
			var sizes = new List<EmployeeSize>();
			employee.Sizes.Returns(sizes);
			employee.Sizes.Add(new EmployeeSize{Size = new Size {Name = "182"}, SizeType = heightType, Employee = employee});
			employee.Sizes.Add(new EmployeeSize{Size = size, SizeType = sizeType, Employee = employee});
			var normItem = Substitute.For<NormItem>();
			normItem.ProtectionTools.Returns(protectionTools);

			var employeeCardItem = new EmployeeCardItem(employee, normItem);

			var stockPosition = new StockPosition(nomenclature, 0, size,null, null);
			Assert.That(employeeCardItem.MatchStockPosition(stockPosition)); 
		}

		[Test(Description = "Проверяем что находим соответствие размеров.")]
		public void MatcheStockPosition_SizeTest()
		{
			var employee = new EmployeeCard();
			employee.Sex = Sex.M;
			var sizeType = new SizeType {CategorySizeType = CategorySizeType.Size};
			var size = new Size {Name = "52", SizeType = sizeType};
			employee.Sizes.Add(new EmployeeSize{Size = size, SizeType = sizeType, Employee = employee});

			var itemType = Substitute.For<ItemsType>();
			itemType.SizeType.Returns(sizeType);
			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.Id.Returns(25);
			nomenclature.Type.Returns(itemType);
			nomenclature.MatchingEmployeeSex(Sex.M).Returns(true);
			var protectionTools = Substitute.For<ProtectionTools>();
			protectionTools.MatchedNomenclatures.Returns(new[] { nomenclature });
			var normItem = Substitute.For<NormItem>();
			normItem.ProtectionTools.Returns(protectionTools);

			var employeeItem = new EmployeeCardItem(employee, normItem);

			var stockPosition = new StockPosition(nomenclature, 0, size,null, null);
			var result = employeeItem.MatchStockPosition(stockPosition);
			Assert.That(result, Is.True);
		}

		[Test(Description = "Проверяем что находим соответствия размеров когда в сотруднике установлен диапазон размера.")]
		public void MatcheStockPosition_RangeSizeInEmployeeSize()
		{
			var employee = new EmployeeCard {Sex = Sex.M,};
			var sizeType = new SizeType {CategorySizeType = CategorySizeType.Size};
			var size52And54 = new Size
				{Name = "52-54", SizeType = sizeType, ShowInEmployee = true};
			var size52 = new Size {
				Name = "52", 
				SizeType = sizeType, 
				ShowInEmployee = true, 
				SuitableSizes = new List<Size> {size52And54}
			};
			employee.Sizes.Add(new EmployeeSize{Size = size52And54, SizeType = sizeType});

			var itemType = Substitute.For<ItemsType>();
			itemType.Category.Returns(ItemTypeCategory.wear);
			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.Id.Returns(25);
			nomenclature.Type.Returns(itemType);
			nomenclature.MatchingEmployeeSex(Sex.M).Returns(true);

			var protectionTools = Substitute.For<ProtectionTools>();
			protectionTools.MatchedNomenclatures.Returns(new[] { nomenclature });
			var normItem = Substitute.For<NormItem>();
			normItem.ProtectionTools.Returns(protectionTools);

			var employeeItem = new EmployeeCardItem(employee, normItem);
			var stockPosition = new StockPosition(nomenclature, 0, size52,null, null);
			var result = employeeItem.MatchStockPosition(stockPosition);
			Assert.That(result, Is.True);
		}

		[Test(Description = "Проверяем что находим соответствия размеров когда в сотруднике установлен диапазон роста.")]
		public void MatcheStockPosition_RangeGrowthInEmployeeSize()
		{
			var employee = new EmployeeCard {
				Sex = Sex.M
			};
			var sizeType = new SizeType {CategorySizeType = CategorySizeType.Size};
			var heightType = new SizeType {CategorySizeType = CategorySizeType.Height};
			var size52 = new Size {Name = "52", SizeType = sizeType, ShowInEmployee = true};
			var height170And176 = new Size
				{Name = "170-176", SizeType = heightType, ShowInEmployee = true};
			var height170 = new Size {
				Name = "170", 
				SizeType = heightType, 
				ShowInEmployee = true, 
				SuitableSizes = new List<Size> {height170And176}
			};
			employee.Sizes.Add(new EmployeeSize{Size = height170And176, SizeType = heightType});
			employee.Sizes.Add(new EmployeeSize{Size = size52, SizeType = sizeType});

			var itemType = Substitute.For<ItemsType>();
			itemType.Category.Returns(ItemTypeCategory.wear);
			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.Id.Returns(25);
			nomenclature.Type.Returns(itemType);
			nomenclature.MatchingEmployeeSex(Sex.M).Returns(true);
			var protectionTools = Substitute.For<ProtectionTools>();
			protectionTools.MatchedNomenclatures.Returns(new[] { nomenclature });
			var normItem = Substitute.For<NormItem>();
			normItem.ProtectionTools.Returns(protectionTools);

			var employeeItem = new EmployeeCardItem(employee, normItem);

			var stockPosition = new StockPosition(nomenclature, 0, size52, height170, null);
			var result = employeeItem.MatchStockPosition(stockPosition);
			Assert.That(result, Is.True);
		}

		[Test(Description = "Проверяем что при поиске соответствия обрабатываем установленный пол для номенклатуры.")]
		[TestCase(Sex.M, ClothesSex.Men, ExpectedResult = true)]
		[TestCase(Sex.M, ClothesSex.Women, ExpectedResult = false)]
		[TestCase(Sex.M, ClothesSex.Universal, ExpectedResult = true)]
		[TestCase(Sex.F, ClothesSex.Men, ExpectedResult = false)]
		[TestCase(Sex.F, ClothesSex.Women, ExpectedResult = true)]
		[TestCase(Sex.F, ClothesSex.Universal, ExpectedResult = true)]
		[TestCase(Sex.None, ClothesSex.Universal, ExpectedResult = true)]
		public bool MatcheStockPosition_ClothesSex(Sex employeeSex, ClothesSex clothesSex)
		{
			var employee = new EmployeeCard();
			employee.Sex = employeeSex;

			var itemType = Substitute.For<ItemsType>();
			var nomenclature = new Nomenclature {
				Id = 25,
				Type = itemType,
				Sex = clothesSex
			};
			var protectionTools = Substitute.For<ProtectionTools>();
			protectionTools.MatchedNomenclatures.Returns(new[] { nomenclature });
			var normItem = Substitute.For<NormItem>();
			normItem.ProtectionTools.Returns(protectionTools);
			var employeeItem = new EmployeeCardItem(employee, normItem);

			return employeeItem.MatchStockPosition(new StockPosition(nomenclature, 0, null, null, null));
		}
		#endregion
		#region LastIssued
		[Test(Description = "Проверяем базовый случай отображения последних выдач")]
		public void LastIssued_IssuesExistCase()
		{
			var graph = new IssueGraph(new List<EmployeeIssueOperation> {
				new EmployeeIssueOperation { //Старая выдача не отображаем
					OperationTime = new DateTime(2020, 1, 1),
					StartOfUse = new DateTime(2020, 1, 1),
					ExpiryByNorm = new DateTime(2020, 2, 1),
					AutoWriteoffDate = new DateTime(2020, 2, 1),
					Issued = 1
				},
				new EmployeeIssueOperation { //Первая отображаемая
					OperationTime = new DateTime(2022, 1, 1),
					StartOfUse = new DateTime(2022, 1, 1),
					ExpiryByNorm = new DateTime(2023, 1, 1),
					AutoWriteoffDate = new DateTime(2023, 1, 1),
					Issued = 1
				},
				new EmployeeIssueOperation { //Вторая отображаемая
					OperationTime = new DateTime(2022, 4, 1),
					StartOfUse = new DateTime(2022, 4, 1),
					ExpiryByNorm = new DateTime(2023, 4, 1),
					AutoWriteoffDate = new DateTime(2023, 4, 1),
					Issued = 1
				}
			});
			
			var item = new EmployeeCardItem {
				Graph = graph
			};
			//Отображаем только первую, вторая еще как бы не выдана
			Assert.That(item.LastIssued(new DateTime(2022, 3,1)).Count(), Is.EqualTo(1));
			var issue = item.LastIssued(new DateTime(2022, 3, 1)).First();
			Assert.That(issue.date, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(issue.amount, Is.EqualTo(1));
			Assert.That(issue.removed, Is.EqualTo(0));
			
			//Отображаем обе выдачи
			Assert.That(item.LastIssued(new DateTime(2022, 6,1)).Count(), Is.EqualTo(2));
			var issue1 = item.LastIssued(new DateTime(2022, 6, 1)).First(x => x.date == new DateTime(2022, 1, 1));
			Assert.That(issue1.amount, Is.EqualTo(1));
			Assert.That(issue1.removed, Is.EqualTo(0));
			var issue2 = item.LastIssued(new DateTime(2022, 6, 1)).First(x => x.date == new DateTime(2022, 4, 1));
			Assert.That(issue2.amount, Is.EqualTo(1));
			Assert.That(issue2.removed, Is.EqualTo(0));
			
			//Только оставшуюся
			Assert.That(item.LastIssued(new DateTime(2023, 2,1)).Count(), Is.EqualTo(1));
			issue2 = item.LastIssued(new DateTime(2023, 2, 1)).First();
			Assert.That(issue2.amount, Is.EqualTo(1));
			Assert.That(issue2.removed, Is.EqualTo(0));
			
			//Последнюю даже с учетом того что уже не числится
			Assert.That(item.LastIssued(new DateTime(2024, 2,1)).Count(), Is.EqualTo(1));
			issue2 = item.LastIssued(new DateTime(2024, 2, 1)).First();
			Assert.That(issue2.amount, Is.EqualTo(1));
			Assert.That(issue2.removed, Is.EqualTo(0));
		}
		
		[Test(Description = "Проверяем что в последних выдачах адекватно отображаем количество списанного")]
		public void LastIssued_ShowRemoveCountCase() {
			var issue = new EmployeeIssueOperation {
				//Выдача
				OperationTime = new DateTime(2022, 1, 1),
				StartOfUse = new DateTime(2022, 1, 1),
				ExpiryByNorm = new DateTime(2023, 1, 1),
				AutoWriteoffDate = new DateTime(2023, 1, 1),
				Issued = 10
			};
			
			var graph = new IssueGraph(new List<EmployeeIssueOperation> {
				issue,
				new EmployeeIssueOperation { //Списание
					OperationTime = new DateTime(2022, 2, 1),
					IssuedOperation = issue,
					Returned = 2
				},
				new EmployeeIssueOperation { //Списание
					OperationTime = new DateTime(2022, 4, 1),
					IssuedOperation = issue,
					Returned = 1
				}
			});
			
			var item = new EmployeeCardItem {
				Graph = graph
			};
			//Отображаем полную выдачу
			Assert.That(item.LastIssued(new DateTime(2022, 1,20)).Count(), Is.EqualTo(1));
			var lastissue = item.LastIssued(new DateTime(2022, 1, 20)).First();
			Assert.That(lastissue.date, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(lastissue.amount, Is.EqualTo(10));
			Assert.That(lastissue.removed, Is.EqualTo(0));
			
			//С первым списанием
			Assert.That(item.LastIssued(new DateTime(2022, 2,10)).Count(), Is.EqualTo(1));
			lastissue = item.LastIssued(new DateTime(2022, 2, 10)).First();
			Assert.That(lastissue.date, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(lastissue.amount, Is.EqualTo(10));
			Assert.That(lastissue.removed, Is.EqualTo(2));
			
			//Со обоими списаниями
			Assert.That(item.LastIssued(new DateTime(2022, 5,10)).Count(), Is.EqualTo(1));
			lastissue = item.LastIssued(new DateTime(2022, 5, 10)).First();
			Assert.That(lastissue.date, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(lastissue.amount, Is.EqualTo(10));
			Assert.That(lastissue.removed, Is.EqualTo(3));
			
			//После основного списания
			Assert.That(item.LastIssued(new DateTime(2023, 5,10)).Count(), Is.EqualTo(1));
			lastissue = item.LastIssued(new DateTime(2023, 5, 20)).First();
			Assert.That(lastissue.date, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(lastissue.amount, Is.EqualTo(10));
			//Assert.That(lastissue.removed, Is.EqualTo(3)); Здесь не знаю как правильно, поэтому пока не проверяем, предположу то не надо усложнять вро уже списанное.
		}

		[Test(Description = "Проверяем что в последних выдачах не показываем полное списание, в дату самого списания тоже.")]
		public void LastIssued_DontShowFullRemovedCase() {
			var firstIssue = new EmployeeIssueOperation {
				//Старая выдача не отображаем
				OperationTime = new DateTime(2023, 1, 13),
				StartOfUse = new DateTime(2023, 1, 13),
				ExpiryByNorm = new DateTime(2024, 1, 13),
				AutoWriteoffDate = new DateTime(2024, 1, 13),
				Issued = 1
			};
			var graph = new IssueGraph(new List<EmployeeIssueOperation> {
				firstIssue,
				new EmployeeIssueOperation { //Выдали заново
					OperationTime = new DateTime(2023, 2, 13),
					StartOfUse = new DateTime(2023, 2, 13),
					ExpiryByNorm = new DateTime(2024, 2, 13),
					AutoWriteoffDate = new DateTime(2024, 2, 13),
					Issued = 1
				},
				new EmployeeIssueOperation { //Списали выданное ранее
					OperationTime = new DateTime(2023, 2, 13),
					Returned = 1,
					IssuedOperation = firstIssue
				}
			});
			
			var item = new EmployeeCardItem {
				Graph = graph
			};
			//Отображаем только действующую выдачу.
			var today = new DateTime(2023, 2, 13);
			Assert.That(item.LastIssued(today).Count(), Is.EqualTo(1));
			var issue = item.LastIssued(today).First();
			Assert.That(issue.date, Is.EqualTo(new DateTime(2023, 2, 13)));
			Assert.That(issue.amount, Is.EqualTo(1));
			Assert.That(issue.removed, Is.EqualTo(0));
		}
		#endregion
	}
}
