﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace workwear.Domain.Operations.Graph
{
	public class GraphInterval
	{
		public DateTime StartDate;
		
		/// <summary>
		/// Интервал содержит операцию переопределяющую все предыдущие выдачи. То есть если до этого чтото было не списано, то после этой выдачи оно обнуляется
		/// </summary>
		public bool Reset = false;
		public List<GraphItem> ActiveItems = new List<GraphItem>();
		public int CurrentCount;

		public int Issued => ActiveItems.Sum(x => x.IssuedAtDate(StartDate));
		public int WriteOff => ActiveItems.Sum(x => x.WriteoffAtDate(StartDate));
	}
}
