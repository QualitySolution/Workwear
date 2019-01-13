using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using workwear.Domain.Operations;
using workwear.Domain.Operations.Graph;

namespace WorkwearTest.Operations.Graph
{
	[TestFixture(TestOf = typeof(IssueGraph))]
	public class IssueGraphTest
	{

		private static object[] FineIntervalForDateCases =
		{
			new TestCaseData(new DateTime (2018, 2, 1),
				new DateTime[]{new DateTime(2018, 1 ,1 ), new DateTime(2018, 2, 1), new DateTime(2018, 3, 1), new DateTime(2025, 1, 1), new DateTime(2008, 1, 1) })
				.Returns(new DateTime(2018, 2, 1))
				.SetDescription("Выбор правильного интервала когда дата совпадает с началом интервала."),
			new TestCaseData(new DateTime (2018, 3, 9),
				new DateTime[]{new DateTime(2018, 1 ,1 ), new DateTime(2018, 3, 10), new DateTime(2018, 2, 15), new DateTime(2025, 1, 1), new DateTime(2008, 3, 1) })
				.Returns(new DateTime(2018, 2, 15))
				.SetDescription("Выбор правильного интервала когда дата на день раньше с начала следующего интервала интервала."),
			new TestCaseData(new DateTime (2018, 3, 9),
				new DateTime[]{new DateTime(2018, 3, 15 ), new DateTime(2018, 3, 7), })
				.Returns(new DateTime(2018, 3, 7))
				.SetDescription("Выбор правильного интервала в независимости от порядка следования интервалов."),
		};

		[Test]
		[TestCaseSource(nameof(FineIntervalForDateCases))]
		public DateTime FineIntervalForDateTest(DateTime fineDate, DateTime[] intervalDates)
		{
			var graph = new IssueGraph();
			graph.Intervals.AddRange(intervalDates.Select(x => new GraphInterval {StartDate = x}));

			return graph.IntervalOfDate(fineDate).StartDate;
		}

		[Test(Description = "Возвращаем пустой интервал если ищем дату до любых интервалов.")]
		public void FineIntervalForDateReturnNullBeforeIntervalsTest()
		{
			var graph = new IssueGraph();
			graph.Intervals.Add( new GraphInterval { StartDate = new DateTime(2018, 4, 1) });

			Assert.That(graph.IntervalOfDate(new DateTime(2018, 2, 2)), Is.Null);
		}

		[Test(Description = "Проверяем что механизм создания графа добавляет в конце пустой интервал с нулевым количеством числящегося.")]
		public void IssueGraphConstructor_ExistEndingIntervalWithZeroAmountTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation>() { operation1 };
			var graph = new IssueGraph(list);

			Assert.That(graph.OrderedIntervals.Last().CurrentCount, Is.EqualTo(0), "Количество в последнем интервале должно быть 0, при наличии автосписания.");
			Assert.That(graph.OrderedIntervals.First().CurrentCount, Is.EqualTo(10), "Количество в первом интервале должно быть 10.");
		}

		[Test(Description = "Проверяем что механизм создания графа создаст 3 интервала на 2 операции, выдачу, списание части, автосписание остатка.")]
		public void IssueGraphConstructor_Create3InntervalsTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.OperationTime.Returns(new DateTime(2018, 1, 15));
			operation2.Returned.Returns(2);

			var list = new List<EmployeeIssueOperation>() { operation1, operation2 };
			var graph = new IssueGraph(list);

			Assert.That(graph.Intervals.Count, Is.GreaterThanOrEqualTo(3));
			Assert.That(graph.Intervals.Last().StartDate, Is.EqualTo(new DateTime(2018, 2, 1)));
			Assert.That(graph.Intervals[1].StartDate, Is.EqualTo(new DateTime(2018, 1, 15)));
		}

		[Test(Description = "Проверяем что механизм создания интервалов не будет создавать несколько интервалов на один и тот же день, если в операции присутствует время.")]
		public void IssueGraphConstructor_CraeteOnlyOneIntervalForOneDayTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));

			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.OperationTime.Returns(new DateTime(2018, 1, 1, 17, 0, 0));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1, 18, 1, 1));

			var list = new List<EmployeeIssueOperation>() { operation1, operation2 };
			var graph = new IssueGraph(list);

			Assert.That(graph.Intervals.Count, Is.EqualTo(2));
			Assert.That(graph.Intervals.First().StartDate, Is.EqualTo(new DateTime(2018, 1, 1)));
			Assert.That(graph.Intervals.Last().StartDate, Is.EqualTo(new DateTime(2018, 2, 1)));
		}

		[Test(Description = "Проверяем что механизм при подсчете итого выданного за день и возвращенного, отображаются все операции. Создан про реальному кейсу, в котором проблема была в наличии времени в операции.")]
		public void IssuedAndWriteofAtDay_ShowAllMovmentsInOneDayTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.Id.Returns(1);
			operation1.OperationTime.Returns(new DateTime(2017, 11, 28));
			operation1.Issued.Returns(1);

			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.Id.Returns(2);
			operation2.OperationTime.Returns(new DateTime(2018, 12, 16));
			operation2.IssuedOperation.Returns(operation1);
			operation2.Returned.Returns(1);

			var operation3 = Substitute.For<EmployeeIssueOperation>();
			operation3.Id.Returns(3);
			operation3.OperationTime.Returns(new DateTime(2018, 12, 16, 17, 51, 0));
			operation3.Issued.Returns(1);

			var operation4 = Substitute.For<EmployeeIssueOperation>();
			operation4.Id.Returns(4);
			operation4.OperationTime.Returns(new DateTime(2018, 12, 16, 17, 21, 0));
			operation4.IssuedOperation.Returns(operation3);
			operation4.Returned.Returns(1);

			var operation5 = Substitute.For<EmployeeIssueOperation>();
			operation5.Id.Returns(5);
			operation5.OperationTime.Returns(new DateTime(2018, 12, 16));
			operation5.Issued.Returns(2);

			var list = new List<EmployeeIssueOperation>() { operation1, operation2, operation3, operation4, operation5 };
			var graph = new IssueGraph(list);

			//Assert.That(graph.Intervals.Count, Is.GreaterThanOrEqualTo(3));
			Assert.That(graph.Intervals.Last().Issued, Is.EqualTo(3));
			Assert.That(graph.Intervals.Last().WriteOff, Is.EqualTo(2));
		}
	}
}
