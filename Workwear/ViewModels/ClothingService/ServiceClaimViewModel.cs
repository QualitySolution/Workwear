using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.ClothingService;

namespace Workwear.ViewModels.ClothingService {
	public class ServiceClaimViewModel : EntityDialogViewModelBase<ServiceClaim> {
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }

		public ServiceClaimViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			BarcodeInfoViewModel barcodeInfoViewModel,
			INavigationManager navigation,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			BarcodeInfoViewModel.Barcode = Entity.Barcode;
		}

		#region Sensitive
		public bool CanEdit => !Entity.IsClosed;
		#endregion
	}
}
