using System;

namespace Workwear.Models.Import {
	public class SetValueAction {
		public int Order;
		public Action Action;

		public SetValueAction(int order, Action action) {
			Order = order;
			Action = action;
		}
	}
}
