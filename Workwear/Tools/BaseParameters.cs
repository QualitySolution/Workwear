using System;
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
		public virtual bool DefaultAutoWriteoff {
			get => Dynamic.DefaultAutoWriteoff(typeof(bool)) ?? true;
			set => Dynamic.DefaultAutoWriteoff = value;
		}

		/// <summary>
		/// Используются ли диапазоны размеров в карточке сотрудника.
		/// </summary>
		public virtual bool EmployeeSizeRanges {
			get => Dynamic.EmployeeSizeRanges(typeof(bool)) ?? false;
			set => Dynamic.EmployeeSizeRanges = value;
		}
		public virtual int ColDayAheadOfShedule {
			get => Dynamic.ColDayAheadOfShedule(typeof(int)) ?? 0;
			set => Dynamic.ColDayAheadOfShedule = value;
		}
		#endregion
	}
}
