using NUnit.Framework;
using System;
using System.Linq;
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
	}
}
