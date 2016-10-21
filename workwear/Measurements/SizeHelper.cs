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
			typeof(SizeStandartMenShoes),
			typeof(SizeStandartWomenShoes),
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

		public static GrowthStandartWear? GetGrowthStandart(СlothesType wearCategory, Sex sex)
		{
			return GetGrowthStandart (wearCategory, sex == Sex.F ? ClothesSex.Women : ClothesSex.Men);
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

		public static string[] GetSizesList (object stdEnum)
		{
			var array = GetSizeLookup (stdEnum);

			if (array != null)
				return ReadSizeArray (array, (int)stdEnum);

			if (stdEnum is GrowthStandartWear)
			{
				if ((GrowthStandartWear)stdEnum == GrowthStandartWear.Men)
					return LookupSizes.MenGrowth.Select (g => g.Name).ToArray ();
				else
					return LookupSizes.WomenGrowth.Select (g => g.Name).ToArray ();
			}

			throw new ArgumentException ( String.Format ("Неизвестный стандарт размера {0}", stdEnum.ToString ()), "stdEnum");
		}

		private static string[,] GetSizeLookup(object stdEnum)
		{
			if (stdEnum is SizeStandartMenWear)
				return LookupSizes.MenWear;

			if (stdEnum is SizeStandartWomenWear)
				return LookupSizes.WomenWear;

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

		private static string[] ReadSizeArray(string[,] array, int column)
		{
			var list = new List<string> ();

			for(int i = 0; i < array.GetLength (0); i++)
			{
				if (string.IsNullOrEmpty (array [i, column]))
					continue;
				list.Add (array[i, column]);
			}

			return list.Distinct ().ToArray ();
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
				if(lookupArray[sizeIx, original] == size)
				{
					foreach (var std in Enum.GetValues(sizeStd.GetType ()))
					{
						var newPiar = new SizePair (GetSizeStdCode (std), lookupArray[sizeIx, (int)std]);

						if (newPiar.Size == null)
							continue;

						if (!result.Any (pair => pair.StandardCode == newPiar.StandardCode && pair.Size == newPiar.Size))
							result.Add (newPiar);
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

