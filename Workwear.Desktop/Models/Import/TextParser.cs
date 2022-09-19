using System;

namespace Workwear.Models.Import {
	public static class TextParser {
		public static string PrepareForCompare(string input) {
			if(input == null)
				return null;
			var text = String.Join(" ", input.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries));
			return text.ToLower().Replace('ё', 'е');
		}
	}
}
