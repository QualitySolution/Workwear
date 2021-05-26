using System;
using System.Collections.Generic;

namespace Workwear.Measurements
{

	public struct WearGrowth{
		public string Name;
		public int Upper;
		public int Lower;
		public SizeUse Use;
		public string[] Suitable;

		public WearGrowth(string name, int upper, int lower, SizeUse use = SizeUse.Both, params string[] suitable)
		{
			Name = name;
			Upper = upper;
			Lower = lower;
			Use = use;
			Suitable = suitable;
		}

		public IEnumerable<string> Appropriated
		{
			get{
				yield return Name;

				if (Suitable == null || Suitable.Length == 0)
					yield break;

				foreach (var grow in Suitable)
					yield return grow;
			}
		}
	}
}
