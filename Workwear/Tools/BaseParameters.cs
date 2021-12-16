using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using QS.BaseParameters;

namespace workwear.Tools
{
	public class  BaseParameters : ParametersService
	{
		public BaseParameters(DbConnection connection) : base(connection)
		{
		}

		/// <summary>
		/// Используется только для тестов!!!
		/// </summary>
		public BaseParameters()
		{
		}

		#region Типизированный доступ и дефолтные значения
		//Ключевое слово virtual у свойств необходимо для возможности подмены в тестах.

		public virtual bool DefaultAutoWriteoff {
			get => Dynamic.DefaultAutoWriteoff(typeof(bool)) ?? true;
			set => Dynamic[nameof(DefaultAutoWriteoff)] = value;
		}

		/// <summary>
		/// Используются ли диапазоны размеров в карточке сотрудника.
		/// </summary>
		public virtual bool EmployeeSizeRanges {
			get => Dynamic.EmployeeSizeRanges(typeof(bool)) ?? false;
			set => Dynamic[nameof(EmployeeSizeRanges)] = value;
		}
		public virtual int ColDayAheadOfShedule {
			get => Dynamic.ColDayAheadOfShedule(typeof(int)) ?? 0;
			set => Dynamic[nameof(ColDayAheadOfShedule)] = value;
		}

		/// <summary>
		/// Проверять остатки при расходе со склада.
		/// </summary>
		public virtual bool CheckBalances {
			get => Dynamic.CheckBalances(typeof(bool)) ?? true;
			set => Dynamic[nameof(CheckBalances)] = value;
		}
		/// <summary>
		/// Спрашивать о  перенесении начала эксплуатации:
		/// </summary>
		public virtual ShiftExpluatacion ShiftEpluatacion {
			get => Dynamic.ShiftExpluatacion(typeof(ShiftExpluatacion)) ?? ShiftExpluatacion.Ask;
			set => Dynamic[nameof(ShiftExpluatacion)] = value;
		}

		#endregion
	}
	
	public enum ShiftExpluatacion
	{
		[Display(Name ="Всегда да")]
		Yes,
		[Display(Name = "Всегда нет")]
		No,
		[Display(Name = "Спрашивать")]
		Ask
	}
}
