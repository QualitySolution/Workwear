using System;
using System.Collections.Generic;
using System.ComponentModel;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.ClothingService;
using Workwear.Repository.Stock;
using ClaimState = Workwear.Domain.ClothingService.ClaimState;

namespace Workwear.ViewModels.ClothingService {
	public class ClothingMoveViewModel: UowDialogViewModelBase, IWindowDialogSettings {
		private readonly IUserService userService;
		private readonly BarcodeRepository barcodeRepository;
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }
		
		public ClothingMoveViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			BarcodeInfoViewModel barcodeInfoViewModel,
			IUserService userService,
			BarcodeRepository barcodeRepository,
			ServiceClaim serviceClaim = null
		) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider)
		{
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			Title = "Перемещение спецодежды";
			//Создаем UoW, чтобы передать его через провайдер внутреннему виджету.
			var uow = UoW;
			if(serviceClaim != null) {
				claim = serviceClaim;
				BarcodeInfoViewModel.BarcodeText = serviceClaim.Barcode.Title;
				MoveDefiniteClaim = true;
			} else
				BarcodeInfoViewModel.PropertyChanged += BarcodeInfoViewModelOnPropertyChanged;
		}
		
		
		private void BarcodeInfoViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(BarcodeInfoViewModel.Barcode) == e.PropertyName) {
				if(BarcodeInfoViewModel.Barcode == null) {
					Claim = null;
					return;
				}
				Claim = barcodeRepository.GetActiveServiceClaimFor(BarcodeInfoViewModel.Barcode);
				if(Claim == null)
					BarcodeInfoViewModel.LabelInfo = $"Спецодежда не была принята в стирку.";
			}
		}

		#region Cвойства модели
		ServiceClaim claim;
		[PropertyChangedAlso(nameof(SensitiveAccept))]
		[PropertyChangedAlso(nameof(Operations))]
		[PropertyChangedAlso(nameof(SensitivePrint))]
		public virtual ServiceClaim Claim {
			get => claim;
			set => SetField(ref claim, value);
		}
		#endregion
		
		#region Свойства View
		private ClaimState state = ClaimState.InTransit;
		public virtual ClaimState State {
			get => state;
			set => SetField(ref state, value);
		}
		
		private string comment;
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		public IObservableList<StateOperation> Operations => Claim?.States ?? new ObservableList<StateOperation>();
		
		public virtual bool SensitiveAccept => Claim != null;
		public virtual bool SensitivePrint => (Claim?.Barcode != null);
		public virtual bool MoveDefiniteClaim { get; } = false; //Движение единственного объекта
		public virtual bool SensitiveBarcode => !MoveDefiniteClaim;
	
		#endregion
		
		#region Действия View

		public void Accept() {
			var status = new StateOperation {
				OperationTime = DateTime.Now,
				State = State,
				Claim = Claim,
				User = userService.GetCurrentUser(),
				Comment = Comment
			};
			Claim.States.Add(status);
			if(State == ClaimState.Returned)
				Claim.IsClosed = true;
			
			if(MoveDefiniteClaim) {
				Close(false, CloseSource.Self);
				return;
			}

			UoW.Save(Claim);
			UoW.Commit();
			
			BarcodeInfoViewModel.BarcodeText = String.Empty;
			BarcodeInfoViewModel.Barcode = null;
			BarcodeInfoViewModel.LabelInfo = null;
			BarcodeInfoViewModel.Employee = null;
		}
		public void PrintLabel(ServiceClaim claim) {
			var reportInfo = new ReportInfo {
				Title = "Этикетка",
				Identifier = "ClothingService.ClothingMoveSticker",
				Parameters = new Dictionary<string, object> {
					{"barcode_id", claim.Barcode.Id},
					{"service_claim_id", claim.Id},
					{"manufacturer_code", claim.Barcode.Title.Substring(2,5)},
					{"number_system", claim.Barcode.Title.Substring(0,2)},
					{"product_code", claim.Barcode.Title.Substring(7,5)}
				}
			};
			
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
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
