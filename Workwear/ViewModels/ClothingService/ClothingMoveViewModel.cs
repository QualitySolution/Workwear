using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gamma.Utilities;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.ViewModels.Control;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Stock;
using Workwear.Domain.Postomats;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using ClaimState = Workwear.Domain.ClothingService.ClaimState;

namespace Workwear.ViewModels.ClothingService {
	public class ClothingMoveViewModel: UowDialogViewModelBase, IWindowDialogSettings {
		private readonly IUserService userService;
		private readonly IInteractiveService interactiveService;
		private readonly BarcodeRepository barcodeRepository;
		private readonly BaseParameters baseParameters;
		private readonly NotificationManagerService notificationManager;
		public readonly FeaturesService FeaturesService;
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }
		
		public ClothingMoveViewModel(
			INavigationManager navigation,
			IInteractiveService interactiveService,
			IUnitOfWorkFactory unitOfWorkFactory,
			IUserService userService,
			BarcodeInfoViewModel barcodeInfoViewModel,
			BarcodeRepository barcodeRepository,
			BaseParameters baseParameters,
			FeaturesService featuresService,
			NotificationManagerService notificationManager,
			PostomatManagerService postomatService,
			UnitOfWorkProvider unitOfWorkProvider,
			ServiceClaim serviceClaim = null
		) : base(unitOfWorkFactory, navigation, unitOfWorkProvider: unitOfWorkProvider) 
		{
			Dictionary<string, (string title, Action action)> ActionBarcodes = new Dictionary<string, (string title, Action action)>();

			ActionBarcodes = ActionBarcodes.Concat(GetActionBarcodes())
				.ToDictionary(x => x.Key, x => x.Value);
			ActionBarcodes = ActionBarcodes.Concat(GetActionServicesBarcodes())
				.ToDictionary(x => x.Key, x => x.Value);
			
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			this.notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			barcodeInfoViewModel.ActionBarcodes = ActionBarcodes;
			if(postomatService == null) throw new ArgumentNullException(nameof(postomatService));
			this.FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			Title = "Перемещение спецодежды";
			
			_ = UoW; //дёргаем UoW, чтобы передать его через провайдер внутреннему виджету.
			if(serviceClaim != null) {
				claim = serviceClaim;
				BarcodeInfoViewModel.BarcodeText = serviceClaim.Barcode.Title;
				MoveDefiniteClaim = true;
			}
			else {
				BarcodeInfoViewModel.PropertyChanged += BarcodeInfoViewModelOnPropertyChanged;
			}
			
			if(this.FeaturesService.Available(WorkwearFeature.Postomats))
				Postomats = postomatService.GetPostomatList(PostomatListType.Aso);

			Services.ContentChanged += ServicesOnContentChanged;
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
				OnPropertyChanged(nameof(CanAddClaim));
			}
		}
		
		private void ServicesOnContentChanged(object sender, EventArgs e) {
			if(sender is SelectableEntity<Service> item) {
				if(item.Select && !Claim.ProvidedServices.Any(provided => DomainHelper.EqualDomainObjects(item.Entity, provided)))
					Claim.ProvidedServices.Add(item.Entity);
				else if(!item.Select && Claim.ProvidedServices.Any(provided => DomainHelper.EqualDomainObjects(item.Entity, provided)))
					Claim.ProvidedServices.Remove(item.Entity);
                UoW.Save(claim);
				UoW.Commit();
			}
		}

		#region Cвойства модели
		ServiceClaim claim;
		[PropertyChangedAlso(nameof(SensitiveAccept))]
		[PropertyChangedAlso(nameof(Operations))]
		[PropertyChangedAlso(nameof(SensitivePrint))]
		[PropertyChangedAlso(nameof(CanAddClaim))]
		[PropertyChangedAlso(nameof(NeedRepair))]
		[PropertyChangedAlso(nameof(DefectText))]
		[PropertyChangedAlso(nameof(Postomat))]
		public virtual ServiceClaim Claim {
			get => claim;
			set {
				if(SetField(ref claim, value) && claim != null) {
					services.Clear();
					foreach(var service in claim.Barcode.Nomenclature.UseServices) //Делаем список для заполнеия услуг во вьюшке
						services.Add(new SelectableEntity<Service>(service.Id, service.Name, entity:service)
							{Select = Claim.ProvidedServices.Any(provided => DomainHelper.EqualDomainObjects(service, provided))});
					OnPropertyChanged(nameof(Services));
					NeedRepair = claim.NeedForRepair;
					DefectText = claim.Defect;
				}
			}
		}

		private ClaimState state = ClaimState.InTransit;
		public virtual ClaimState State {
			get => state;
			set => SetField(ref state, value);
		}

		public virtual StateOperation LastStateOperation {
			get => Claim.States.OrderBy(o => o.OperationTime).Last(); 
		}
		
		private string comment;
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		private bool needRepair;
		public virtual bool NeedRepair {
			get => needRepair;
			set => SetField(ref needRepair, value);
		}
		
		private string defectText;
		public virtual string DefectText {
			get => defectText;
			set => SetField(ref defectText, value);		
		}
		public IList<PostomatInfo> Postomats { get; } = new List<PostomatInfo>();
		
		private PostomatInfo postomat;
		public virtual PostomatInfo Postomat {
			get => Postomats.FirstOrDefault(x => x.Id == (Claim?.PreferredTerminalId ?? postomat?.Id));
			set {
				if(SetField(ref postomat, value) && Claim != null) {
					Claim.PreferredTerminalId = value?.Id ?? 0;
					UoW.Save(Claim);
					UoW.Commit();
				}
			}
		}

		private IObservableList<SelectableEntity<Service>> services = new ObservableList<SelectableEntity<Service>>();
		public virtual IObservableList<SelectableEntity<Service>> Services {
			get => services;
			set => SetField(ref services, value);
		}
		
		public IObservableList<StateOperation> Operations => Claim?.States ?? new ObservableList<StateOperation>();

		public virtual bool ShowTerminal => FeaturesService.Available(WorkwearFeature.Postomats);
		public virtual bool CanAddClaim => BarcodeInfoViewModel.Barcode != null && Claim == null;
		public virtual bool SensitiveAccept => Claim != null;
		public virtual bool SensitivePrint => (Claim?.Barcode != null);
		public virtual bool MoveDefiniteClaim { get; } = false; //Движение единственного объекта
		public virtual bool SensitiveBarcode => !MoveDefiniteClaim;
	
		#endregion

		public string GetTerminalLabel(uint id) => Postomats.FirstOrDefault(p => p.Id == id)?.Location ?? "";

		#region Действия

		public bool SetState(ClaimState state) {
			if(Claim == null) {
				BarcodeInfoViewModel.LabelInfo = "Не принято на обслуживание или не найдена метка(штрихкод).";
				return false;
			}
			if(state == LastStateOperation.State) {
				BarcodeInfoViewModel.LabelInfo = "Статус прежний.";
				return false;
			}
			State = state;
			return true;
		}
		public void ChangeState(ClaimState state) {
			if(SetState(state)) 
				Accept();
		}
		
		public void ActiveInRepair() {
			if(Claim != null) {
				claim.NeedForRepair = NeedRepair = true;
				UoW.Save(claim);
				UoW.Commit();
			}  
        }

		public void SetService(Service service) {
			if(Claim == null) {
				BarcodeInfoViewModel.LabelInfo = "Не принято на обслуживание или не найдена метка(штрихкод).";
				return;
			}
			if(service == null) {
				BarcodeInfoViewModel.LabelInfo = "Услуга не найдена.";
				return;
			}
			var ser = Services.FirstOrDefault(s => s.Entity.Id == service.Id);
			if(ser != default)
				ser.Select = true;
			else
				BarcodeInfoViewModel.LabelInfo = $"Услуги \"{service.Name}\" нет в этой номенклатуре.";
		}

		public void CreateNew() {
			if(BarcodeInfoViewModel.Barcode == null) {
				BarcodeInfoViewModel.LabelInfo = "Штрихкод не найден.";
				return;
			}
			if(Claim != null) {
				BarcodeInfoViewModel.LabelInfo = "Уже принято на обслуживание.";
				return;
			}

			Claim = new ServiceClaim {
				Barcode = BarcodeInfoViewModel.Barcode,
				Employee = BarcodeInfoViewModel?.Employee,
				IsClosed = false,
				PreferredTerminalId = Postomat?.Id
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

		public void Accept() {
			var pDocItems = UoW.Session.QueryOver<PostomatDocumentItem>()
				.Where(x => x.ServiceClaim.Id == claim.Id)
				.Where(x => x.DispenseTime == null)
				.List();
			if(pDocItems.Count() > 0 
			   && interactiveService.Question("СИЗ уже добавлен в документ постомата." +
			                                  " Если изменить статус, вещь будет считаться выданной, " +
			                                  "а соответствующая ячейка постомата пустой. \n Продолжить?")) 
				foreach(var item in pDocItems) {
					item.DispenseTime = DateTime.Now;
					UoW.Save(item);
				}

			StateOperation newStatus = null;
			if(State != LastStateOperation.State) {
				newStatus = new StateOperation {
					OperationTime = DateTime.Now,
					State = State,
					Claim = Claim,
					User = userService.GetCurrentUser(),
					Comment = Comment
				};
				Claim.States.Add(newStatus);
				if(State == ClaimState.Returned)
					Claim.IsClosed = true;
			}
			else if(LastStateOperation.Comment != Comment)
				LastStateOperation.Comment = Comment;

			claim.NeedForRepair = NeedRepair;
			if(NeedRepair && claim.Defect != DefectText)
				claim.Defect = DefectText;
			
			if(MoveDefiniteClaim) { //Сохранеине и коммит в вызвавающем объекте
				Close(false, CloseSource.Self);
				return;
			}
			
			UoW.Save(Claim);
			UoW.Commit();
			if(newStatus != null)
				SendPush(newStatus);
			Comment = String.Empty;
		}
		public void PrintLabel() {
			if(claim == null)
				claim = this.Claim;
			if(claim == null){
				BarcodeInfoViewModel.LabelInfo = "Не принято на обслуживание";
				return;
			}

			ReportInfo reportInfo; 
			switch(claim.Barcode.Type) {
				case BarcodeTypes.EAN13:
					reportInfo = new ReportInfo {
						Title = "Этикетка",
						Identifier = "ClothingService.ClothingMoveStickerBarcode",
						Parameters = new Dictionary<string, object> {
							{ "barcode_id", claim.Barcode.Id },
							{ "service_claim_id", claim.Id },
							{ "manufacturer_code", claim.Barcode.Title.Substring(2, 5) },
							{ "number_system", claim.Barcode.Title.Substring(0, 2) },
							{ "product_code", claim.Barcode.Title.Substring(7, 5) },
							{ "show_terminal", ShowTerminal },
							{ "preferred_terminal", claim.PreferredTerminalId.HasValue
									? GetTerminalLabel(claim.PreferredTerminalId.Value)
									: string.Empty}
						}
					};  break;
				case BarcodeTypes.EPC96 :
					reportInfo = new ReportInfo {
						Title = "Этикетка",
						Identifier = "ClothingService.ClothingMoveSticker",
						Parameters = new Dictionary<string, object> {
							{ "barcode_id", claim.Barcode.Id },
							{ "service_claim_id", claim.Id }
						}
					};  break;
				default: throw new NotImplementedException();
					
			}
			
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
		
		private Dictionary<string, (string, Action)> GetActionBarcodes() {
			return new Dictionary<string, (string, Action)>() {
				["2000000000008"] = ("Изменить статус", () => Accept()),
				["2000000000015"] = ($"Статус \"{ClaimState.InTransit.GetEnumTitle()}\"", () => SetState(ClaimState.InTransit)),
				["2000000000022"] = ($"Статус \"{ClaimState.DeliveryToLaundry.GetEnumTitle()}\"", () => SetState(ClaimState.DeliveryToLaundry)),
				["2000000000039"] = ($"Статус \"{ClaimState.InRepair.GetEnumTitle()}\"", () => SetState(ClaimState.InRepair)),
				["2000000000046"] = ($"Статус \"{ClaimState.InDryCleaning.GetEnumTitle()}\"", () => SetState(ClaimState.InDryCleaning)),
				["2000000000053"] = ($"Статус \"{ClaimState.InWashing.GetEnumTitle()}\"", () => SetState(ClaimState.InWashing)),
				["2000000000060"] = ($"Статус \"{ClaimState.AwaitIssue.GetEnumTitle()}\"", () => SetState(ClaimState.AwaitIssue)),
				["2000000000077"] = ($"Статус \"{ClaimState.Returned.GetEnumTitle()}\"", () => SetState(ClaimState.Returned)),
				["2000000000084"] = ("Принять на обслуживание", () => CreateNew()),
				["2000000000091"] = ("Печать этикетки", () => PrintLabel()),
				["2000000000107"] = ("Необходим ремнот", () => ActiveInRepair()),
				["2000000000114"] = ($"Сменить статус на \"{ClaimState.InTransit.GetEnumTitle()}\"", () => ChangeState(ClaimState.InTransit)),
				["2000000000121"] = ($"Сменить статус на \"{ClaimState.DeliveryToLaundry.GetEnumTitle()}\"", () => ChangeState(ClaimState.DeliveryToLaundry)),
				["2000000000138"] = ($"Сменить статус на \"{ClaimState.InRepair.GetEnumTitle()}\"", () => ChangeState(ClaimState.InRepair)),
				["2000000000145"] = ($"Сменить статус на \"{ClaimState.InDryCleaning.GetEnumTitle()}\"", () => ChangeState(ClaimState.InDryCleaning)),
				["2000000000152"] = ($"Сменить статус на \"{ClaimState.InWashing.GetEnumTitle()}\"", () => ChangeState(ClaimState.InWashing)),
				["2000000000169"] = ($"Сменить статус на \"{ClaimState.AwaitIssue.GetEnumTitle()}\"", () => ChangeState(ClaimState.AwaitIssue)),
				["2000000000176"] = ($"Сменить статус на \"{ClaimState.Returned.GetEnumTitle()}\"", () => ChangeState(ClaimState.Returned)),
				//["2000000000183"] = ("", null),
				//["2000000000190"] = ("", null),
				//["2000000000206"] = ("", null), используется ClothingAddViewModel
				//["2000000000213"] = ("", null), используется ClothingAddViewModel
				//["2000000000220"] = ("", null),
				//["2000000000237"] = ("", null),
				//["2000000000244"] = ("", null),
				//["2000000000251"] = ("", null),
				//["2000000000268"] = ("", null),
				//["2000000000275"] = ("", null),
				//["2000000000282"] = ("", null),
			};
		}
		private Dictionary<string, (string, Action)> GetActionServicesBarcodes() {
			var services = UoW.GetAll<Service>()
				.Where(s => s.Code != null)
				.ToList();

			Dictionary<string, (string, Action)> dictionary = new Dictionary<string, (string, Action)>();
			foreach(var s in services) {
				if(!dictionary.ContainsKey(s.Code))
					dictionary.Add(s.Code, ($"Добавить услугу \"{s.Name}\"", () => SetService(s)));
			}
			return dictionary;
		}

		public void SendPush(StateOperation status) {
			var claimState = status.State;
			var nomenclature = status.Claim.Barcode.Nomenclature.Name;
			var phone = status.Claim.Employee.PhoneNumber;
			if(status.Claim.Employee.LkRegistered && (claimState == ClaimState.InDryCleaning || claimState == ClaimState.InRepair
			   || claimState == ClaimState.InWashing))
				notificationManager.SendMessages(new[] { MakeNotificationMessage(claimState, nomenclature, phone) });
		}

		#endregion

		#region IWindowDialogSettings implementation
		public bool IsModal { get; } = true;
		public bool EnableMinimizeMaximize { get; } = false;
		public bool Resizable { get; } = true;
		public bool Deletable { get; } = true;
		public WindowGravity WindowPosition { get; } = WindowGravity.Center;
		#endregion
		
		private OutgoingMessage MakeNotificationMessage(ClaimState claimState, string nomenclatureName, string phone)
		{
			string text = String.Empty;
			switch (claimState)
			{
				case ClaimState.InDryCleaning:
					text = $"Ваша спецодежда {nomenclatureName} перемещена в химчистку, срок обслуживания увеличится на три рабочих дня.";
					break;
				case ClaimState.InRepair:
					text = $"Ваша спецодежда {nomenclatureName} перемещена в ремонт, срок обслуживания увеличится на два рабочих дня.";
					break;
				case ClaimState.InWashing:
					text = $"Ваша спецодежда {nomenclatureName} принята на обслуживание, срок составит 5 рабочих дней.";
					break;
			}
			OutgoingMessage message = new OutgoingMessage {
				Phone = phone,
				Title = "Изменение статуса обслуживания одежды",
				Text = text
			};
			return message;
		}
	}
}
