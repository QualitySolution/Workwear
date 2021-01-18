using System;
using workwear.Tools;

namespace workwear.Measurements
{
	/// <summary>
	/// Предоставляет различнную информацию о работе с размерами.
	/// </summary>
	public class SizeService
	{
		private readonly BaseParameters baseParameters;

		public SizeService(BaseParameters baseParameters)
		{
			this.baseParameters = baseParameters;
		}

		#region Получить списки размеров

		/// <summary>
		/// Получения списка доступных размеров для использования в сотруднике.
		/// В том числе и ростов по коду стандарта роста.
		/// </summary>
		/// <param name="stdCode">Код стандарта размера</param>
		public string[] GetSizesForEmployee(string stdCode)
		{
			if(stdCode == null) return new string[] { " " };
			return SizeHelper.GetSizesList(SizeHelper.GetSizeStdEnum(stdCode), GetExcludedSizeUseForEmployee());
		}

		/// <summary>
		/// Получения списка доступных размеров для использования в сотруднике.
		/// В том числе и ростов по коду стандарта роста.
		/// </summary>
		/// <param name="std">стандарта размера</param>
		public string[] GetSizesForEmployee(Enum std)
		{
			return SizeHelper.GetSizesList(std, GetExcludedSizeUseForEmployee());
		}
		#endregion

		#region Внутреннее
		/// <summary>
		/// Возвращает исключения для использования размеров в сотрудниках.
		/// </summary>
		private SizeUse[] GetExcludedSizeUseForEmployee()
		{
			return baseParameters.EmployeeSizeRanges ? new SizeUse[] { } : new SizeUse[] { SizeUse.СlothesOnly };
		}
		#endregion
	}
}
