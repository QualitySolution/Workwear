using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Supply;

namespace Workwear.ViewModels.Supply {
	public class ShipmentDiffCauseViewModel: UowDialogViewModelBase, IWindowDialogSettings {
		private ShipmentItem[] selectedItems;
		private IUnitOfWork uow;
		
		public ShipmentDiffCauseViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			ShipmentItem[] selectedItems,
			string initialDiffCause,
			IUnitOfWork unitOfWork
		) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider) {
			this.selectedItems = selectedItems;
			this.uow = unitOfWork;
			DiffCause = initialDiffCause;
			Title = "Заполнение причины расхождения";
		}

		
		#region Свойства View
		private string diffCause;
		public virtual string DiffCause {
			get => diffCause;
			set => SetField(ref diffCause, value);
		}
		#endregion
		
		#region Действия View
		public void FillDiffCause() {
			foreach(var item in selectedItems) {
				item.DiffCause = this.DiffCause;
				uow.Save(item);
			}
		}

		#endregion
		#region IWindowDialogSettings implementation
		public bool IsModal { get; } = true;
		public bool EnableMinimizeMaximize { get; } = false;
		public bool Resizable { get; } = true;
		public bool Deletable { get; } = true;
		public WindowGravity WindowPosition { get; } = WindowGravity.Center;
		#endregion
	}
}
