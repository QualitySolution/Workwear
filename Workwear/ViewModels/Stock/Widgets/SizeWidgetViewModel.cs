using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using workwear.Domain.Sizes;
using workwear.Domain.Stock;
using Workwear.Measurements;

namespace workwear.ViewModels.Stock.Widgets
{
	public class SizeWidgetViewModel : WindowDialogViewModelBase
	{
		public IList<Size> WearGrowths { get;private set; }
		public IList<Size> WearSizes { get;private set; }
		public bool IsUseGrowth { get; set; }

		private readonly Nomenclature nomenclature;
		private readonly IUnitOfWork uoW;
		public Action<object, AddedSizesEventArgs> AddedSizes { get; set; } = (s,e) => { };
		public SizeWidgetViewModel(
			Nomenclature nomenclature,
			INavigationManager navigationManager,
			IUnitOfWork uoW) : base(navigationManager)
		{
			IsModal = true;
			Title = "Добавить размеры:";
			this.nomenclature = nomenclature;
			this.uoW = uoW;
			ConfigureSizes();
		}

		/// <summary>
		/// Конфигурирует списки ростов и размеров
		/// </summary>
		private void ConfigureSizes() {
			if (nomenclature.Type.HeightType != null) {
				WearGrowths = SizeService.GetSize(uoW, nomenclature.Type.HeightType,false, true).ToList();
				IsUseGrowth = true;
			}
			WearSizes = nomenclature?.Type?.SizeType != null ? 
				SizeService.GetSize(uoW, nomenclature.Type.SizeType,false, true).ToList() : new List<Size>();
		}

		/// <summary>
		/// Добавляет размеры, превращая их в объекты номенклатуры и вызывает закрытие диалога
		/// </summary>
		/// <param name="currentGrowth">Выбранный рост.</param>
		/// <param name="sizes"> Словарь размеров, где ключ - размер , значение - количество .</param>
		public void AddSizes(Size currentGrowth, Dictionary<Size, int> sizes) {
			var args = new AddedSizesEventArgs(nomenclature,currentGrowth,sizes);
			AddedSizes(this, args);
			Close(false, CloseSource.Self);
		}
	}
	/// <summary>
	/// Класс содержащий объекты номеклатуры, с добавленными размерами
	/// </summary>
	public class AddedSizesEventArgs : EventArgs {
		public readonly Nomenclature Source;
		public readonly Size Height;
		/// <summary>
		/// Key - size, value - amount
		/// </summary>
		public readonly Dictionary<Size, int> SizesWithAmount;
		public AddedSizesEventArgs(Nomenclature nomenclature,Size height, Dictionary<Size, int> sizesWithAmount) {
			Source = nomenclature;
			Height = height;
			SizesWithAmount = sizesWithAmount;
		}
	}
}
