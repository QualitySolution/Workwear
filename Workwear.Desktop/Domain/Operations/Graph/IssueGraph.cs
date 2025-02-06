﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

[assembly:InternalsVisibleTo("Workwear.Test")]
namespace Workwear.Domain.Operations.Graph
{
	
	public interface IGraphIssueOperation:IDomainObject {
		DateTime OperationTime { get; }
		DateTime? StartOfUse { get; }
		DateTime? AutoWriteoffDate { get; }
		int Issued { get; set; }
		int Returned { get; }
		bool OverrideBefore { get; }

		IGraphIssueOperation IssuedOperation { get; }
	};
	
	public class IssueGraph
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public List<GraphInterval> Intervals = new List<GraphInterval>();

		private readonly IList<IGraphIssueOperation> operations;

		public IssueGraph() {
			operations = new List<IGraphIssueOperation>();
		}
		
		public IEnumerable<GraphInterval> OrderedIntervals => Intervals.OrderBy(x => x.StartDate);

		public IEnumerable<GraphInterval> OrderedIntervalsReverse => Intervals.OrderByDescending(x => x.StartDate);

		public IssueGraph(IList<IGraphIssueOperation> issues) {
			operations = issues; 
			Refresh();
		} 
		//Пока только для тестов	
	/*	public IssueGraph(IList<IGraphIssueOperation> issues) {
			operations = issues; 
			Refresh();
		}
*/
		/// <summary>
		/// Метод перестраивает граф после изменения дат в его операциях.
		/// Если необходимо добавить или удалить операции просто пересоздайте граф.
		/// </summary>
		public void Refresh(){
			Intervals = new List<GraphInterval>();
			List<DateTime> starts = operations.Select(x => x.OperationTime.Date).ToList();
			starts.AddRange(operations.Where(x => x.StartOfUse.HasValue).Select(x => x.StartOfUse.Value.Date));
			starts.AddRange(operations.Where(x => x.AutoWriteoffDate.HasValue).Select(x => x.AutoWriteoffDate.Value.Date));
			starts = starts.Distinct().OrderBy(x => x.Ticks).ToList();

			var graphItems = operations.Where(x => x.Issued > 0).Select(x => new GraphItem(x)).ToList();
			foreach (var issue in operations.Where(x => x.Returned > 0))
			{
				if (issue.IssuedOperation == null)
				{
					logger.Error($"{nameof(IGraphIssueOperation)}:{issue.Id} списывает спецодежду с сотрудника при этом не имеет ссылки на операцию по которой эта одежда была выдана сотруднику. Операция была пропущена в построение графа выдачи.");
					continue;
				}

				var item = graphItems.FirstOrDefault(x => x.IssueOperation.Id == issue.IssuedOperation.Id);
				if (item == null)
				{
					logger.Error($"{nameof(IGraphIssueOperation)}:{issue.Id} ссылается на некоректную операцию выдачи. Операция была пропущена в построение графа выдачи.");
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
				                                        (x.IssueOperation.AutoWriteoffDate == null || x.IssueOperation.AutoWriteoffDate >= date) &&
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

		public void AddOperations(IList<IGraphIssueOperation> operations) {
			foreach(var op in operations) 
				this.operations.Add(op);
			if(operations.Any())
				Refresh();
		}

		/// <summary>
		///  Возвращает операцию с указанной датой автосписания, если такая есть.
		///  Не учитывает ручные списания
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public IGraphIssueOperation GetWrittenOffOperation(DateTime date) {
			return operations.FirstOrDefault(o => o.AutoWriteoffDate.GetValueOrDefault().Date == date.Date );
		}
		
		public int AmountAtBeginOfDay(DateTime date, IGraphIssueOperation excludeOperation = null)
		{
			var interval = IntervalOfDate(date);
			if (interval == null)
				return 0;
			return interval.ActiveItems.Sum(x => x.AmountAtBeginOfDay(date, excludeOperation));
		}

		public int AmountAtEndOfDay(DateTime date, IGraphIssueOperation excludeOperation = null)
		{
			var interval = IntervalOfDate(date);
			if (interval == null)
				return 0;
			return interval.ActiveItems.Sum(x => x.AmountAtEndOfDay(date, excludeOperation));
		}

		public int UsedAmountAtEndOfDay(DateTime date, IGraphIssueOperation excludeOperation = null)
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
			return OrderedIntervalsReverse.FirstOrDefault(x => x.StartDate <= date.Date);
		}

		#endregion

		#region Статические

		internal static Func<EmployeeCard, ProtectionTools, IssueGraph> MakeIssueGraphTestGap;
		//Фабрика
		public static IssueGraph MakeIssueGraph(IUnitOfWork uow, EmployeeCard employee, ProtectionTools protectionTools, EmployeeIssueOperation[] unsavedOprarations = null)
		{
			if(MakeIssueGraphTestGap != null)
				return MakeIssueGraphTestGap(employee, protectionTools);
			
			var issues = uow.Session.QueryOver<EmployeeIssueOperation>()
					.Where(x => x.Employee.Id == employee.Id)
					.Where(x => x.ProtectionTools.Id == protectionTools.Id)
					.OrderBy(x => x.OperationTime).Asc
					.List();
			if(unsavedOprarations != null)
				foreach (var operation in unsavedOprarations)
				{
					if(!issues.Any(x => x.IsSame(operation)))
						issues.Add(operation);
				}
			
			return new IssueGraph(issues as IList<IGraphIssueOperation>);
		}
		#endregion
	}
}
