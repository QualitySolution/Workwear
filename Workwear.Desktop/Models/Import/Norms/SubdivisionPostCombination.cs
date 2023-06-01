using System;
using System.Collections.Generic;
using System.Linq;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Import;

namespace Workwear.Models.Import.Norms
{
	public class SubdivisionPostCombination
	{
		private readonly SettingsNormsViewModel settings;
		public readonly string NameValue;
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

		public SubdivisionPostCombination(SettingsNormsViewModel settings, string nameValue, string postValue, string subdivisionValue, string departmentValue)
		{
			this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
			NameValue = nameValue;
			PostValue = postValue;
			SubdivisionValue = subdivisionValue;
			DepartmentValue = departmentValue;
			if (!String.IsNullOrWhiteSpace(PostValue)) 
				PostNames = SplitValue(PostValue);
			if(!String.IsNullOrWhiteSpace(SubdivisionValue))
				SubdivisionNames = SplitValue(SubdivisionValue);
			if (!String.IsNullOrWhiteSpace(DepartmentValue))
				DepartmentNames = SplitValue(DepartmentValue);
		}

		public IEnumerable<(string subdivision, string department, string post)> AllPostNames {
			get {
				if(PostNames == null)
					yield break;
				
				var subdivisions = SubdivisionNames ?? new string[] { null };
				var departments = DepartmentNames ?? new string[] { null };

				foreach (var subdivisionName in subdivisions)
					foreach (var departmentName in departments)
						foreach (var postName in PostNames)
							yield return (subdivisionName, departmentName, postName);
			}
		}

		#region private
		/// <summary>
		/// Разбивает строку на части через разделители.
		/// </summary>
		string[] SplitValue(string value) {
			if(String.IsNullOrEmpty(settings.ListSeparator))
				return new []{value};
			
			return value.Split(settings.ListSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim()).ToArray();
		}
		#endregion
	}
}
