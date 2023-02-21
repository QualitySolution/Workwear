using System;
using System.Collections.Generic;
using System.Linq;

namespace Workwear.Domain.Operations.Graph
{
	public class GraphInterval
	{
		public DateTime StartDate;
		
		/// <summary>
		/// Интервал содержит операцию переопределяющую все предыдущие выдачи. То есть если до этого что-то было не списано, то после этой выдачи оно обнулятся
		/// </summary>
		public bool Reset = false;
		/// <summary>
		/// Все выдачи графа затрагивающие стартовый переход периода
		/// </summary>
		public readonly List<GraphItem> ActiveItems = new List<GraphItem>();

		/// <summary>
		/// Все выдачи графа где выданное количество действует в течении интервала. То есть ActiveItems, но без списанного в это день.
		/// </summary>
		public IEnumerable<GraphItem> ActiveIssues => ActiveItems.Where(x => x.AmountAtEndOfDay(StartDate) > 0);
		public int CurrentCount;

		public int Issued => ActiveItems.Sum(x => x.IssuedAtDate(StartDate));
		public int WriteOff => ActiveItems.Sum(x => x.WriteoffAtDate(StartDate));
		/// <summary>
		/// Отображает какое количество выданного используется в течении интервала. То есть исключаем выданное для которого дата начала использования еще не наступила.
		/// </summary>
		public int Used => ActiveItems.Where(x => x.IssueOperation.StartOfUse <= StartDate).Sum(x => x.AmountAtEndOfDay(StartDate));
	}
}
