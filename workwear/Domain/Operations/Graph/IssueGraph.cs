using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Repository.Stock;

namespace workwear.Domain.Operations.Graph
{
	public class IssueGraph
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public List<GraphInterval> Intervals = new List<GraphInterval>();

		public IssueGraph()
		{
		}

		public static IssueGraph MakeIssueGraph(IUnitOfWork uow, EmployeeCard employee, ItemsType itemsType)
		{
			var graph = new IssueGraph();

			var nomenclatures = NomenclatureRepository.GetNomenclaturesOfType(uow, itemsType);

			var issues = uow.Session.QueryOver<EmployeeIssueOperation>()
					.Where(x => x.Employee.Id == employee.Id)
					.Where(x => x.Nomenclature.Id.IsIn(nomenclatures.Select(n => n.Id).ToArray()))
					.OrderBy(x => x.OperationTime).Asc
					.List();

			//создаем интервалы.
			List<DateTime> starts = issues.Select(x => x.OperationTime).ToList();
			starts.AddRange(issues.Where(x => x.AutoWriteoffDate.HasValue).Select(x => x.AutoWriteoffDate.Value));
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
				if(item == null)
				{
					logger.Error($"{typeof(EmployeeIssueOperation).Name}:{issue.Id} ссылается на некоректную операцию выдачи. Операция была пропущена в построение графа выдачи.");
					continue;
				}
				item.WriteOffOperations.Add(issue);
			}

			//graphItems.ForEach(x => x.ReorderWriteoff());

			foreach (var date in starts)
			{
				var interval = new GraphInterval();
				interval.StartDate = date;
				foreach(var item in graphItems.Where(x => x.IssueOperation.OperationTime <= date))
				{
					int count = item.AmountAtDate(date);
					if (count <= 0)
						continue;
					interval.ActiveItems.Add(item);
					interval.CurrentCount += count;
				}
				graph.Intervals.Add(interval);
			}

			return graph;
		}

	}

	public class GraphInterval
	{
		public DateTime StartDate;
		public List<GraphItem> ActiveItems = new List<GraphItem>();
		public int CurrentCount;
	}

	public class GraphItem
	{
		public EmployeeIssueOperation IssueOperation;
		public List<EmployeeIssueOperation> WriteOffOperations = new List<EmployeeIssueOperation>();

		public GraphItem(EmployeeIssueOperation issueOperation)
		{
			IssueOperation = issueOperation;
		}

		public void ReorderWriteoff()
		{
			WriteOffOperations = WriteOffOperations.OrderBy(x => x.OperationTime.Ticks).ToList();
		}

		public int AmountAtDate(DateTime date)
		{
			if (IssueOperation.AutoWriteoffDate.HasValue && IssueOperation.AutoWriteoffDate.Value <= date)
				return 0;

			return IssueOperation.Issued - WriteOffOperations.Where(x => x.OperationTime <= date).Sum(x => x.Returned);
		}
	}
}
