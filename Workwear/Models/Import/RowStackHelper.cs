using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace workwear.Models.Import {
	static public class RowStackHelper {
		public static void NewRow(Stack<IRow> stack, IRow row) {
			if(stack.Any()) {
				var shiftLevel = row.OutlineLevel - stack.Peek().OutlineLevel;
				if(shiftLevel >= 0)
					for(int i = 0; i <= shiftLevel; i++) {
						stack.Pop();
					}
				else {
					for(int i = shiftLevel; i < 0; i++) {
						stack.Push(row);
					}
				}
			}

			stack.Push(row);
		}
	}
}
