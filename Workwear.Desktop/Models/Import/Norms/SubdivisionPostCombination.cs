using System;
using System.Collections.Generic;
using System.Linq;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Import.Norms
{
	public class SubdivisionPostCombination
	{
		public readonly string[] PostNames;
		public readonly string PostValue;
		public readonly string SubdivisionValue;
		public readonly string[] SubdivisionNames;
		public readonly string DepartmentName;

		public readonly List<Post> Posts = new List<Post>();
		public readonly List<Norm> Norms = new List<Norm>();

		public Norm EditingNorm => Norms.FirstOrDefault();

		public readonly HashSet<ProtectionTools> WillAddedProtectionTools = new HashSet<ProtectionTools>();

		public SubdivisionPostCombination(string postValue, string subdivisionValue, string departmentName)
		{
			PostValue = postValue ?? throw new ArgumentNullException(nameof(postValue));
			PostNames = PostValue.Split(new[] { ',', ';', '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim()).ToArray();
			SubdivisionValue = subdivisionValue;
			if(!String.IsNullOrWhiteSpace(SubdivisionValue))
				SubdivisionNames = SubdivisionValue.Split(new[] { ',', ';', '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.Trim()).ToArray();
			DepartmentName = departmentName;
		}

		public IEnumerable<(string subdivision, string post)> AllPostNames {
			get {
				if (SubdivisionNames == null || SubdivisionNames.Length == 0)
					foreach (var postName in PostNames)
						yield return (null, postName);
				else
					foreach (var subdivisionName in SubdivisionNames)
					foreach (var postName in PostNames)
						yield return (subdivisionName, postName);
			}
		}
	}
}
