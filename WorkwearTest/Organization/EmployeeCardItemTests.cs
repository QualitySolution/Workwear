using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using workwear.Domain.Operations;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Measurements;
using Workwear.Measurements;
using Workwear.Domain.Company;
using workwear.Domain.Sizes;
using workwear.Tools;

namespace WorkwearTest.Organization
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

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
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

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
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

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
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

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
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

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
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

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
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

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
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

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 2, 1)));
		}
		#endregion

		#region CalculateRequiredIssue

		[Test(Description = "Не получали совсем, должны получить все")]
		public void CalculateRequiredIssue_NotRecivedCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 5
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Amount = 0,
				LastIssue = null,
				NextIssue = DateTime.Today.AddDays(30)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters), Is.EqualTo(5));
		}

		[Test(Description = "Все получили")]
		public void CalculateRequiredIssue_RecivedFullyCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 4
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Amount = 4,
				LastIssue = DateTime.Today.AddDays(-30),
				NextIssue = DateTime.Today.AddDays(30)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters), Is.EqualTo(0));
		}
		
		[Test(Description = "Получили частично")]
		public void CalculateRequiredIssue_RecivedPartCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 4
			};
			//FIXME Возможно некоректная ситуация, надо подумать как в этом месте будет более правильно поставить дату следующей выдачи
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Amount = 1,
				LastIssue = DateTime.Today.AddDays(-30),
				NextIssue = DateTime.Today.AddDays(30)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters), Is.EqualTo(3));
		}
		
		[Test(Description = "Получили, но дожны получить заново")]
		public void CalculateRequiredIssue_NextIssueIsNullCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 2
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Amount = 2,
				LastIssue = DateTime.Today.AddDays(-30),
				NextIssue = null
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters), Is.EqualTo(0));
		}
		
		[Test(Description = "Проверяем учитывает ли расчет даты следущей выдачи дату автосписания.")]
		public void CalculateRequiredIssue_ReciveExpiredCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);
			
			var norm = new NormItem {
				Amount = 1
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Amount = 1,
				LastIssue = DateTime.Today.AddDays(-40),
				NextIssue = DateTime.Today.AddDays(-10)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters), Is.EqualTo(1));
		}
		[Test(Description = "В настройках базы стоит возможность выдачи раньше на 30 дней.")]
		public void CalculateRequiredIssue_ReciveBeforeExpiredCase()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(30);
			
			var norm = new NormItem {
				Amount = 1
			};
			var item = new EmployeeCardItem {
				ActiveNormItem = norm,
				Amount = 1,
				LastIssue = DateTime.Today.AddMonths(-5),
				NextIssue = DateTime.Today.AddDays(-10)
			};
			Assert.That(item.CalculateRequiredIssue(baseParameters), Is.EqualTo(1));
		}
		
		#endregion
		#region MatcheStockPosition

		[Test(Description = "Проверяем случай при котором у складской позиции отсутствует рост, " +
		                    "значит эта номенклатура без роста и не надо сравнивать ее по росту с сотрудником.")]
		public void MatcheStockPosition_WithoutGrowthCase()
		{
			var heightType = new SizeType {Category = Category.Height};
			var sizeType = new SizeType {Category = Category.Size};
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

			var stockPosition = new StockPosition(nomenclature, 0, size,null);
			Assert.That(employeeCardItem.MatcheStockPosition(stockPosition)); 
		}

		[Test(Description = "Проверяем что находим соответствие размеров.")]
		public void MatcheStockPosition_SizeTest()
		{
			var employee = new EmployeeCard();
			employee.Sex = Sex.M;
			var sizeType = new SizeType {Category = Category.Size};
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

			var stockPosition = new StockPosition(nomenclature, 0, size,null);
			var result = employeeItem.MatcheStockPosition(stockPosition);
			Assert.That(result, Is.True);
		}

		[Test(Description = "Проверяем что находим соответствия размеров когда в сотруднике установлен диапазон размера.")]
		public void MatcheStockPosition_RangeSizeInEmployeeSize()
		{
			var employee = new EmployeeCard {Sex = Sex.M,};
			var sizeType = new SizeType {Category = Category.Size};
			var size52 = new Size {Name = "52", SizeType = sizeType};
			var size54 = new Size {Name = "54", SizeType = sizeType};
			var size52And54 = new Size
				{Name = "52-54", SizeType = sizeType, SuitableSizes = new List<Size> {size52, size54}};
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
			var stockPosition = new StockPosition(nomenclature, 0, size52,null);
			var result = employeeItem.MatcheStockPosition(stockPosition);
			Assert.That(result, Is.True);
		}

		[Test(Description = "Проверяем что находим соответствия размеров когда в сотруднике установлен диапазон роста.")]
		public void MatcheStockPosition_RangeGrowthInEmployeeSize()
		{
			var employee = new EmployeeCard {
				Sex = Sex.M
			};
			var sizeType = new SizeType {Category = Category.Size};
			var heightType = new SizeType {Category = Category.Height};
			var size52 = new Size {Name = "52", SizeType = sizeType};
			var height170 = new Size {Name = "170", SizeType = heightType};
			var height176 = new Size {Name = "176", SizeType = heightType};
			var height170And176 = new Size
				{Name = "170-176", SizeType = heightType, SuitableSizes = new List<Size> {height170, height176}};
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

			var stockPosition = new StockPosition(nomenclature, 0, size52, height170);
			var result = employeeItem.MatcheStockPosition(stockPosition);
			Assert.That(result, Is.True);
		}
		#endregion
	}

	public class EmployeeCardItemTested : EmployeeCardItem
	{
		public Func<IssueGraph> GetIssueGraphForItemFunc;

		protected internal override IssueGraph GetIssueGraphForItem(IUnitOfWork uow)
		{
			return GetIssueGraphForItemFunc();
		}
	}
}
