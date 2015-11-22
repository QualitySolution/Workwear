using System;
using Gamma.Utilities;

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
	}
}

