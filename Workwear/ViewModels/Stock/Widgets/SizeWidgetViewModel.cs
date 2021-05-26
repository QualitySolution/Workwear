using System;
using System.Collections.Generic;
using System.Linq;
using QS.Navigation;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Measurements;
using Workwear.Measurements;

namespace workwear.ViewModels.Stock.Widgets
{
	public class SizeWidgetViewModel : WindowDialogViewModelBase
	{
		public IList<string> WearGrowths { get;private set; }
		public IList<string> WearSizes { get;private set; }
		public bool IsUseGrowth { get; set; } = false;

		private Nomenclature nomenclature;
		private readonly SizeService sizeService;
		public readonly Dictionary<string, List<string>> ExcludedSizesDictionary;
		public Action<object, AddedSizesEventArgs> AddedSizes { get; set; } = (s,e) => { };

		public SizeWidgetViewModel(
			Nomenclature nomenclature,
			INavigationManager navigationManager,
			SizeService sizeService
			) : base(navigationManager)
		{
			IsModal = true;
			Title = "Добавить размеры:";
			this.nomenclature = nomenclature;
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			ClothesSex sex = nomenclature.Sex ?? throw new ArgumentNullException("At SizeWidgetViewModel.SizeWidgetViewModel() constructor, " + typeof(ClothesSex).Name + " variable was null!");
			СlothesType clothesType = nomenclature.Type.WearCategory ?? throw new NullReferenceException("At SizeWidgetViewModel.SizeWidgetViewModel() constructor" + typeof(СlothesType).Name + " variable was null!");
			ConfigureSizes(sex,clothesType);
		}

		/// <summary>
		/// Используйте этот конструктор, если необходимо исключить какие-то размеры из списка виджета
		/// </summary>
		/// <param name="ExcludedSizesDictionary">Словарь исключаемых размеров(IDictionary <!--growth,sizes--> ), смотри класс workwear.Measurements.LookupSizes.</param>
		public SizeWidgetViewModel(
			Dictionary<string, List<string>> ExcludedSizesDictionary,
			Nomenclature nomenclature,
			INavigationManager navigationManager,
			SizeService sizeService
			) : this(nomenclature, navigationManager, sizeService)
		{
			this.ExcludedSizesDictionary = ExcludedSizesDictionary;
		}

		/// <summary>
		/// Конфигурирует списки ростов и размеров по полу и по типу одежды
		/// </summary>
		/// <param name="sex">Тип одежды по полу.</param>
		/// <param name="clothesType">Тип одежды.</param>
		private void ConfigureSizes(ClothesSex sex, СlothesType clothesType)
		{
			if(SizeHelper.HasGrowthStandart(clothesType)) {
				WearGrowths = sizeService.GetGrowthForEmployee();
				IsUseGrowth = true;
			}
			this.WearSizes = SizeHelper.GetSizesListByStdCode(nomenclature.SizeStd).ToList();
		}

		/// <summary>
		/// Добавляет размеры,превращая их в объекты номенклатуры, и вызывает закрытие диалога
		/// </summary>
		/// <param name="currentGrowth">Выбранный рост.</param>
		/// <param name="sizes"> Словарь размеров, где ключ - размер , значение - количество .</param>
		public void AddSizes(string currentGrowth,Dictionary<string, int> sizes)
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
		/// <summary>
		/// Key - size, value - amount
		/// </summary>
		public readonly Dictionary<string, int> SizesWithAmount;
		public AddedSizesEventArgs(Nomenclature nomenclature,string growth , Dictionary<string, int> SizesWithAmount)
		{
			this.Source = nomenclature;
			Growth = growth;
			this.SizesWithAmount = SizesWithAmount;
		}
	}
}
