using System;
using QSSupportLib;

namespace workwear.Tools
{
	public static class  BaseParameters
	{
		public static bool DefaultAutoWriteoff => MainSupport.BaseParameters?.All == null 
			|| Boolean.Parse(MainSupport.BaseParameters.All[BaseParameterNames.DefaultAutoWriteoff.ToString()]);
		public static bool EmployeeSizeRanges => MainSupport.BaseParameters?.All != null 
			&& MainSupport.BaseParameters.All.ContainsKey(BaseParameterNames.EmployeeSizeRanges.ToString()) 
			&& Boolean.Parse(MainSupport.BaseParameters.All[BaseParameterNames.EmployeeSizeRanges.ToString()]);
	}

	public enum BaseParameterNames
	{
		DefaultAutoWriteoff,
		EmployeeSizeRanges
	}
}
