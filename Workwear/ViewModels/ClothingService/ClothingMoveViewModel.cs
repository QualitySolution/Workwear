using System;
using System.Collections.Generic;
using System.ComponentModel;
using Gamma.Utilities;
using System.Linq;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
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
using Workwear.Tools.Features;
using ClaimState = Workwear.Domain.ClothingService.ClaimState;

namespace Workwear.ViewModels.ClothingService {
	public class ClothingMoveViewModel: UowDialogViewModelBase, IWindowDialogSettings {
		private readonly IUserService userService;
		private readonly BarcodeRepository barcodeRepository;
		private readonly Dictionary<string, (string title, Action<object> action)> ActionBarcodes;
		readonly IDictionary<uint, string> postomatsLabels = new Dictionary<uint, string>();
		public readonly FeaturesService FeaturesService;
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }
		
		public ClothingMoveViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			BarcodeInfoViewModel barcodeInfoViewModel,
			IUserService userService,
			BarcodeRepository barcodeRepository,
			PostomatManagerService postomatService,
			FeaturesService featuresService,
			ServiceClaim serviceClaim = null
		) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider) 
		{
			ActionBarcodes = SetActionBarcodes();
			
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			barcodeInfoViewModel.ActionBarcodes = ActionBarcodes;
			if(postomatService == null) throw new ArgumentNullException(nameof(postomatService));
			this.FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			Title = "Перемещение спецодежды";
			//Создаем UoW, чтобы передать его через провайдер внутреннему виджету.
			var uow = UoW;
			if(serviceClaim != null) {
				claim = serviceClaim;
				BarcodeInfoViewModel.BarcodeText = serviceClaim.Barcode.Title;
				MoveDefiniteClaim = true;
			} else
				BarcodeInfoViewModel.PropertyChanged += BarcodeInfoViewModelOnPropertyChanged;
			if(featuresService.Available(WorkwearFeature.Postomats))
				postomatsLabels = postomatService.GetPostomatList(PostomatListType.Aso).ToDictionary(x => x.Id, x => $"{x.Name} ({x.Location})");
		}
		
		
		private void BarcodeInfoViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(BarcodeInfoViewModel.Barcode) == e.PropertyName) {
				if(BarcodeInfoViewModel.Barcode != null && BarcodeInfoViewModel.ActivAction != null) {
					BarcodeInfoViewModel.ActivAction(Claim);
					BarcodeInfoViewModel.ActivAction = null;
					return;
				}
				
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
		
		public string GetTerminalLabel(uint id) => postomatsLabels.ContainsKey(id) ? postomatsLabels[id] : string.Empty;
		
		#region Действия

		public void SetState(ClaimState state) {
			if(Claim != null) {
				State = state;
			}
		}
		public void ChngeState(ClaimState state) {
			if(Claim != null) {
				State = state;
				Accept(false);
			}
		}

		public void CreateNew() {
			if(BarcodeInfoViewModel.Barcode != null && Claim == null) {
				Claim = new ServiceClaim {
					Barcode = BarcodeInfoViewModel.Barcode,
					Employee = BarcodeInfoViewModel?.Employee,
					IsClosed = false
				};
				UoW.Save(Claim);
				
				var status = new StateOperation {
					Claim = Claim,
					TerminalId = Claim.PreferredTerminalId,
					OperationTime = DateTime.Now,
					State = ClaimState.WaitService,
					User = userService.GetCurrentUser()
				};
				UoW.Save(status);
				claim.States.Add(status);
				UoW.Save(claim);
				UoW.Commit();
				
				BarcodeInfoViewModel.LabelInfo = String.Empty;
				BarcodeInfoViewModel.BarcodeText = String.Empty;
			}
		}

		public void Accept(bool clear = true) {
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

			if(clear) {
				BarcodeInfoViewModel.BarcodeText = String.Empty;
				BarcodeInfoViewModel.Barcode = null;
				BarcodeInfoViewModel.LabelInfo = null;
				BarcodeInfoViewModel.Employee = null;
			}
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
					{"product_code", claim.Barcode.Title.Substring(7,5)},
					{"preferred_terminal",  claim.PreferredTerminalId.HasValue
						? GetTerminalLabel(claim.PreferredTerminalId.Value)
						: string.Empty}
				}
			};
			
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
		
		private Dictionary<string, (string, Action<object>)> SetActionBarcodes() {
			return new Dictionary<string, (string, Action<object>)>() {
				["2000000000008"] = ("Изменить статус", (s) => Accept()),
				["2000000000015"] = ($"Статус \"{ClaimState.InTransit.GetEnumTitle()}\"", (s) => SetState(ClaimState.InTransit)),
				["2000000000022"] = ($"Статус \"{ClaimState.DeliveryToLaundry.GetEnumTitle()}\"", (s) => SetState(ClaimState.DeliveryToLaundry)),
				["2000000000039"] = ($"Статус \"{ClaimState.InRepair.GetEnumTitle()}\"", (s) => SetState(ClaimState.InRepair)),
				["2000000000046"] = ($"Статус \"{ClaimState.InDryCleaning.GetEnumTitle()}\"", (s) => SetState(ClaimState.InDryCleaning)),
				["2000000000053"] = ($"Статус \"{ClaimState.InWashing.GetEnumTitle()}\"", (s) => SetState(ClaimState.InWashing)),
				["2000000000060"] = ($"Статус \"{ClaimState.AwaitIssue.GetEnumTitle()}\"", (s) => SetState(ClaimState.AwaitIssue)),
				["2000000000077"] = ($"Статус \"{ClaimState.Returned.GetEnumTitle()}\"", (s) => SetState(ClaimState.Returned)),
				["2000000000084"] = ("Принять на обслуживание", (s) => CreateNew()),
				//["2000000000091"] = ("", null),
				//["2000000000107"] = ("", null),
				["2000000000114"] = ($"Сменить статус на \"{ClaimState.InTransit.GetEnumTitle()}\"", (s) => ChngeState(ClaimState.InTransit)),
				["2000000000121"] = ($"Сменить статус на \"{ClaimState.DeliveryToLaundry.GetEnumTitle()}\"", (s) => ChngeState(ClaimState.DeliveryToLaundry)),
				["2000000000138"] = ($"Сменить статус на \"{ClaimState.InRepair.GetEnumTitle()}\"", (s) => ChngeState(ClaimState.InRepair)),
				["2000000000145"] = ($"Сменить статус на \"{ClaimState.InDryCleaning.GetEnumTitle()}\"", (s) => ChngeState(ClaimState.InDryCleaning)),
				["2000000000152"] = ($"Сменить статус на \"{ClaimState.InWashing.GetEnumTitle()}\"", (s) => ChngeState(ClaimState.InWashing)),
				["2000000000169"] = ($"Сменить статус на \"{ClaimState.AwaitIssue.GetEnumTitle()}\"", (s) => ChngeState(ClaimState.AwaitIssue)),
				["2000000000176"] = ($"Сменить статус на \"{ClaimState.Returned.GetEnumTitle()}\"", (s) => ChngeState(ClaimState.Returned)),
				//["2000000000183"] = ("", null),
				//["2000000000190"] = ("", null),
				//["2000000000206"] = ("", null),
				//["2000000000213"] = ("", null),
				//["2000000000220"] = ("", null),
				//["2000000000237"] = ("", null),
				//["2000000000244"] = ("", null),
				//["2000000000251"] = ("", null),
				//["2000000000268"] = ("", null),
				//["2000000000275"] = ("", null),
				//["2000000000282"] = ("", null),
			};
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
