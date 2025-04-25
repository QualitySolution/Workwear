using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using Workwear.Domain.Supply;
using workwear.Journal.ViewModels.Supply;

namespace Workwear.Journal.Filter.ViewModels.Supply {
	public class ShipmentJournalFilterViewModel : JournalFilterViewModelBase<ShipmentJournalFilterViewModel> {
		private readonly ShipmentJournalViewModel journalViewModel;
		public ShipmentJournalFilterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			JournalViewModelBase journal,
			Action<ShipmentJournalFilterViewModel> setFilterParameters = null
		) : base(journal, unitOfWorkFactory) {
			
			journalViewModel = (ShipmentJournalViewModel)journal;
			CanNotify = false;
			setFilterParameters?.Invoke(this);
			CanNotify = true;
		}

		#region Ограничения

		private ShipmentStatus? status;
		public ShipmentStatus? Status {
			get => status;
			set => SetField(ref status, value);
		}
		
		private bool notFullOrdered;
		public bool NotFullOrdered {
			get => notFullOrdered;
			set => SetField(ref notFullOrdered, value);
		}

		public string ColorsLegendText => journalViewModel.ColorsLegendText;

		#endregion
	}
}
