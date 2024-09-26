using System.Linq;
using QS.DomainModel.Entity;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Models.Analytics;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Analytics {
	public class WarehouseForecastingItem : PropertyChangedBase {

		#region Группировка
		private ProtectionTools protectionTool;

		public WarehouseForecastingItem(
			IGrouping<(ProtectionTools ProtectionTools, Size Size, Size Height), FeatureIssue> group,
			StockBalance[] stocks,
			ClothesSex sex) {
			ProtectionTool = group.Key.ProtectionTools;
			Size = group.Key.Size;
			Height = group.Key.Height;
			Sex = sex;
			InStock = stocks
				.Where(x => x.Position.Nomenclature.Sex == Sex)
				.Where(x => x.Position.WearSize == Size && x.Position.Height == Height)
				.Sum(x => x.Amount);
			Unissued = group.Where(x => x.DelayIssueDate.HasValue).Sum(x => x.Amount);
		}

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
		
		private ClothesSex Sex { get; set; }
		#endregion

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
