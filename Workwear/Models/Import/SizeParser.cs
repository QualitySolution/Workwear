using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using Workwear.Measurements;

namespace workwear.Models.Import
{
	public static class SizeParser
	{
		public static SizeAndHeight ParseSizeAndGrowth(string value, IUnitOfWork uow, SizeService sizeService)
		{
			var result = new SizeAndHeight();

			var parts = value.Split(' ');
			var onlySize = parts[0];
			string size1 = "", size2 = "", growth1 = "", growth2 = "", number = "";
			bool isHyphen1 = false, isHyphen2 = false, isSeparator = false, isFloat= false;

			var sizeWear = new List<string> { "M", "L", "XL", "XXL", "XXXL", "4XL", "5XL" };

			if(onlySize.Count(x => x == 'L') == 1) {
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

				if(number.Length > 0 && !isHyphen1 && !isHyphen2 && !isSeparator)
					size1 = !isFloat && int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
				else if(number.Length > 0 && isHyphen1 && !isHyphen2 && !isSeparator)
					size2 = !isFloat && int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
				else if(number.Length > 0 && !isHyphen1 && !isHyphen2 && isSeparator)
					growth1 = number;
				else if(number.Length > 0 && isHyphen1 && !isHyphen2 && isSeparator)
					growth1 = number;
				else if(number.Length > 0 && isHyphen1 && isHyphen2 && isSeparator)
					growth2 = number;
				else if(number.Length > 0 && !isHyphen1 && isHyphen2 && isSeparator)
					growth2 = number;
			}

			result.Size = size2.Length > 0 ? size1 + "-" + size2 : size1;
			result.Height = growth2.Length > 0 ? growth1 + "-" + growth2 : growth1;

			return result;
		}

		public static Size ParseSize(IUnitOfWork uow, string value, SizeService sizeService, SizeType sizeType) =>
			sizeService.GetSize(uow, sizeType)
				.FirstOrDefault(x => x.Name == value || x.AlternativeName == value);

		#region Рост
		public static string HeightToGOST(string height) {
			if(int.TryParse(height, out int realHeight)) {
				var found = UniversalWearHeights.FirstOrDefault(x => realHeight >= x.Lower && realHeight < x.Upper);
				if(found != null)
					return found.Name;
			}

			return height;
		}

		public static readonly WearHeight[] UniversalWearHeights = new WearHeight[] {
			new WearHeight("146", 143, 149),
			new WearHeight("152", 149, 155),
			new WearHeight("158", 155, 161),
			new WearHeight("164", 161, 167),
			new WearHeight("170", 167, 173),
			new WearHeight("176", 173, 179),
			new WearHeight("182", 179, 185),
			new WearHeight("188", 185, 191),
			new WearHeight("194", 191, 197),
			new WearHeight("200", 197, 203),
			new WearHeight("210", 203, 210),
		};
		#endregion
	}
	public struct SizeAndHeight {
		public string Size;
		public string Height;
	}
	
	public class WearHeight{
		public string Name;
		public int Upper;
		public int Lower;

		public WearHeight(string name, int lower, int upper)
		{
			Name = name;
			Upper = upper;
			Lower = lower;
		}
	}
}
