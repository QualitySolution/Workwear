using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Users;
using Workwear.Models.Import;
using Workwear.Repository.Company;
using Workwear.Tools.User;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Regulations;

namespace Workwear.ViewModels.Communications 
{
	public class ClaimsViewModel : UowDialogViewModelBase 
	{
		
		private readonly ClaimsManagerService claimsManager;
		private readonly EmployeeRepository employeeRepository;
		private readonly CurrentUserSettings currentUserSettings;
		private readonly uint sizePage = 300;
		private Dictionary<string, FIO> employeeNames;

		public ClaimsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			ClaimsManagerService claimsManager,
			EmployeeRepository employeeRepository,
			CurrentUserSettings currentUserSettings
			) : base(unitOfWorkFactory, navigation, UoWTitle: "Обращения сотрудников") 
		{
			Title = "Обращения сотрудников";
			this.claimsManager = claimsManager;
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.currentUserSettings = currentUserSettings ?? throw new ArgumentNullException(nameof(currentUserSettings));
			employeeRepository.RepoUow = UoW;
			messagesSelectClaims = new List<ClaimMessage>();
			Claims = new List<Claim>();
		}
		

		#region View
		
		private List<Claim> claims;
		public List<Claim> Claims {
			get => claims;
			set => SetField(ref claims, value);
		}

		private Claim selectClaim;
		[PropertyChangedAlso(nameof(SensitiveCloseClaim))]
		[PropertyChangedAlso(nameof(SensitiveSend))]
		[PropertyChangedAlso(nameof(ClaimTitle))]
		[PropertyChangedAlso(nameof(VisibleProtectionTools))]
		public Claim SelectClaim {
			get => selectClaim;
			set {
				if(SetField(ref selectClaim, value)) {
					RefreshMessage();
					if(SelectClaim != null)
						Employee = employeeRepository.GetEmployeeByPhone(SelectClaim.UserPhone);
					else
						Employee = null;
					
					if(SelectClaim != null && SelectClaim.ProtectionToolsId > 0)
						ProtectionTools = UoW.GetById<ProtectionTools>((int)SelectClaim.ProtectionToolsId);
					else
						ProtectionTools = null;
				}
			}
		}

		private IList<ClaimMessage> messagesSelectClaims;
		public IList<ClaimMessage> MessagesSelectClaims {
			get => messagesSelectClaims;
			set => SetField(ref messagesSelectClaims, value);
		}
		
		public ClaimListType ListType {
			get => currentUserSettings.Settings.DefaultClaimListType;
			set {
				currentUserSettings.Settings.DefaultClaimListType = value;
				RefreshClaims();
			}
		}
		
		protected ShowClaim ShowType {
			get {
				switch(ListType) {
					case ClaimListType.NotAnswered:
						return ShowClaim.NotAnswered;
					case ClaimListType.NotClosed:
						return ShowClaim.NotClosed;
					case ClaimListType.All:
						return ShowClaim.All;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private string textMessage;
		[PropertyChangedAlso(nameof(SensitiveSend))]
		public string TextMessage {
			get => textMessage;
			set => SetField(ref textMessage, value);
		}
		
		private EmployeeCard employee;
		[PropertyChangedAlso(nameof(SensitiveOpenEmployee))]
		[PropertyChangedAlso(nameof(EmployeeName))]
		public EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
		}
		
		private ProtectionTools protectionTools;
		[PropertyChangedAlso(nameof(SensitiveOpenProtectionTools))]
		[PropertyChangedAlso(nameof(ProtectionToolsTitle))]
		public ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}
		
		public string ClaimTitle => SelectClaim?.Title;
		public string EmployeeName => Employee?.FullName ?? $"Неизвестный сотрудник {SelectClaim?.UserPhone}";
		public string ProtectionToolsTitle => ProtectionTools?.Name ?? "Неизвестная номенклатура нормы";

		public bool VisibleProtectionTools => (SelectClaim?.ProtectionToolsId ?? 0) > 0;
		public bool SensitiveOpenEmployee => Employee != null;
		public bool SensitiveOpenProtectionTools => ProtectionTools != null;
		public bool SensitiveSend => SelectClaim != null && !String.IsNullOrWhiteSpace(TextMessage);
		public bool SensitiveCloseClaim => SelectClaim != null && SelectClaim.ClaimState != ClaimState.Closed;

		#endregion

		#region ViewMethods

		public string GetEmployeeName(string phone) {
			if(employeeNames.TryGetValue(phone, out FIO fio))
				return fio.ShortName;
			else
				return phone;
		}

		public void RefreshClaims() {
			Claims = claimsManager.GetClaims(sizePage, 0, ShowType).ToList();
			employeeNames = employeeRepository.GetFioByPhones(claims.Select(x => x.UserPhone).Where(x => !String.IsNullOrEmpty(x)).Distinct().ToArray());
		}

		public void SendAnswer() {
			claimsManager.Send(SelectClaim.Id, TextMessage);
			RefreshMessage();
			TextMessage = String.Empty;
			RefreshClaims();
		}

		public void CloseClaim() {
			claimsManager.CloseClaim(SelectClaim.Id, TextMessage);
			RefreshMessage();
			TextMessage = String.Empty;
			RefreshClaims();
		}

		private void RefreshMessage() {
			if(SelectClaim != null)
				MessagesSelectClaims = claimsManager.GetMessages(SelectClaim.Id);
		}

		public bool UploadClaims() {
			var newClaims = claimsManager.GetClaims(sizePage, (uint)Claims.Count, ShowType).ToList();
			if (newClaims.Count == 0)
				return false;
			Claims = Claims.Union(newClaims).ToList();
			employeeNames = employeeRepository.GetFioByPhones(claims.Select(x => x.UserPhone).Where(x => !String.IsNullOrEmpty(x)).Distinct().ToArray());
			return true;
		}
		
		public void OpenEmployee() {
			NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(Employee.Id));
		}
		
		public void OpenProtectionTools() {
			NavigationManager.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(ProtectionTools.Id));
		}
		#endregion
	}
}
