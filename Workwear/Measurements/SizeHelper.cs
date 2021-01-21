using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using workwear.Domain.Company;

namespace workwear.Measurements
{
	public static class SizeHelper
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Стандарты

		public static readonly Type[] AllSizeStdEnums = new[] {
			typeof(SizeStandartMenWear),
			typeof(SizeStandartWomenWear),
			typeof(SizeStandartUnisexWear),
			typeof(SizeStandartMenShoes),
			typeof(SizeStandartWomenShoes),
			typeof(SizeStandartUnisexShoes),
			typeof(SizeStandartHeaddress),
			typeof(SizeStandartGloves),
			typeof(SizeStandartMittens)
		};

		#region Работа с кодами стандартов

		public static string GetSizeStdCode(object standartEnum)
		{
			Enum value = standartEnum as Enum;
			if (value == null)
				throw new InvalidCastException ("standartEnum должент быть перечислением.");
			
			var att = value.GetAttribute<StdCodeAttribute> ();
			return att == null ? null : att.Code;
		}

		public static object GetSizeStdEnum(string code)
		{
			if (String.IsNullOrWhiteSpace (code))
				return null;

			var sizeAndGrow = AllSizeStdEnums.Concat(new[] { typeof(GrowthStandartWear) });

			foreach(var type in sizeAndGrow)
			{
				foreach(var field in type.GetFields ())
				{
					var att = field.GetCustomAttributes (typeof(StdCodeAttribute), false);
					if(att.Length > 0)
					{
						if ((att [0] as StdCodeAttribute).Code == code)
							return field.GetValue (null);
					}
				}
			}
			logger.Warn ("Код размера {0} не найден.", code);
			return null;
		}

		#endregion

		#region Получение отображаемого названия стандарта

		public static string GetSizeStdShortTitle(string code)
		{
			if(GetSizeStdEnum(code) == null)
				return "";
			var std = (Enum)GetSizeStdEnum(code);
			return std.GetEnumShortTitle();
		}

		#endregion

		public static IList<string> GetSizeStandartsListOfCodes(СlothesType wearCategory, Sex sex)
		{
			var stdType = GetSizeStandartsEnum(wearCategory, sex);
			return Enum.GetValues(stdType).Cast<object>().Select(GetSizeStdCode).ToList();
		}

		public static Type GetSizeStandartsEnum(СlothesType wearCategory, Sex sex)
		{
			return GetSizeStandartsEnum (wearCategory, sex == Sex.F ? ClothesSex.Women : ClothesSex.Men);
		}

		public static Type GetSizeStandartsEnum(СlothesType wearCategory, ClothesSex sex)
		{
			var att = wearCategory.GetAttributes<SizeStandartsAttribute> ();
			if (att.Length == 0)
				return null;

			var found = att.FirstOrDefault (a => a.Sex == sex || a.Sex == ClothesSex.Universal);

			return found != null ? found.StandartsEnumType : null;
		}

		public static bool HasСlothesSizeStd(СlothesType wearCategory)
		{
			var att = wearCategory.GetAttributes<SizeStandartsAttribute> ();
			return att.Length > 0;
		}

		/// <summary>
		/// Проверяет нужно ли использовать рост(Growth) для текущего типа одежды
		/// </summary>
		/// <returns><c>true</c>, если нужно, <c>false</c>, если не нужно.</returns>
		/// <param name="wearCategory">Тип одежды (enum).</param>
		public static bool HasGrowthStandart(СlothesType wearCategory)
		{
			var att = wearCategory.GetAttribute<NeedGrowthAttribute>();
			return att != null;
		}

		public static GrowthStandartWear[] GetGrowthStandart(СlothesType wearCategory, Sex sex, SizeUsePlace place)
		{
			var att = wearCategory.GetAttribute<NeedGrowthAttribute> ();
			if (att == null)
				return null;
			
			var list = new List<GrowthStandartWear>();
			list.Add(GetGrowthStandart (wearCategory, sex == Sex.F ? ClothesSex.Women : ClothesSex.Men).Value);
			if (place == SizeUsePlace.Сlothes)
				list.Add(GetGrowthStandart(wearCategory, ClothesSex.Universal).Value);
			return list.ToArray();
		}

		public static SizeStandartsAttribute GetSizeStandartAttributeFromStd(object std)
		{
			return typeof(СlothesType).GetFields()
				.SelectMany(x => x.GetCustomAttributes(false).OfType<SizeStandartsAttribute>())
				.First(att => att.StandartsEnumType == std.GetType());
		}

		public static СlothesType GetСlothesTypeFromStd(object std)
		{
			return (СlothesType)typeof(СlothesType).GetFields()
				.Where(x => x.GetCustomAttributes(false).OfType<SizeStandartsAttribute>().Any(att => att.StandartsEnumType == std.GetType()))
				.First().GetValue(null);
		}

