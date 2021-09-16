using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;

namespace Workwear.Measurements
{
	/// <summary>
	/// Предоставляет различную информацию о работе с размерами.
	/// </summary>
	public class SizeService
	{
		private readonly ISizeSettings settings;

		public SizeService(ISizeSettings settings)
		{
			this.settings = settings;
		}

		#region Стандарты

		private static readonly Type[] AllSizeStdEnums = new[] {
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
		#endregion

		#region Работа с кодами стандартов

		public string GetSizeStdCode(object standartEnum)
		{
			Enum value = standartEnum as Enum;
			if(value == null)
				throw new InvalidCastException("standartEnum должен быть перечислением.");

			var att = value.GetAttribute<StdCodeAttribute>();
			return att == null ? null : att.Code;
		}

		public object GetSizeStdEnum(string code)
		{
			if(String.IsNullOrWhiteSpace(code))
				return null;

			foreach(var type in AllSizeStdEnums) {
				foreach(var field in type.GetFields()) {
					var att = field.GetCustomAttributes(typeof(StdCodeAttribute), false);
					if(att.Length > 0) {
						if((att[0] as StdCodeAttribute).Code == code)
							return field.GetValue(null);
					}
				}
			}
			throw new InvalidOperationException ($"Код размера {code} не найден.");
		}

		#endregion

		#region Получение отображаемого названия стандарта

		public string GetSizeStdShortTitle(string code)
		{
			if(GetSizeStdEnum(code) == null)
				return "";
			var std = (Enum)GetSizeStdEnum(code);
			return std.GetEnumShortTitle();
		}

		#endregion

		#region Получение стандарта

		#endregion

		#region Получить списки размеров

		/// <summary>
		/// Получения списка доступных размеров для использования в сотруднике.
		/// </summary>
		/// <param name="stdCode">Код стандарта размера</param>
		public string[] GetSizesForEmployee(string stdCode)
		{
			if(stdCode == null) return new string[] { " " };
			return GetSizesList(GetSizeStdEnum(stdCode), GetExcludedSizeUseForEmployee());
		}

		/// <summary>
		/// Получения списка доступных размеров для использования в номеклатуре.
		/// </summary>
		/// <param name="stdCode">Код стандарта размера</param>
		public string[] GetSizesForNomeclature(string stdCode)
		{
			if(stdCode == null) return new string[] { " " };
			return GetSizesList(GetSizeStdEnum(stdCode), GetExcludedSizeUseForNomencleture());
		}

		/// <summary>
		/// Получения списка доступных размеров для использования в сотруднике.
		/// </summary>
		/// <param name="std">стандарта размера</param>
		public string[] GetSizesForEmployee(Enum std)
		{
			return GetSizesList(std, GetExcludedSizeUseForEmployee());
		}

		/// <summary>
		/// Получения списка все доступных размеров, всех стандартов для использования в сотруднике.
		/// </summary>
		/// <param name="standartsEnum">Тип перечисления стандартов</param>
		public SizePair[] GetAllSizesForEmployee(Type standartsEnum)
		{
			var list = new List<SizePair>();
			foreach(var std in Enum.GetValues(standartsEnum))
				foreach(var size in GetSizesList(std, GetExcludedSizeUseForEmployee()))
					list.Add(new SizePair(GetSizeStdCode(std), size));
			return list.ToArray();
		}

		/// <summary>
		/// Получения списка все доступных размеров, всех стандартов для использования в номеклатуре.
		/// </summary>
		/// <param name="standartsEnum">Тип перечисления стандартов</param>
		public SizePair[] GetAllSizesForNomeclature(Type standartsEnum)
		{
			var list = new List<SizePair>();
			foreach(var std in Enum.GetValues(standartsEnum))
				foreach(var size in GetSizesList(std, GetExcludedSizeUseForNomencleture()))
					list.Add(new SizePair(GetSizeStdCode(std), size));
			return list.ToArray();
		}

		/// <summary>
		/// Получения списка доступных ростов для использования в сотруднике.
		/// </summary>>
		public string[] GetGrowthForEmployee()
		{
			return LookupSizes.UniversalGrowth.Where(x => !GetExcludedSizeUseForEmployee().Contains(x.Use)).Select(g => g.Name).ToArray();
		}

		/// <summary>
		/// Получения списка доступных ростов для использования в номеклатуре.
		/// </summary>>
		public string[] GetGrowthForNomenclature()
		{
			return LookupSizes.UniversalGrowth.Where(x => x.Use != SizeUse.HumanOnly).Select(g => g.Name).ToArray();
		}
		#endregion

		#region Внутреннее Размеры
		/// <summary>
		/// Возвращает исключения для использования размеров в сотрудниках.
		/// </summary>
		private SizeUse[] GetExcludedSizeUseForEmployee()
		{
			return settings.EmployeeSizeRanges ? new SizeUse[] { } : new SizeUse[] { SizeUse.СlothesOnly };
		}

		/// <summary>
		/// Возвращает исключения для использования размеров в номеклатуре.
		/// </summary>
		private SizeUse[] GetExcludedSizeUseForNomencleture()
		{
			return new SizeUse[] { };
		}

		private string[] GetSizesList(object stdEnum, params SizeUse[] excludeUse)
		{
			var array = GetSizeLookup(stdEnum.GetType());

			if(array == null)
				throw new ArgumentException(String.Format("Неизвестный стандарт размера {0}", stdEnum));

			return array
				.Where(x => !excludeUse.Contains(x.Use))
				.Select(x => x.Names[(int)stdEnum])
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct().ToArray();
		}

		private WearSize[] GetSizeLookup(Type stdEnum)
		{
			if(stdEnum == typeof(SizeStandartMenWear))
				return LookupSizes.MenWear;

			if(stdEnum == typeof(SizeStandartWomenWear))
				return LookupSizes.WomenWear;

			if(stdEnum == typeof(SizeStandartUnisexWear))
				return LookupSizes.UnisexWear;

			if(stdEnum == typeof(SizeStandartMenShoes))
				return LookupSizes.MenShoes;

			if(stdEnum == typeof(SizeStandartWomenShoes))
				return LookupSizes.WomenShoes;

			if(stdEnum == typeof(SizeStandartUnisexShoes))
				return LookupSizes.UnisexShoes;

			if(stdEnum == typeof(SizeStandartUnisexShoes))
				return LookupSizes.WomenShoes;

			if(stdEnum == typeof(SizeStandartHeaddress))
				return LookupSizes.Headdress;

			if(stdEnum == typeof(SizeStandartGloves))
				return LookupSizes.Gloves;

			if(stdEnum == typeof(SizeStandartMittens))
				return LookupSizes.Mittens;

			return null;
		}
		#endregion
	}
}
