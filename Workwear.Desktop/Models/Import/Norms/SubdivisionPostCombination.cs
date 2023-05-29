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
		public readonly string DepartmentValue;
		public readonly string[] DepartmentNames;

		public readonly List<Post> Posts = new List<Post>();
		public readonly List<Norm> Norms = new List<Norm>();

		public Norm EditingNorm => Norms.FirstOrDefault();

		public readonly HashSet<ProtectionTools> WillAddedProtectionTools = new HashSet<ProtectionTools>();

		public SubdivisionPostCombination(string postValue, string subdivisionValue, string departmentValue)
		{
			PostValue = postValue ?? throw new ArgumentNullException(nameof(postValue));
			PostNames = PostValue.Split(new[] { ',', ';', '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim()).ToArray();
			SubdivisionValue = subdivisionValue;
			if(!String.IsNullOrWhiteSpace(SubdivisionValue))
				SubdivisionNames = SubdivisionValue.Split(new[] { ',', ';', '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.Trim()).ToArray();
			if (!String.IsNullOrWhiteSpace(departmentValue))
				DepartmentNames = departmentValue.Split(new[] { ',', ';', '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.Trim()).ToArray();
		}

		public IEnumerable<(string subdivision, string department, string post)> AllPostNames {
			get {
				var subdivisions = SubdivisionNames ?? new string[] { null };
				var departments = DepartmentNames ?? new string[] { null };
				
				foreach (var subdivisionName in subdivisions)
					foreach (var departmentName in departments)
						foreach (var postName in PostNames)
							yield return (subdivisionName, departmentName, postName);
			}
		}
	}
}
