using System;
using System.ComponentModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.ClothingService;
using Workwear.Repository.Stock;

namespace Workwear.ViewModels.ClothingService {
	public class ClothingReceiptViewModel : UowDialogViewModelBase, IWindowDialogSettings {
		private readonly IUserService userService;
		private readonly BarcodeRepository barcodeRepository;
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }

		public ClothingReceiptViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			BarcodeInfoViewModel barcodeInfoViewModel,
			IUserService userService,
			BarcodeRepository barcodeRepository
				) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider)
		{
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			Title = "Приём в стирку";
			//Создаем UoW чтобы передать его через провайдер внутреннему виджету.
		 	var uow = UoW;
		    BarcodeInfoViewModel.PropertyChanged += BarcodeInfoViewModelOnPropertyChanged;
		}

		private void BarcodeInfoViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(BarcodeInfoViewModel.Barcode) == e.PropertyName) {
				if(BarcodeInfoViewModel.Barcode == null) {
					SensitiveAccept = false;
					return;
				}
				var activeClaim = barcodeRepository.GetActiveServiceClaimFor(BarcodeInfoViewModel.Barcode);
				SensitiveAccept = activeClaim == null;
				if(activeClaim != null)
					BarcodeInfoViewModel.LabelInfo = $"Спецодежда уже в работе по заявке №{activeClaim.Id}";
			}
		}

		#region Свойства View
		private bool needRepair;
		[PropertyChangedAlso(nameof(SensitiveDefect))]
		public virtual bool NeedRepair {
			get => needRepair;
			set => SetField(ref needRepair, value);
		}
		
		private string defect;
		public virtual string Defect {
			get => defect;
			set => SetField(ref defect, value);
		}

		private bool sensitiveAccept;
		public virtual bool SensitiveAccept {
			get => sensitiveAccept;
			set => SetField(ref sensitiveAccept, value);
		}
		
		public bool SensitiveDefect => NeedRepair;
		#endregion
		
		#region Действия View

		public void Accept() {
			var claim = new ServiceClaim {
				Barcode = BarcodeInfoViewModel.Barcode,
				NeedForRepair = NeedRepair,
				Defect = Defect,
				IsClosed = false
			};
			
			var status = new StateOperation {
				OperationTime = DateTime.Now,
				State = ClaimState.WaitService,
				Claim = claim,
				User = userService.GetCurrentUser()
			};
			claim.States.Add(status);
			UoW.Save(claim);
			UoW.Commit();
			
			BarcodeInfoViewModel.BarcodeText = String.Empty;
			BarcodeInfoViewModel.Barcode = null;
			BarcodeInfoViewModel.LabelInfo = null;
			BarcodeInfoViewModel.Employee = null;
			NeedRepair = false;
			Defect = null;
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
