using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;

namespace Workwear.Test.Domain.Operations.Graph
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

		#region IssueGraphConstructor
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
			operation1.Issued.Returns(1);

			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.OperationTime.Returns(new DateTime(2018, 1, 1, 17, 0, 0));
			operation2.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1, 18, 1, 1));
			operation2.Issued.Returns(2);

			var list = new List<EmployeeIssueOperation>() { operation1, operation2 };
			var graph = new IssueGraph(list);

			Assert.That(graph.Intervals.Count, Is.EqualTo(2));
			Assert.That(graph.Intervals.First().StartDate, Is.EqualTo(new DateTime(2018, 1, 1)));
			Assert.That(graph.Intervals.Last().StartDate, Is.EqualTo(new DateTime(2018, 2, 1)));
		}
		#endregion
		#region ActiveIssues
		[Test(Description = "Проверяем что при создании интервалов в ActiveItems не попадают операции которые уже списаны.")]
		public void ActiveIssues_DontAddWriteoffItemsTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(1);

			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.OperationTime.Returns(new DateTime(2018, 1, 15));
			operation2.AutoWriteoffDate.Returns(new DateTime(2018, 2, 15));
			operation2.Issued.Returns(2);

			var list = new List<EmployeeIssueOperation>() { operation1, operation2 };
			var graph = new IssueGraph(list);
			
			var interval = graph.IntervalOfDate(new DateTime(2018, 2, 7));
			Assert.That(interval.StartDate, Is.EqualTo(new DateTime(2018, 2, 1)));
			Assert.That(interval.ActiveIssues.Count, Is.EqualTo(1));
			Assert.That(interval.ActiveIssues.First().IssueOperation.Issued, Is.EqualTo(2));
		}
		#endregion

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

		[Test(Description = "Проверяем что не реагируем на время внутри поля StartOfUse при подсчете количества. Реальный баг.")]
		[Category("Real case")]
		public void UsedAmountAtEndOfDay_IgnoreTimeTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1, 9, 0, 0));
			operation1.StartOfUse.Returns(new DateTime(2018, 1, 1, 14, 0, 0));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(2);

			var list = new List<EmployeeIssueOperation>() { operation1 };
			var graph = new IssueGraph(list);

			Assert.That(graph.UsedAmountAtEndOfDay(new DateTime(2018, 1, 1)), Is.EqualTo(2));
		}
		
		#region AmountAtDay
		[Test(Description = "Проверяем что операции с флагом сброса обнуляют предыдущие интервалы.")]
		public void AmountAtDay_OverrideBeforeTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.Id.Returns(1);
			operation1.OperationTime.Returns(new DateTime(2017, 11, 28));
			operation1.StartOfUse.Returns(new DateTime(2017, 11, 28));
			operation1.ExpiryByNorm.Returns(new DateTime(2020, 11, 28));
			operation1.AutoWriteoffDate.Returns(new DateTime(2020, 11, 28));
			operation1.Issued.Returns(1);
			
			var operation3 = Substitute.For<EmployeeIssueOperation>();
			operation3.Id.Returns(3);
			operation3.OperationTime.Returns(new DateTime(2018, 12, 16, 17, 51, 0));
			operation3.StartOfUse.Returns(new DateTime(2018, 12, 16));
			operation3.ExpiryByNorm.Returns(new DateTime(2021, 12, 16));
			operation3.AutoWriteoffDate.Returns(new DateTime(2021, 12, 16));
			operation3.Issued.Returns(1);

			var operationOverride = Substitute.For<EmployeeIssueOperation>();
			operationOverride.Id.Returns(4);
			operationOverride.OperationTime.Returns(new DateTime(2019, 12, 16, 17, 21, 0));
			operationOverride.StartOfUse.Returns(new DateTime(2019, 12, 16));
			operationOverride.ExpiryByNorm.Returns(new DateTime(2021, 1, 10));
			operationOverride.AutoWriteoffDate.Returns(new DateTime(2021, 1, 10));
			operationOverride.OverrideBefore.Returns(true);
			operationOverride.Issued.Returns(5);
			
			var list = new List<EmployeeIssueOperation>() { operation1, operation3, operationOverride };
			var graph = new IssueGraph(list);
			
			//Проверяем корректное начисление количества
			Assert.That(graph.UsedAmountAtEndOfDay(new DateTime(2019, 12, 15)), Is.EqualTo(2));
			Assert.That(graph.AmountAtEndOfDay(new DateTime(2019, 12, 16)), Is.EqualTo(5));
			Assert.That(graph.UsedAmountAtEndOfDay(new DateTime(2020, 1, 15)), Is.EqualTo(5));
			Assert.That(graph.UsedAmountAtEndOfDay(new DateTime(2021, 1, 15)), Is.EqualTo(0));
			//Проверяем что не создали пустых интервалов.
			var last = graph.OrderedIntervals.Last();
			Assert.That(last.StartDate, Is.EqualTo(new DateTime(2021, 1, 10)));
		}
		
		[Test(Description = "Проверяем что операции с флагом сброса обнуляют так же другие операции в этот день. " +
		                    "Это нужно так как пользователь иногда хочет пере-установить выдачу на тот же день что и последняя выдача, " +
		                    "ради того чтобы сбросить предыдущую историю, в которой было выдано больше необходимого.")]
		public void AmountAtDay_OverrideBeforeInOneDayTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.Id.Returns(1);
			operation1.OperationTime.Returns(new DateTime(2017, 11, 28));
			operation1.StartOfUse.Returns(new DateTime(2017, 11, 28));
			operation1.ExpiryByNorm.Returns(new DateTime(2020, 11, 28));
			operation1.AutoWriteoffDate.Returns(new DateTime(2020, 11, 28));
			operation1.Issued.Returns(1);
			
			var operation3 = Substitute.For<EmployeeIssueOperation>();
			operation3.Id.Returns(3);
			operation3.OperationTime.Returns(new DateTime(2019, 12, 16, 0, 0, 0));
			operation3.StartOfUse.Returns(new DateTime(2019, 12, 16));
			operation3.ExpiryByNorm.Returns(new DateTime(2021, 12, 16));
			operation3.AutoWriteoffDate.Returns(new DateTime(2021, 12, 16));
			operation3.Issued.Returns(1);

			var operationOverride = Substitute.For<EmployeeIssueOperation>();
			operationOverride.Id.Returns(4);
			operationOverride.OperationTime.Returns(new DateTime(2019, 12, 16, 17, 21, 0));
			operationOverride.StartOfUse.Returns(new DateTime(2019, 12, 16));
			operationOverride.ExpiryByNorm.Returns(new DateTime(2021, 1, 10));
			operationOverride.AutoWriteoffDate.Returns(new DateTime(2021, 1, 10));
			operationOverride.OverrideBefore.Returns(true);
			operationOverride.Issued.Returns(5);
			
			var list = new List<EmployeeIssueOperation>() { operation1, operation3, operationOverride };
			var graph = new IssueGraph(list);
			
			//Проверяем корректное начисление количества
			Assert.That(graph.UsedAmountAtEndOfDay(new DateTime(2019, 12, 15)), Is.EqualTo(1));
			Assert.That(graph.AmountAtEndOfDay(new DateTime(2019, 12, 16)), Is.EqualTo(5));
			Assert.That(graph.UsedAmountAtEndOfDay(new DateTime(2020, 1, 15)), Is.EqualTo(5));
			Assert.That(graph.UsedAmountAtEndOfDay(new DateTime(2021, 1, 15)), Is.EqualTo(0));
			//Проверяем что не создали пустых интервалов.
			var last = graph.OrderedIntervals.Last();
			Assert.That(last.StartDate, Is.EqualTo(new DateTime(2021, 1, 10)));
			//Проверяем что имеется отметка что данный интервал сбрасывает предыдущую историю.
			var beforeLast = graph.OrderedIntervalsReverse.Skip(1).First();
			Assert.That(beforeLast.Reset, Is.True);
		}
		#endregion
		
		[Test(Description = "Проверяем обработку операций с выдачей и списанием в одной операции.")]
		public void AmountAtDay_OperationIssuedAndRemove_Test()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1, 9, 0, 0));
			operation1.StartOfUse.Returns(new DateTime(2018, 1, 1, 14, 0, 0));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018,6, 1));
			operation1.Issued.Returns(2);
			
			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.IssuedOperation.Returns(operation1);
			operation2.OperationTime.Returns(new DateTime(2018, 4, 1, 9, 0, 0));
			operation2.StartOfUse.Returns(new DateTime(2018, 4, 1, 9, 0, 0));
			operation2.AutoWriteoffDate.Returns(new DateTime(2018,8, 1));
			operation2.Issued.Returns(2);
			operation2.Returned.Returns(2);

			var list = new List<EmployeeIssueOperation>() { operation1, operation2 };
			var graph = new IssueGraph(list);
			
			Assert.That(graph.AmountAtEndOfDay(new DateTime(2018, 3, 1)), Is.EqualTo(2));
			Assert.That(graph.AmountAtEndOfDay(new DateTime(2018, 5, 1)), Is.EqualTo(2));
			Assert.That(graph.AmountAtEndOfDay(new DateTime(2018, 7, 1)), Is.EqualTo(2));
		}
	}
}
