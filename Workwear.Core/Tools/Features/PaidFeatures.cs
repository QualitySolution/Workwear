using System;

namespace Workwear.Tools.Features {
	[Flags]
	public enum PaidFeatures : uint {
		None = 0,
		Barcodes = 1,
		ClothingService = 2,
		IdentityCards = 4
	}
}
