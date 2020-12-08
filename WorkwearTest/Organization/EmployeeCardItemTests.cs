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

namespace WorkwearTest.Organization
{
	[TestFixture(TestOf = typeof(EmployeeCardItem))]
	public class EmployeeCardItemTests
	{
		#region UpdateNextIssue

		[Test(Description = "Проверяем учитывает ли расчет даты следущей выдачи дату автосписания.")]
		public void UpdateNextIssue_AutoWriteoffCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation>() { operation1 };
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

		[Test(Description = "Проверяем пороставляет ли расчет следующей выдачи дату износа по норме в том случае если авто списание последней выдачи отключено.")]
		public void UpdateNextIssue_NotWriteoffCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.ExpiryByNorm.Returns(new DateTime(2018, 3, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation>() { operation1 };
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
		public void UpdateNextIssue_WihtoutWriteoffAndWithoutExpiryByNormCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.ExpiryByNorm.Returns(x => null);
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation>() { operation1 };
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

		[Test(Description = "Тест проверяет коректную установку следующей выдачи в случает когда по норме положено 10, выдали 10, потом списали 2. Следующая выдача должна быть первой датой когда стало меньше нормы, то есть в день списания.")]
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

			var list = new List<EmployeeIssueOperation>() { operation1, operation2 };
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

		[Test(Description = "Если выдачи этого типа сиз еще не было, дату следующей выдачи должны устанавливать датой создания потребности.")]
		public void UpdateNextIssue_WithoutIssuesNextDateEqualCreateItemDate()
		{
			var uow = Substitute.For<IUnitOfWork>();
			var graph = Substitute.For<IssueGraph>();
			var employee = Substitute.For<EmployeeCard>();

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
			item.Created = new DateTime(2018, 1, 15);

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 1, 15)));
		}

