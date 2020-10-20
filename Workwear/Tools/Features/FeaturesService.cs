using System;
namespace workwear.Tools.Features
{
	public class FeaturesService
	{
		public FeaturesService()
		{
		}

		public bool Available(WorkwearFeature feature)
		{
			if(feature == WorkwearFeature.Warehouses)
				#if ENTERPRISE
				return true;
				#else
				return false;
				#endif
			return false;
		}
	}

	public enum WorkwearFeature
	{
		Warehouses,
	}
}
