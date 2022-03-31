using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using workwear.Domain.Sizes;

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
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
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
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
		public string GetSizeStdCode(object standartEnum)
		{
			Enum value = standartEnum as Enum;
			if(value == null)
				throw new InvalidCastException("standartEnum должен быть перечислением.");

			var att = value.GetAttribute<StdCodeAttribute>();
			return att == null ? null : att.Code;
		}
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
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
		#region Получить списки размеров
		/// <summary>
		/// Получения списка доступных размеров для использования в номеклатуре.
		/// </summary>
		/// <param name="stdCode">Код стандарта размера</param>
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
		public string[] GetSizesForNomeclature(string stdCode)
		{
			if(stdCode == null) return new string[] { " " };
			return GetSizesList(GetSizeStdEnum(stdCode), GetExcludedSizeUseForNomencleture());
		}
		/// <summary>
		/// Получения списка все доступных размеров, всех стандартов для использования в сотруднике.
		/// </summary>
		/// <param name="standartsEnum">Тип перечисления стандартов</param>
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
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
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
		public SizePair[] GetAllSizesForNomeclature(Type standartsEnum)
		{
			var list = new List<SizePair>();
			foreach(var std in Enum.GetValues(standartsEnum))
				foreach(var size in GetSizesList(std, GetExcludedSizeUseForNomencleture()))
					list.Add(new SizePair(GetSizeStdCode(std), size));
			return list.ToArray();
		}
		/// <summary>
		/// Получения списка доступных ростов для использования в номеклатуре.
		/// </summary>>
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
		public string[] GetGrowthForNomenclature()
		{
			return LookupSizes.UniversalGrowth.Where(x => x.Use != SizeUse.HumanOnly).Select(g => g.Name).ToArray();
		}
		#endregion
		#region Внутреннее Размеры
		/// <summary>
		/// Возвращает исключения для использования размеров в сотрудниках.
		/// </summary>
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
		private SizeUse[] GetExcludedSizeUseForEmployee()
		{
			return settings.EmployeeSizeRanges ? new SizeUse[] { } : new SizeUse[] { SizeUse.СlothesOnly };
		}
		/// <summary>
		/// Возвращает исключения для использования размеров в номеклатуре.
		/// </summary>
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
		private SizeUse[] GetExcludedSizeUseForNomencleture()
		{
			return new SizeUse[] { };
		}
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
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
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")] 
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
		#region Новые размеры
		public static IList<Size> GetSize(IUnitOfWork UoW, SizeType sizeType = null) {
			var sizes = UoW.Session.QueryOver<Size>();
			return sizeType is null ? 
				sizes.List() : sizes.Where(x => x.SizeType == sizeType).List();
		}
		public static IList<SizeType> GetSizeType(IUnitOfWork UoW) 
			=> UoW.Session.QueryOver<SizeType>().List();
		public static IList<SizeType> GetSizeTypeByCategory(IUnitOfWork UoW, Category category) 
			=> UoW.Session.QueryOver<SizeType>()
				.Where(x => x.Category == category).List();

		public static IEnumerable<Size> GetSizeByCategory(IUnitOfWork UoW, Category category) {
			SizeType sizeTypeAlias = null;
			var query = UoW.Session.QueryOver<Size>()
				.JoinAlias(x => x.SizeType, () => sizeTypeAlias)
				.Where(x => sizeTypeAlias.Category == category).List();
			return query;
		}
		#endregion
	}
}
