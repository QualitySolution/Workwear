using System;
using workwear.Tools;

namespace Workwear.Measurements
{
	public class BaseSizeSettings : ISizeSettings
	{
		private readonly BaseParameters baseParameters;

		public BaseSizeSettings(BaseParameters baseParameters)
		{
			this.baseParameters = baseParameters;
		}

		public bool EmployeeSizeRanges => baseParameters.EmployeeSizeRanges;
	}
}
