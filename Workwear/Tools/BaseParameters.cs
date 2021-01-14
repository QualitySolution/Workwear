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

		#region Типизированный доступ и дефолтные значения
		public bool DefaultAutoWriteoff {
			get => Dynamic.DefaultAutoWriteoff(typeof(bool)) ?? true;
			set => Dynamic.DefaultAutoWriteoff = value;
		}
		public bool EmployeeSizeRanges {
			get => Dynamic.EmployeeSizeRanges(typeof(bool)) ?? false;
			set => Dynamic.EmployeeSizeRanges = value;
		}
		public int ColDayAheadOfShedule {
			get => Dynamic.ColDayAheadOfShedule(typeof(int)) ?? 0;
			set => Dynamic.ColDayAheadOfShedule = value;
		}
		#endregion
	}
}
