using NPOI.SS.UserModel;
using Workwear.Domain.Regulations;

namespace workwear.Models.Import.Norms
{
	public class SheetRowNorm : SheetRowBase<SheetRowNorm>
	{
		public SheetRowNorm(IRow[] cells) : base(cells)
		{
		}

		#region Найденные соответствия
		public SubdivisionPostCombination SubdivisionPostCombination;
		public NormItem NormItem;
		#endregion
	}
}