		[Test(Description = "Проверяем что если дата создания строки с нормой, допустим удалили норму и добавили, после даты износа выданного, то следующая выдача не перескочит на дату создания новой строки.")]
		public void UpdateNextIssue_NotBreakNextIssueDateAfterRecreateItem()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2017, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2017, 10, 1));
			operation1.Issued.Returns(10);

			var uow = Substitute.For<IUnitOfWork>();
			var list = new List<EmployeeIssueOperation>() { operation1 };
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

		[Test(Description = "Проверяем сдвигается ли дата следущей выдачи на первый день после отпуска.")]
		public void UpdateNextIssue_MoveDateToLeaveEndCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation>() { operation1 };
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

		[Test(Description = "Проверяем что при сдвиге на конец отпуска, обращаем внимание на случаи когда отпуск еще не начался.")]
		public void UpdateNextIssue_NotMoveDateWhenLeaveNotBeginCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation>() { operation1 };
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

		#region MatcheStockPosition

		[Test(Description = "Проверяем случай при котором у складской позиции отсутсвует рост, значит эта номеклатура без роста и не надо сравнивать ее по росту с сотрудником.")]
		[TestCase("")]
		[TestCase(null)]
		[TestCase("182")]
		[TestCase("182-188")]
		public void MatcheStockPosition_WithoutGrowthCase(string growth)
		{
			var itemType = Substitute.For<ItemsType>();
			itemType.WearCategory.Returns(СlothesType.Wear);
			var sizeStd = SizeHelper.GetSizeStdCode(SizeStandartMenWear.Rus);
			var growthStd = SizeHelper.GetSizeStdCode(GrowthStandartWear.Men);
			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.Type.Returns(itemType);
			nomenclature.SizeStd.Returns(sizeStd);
			nomenclature.WearGrowthStd.Returns(growthStd);
			var protectionTools = Substitute.For<ProtectionTools>();
			protectionTools.MatchedNomenclatures.Returns(new[] { nomenclature });
			var employee = Substitute.For<EmployeeCard>();
			employee.Sex.Returns(Sex.M);
			employee.GetSize(СlothesType.Wear).Returns(new SizePair(sizeStd, "52"));
			employee.WearGrowth.Returns("182");
			var normItem = Substitute.For<NormItem>();
			normItem.ProtectionTools.Returns(protectionTools);

			EmployeeCardItem employeeCardItem = new EmployeeCardItem(employee, normItem);

			StockPosition stockPosition = new StockPosition(nomenclature, "52", growth, 0);
			Assert.That(employeeCardItem.MatcheStockPosition(stockPosition)); 
		}

		#endregion
		#endregion
		#region MatcheStockPosition

		[Test(Description = "Проверяем что находим соответсвиt размеров.")]
		public void MatcheStockPosition_SizeTest()
		{
			var employee = new EmployeeCard();
			employee.Sex = Sex.M;
			employee.WearSizeStd = SizeHelper.GetSizeStdCode(SizeStandartMenWear.Rus);
			employee.WearSize = "52";
			employee.WearGrowth = "170";

			var itemType = Substitute.For<ItemsType>();
			itemType.Category.Returns(ItemTypeCategory.wear);
			itemType.WearCategory.Returns(СlothesType.Wear);
			var nomeclature = Substitute.For<Nomenclature>();
			nomeclature.Id.Returns(25);
			nomeclature.Type.Returns(itemType);
			nomeclature.WearGrowthStd.Returns(SizeHelper.GetSizeStdCode(GrowthStandartWear.Universal));
			nomeclature.SizeStd.Returns(SizeHelper.GetSizeStdCode(SizeStandartMenWear.Rus));

			var employeeItem = new EmployeeCardItem();
			employeeItem.EmployeeCard = employee;
			employeeItem.Item = itemType;
			employeeItem.MatchedNomenclature.Add(nomeclature);

			var stockPosition = new StockPosition(nomeclature, "52", "170", 0);
			var result = employeeItem.MatcheStockPosition(stockPosition);
			Assert.That(result, Is.True);
		}

		[Test(Description = "Проверяем что находим соответсвия размеров когда в сотруднике установлен диапазон размера.")]
		public void MatcheStockPosition_RangeSizeInEmployeeSize()
		{
			var employee = new EmployeeCard();
			employee.Sex = Sex.M;
			employee.WearSizeStd = SizeHelper.GetSizeStdCode(SizeStandartMenWear.Rus);
			employee.WearSize = "52-54";
			employee.WearGrowth = "170";

			var itemType = Substitute.For<ItemsType>();
			itemType.Category.Returns(ItemTypeCategory.wear);
			itemType.WearCategory.Returns(СlothesType.Wear);
			var nomeclature = Substitute.For<Nomenclature>();
			nomeclature.Id.Returns(25);
			nomeclature.Type.Returns(itemType);
			nomeclature.WearGrowthStd.Returns(SizeHelper.GetSizeStdCode(GrowthStandartWear.Universal));
			nomeclature.SizeStd.Returns(SizeHelper.GetSizeStdCode(SizeStandartMenWear.Rus));

			var employeeItem = new EmployeeCardItem();
			employeeItem.EmployeeCard = employee;
			employeeItem.Item = itemType;
			employeeItem.MatchedNomenclature.Add(nomeclature);

			var stockPosition = new StockPosition(nomeclature, "52", "170", 0);
			var result = employeeItem.MatcheStockPosition(stockPosition);
			Assert.That(result, Is.True);
		}

		[Test(Description = "Проверяем что находим соответсвия размеров когда в сотруднике установлен диапазон роста.")]
		public void MatcheStockPosition_RangeGrowthInEmployeeSize()
		{
			var employee = new EmployeeCard();
			employee.Sex = Sex.M;
			employee.WearSizeStd = SizeHelper.GetSizeStdCode(SizeStandartMenWear.Rus);
			employee.WearSize = "52";
			employee.WearGrowth = "170-176";

			var itemType = Substitute.For<ItemsType>();
			itemType.Category.Returns(ItemTypeCategory.wear);
			itemType.WearCategory.Returns(СlothesType.Wear);
			var nomeclature = Substitute.For<Nomenclature>();
			nomeclature.Id.Returns(25);
			nomeclature.Type.Returns(itemType);
			nomeclature.WearGrowthStd.Returns(SizeHelper.GetSizeStdCode(GrowthStandartWear.Universal));
			nomeclature.SizeStd.Returns(SizeHelper.GetSizeStdCode(SizeStandartMenWear.Rus));

			var employeeItem = new EmployeeCardItem();
			employeeItem.EmployeeCard = employee;
			employeeItem.Item = itemType;
			employeeItem.MatchedNomenclature.Add(nomeclature);

			var stockPosition = new StockPosition(nomeclature, "52", "170", 0);
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
