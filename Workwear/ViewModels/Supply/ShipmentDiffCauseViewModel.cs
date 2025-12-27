using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace Workwear.ViewModels.Supply {
	public class ShipmentDiffCauseViewModel: UowDialogViewModelBase, IWindowDialogSettings {

		public ShipmentDiffCauseViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation
		) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider) {
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
