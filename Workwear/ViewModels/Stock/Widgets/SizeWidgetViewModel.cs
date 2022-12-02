using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Measurements;

namespace Workwear.ViewModels.Stock.Widgets
{
	public class SizeWidgetViewModel : WindowDialogViewModelBase
	{
		public IList<Size> WearHeights { get;private set; }
		public IList<Size> WearSizes { get;private set; }
		public IList<AddSizeItem> SizeItems { get; private set; }
		public bool IsUseHeight { get; set; }
		public IList<IncomeItem> ExistItems { get; set; }
		private readonly SizeService sizeService;

		private readonly Nomenclature nomenclature;
		private readonly IUnitOfWork uoW;
		public Action<object, AddedSizesEventArgs> AddedSizes { get; set; } = (s,e) => { };
		public SizeWidgetViewModel(
			IncomeItem selectItem,
			INavigationManager navigationManager,
			IUnitOfWork uoW,
			SizeService sizeService,
			IList<IncomeItem> existItems = null) : base(navigationManager)
		{
			this.sizeService = sizeService;
			IsModal = true;
			Title = "Добавить размеры:";
			nomenclature = selectItem.Nomenclature;
			this.uoW = uoW;
			ExistItems = existItems;
			ConfigureSizes();
			height = selectItem.Height ?? WearHeights?.FirstOrDefault();
			UpdateAmounts();
		}

		/// <summary>
		/// Конфигурирует списки ростов и размеров
		/// </summary>
		private void ConfigureSizes() {
			if (nomenclature.Type.HeightType != null) {
				WearHeights = sizeService.GetSize(uoW, nomenclature.Type.HeightType,false, true).ToList();
				IsUseHeight = true;
			}
			WearSizes = nomenclature?.Type?.SizeType != null ? 
				sizeService.GetSize(uoW, nomenclature.Type.SizeType,false, true).ToList() : new List<Size>();

			SizeItems = WearSizes.Select(s => new AddSizeItem(s)).ToList();
			foreach(var item in SizeItems) {
				item.PropertyChanged += Item_PropertyChanged;
			}
		}

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(SensitiveAddButton));
			OnPropertyChanged(nameof(TotalText));
		}

		#region Свойства View
		private Size height;
		public virtual Size Height {
			get => height;
			set {
				if(SetField(ref height, value)) {
					UpdateAmounts();
				}
			}
		}

		public bool SensitiveAddButton => SizeItems.Any(x => x.IsUsed && x.Amount > 0);
		public string TotalText => $"Размеров выбрано: {SizeItems.Count(x => x.IsUsed)}\nКоличество итого: {SizeItems.Where(x => x.IsUsed).Sum(x => x.Amount)}";
		#endregion

		#region Actions
		/// <summary>
		/// Добавляет размеры, превращая их в объекты номенклатуры и вызывает закрытие диалога
		/// </summary>
		public void AddSizes() {
			var args = new AddedSizesEventArgs(nomenclature, Height, SizeItems.Where(x => x.IsUsed).ToList());
			AddedSizes(this, args);
			Close(false, CloseSource.Save);
		}
		#endregion

		#region private

		private void UpdateAmounts() {
			foreach(var item in SizeItems) {
				if(!item.IsUsed)
					item.Amount = ExistItems.Where(x => x.Height == Height && x.WearSize == item.Size).Select(x => x.Amount).FirstOrDefault();
			}
		}
		#endregion
	}

	public class AddSizeItem : PropertyChangedBase {
		public AddSizeItem(Size size) {
			Size = size ?? throw new ArgumentNullException(nameof(size));
		}

		public Size Size { get; }

		private bool isUsed;
		public virtual bool IsUsed {
			get => isUsed;
			set => SetField(ref isUsed, value);
		}

		private int amount;
		public virtual int Amount {
			get => amount;
			set => SetField(ref amount, value);
		}
	}

	/// <summary>
	/// Класс содержащий объекты номенклатуры, с добавленными размерами
	/// </summary>
	public class AddedSizesEventArgs : EventArgs {
		public readonly Nomenclature Source;
		public readonly Size Height;
		public readonly List<AddSizeItem> SizesWithAmount;
		public AddedSizesEventArgs(Nomenclature nomenclature, Size height, List<AddSizeItem> sizesWithAmount) {
			Source = nomenclature;
			Height = height;
			SizesWithAmount = sizesWithAmount;
		}
	}
}
