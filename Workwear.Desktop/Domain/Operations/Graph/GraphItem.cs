﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Workwear.Domain.Operations.Graph
{
	public class GraphItem
	{
		public IGraphIssueOperation IssueOperation;
		public List<IGraphIssueOperation> WriteOffOperations = new List<IGraphIssueOperation>();

		public GraphItem(IGraphIssueOperation issueOperation)
		{
			IssueOperation = issueOperation;
		}

		public void ReorderWriteoff()
		{
			WriteOffOperations = WriteOffOperations.OrderBy(x => x.OperationTime.Ticks).ToList();
		}

		/// <summary>
		/// Количество числящееся на условно "начало" дня, включает только полученное в этот день, но не списанное в этот день.
		/// </summary>
		/// <param name="excludeOperation">Исключить из расчета указанные операции</param>
		public int AmountAtBeginOfDay(DateTime date, IGraphIssueOperation excludeOperation = null)
		{
			if (IssueOperation == excludeOperation || (IssueOperation.Id > 0 && IssueOperation.Id == excludeOperation?.Id))
				return 0;

			if (IssueOperation.OperationTime.Date > date)
				return 0;

			if (IssueOperation.AutoWriteoffDate.HasValue && IssueOperation.AutoWriteoffDate.Value.Date < date.Date)
				return 0;

			return IssueOperation.Issued - WriteOffOperations.Where(x => !(x == excludeOperation || (x.Id > 0 && x.Id == excludeOperation?.Id)))
				.Where(x => x.OperationTime.Date < date.Date).Sum(x => x.Returned);
		}
		
		/// <summary>
		/// Количество числящееся на конец дня, включает списания произведенные в этот день.
		/// </summary>
		/// <param name="excludeOperation">Исключить из расчета указанные операции</param>
		public int AmountAtEndOfDay(DateTime date, IGraphIssueOperation excludeOperation = null)
		{
			if (IssueOperation == excludeOperation || (IssueOperation.Id > 0 && IssueOperation.Id == excludeOperation?.Id))
				return 0;

			if (IssueOperation.OperationTime.Date > date)
				return 0;

			if (IssueOperation.AutoWriteoffDate.HasValue && IssueOperation.AutoWriteoffDate.Value.Date <= date.Date)
				return 0;

			return IssueOperation.Issued - WriteOffOperations.Where(x => !(x == excludeOperation || (x.Id > 0 && x.Id == excludeOperation?.Id)))
				.Where(x => x.OperationTime.Date <= date.Date).Sum(x => x.Returned);
		}

		public int IssuedAtDate(DateTime date)
		{
			return IssueOperation.OperationTime.Date == date.Date ? IssueOperation.Issued : 0;
		}

		public int WriteoffAtDate(DateTime date)
		{
			if(IssueOperation.AutoWriteoffDate.HasValue && IssueOperation.AutoWriteoffDate.Value.Date == date.Date)
			{
				return AmountAtBeginOfDay(date);
			}
			else
				return WriteOffOperations.Where(x => x.OperationTime.Date == date.Date).Sum(x => x.Returned);
		}
	}
}