		public static object GetDefaultSizeStd(СlothesType wearCategory, Sex sex)
		{
			var type = SizeHelper.GetSizeStandartsEnum(wearCategory, sex);
			return Enum.GetValues(type).GetValue(0);
		}

		public static object GetDefaultSizeStd(СlothesType wearCategory, ClothesSex sex)
		{
			var type = SizeHelper.GetSizeStandartsEnum(wearCategory, sex);
			return type != null ? Enum.GetValues(type).GetValue(0) : null;
		}

		public static GrowthStandartWear? GetGrowthStandart(СlothesType wearCategory, ClothesSex sex)
		{
			var att = wearCategory.GetAttribute<NeedGrowthAttribute> ();
			if (att == null)
				return null;

			if (sex == ClothesSex.Women)
				return GrowthStandartWear.Women;

			if (sex == ClothesSex.Men)
				return GrowthStandartWear.Men;

			if (sex == ClothesSex.Universal)
				return GrowthStandartWear.Universal;

			return null;
		}

		#region Sex

		public static bool IsUniversalСlothes(СlothesType wearCategory)
		{
			var att = wearCategory.GetAttributes<SizeStandartsAttribute> ();
			if (att.Length == 0)
				throw new InvalidOperationException (String.Format ("У вида одежды {0} отсутствует атрибут SizeStandartsAttribute.", wearCategory));

			var found = att.FirstOrDefault (a => a.Sex == ClothesSex.Universal);

			return found != null;
		}

		public static bool HasClothesSex(СlothesType wearCategory)
		{
			var att = wearCategory.GetAttributes<SizeStandartsAttribute>();
			return att.Any(a => a.Sex == ClothesSex.Men || a.Sex == ClothesSex.Women);
		}

		#endregion

		public static SizeStandartsAttribute[] GetStandartsForСlothes(СlothesType wearCategory)
		{
			return wearCategory.GetAttributes<SizeStandartsAttribute> ();
		}

		#endregion

		#region Размеры

		[Obsolete("Желательно не использовать напряму, а использовать специальные методы из SizeService")]
		public static string[] GetSizesListByStdCode(string stdCode, params SizeUse[] excludeUse)
		{
			if(stdCode == null) return new string[] { " "};
			return GetSizesList(GetSizeStdEnum(stdCode), excludeUse);
		}
		public static string[] GetGrowthsArray(object stdEnum)
		{
			if(stdEnum is GrowthStandartWear) {
				switch((GrowthStandartWear)stdEnum) {
					case GrowthStandartWear.Men:
						return LookupSizes.MenGrowth.Select(g => g.Name).ToArray();
					case GrowthStandartWear.Women:
						return LookupSizes.WomenGrowth.Select(g => g.Name).ToArray();
					case GrowthStandartWear.Universal:
						return LookupSizes.UniversalGrowth.Select(g => g.Name).ToArray();
				}
			}
			return null;
		}

		public static string[] GetSizesList (object stdEnum, params SizeUse[] excludeUse)
		{
			var array = GetSizeLookup (stdEnum.GetType());

			if (array != null)
			{
				return array
					.Where(x => !excludeUse.Contains(x.Use))
					.Select(x => x.Names[(int)stdEnum])
					.Where(x => !String.IsNullOrEmpty(x))
					.Distinct().ToArray();
			}
				

			if (stdEnum is GrowthStandartWear)
			{
				switch ((GrowthStandartWear)stdEnum)
				{
					case GrowthStandartWear.Men:
						return LookupSizes.MenGrowth.Where(x => !excludeUse.Contains(x.Use)).Select(g => g.Name).ToArray();
					case GrowthStandartWear.Women:
						return LookupSizes.WomenGrowth.Where(x => !excludeUse.Contains(x.Use)).Select(g => g.Name).ToArray();
					case GrowthStandartWear.Universal:
						return LookupSizes.UniversalGrowth.Where(x => !excludeUse.Contains(x.Use)).Select(g => g.Name).ToArray();
				}
			}

			throw new ArgumentException ( String.Format ("Неизвестный стандарт размера {0}", stdEnum.ToString ()), "stdEnum");
		}

		private static WearGrowth[] GetWearGrowthLookup(GrowthStandartWear std)
		{
			switch (std) {
				case GrowthStandartWear.Men:
					return LookupSizes.MenGrowth;
				case GrowthStandartWear.Women:
					return LookupSizes.WomenGrowth;
				case GrowthStandartWear.Universal:
					return LookupSizes.UniversalGrowth;
				default:
					return null;
			}
		}

