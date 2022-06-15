using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using workwear.Domain.Company;
using workwear.Domain.Regulations;

namespace workwear.Models.Import
{
	public class SheetRowNorm : SheetRowBase<DataTypeNorm>
	{
		public SheetRowNorm(IRow cells) : base(cells)
		{
		}

		#region Найденные соответствия
		public SubdivisionPostCombination SubdivisionPostCombination;
		public NormItem NormItem;
		#endregion
	}

	public class SubdivisionPostCombination
	{
		public readonly string[] PostNames;
		public readonly string PostValue;
		public readonly string SubdivisionName;

		public readonly List<Post> Posts = new List<Post>();
		public readonly List<Norm> Norms = new List<Norm>();

		public SubdivisionPostCombination(string postNames, string subdivisionName)
		{
			PostValue = postNames ?? throw new ArgumentNullException(nameof(postNames));
			PostNames = PostValue.Split(new[] { ',', ';', '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
			SubdivisionName = subdivisionName;
		}
	}
}
