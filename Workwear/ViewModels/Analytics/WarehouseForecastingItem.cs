using QS.DomainModel.Entity;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Analytics {
	public class WarehouseForecastingItem : PropertyChangedBase {
		
		private ProtectionTools protectionTool;
		public virtual ProtectionTools ProtectionTool {
			get => protectionTool;
			set => SetField(ref protectionTool, value);
		}

		private Size size;
		public virtual Size Size {
			get => size;
			set => SetField(ref size, value);
		}

		private Size height;
		public virtual Size Height {
			get => height;
			set => SetField(ref height, value);
		}

		private int inStock;
		public virtual int InStock {
			get => inStock;
			set => SetField(ref inStock, value);
		}
		
		private int unissued;
		public virtual int Unissued {
			get => unissued;
			set => SetField(ref unissued, value);
		}
		

		#region Расчетные для отображения

		public string SizeText => SizeService.SizeTitle(size, height);

		#endregion
	}
}
