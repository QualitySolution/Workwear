using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace Workwear.Domain.Operations.Graph
{
	public class IssueGraph
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public List<GraphInterval> Intervals = new List<GraphInterval>();

		public IssueGraph()
		{

		}

		public IEnumerable<GraphInterval> OrderedIntervals => Intervals.OrderBy(x => x.StartDate);

		public IEnumerable<GraphInterval> OrderedIntervalsReverse => Intervals.OrderByDescending(x => x.StartDate);

		public IssueGraph(IList<EmployeeIssueOperation> issues)
		{
			//создаем интервалы.
			List<DateTime> starts = issues.Select(x => x.OperationTime.Date).ToList();
			starts.AddRange(issues.Where(x => x.StartOfUse.HasValue).Select(x => x.StartOfUse.Value.Date));
			starts.AddRange(issues.Where(x => x.AutoWriteoffDate.HasValue).Select(x => x.AutoWriteoffDate.Value.Date));
			starts = starts.Distinct().OrderBy(x => x.Ticks).ToList();

			var graphItems = issues.Where(x => x.Issued > 0).Select(x => new GraphItem(x)).ToList();
			foreach (var issue in issues.Where(x => x.Returned > 0))
			{
				if (issue.IssuedOperation == null)
				{
					logger.Error($"{typeof(EmployeeIssueOperation).Name}:{issue.Id} списывает спецодежду с сотрудника при этом не имеет ссылки на операцию по которой эта одежда была выдана сотруднику. Операция была пропущена в построение графа выдачи.");
					continue;
				}

				var item = graphItems.FirstOrDefault(x => x.IssueOperation.Id == issue.IssuedOperation.Id);
				if (item == null)
				{
					logger.Error($"{typeof(EmployeeIssueOperation).Name}:{issue.Id} ссылается на некоректную операцию выдачи. Операция была пропущена в построение графа выдачи.");
					continue;
				}
				item.WriteOffOperations.Add(issue);
			}

			var resetDate = new DateTime();
			foreach (var date in starts)
			{
				var interval = new GraphInterval();
				interval.StartDate = date;
				interval.Reset = graphItems.Any(x =>
					x.IssueOperation.OperationTime.Date == date && x.IssueOperation.OverrideBefore);
				if (interval.Reset)
					resetDate = date;
				var activeItems = graphItems.Where(x => x.IssueOperation.OperationTime.Date <= date &&
				                                        (x.IssueOperation.OperationTime.Date > resetDate ||
				                                         (x.IssueOperation.OperationTime.Date == resetDate &&
				                                          x.IssueOperation.OverrideBefore)));
				foreach (var item in activeItems)
				{
					if (item.AmountAtBeginOfDay(date) <= 0)
						continue;
					interval.ActiveItems.Add(item);
					interval.CurrentCount += item.AmountAtEndOfDay(date);
				}
    
				if (interval.Issued != 0 || interval.WriteOff != 0)
					Intervals.Add(interval);
			}
		}

		#region Методы

		public int AmountAtBeginOfDay(DateTime date, EmployeeIssueOperation excludeOperation = null)
		{
			var interval = IntervalOfDate(date);
			if (interval == null)
				return 0;
			return interval.ActiveItems.Sum(x => x.AmountAtBeginOfDay(date, excludeOperation));
		}

		public int AmountAtEndOfDay(DateTime date, EmployeeIssueOperation excludeOperation = null)
		{
			var interval = IntervalOfDate(date);
			if (interval == null)
				return 0;
			return interval.ActiveItems.Sum(x => x.AmountAtEndOfDay(date, excludeOperation));
		}

		public int UsedAmountAtEndOfDay(DateTime date, EmployeeIssueOperation excludeOperation = null)
		{
			var interval = IntervalOfDate(date);
			if(interval == null)
				return 0;
			return interval.ActiveItems
				.Where(x => x.IssueOperation.StartOfUse == null || x.IssueOperation.StartOfUse.Value.Date <= date.Date)
				.Sum(x => x.AmountAtEndOfDay(date, excludeOperation));
		}

		public GraphInterval IntervalOfDate(DateTime date)
		{
			return Intervals.Where(x => x.StartDate <= date.Date).OrderByDescending(x => x.StartDate).Take(1).SingleOrDefault();
		}

		#endregion

		#region Статические

		internal static Func<EmployeeCard, ProtectionTools, IssueGraph> MakeIssueGraphTestGap;

		public static IssueGraph MakeIssueGraph(IUnitOfWork uow, EmployeeCard employee, ProtectionTools protectionTools, EmployeeIssueOperation[] unsavedOprarations = null)
		{
			if(MakeIssueGraphTestGap != null)
				return MakeIssueGraphTestGap(employee, protectionTools);
				
			var matchedProtectionTools = protectionTools.MatchedProtectionTools;

			var issues = uow.Session.QueryOver<EmployeeIssueOperation>()
					.Where(x => x.Employee.Id == employee.Id)
					.Where(x => x.ProtectionTools.Id.IsIn(matchedProtectionTools.Select(n => n.Id).ToArray()))
					.OrderBy(x => x.OperationTime).Asc
					.List();
			if(unsavedOprarations != null)
				foreach (var operation in unsavedOprarations)
				{
					if(!issues.Any(x => x.IsSame(operation)))
						issues.Add(operation);
				}
			
			return new IssueGraph(issues);
		}

		#endregion
	}
}
