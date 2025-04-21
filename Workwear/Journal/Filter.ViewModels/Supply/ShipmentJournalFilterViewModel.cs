using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using Workwear.Domain.Supply;

namespace Workwear.Journal.Filter.ViewModels.Supply {
	public class ShipmentJournalFilterViewModel : JournalFilterViewModelBase<ShipmentJournalFilterViewModel> {
		public ShipmentJournalFilterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			JournalViewModelBase journal,
			Action<ShipmentJournalFilterViewModel> setFilterParameters = null
		) : base(journal, unitOfWorkFactory) {

			CanNotify = false;
			setFilterParameters?.Invoke(this);
			CanNotify = true;
		}

		#region Ограничения

		private ShipmentStatus status;
		public ShipmentStatus Status {
			get => status;
			set => SetField(ref status, value);
		}
		
		private bool notFullReceived;
		public bool NotFullReceived {
			get => notFullReceived;
			set => SetField(ref notFullReceived, value);
		}
		
		#endregion
	}
}
