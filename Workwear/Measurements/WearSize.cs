using System;
using System.Collections.Generic;

namespace workwear.Measurements
{

	public struct WearSize{
		public string[] Names;
		public SizeUse Use;
		public string[][] Suitable;

		public WearSize(string[] names, SizeUse use = SizeUse.Both, params string[][] suitable)
		{
			Names = names;
			Use = use;
			Suitable = suitable;
		}

		public IEnumerable<string[]> Appropriated
		{
			get{
				yield return Names;

				if (Suitable == null || Suitable.Length == 0)
					yield break;

				foreach (var size in Suitable)
					yield return size;
			}
		}
	}
}