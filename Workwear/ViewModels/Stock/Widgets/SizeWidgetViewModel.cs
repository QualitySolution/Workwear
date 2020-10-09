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
		public IList<string> WearGrowths { get;private set; }
		public IList<string> WearSizes { get;private set; }
		public bool IsUseGrowth { get; set; } = false;

		private Nomenclature nomenclature;
		public Action<object, AddedSizesEventArgs> AddedSizes { get; set; } = (s,e) => { };

		public SizeWidgetViewModel(Nomenclature nomenclature,
			INavigationManager navigationManager
			) : base(navigationManager)
		{
			IsModal = false;

			this.nomenclature = nomenclature;
			ClothesSex sex = nomenclature.Sex ?? throw new ArgumentNullException("At SizeWidgetViewModel.SizeWidgetViewModel() constructor, " + typeof(ClothesSex).Name + " variable was null!");
			СlothesType clothesType = nomenclature.Type.WearCategory ?? throw new NullReferenceException("At SizeWidgetViewModel.SizeWidgetViewModel() constructor" + typeof(СlothesType).Name + " variable was null!");
			ConfigureSizes(sex,clothesType);
		}

		/// <summary>
		/// Конфигурирует списки ростов и размеров по полу и по типу одежды
		/// </summary>
		/// <param name="sex">Тип одедлы по полу.</param>
		/// <param name="clothesType">Тип одежды.</param>
		private void ConfigureSizes(ClothesSex sex, СlothesType clothesType)
		{
			GrowthStandartWear? standartWear = SizeHelper.GetGrowthStandart(clothesType, sex);
			if(standartWear == null)
				throw new NullReferenceException("At SizeWidgetViewModel.ConfigureSizes() method "+typeof(GrowthStandartWear).Name+" was null!");
			else {
				if(SizeHelper.HasGrowthStandart(clothesType)) {
					WearGrowths = SizeHelper.GetGrowthsArray(standartWear).ToList();
					IsUseGrowth = true;
				}
				this.WearSizes = SizeHelper.GetSizesList(standartWear).ToList();
			}
		}

		/// <summary>
		/// Добавляет размеры,превращая их в объекты номенклатуры, и вызывает закрытие диалога
		/// </summary>
		/// <param name="currentGrowth">Выбранный рост.</param>
		/// <param name="sizes">Список размеро.</param>
		public void AddSizes(string currentGrowth, IEnumerable<string> sizes)
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
		public readonly string Growth;
		public readonly IEnumerable<string> Sizes;
		public AddedSizesEventArgs(Nomenclature nomenclature,string growth , IEnumerable<string> sizes)
		{
			this.Source = nomenclature;
			Growth = growth;
			Sizes = sizes;
		}
	}
}
