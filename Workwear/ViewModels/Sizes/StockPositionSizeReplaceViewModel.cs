using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Sizes;
using Workwear.Tools.Sizes;
using workwear.Journal.ViewModels.Stock;

namespace Workwear.ViewModels.Sizes {
	/// <summary>
	/// Диалог замены конкретного значения размера/роста для выбранной складской позиции
	/// во всех операциях с данной номенклатурой.
	/// </summary>
	public class StockPositionSizeReplaceViewModel : WindowDialogViewModelBase {
		private readonly IInteractiveService interactive;
		private readonly ModalProgressCreator progressCreator;
		private readonly StockPositionSizeReplaceModel model;
		private readonly IUnitOfWork uow;
		private readonly Nomenclature nomenclature;
		private readonly Size currentSize;
		private readonly Size currentHeight;

		public StockPositionSizeReplaceViewModel(
			StockBalanceJournalNode node,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			IInteractiveService interactive,
			ModalProgressCreator progressCreator,
			SizeService sizeService,
			StockPositionSizeReplaceModel model) : base(navigation)
		{
			if(node == null) throw new ArgumentNullException(nameof(node));
			if(uowFactory == null) throw new ArgumentNullException(nameof(uowFactory));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			uow = uowFactory.CreateWithoutRoot();
			nomenclature  = uow.GetById<Nomenclature>(node.Id);
			currentSize   = node.SizeId.HasValue   ? uow.GetById<Size>(node.SizeId.Value)   : null;
			currentHeight = node.HeightId.HasValue ? uow.GetById<Size>(node.HeightId.Value) : null;

			Title = $"Замена размера — {nomenclature?.Name}";

			if(currentSize != null && nomenclature?.Type?.SizeType != null) {
				AvailableSizes = sizeService
					.GetSize(uow, nomenclature.Type.SizeType, onlyUseInNomenclature: true)
					.ToList();
				NewSize = AvailableSizes.FirstOrDefault(s => s.Id == currentSize.Id);
			}

			if(currentHeight != null && nomenclature?.Type?.HeightType != null) {
				AvailableHeights = sizeService
					.GetSize(uow, nomenclature.Type.HeightType, onlyUseInNomenclature: true)
					.ToList();
				NewHeight = AvailableHeights.FirstOrDefault(h => h.Id == currentHeight.Id);
			}
		}

		#region Свойства отображения

		public string NomenclatureName    => nomenclature?.Name;
		public string CurrentSizeName     => currentSize?.Name   ?? "—";
		public string CurrentHeightName   => currentHeight?.Name ?? "—";
		public bool   VisibleSize         => currentSize   != null;
		public bool   VisibleHeight       => currentHeight != null;

		#endregion

		#region Выбор нового значения

		public IList<Size> AvailableSizes   { get; }
		public IList<Size> AvailableHeights { get; }

		private Size newSize;
		public virtual Size NewSize {
			get => newSize;
			set {
				if(SetField(ref newSize, value))
					OnPropertyChanged(nameof(CanReplace));
			}
		}

		private Size newHeight;
		public virtual Size NewHeight {
			get => newHeight;
			set {
				if(SetField(ref newHeight, value))
					OnPropertyChanged(nameof(CanReplace));
			}
		}

		public bool CanReplace =>
			(VisibleSize   && newSize   != null && newSize.Id   != currentSize?.Id)   ||
			(VisibleHeight && newHeight != null && newHeight.Id != currentHeight?.Id);

		#endregion

		#region Команды

		public void Replace() {
			var ok = model.ReplaceInStock(
				uow,
				interactive,
				progressCreator,
				nomenclature,
				currentSize,
				VisibleSize   ? newSize   : null,
				currentHeight,
				VisibleHeight ? newHeight : null);
			if(!ok) return;
			interactive.ShowMessage(ImportanceLevel.Info, "Замена размера успешно выполнена.");
			Close(false, CloseSource.Save);
		}

		public void Cancel() => Close(false, CloseSource.Cancel);

		#endregion

		public override void Close(bool askClose, CloseSource source) {
			uow?.Dispose();
			base.Close(askClose, source);
		}
	}
}