		private static WearSize[] GetSizeLookup(Type stdEnum)
		{
			if (stdEnum == typeof(SizeStandartMenWear))
				return LookupSizes.MenWear;

			if (stdEnum == typeof(SizeStandartWomenWear))
				return LookupSizes.WomenWear;

			if (stdEnum == typeof(SizeStandartUnisexWear))
				return LookupSizes.UnisexWear;

			if (stdEnum == typeof(SizeStandartMenShoes))
				return LookupSizes.MenShoes;

			if (stdEnum == typeof(SizeStandartWomenShoes))
				return LookupSizes.WomenShoes;

			if (stdEnum == typeof(SizeStandartUnisexShoes))
				return LookupSizes.UnisexShoes;

			if (stdEnum == typeof(SizeStandartUnisexShoes))
				return LookupSizes.WomenShoes;

			if (stdEnum == typeof(SizeStandartHeaddress))
				return LookupSizes.Headdress;

			if (stdEnum == typeof(SizeStandartGloves))
				return LookupSizes.Gloves;

			if(stdEnum == typeof(SizeStandartMittens))
				return LookupSizes.Mittens;

			return null;
		}

		public static List<SizePair> MatchSize(SizePair sizePair, SizeUsePlace place)
		{
			return MatchSize (sizePair.StandardCode, sizePair.Size, place);
		}

		public static List<SizePair> MatchSize(string sizeStdCode, string size, SizeUsePlace place)
		{
			return MatchSize (SizeHelper.GetSizeStdEnum (sizeStdCode), size, place);
		}

		public static List<SizePair> MatchSize(object sizeStd, string size, SizeUsePlace place)
		{
			var stdAtt = GetSizeStandartAttributeFromStd(sizeStd);
			var stds = new List<Type>();
			stds.Add(sizeStd.GetType());
			if(place == SizeUsePlace.Сlothes && stdAtt.Sex != ClothesSex.Universal)
			{
				var type = GetСlothesTypeFromStd(sizeStd);
				var unisex = GetStandartsForСlothes(type).FirstOrDefault(x => x.Sex == ClothesSex.Universal);
				if (unisex != null)
					stds.Add(unisex.StandartsEnumType);
			}

			var result = new List<SizePair> ();

			foreach(var sizeType in stds)
			{
				var lookupArray = GetSizeLookup (sizeType);
				if (lookupArray == null)
					continue;

				int original = (int)sizeStd;
				foreach(var lookup in lookupArray)
				{
					if (lookup.Use == SizeUse.СlothesOnly && place == SizeUsePlace.Human)
						continue;
					if (lookup.Use == SizeUse.HumanOnly && place == SizeUsePlace.Сlothes)
						continue;
					
					if(lookup.Appropriated.Any(x => x[original] == size))
					{
						AddSizePair(result, sizeType, lookup);
					}

					if(lookup.Names[original] == size) {
						foreach(var toAdd in lookup.Appropriated) {
							var wearsize = lookupArray.First(x => x.Names[original] == toAdd[original]);
							AddSizePair(result, sizeType, wearsize);
						}
					}
				}
			}
			return result;
		}

		private static void AddSizePair(List<SizePair> sizePairs, Type sizeType, WearSize size)
		{
			foreach(var std in Enum.GetValues(sizeType)) {
				var newPiar = new SizePair(GetSizeStdCode(std), size.Names[(int)std]);

				if(newPiar.Size == null)
					continue;

				if(!sizePairs.Any(pair => pair.StandardCode == newPiar.StandardCode && pair.Size == newPiar.Size))
					sizePairs.Add(newPiar);
			}
		}

		public static List<SizePair> MatchGrow(GrowthStandartWear[] stds, string grow, SizeUsePlace place)
		{
			var result = new List<SizePair>();
			foreach(var std in stds)
			{
				foreach(var lookupGrow in SizeHelper.GetWearGrowthLookup(std))
				{
					if (lookupGrow.Use == SizeUse.СlothesOnly && place == SizeUsePlace.Human)
						continue;
					if (lookupGrow.Use == SizeUse.HumanOnly && place == SizeUsePlace.Сlothes)
						continue;
					if (lookupGrow.Appropriated.Contains(grow))
						result.Add(new SizePair(SizeHelper.GetSizeStdCode(std), lookupGrow.Name));

					if(lookupGrow.Name == grow) {
						foreach(var toAdd in lookupGrow.Appropriated) {
							result.Add(new SizePair(SizeHelper.GetSizeStdCode(std), toAdd));
						}
					}
				}
			}
			return result;
		}

		#endregion

	}

	public class SizePair{
		public string StandardCode { get; private set;}
		public string Size { get; private set;}

		public SizePair(string std, string size)
		{
			StandardCode = std;
			Size = size;
		}
	}
}

