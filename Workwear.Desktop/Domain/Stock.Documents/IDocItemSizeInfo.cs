using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents
{
	public interface IDocItemSizeInfo {
		SizeType HeightType { get; }
		Size Height { get; }
		SizeType WearSizeType { get; }
		Size WearSize { get; }
		int Amount { get; set; }
	}
}
