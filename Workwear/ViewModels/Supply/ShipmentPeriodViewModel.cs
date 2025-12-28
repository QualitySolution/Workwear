using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace Workwear.ViewModels.Supply {
	public class ShipmentPeriodViewModel: UowDialogViewModelBase, IWindowDialogSettings {
		
		public ShipmentPeriodViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation
		) :  base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider) {
			Title = "Заполнение периода";
		}

		#region Свойства View

		private DateTime? startPeriod;
		public DateTime? StartPeriod {
			get => startPeriod;
			set => SetField(ref startPeriod, value);
		}
		private DateTime? endPeriod;
		public DateTime? EndPeriod {
			get => endPeriod;
			set => SetField(ref endPeriod, value);
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
