using System;
using System.Collections.Generic;
using System.Linq;

namespace workwear.Models.Import
{
	public static class SizeParser
	{
		public static SizeAndGrowth ParseSizeAndGrowth(string value)
		{
			var result = new SizeAndGrowth();

			string[] parts = value.Split(' ');
			string onlySize = parts[0];
			string size1 = "", size2 = "", growth1 = "", growth2 = "", number = "";
			bool isHyphen1 = false, isHyphen2 = false, isSeparator = false, isFloat= false;

			List<string> sizeWear = new List<string>() { "M", "L", "XL", "XXL", "XXXL", "4XL", "5XL" };

			if(onlySize.Where(x => x == 'L').Count() == 1) {
				onlySize = onlySize.Replace('2', 'X');
				onlySize = onlySize.Replace("3", "XX");
			}

			if(sizeWear.Contains(onlySize))
				size1 = onlySize;

			foreach(var character in onlySize) {
				if(char.IsDigit(character))
					number += character;
				else if(character == ',' || character == '.') {
					isFloat = true;
					number += ',';
					continue;
				}
				else if(character == '-' && !isSeparator) {
					isHyphen1 = true;
					isFloat = false;
					number = "";
					continue;
				}
				else if(character == '-' && isSeparator) {
					isHyphen2 = true;
					isFloat = false;
					number = "";
					continue;
				}
				else if(character == '/') {
					isSeparator = true;
					isFloat = false;
					number = "";
					continue;
				}
				else { number = ""; break; }

				if(number.Count() > 0 && !isHyphen1 && !isHyphen2 && !isSeparator)
					size1 = !isFloat && int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
				else if(number.Count() > 0 && isHyphen1 && !isHyphen2 && !isSeparator)
					size2 = !isFloat && int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
				else if(number.Count() > 0 && !isHyphen1 && !isHyphen2 && isSeparator)
					growth1 = number;
				else if(number.Count() > 0 && isHyphen1 && !isHyphen2 && isSeparator)
					growth1 = number;
				else if(number.Count() > 0 && isHyphen1 && isHyphen2 && isSeparator)
					growth2 = number;
				else if(number.Count() > 0 && !isHyphen1 && isHyphen2 && isSeparator)
					growth2 = number;
			}

			result.Size = size2.Length > 0 ? size1 + "-" + size2 : size1;
			result.Growth = growth2.Length > 0 ? growth1 + "-" + growth2 : growth1;

			return result;
		}

		public static string ParseSize(string value)
		{
			return value;
		}

		public static string ParseGrowth(string value)
		{
			return value;
		}
	}

	public struct SizeAndGrowth
	{
		public string Size;
		public string Growth;
	}
}
