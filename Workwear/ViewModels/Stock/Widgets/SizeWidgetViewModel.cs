using System;
using System.Collections.Generic;
using System.Linq;
using QS.Navigation;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Measurements;

namespace workwear.ViewModels.Stock.Widgets
{
	public class SizeWidgetViewModel : WindowDialogViewModelBase
	{
		public IList<WearGrowth> WearGrowths { get;private set; }
		public IList<WearSize> WearSizes { get;private set; }
		public bool IsUseGrowth { get; set; } = false;

		private Nomenclature nomenclature;
		public Action<object, AddedSizesEventArgs> AddedSizes { get; set; } = (s,e) => { };

		public SizeWidgetViewModel(Nomenclature nomenclature,
			INavigationManager navigationManager
			) : base(navigationManager)
		{
			IsModal = true;

			this.nomenclature = nomenclature;
			ConfigureSizes(nomenclature.Sex,nomenclature.Type.WearCategory);
			SetWindowPosition(QS.Dialog.WindowGravity.None);
		}

		/// <summary>
		/// Конфигурирует списки ростов и размеров по полу и по типу одежды
		/// </summary>
		/// <param name="sex">Тип одедлы по полу.</param>
		/// <param name="clothesType">Тип одежды.</param>
		private void ConfigureSizes(ClothesSex? sex, СlothesType? clothesType)
		{
			if(sex != null || clothesType != null) {

				if(sex == ClothesSex.Men) {
					WearGrowths = LookupSizes.MenGrowth;
					switch(clothesType) {
						case СlothesType.Wear:
							WearSizes = LookupSizes.MenWear;
							IsUseGrowth = true;
							break;
						case СlothesType.Shoes:
							WearSizes = LookupSizes.MenShoes;
							break;
						case СlothesType.WinterShoes:
							WearSizes = LookupSizes.MenShoes;
							break;
						case СlothesType.Headgear:
							WearSizes = LookupSizes.Headdress;
							break;
						case СlothesType.Gloves:
							WearSizes = LookupSizes.Gloves;
							IsUseGrowth = true;
							break;
					}
				}
				else if(sex == ClothesSex.Women) {
					WearGrowths = LookupSizes.WomenGrowth;
					switch(clothesType) {
						case СlothesType.Wear:
							WearSizes = LookupSizes.WomenWear;
							IsUseGrowth = true;
							break;
						case СlothesType.Shoes:
							WearSizes = LookupSizes.WomenShoes;
							break;
						case СlothesType.WinterShoes:
							WearSizes = LookupSizes.WomenShoes;
							break;
						case СlothesType.Headgear:
							WearSizes = LookupSizes.Headdress;
							break;
						case СlothesType.Gloves:
							WearSizes = LookupSizes.Gloves;
							IsUseGrowth = true;
							break;
					}
				}
				else if(sex == ClothesSex.Universal) {
					WearGrowths = LookupSizes.UniversalGrowth;
					switch(clothesType) {
						case СlothesType.Wear:
							WearSizes = LookupSizes.WomenWear;
							IsUseGrowth = true;
							break;
						case СlothesType.Shoes:
							WearSizes = LookupSizes.WomenShoes;
							break;
						case СlothesType.WinterShoes:
							WearSizes = LookupSizes.WomenShoes;
							break;
						case СlothesType.Headgear:
							WearSizes = LookupSizes.Headdress;
							break;
						case СlothesType.Gloves:
							WearSizes = LookupSizes.Gloves;
							IsUseGrowth = true;
							break;
					}
				}
			}
		}

		/// <summary>
		/// Добавляет размеры,превращая их в объекты номенклатуры, и вызывает закрытие диалога
		/// </summary>
		/// <param name="currentGrowth">Выбранный рост.</param>
		/// <param name="sizes">Список размеро.</param>
		public void AddSizes(WearGrowth currentGrowth, IEnumerable<WearSize> sizes)
		{
			AddedSizesEventArgs args = new AddedSizesEventArgs(nomenclature,currentGrowth,sizes);
			AddedSizes(this, args);
			Close(false, CloseSource.Self);
		}
	}
	/// <summary>
	/// Класс содержащий объекты номеклатуры , с добавленными размерами
	/// </summary>
	public class AddedSizesEventArgs : EventArgs
	{
		public readonly Nomenclature Source;
		public readonly WearGrowth Growth;
		public readonly IEnumerable<WearSize> Sizes;
		public AddedSizesEventArgs(Nomenclature nomenclature,WearGrowth growth , IEnumerable<WearSize> sizes)
		{
			this.Source = nomenclature;
			Growth = growth;
			Sizes = sizes;
		}
	}
}
