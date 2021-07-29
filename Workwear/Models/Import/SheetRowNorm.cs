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
		public SubdivisionPostPair SubdivisionPostPair;
		public NormItem NormItem;
		#endregion
	}

	public class SubdivisionPostPair
	{
		public readonly string PostName;
		public readonly string SubdivisionName;

		public Post Post;
		public List<Norm> Norms = new List<Norm>();

		public SubdivisionPostPair(string postName, string subdivisionName)
		{
			PostName = postName ?? throw new ArgumentNullException(nameof(postName));
			SubdivisionName = subdivisionName;
		}
	}
}
