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
		public virtual AnswerOptions ShiftExpluatacion {
			get => Dynamic.ShiftExpluatacion(typeof(AnswerOptions)) ?? AnswerOptions.Ask;
			set => Dynamic[nameof(ShiftExpluatacion)] = value;
		}
		/// <summary>
		/// Спрашивать об увеличении периода эксплуатации выданного пропорционально количеству
		/// </summary>
		public virtual AnswerOptions ExtendPeriod {
			get => Dynamic.ExtendPeriod(typeof(AnswerOptions)) ?? AnswerOptions.Ask;
			set => Dynamic[nameof(ExtendPeriod)] = value;
		}

		#endregion
	}
	
	public enum AnswerOptions
	{
		[Display(Name = "Спрашивать")]
		Ask,
		[Display(Name ="Всегда да")]
		Yes,
		[Display(Name = "Всегда нет")]
		No
	}
}
