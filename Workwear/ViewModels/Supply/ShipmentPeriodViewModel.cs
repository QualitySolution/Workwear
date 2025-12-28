using System;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Supply;

namespace Workwear.ViewModels.Supply {
	public class ShipmentPeriodViewModel: UowDialogViewModelBase, IWindowDialogSettings {
		private ShipmentItem[] selectedItems;
		private IUnitOfWork uow;
		public ShipmentPeriodViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			ShipmentItem[] selectedItems,
			IUnitOfWork unitOfWork
		) :  base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider) {
			this.selectedItems = selectedItems ?? throw new ArgumentNullException(nameof(selectedItems));
			this.uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			var shipment = uow.GetById<Shipment>(selectedItems.Select(x => x.Shipment.Id)).First();
			StartPeriod = shipment.Items.Select(x => x.StartPeriod)
				.FirstOrDefault(x => x != null);
			EndPeriod = shipment.Items.Select(x => x.EndPeriod)
				.FirstOrDefault(x => x != null);
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

		#region Действия View

		public void FillPeriod() {
			foreach(var item in selectedItems) {
				item.StartPeriod = StartPeriod;
				item.EndPeriod = EndPeriod;
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
