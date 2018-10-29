using System;
using QSSupportLib;

namespace workwear.Tools
{
	public static class  BaseParameters
	{
		public static bool DefaultAutoWriteoff => MainSupport.BaseParameters?.All == null || Boolean.Parse(MainSupport.BaseParameters.All[BaseParameterNames.DefaultAutoWriteoff.ToString()]);
	}

	public enum BaseParameterNames
	{
		DefaultAutoWriteoff
	}
}
