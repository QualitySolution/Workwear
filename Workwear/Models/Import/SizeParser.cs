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

		public static readonly MappingValue[] UniversalWearHeights = new MappingValue[] {
			new MappingValue("146", 143, 149),
			new MappingValue("152", 149, 155),
			new MappingValue("158", 155, 161),
			new MappingValue("164", 161, 167),
			new MappingValue("170", 167, 173),
			new MappingValue("176", 173, 179),
			new MappingValue("182", 179, 185),
			new MappingValue("188", 185, 191),
			new MappingValue("194", 191, 197),
			new MappingValue("200", 197, 203),
			new MappingValue("206", 203, 209),
			new MappingValue("212", 209, 215),
		};
		#endregion

		#region Обхват груди

		public static string BustToSize(string bust) {
			if(int.TryParse(bust, out int realBust)) {
				var found = BustMappingValues.FirstOrDefault(x => realBust >= x.Lower && realBust <= x.Upper);
				if(found != null)
					return found.Name;
			}
			return null;
		}

		public static readonly MappingValue[] BustMappingValues = new MappingValue[] {
			new MappingValue("38", 75, 78),
			new MappingValue("40", 79, 82),
			new MappingValue("42", 83, 86),
			new MappingValue("44", 87, 90),
			new MappingValue("46", 91, 94),
			new MappingValue("48", 95, 98),
			new MappingValue("50", 99, 102),
			new MappingValue("52", 103, 106),
			new MappingValue("54", 107, 110),
			new MappingValue("56", 111, 114),
			new MappingValue("58", 115, 118),
			new MappingValue("60", 119, 122),
			new MappingValue("62", 123, 126),
			new MappingValue("64", 127, 130),
			new MappingValue("66", 131, 134),
			new MappingValue("68", 135, 138),
			new MappingValue("70", 139, 142),
			new MappingValue("72", 143, 146),
			new MappingValue("74", 147, 150),
			new MappingValue("76", 151, 154),
			new MappingValue("78", 155, 158),
			new MappingValue("80", 159, 162),
			new MappingValue("82", 163, 166),
		};

		#endregion
	}
	public struct SizeAndHeight {
		public string Size;
		public string Height;
	}
	
	public class MappingValue{
		public string Name;
		public int Upper;
		public int Lower;

		public MappingValue(string name, int lower, int upper)
		{
			Name = name;
			Upper = upper;
			Lower = lower;
		}
	}
}
