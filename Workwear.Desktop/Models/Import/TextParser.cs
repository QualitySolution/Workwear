using System;
using System.ComponentModel.DataAnnotations;

namespace Workwear.Models.Import {
	public static class TextParser {
		public static string PrepareForCompare(string input) {
			if(input == null)
				return null;
			var text = String.Join(" ", input.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries));
			return text.ToLower().Replace('ё', 'е');
		}
		
		public static int GetMaxStringLenght<TEntity>(string name) {
			var att = typeof(TEntity).GetProperty(name).GetCustomAttributes(typeof(StringLengthAttribute), true);
			if(att.Length == 0)
				throw new InvalidOperationException($"Для свойства {typeof(TEntity)}.{name} не задан необходимый атрибут {typeof(StringLengthAttribute)}.");
			return ((StringLengthAttribute)att[0]).MaximumLength;
		}
	}
}
