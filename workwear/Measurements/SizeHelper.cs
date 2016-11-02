using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using workwear.Domain;

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
		};

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
			
			foreach(var type in AllSizeStdEnums)
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

		public static Type GetSizeStandartsEnum(СlothesType wearCategory, Sex sex)
		{
			return GetSizeStandartsEnum (wearCategory, sex == Sex.F ? ClothesSex.Women : ClothesSex.Men);
		}

		public static Type GetSizeStandartsEnum(СlothesType wearCategory, ClothesSex sex)
		{
			var att = wearCategory.GetAttributes<SizeStandartsAttribute> ();
			if (att.Length == 0)
				return null;

			var found = att.FirstOrDefault (a => a.Sex == sex);

			return found != null ? found.StandartsEnumType : null;
		}

		public static bool HasСlothesSizeStd(СlothesType wearCategory)
		{
			var att = wearCategory.GetAttributes<SizeStandartsAttribute> ();
			return att.Length > 0;
		}

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

		public static bool IsUniversalСlothes(СlothesType wearCategory)
		{
			var att = wearCategory.GetAttributes<SizeStandartsAttribute> ();
			if (att.Length == 0)
				throw new InvalidOperationException (String.Format ("У вида одежды {0} отсутствует атрибут SizeStandartsAttribute.", wearCategory));

			var found = att.FirstOrDefault (a => a.Sex == ClothesSex.Universal);

			return found != null;
		}

		public static SizeStandartsAttribute[] GetStandartsForСlothes(СlothesType wearCategory)
		{
			return wearCategory.GetAttributes<SizeStandartsAttribute> ();
		}

		#endregion

		#region Размеры

		public static string[] GetSizesList (object stdEnum, params SizeUse[] excludeUse)
		{
			var array = GetSizeLookup (stdEnum);

			if (array != null)
				return ReadSizeArray (array, (int)stdEnum);

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

		private static WearSize[] GetSizeLookup(object stdEnum)
		{
			if (stdEnum is SizeStandartMenWear)
				return LookupSizes.MenWear;

			if (stdEnum is SizeStandartWomenWear)
				return LookupSizes.WomenWear;

			if (stdEnum is SizeStandartUnisexWear)
				return LookupSizes.UnisexWear;

			if (stdEnum is SizeStandartMenShoes)
				return LookupSizes.MenShoes;

			if (stdEnum is SizeStandartWomenShoes)
				return LookupSizes.WomenShoes;

			if (stdEnum is SizeStandartUnisexShoes)
				return LookupSizes.UnisexShoes;

			if (stdEnum is SizeStandartUnisexShoes)
				return LookupSizes.WomenShoes;

			if (stdEnum is SizeStandartHeaddress)
				return LookupSizes.Headdress;

			if (stdEnum is SizeStandartGloves)
				return LookupSizes.Gloves;

			return null;
		}

		private static string[] ReadSizeArray(WearSize[] array, int column)
		{
			return array.Select(x => x.Names[column]).Where(x => !String.IsNullOrEmpty(x)).Distinct().ToArray();
		}

		public static void FillSizeCombo(ComboBox combo, string[] sizes)
		{
			combo.Clear ();
			var list = new ListStore (typeof(string));
			foreach (var size in sizes)
				list.AppendValues (size);
			combo.Model = list;
			CellRendererText text = new CellRendererText ();
			combo.PackStart (text, true);
			combo.AddAttribute (text, "text", 0);
		}

		public static List<SizePair> MatchSize(SizePair sizePair)
		{
			return MatchSize (sizePair.StandardCode, sizePair.Size);
		}

		public static List<SizePair> MatchSize(string sizeStdCode, string size)
		{
			return MatchSize (SizeHelper.GetSizeStdEnum (sizeStdCode), size);
		}

		public static List<SizePair> MatchSize(object sizeStd, string size)
		{
			var lookupArray = GetSizeLookup (sizeStd);
			if (lookupArray == null)
				return null;

			var result = new List<SizePair> ();

			int original = (int)sizeStd;
			for(int sizeIx = 0; sizeIx < lookupArray.GetLength (0); sizeIx++)
			{
				if(lookupArray[sizeIx].Names[original] == size)
				{
					foreach (var std in Enum.GetValues(sizeStd.GetType ()))
					{
						var newPiar = new SizePair (GetSizeStdCode (std), lookupArray[sizeIx].Names[(int)std]);

						if (newPiar.Size == null)
							continue;

						if (!result.Any (pair => pair.StandardCode == newPiar.StandardCode && pair.Size == newPiar.Size))
							result.Add (newPiar);
					}
				}
			}
			return result;
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

