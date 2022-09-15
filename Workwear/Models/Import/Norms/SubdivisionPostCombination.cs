using System;
using System.Collections.Generic;
using System.Linq;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace workwear.Models.Import.Norms
{
	public class SubdivisionPostCombination
	{
		public readonly string[] PostNames;
		public readonly string PostValue;
		public readonly string SubdivisionName;

		public readonly List<Post> Posts = new List<Post>();
		public readonly List<Norm> Norms = new List<Norm>();

		public Norm EditingNorm => Norms.FirstOrDefault();

		public readonly HashSet<ProtectionTools> WillAddedProtectionTools = new HashSet<ProtectionTools>();

		public SubdivisionPostCombination(string postNames, string subdivisionName)
		{
			PostValue = postNames ?? throw new ArgumentNullException(nameof(postNames));
			PostNames = PostValue.Split(new[] { ',', ';', '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim()).ToArray();
			SubdivisionName = subdivisionName;
		}
	}
}
