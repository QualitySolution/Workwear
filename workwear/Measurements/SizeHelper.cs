using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;

namespace workwear.Measurements
{
	public static class SizeHelper
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

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

		public static string[] GetSizesList (object stdEnum)
		{
			if (stdEnum is SizeStandartMenWear)
				return ReadSizeArray (LookupSizes.MenWear, (int)stdEnum);
			
			if (stdEnum is SizeStandartWomenWear)
				return ReadSizeArray (LookupSizes.WomenWear, (int)stdEnum);

			if (stdEnum is SizeStandartMenShoes)
				return ReadSizeArray (LookupSizes.MenShoes, (int)stdEnum);

			if (stdEnum is SizeStandartWomenShoes)
				return ReadSizeArray (LookupSizes.WomenShoes, (int)stdEnum);

			if (stdEnum is SizeStandartHeaddress)
				return ReadSizeArray (LookupSizes.Headdress, (int)stdEnum);

			if (stdEnum is SizeStandartGloves)
				return ReadSizeArray (LookupSizes.Gloves, (int)stdEnum);

			if (stdEnum is GrowthStandartWear)
			{
				if ((GrowthStandartWear)stdEnum == GrowthStandartWear.Men)
					return LookupSizes.MenGrowth.Select (g => g.Name).ToArray ();
				else
					return LookupSizes.WomenGrowth.Select (g => g.Name).ToArray ();
			}

			throw new ArgumentException ( String.Format ("Неизвестный стандарт размера {0}", stdEnum.ToString ()), "stdEnum");
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

		public static Type GetSizeStandartsEnum(СlothesType wearCategory)
		{
			var att = wearCategory.GetAttribute<SizeStandartsAttribute> ();
			if (att == null)
				throw new InvalidOperationException (String.Format ("У вида одежды {0} отсутствует атрибут SizeStandartsAttribute.", wearCategory));

			return att.StandartsEnumType;
		}

		public static GrowthStandartWear? GetGrowthStandart(СlothesType wearCategory)
		{
			var att = wearCategory.GetAttribute<NeedGrowthAttribute> ();
			if (att == null)
				return null;

			return att.Sex == workwear.Domain.Sex.F ? GrowthStandartWear.Women : GrowthStandartWear.Men;
		}

	}
}

