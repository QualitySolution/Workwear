using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Regulations;
using Workwear.Models.Import;
using Workwear.Repository.Company;

namespace Workwear.ViewModels.Communications 
{
	public class ClaimsViewModel : UowDialogViewModelBase 
	{
		
		private readonly ClaimsManagerService claimsManager;
		private readonly EmployeeRepository employeeRepository;
		private readonly uint sizePage = 300;
		private Dictionary<string, FIO> employeeNames;

		public ClaimsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			ClaimsManagerService claimsManager,
			EmployeeRepository employeeRepository,
			IValidator validator = null, 
			string UoWTitle = "Обращения сотрудников"
			) : base(unitOfWorkFactory, navigation, validator, UoWTitle) 
		{
			Title = "Обращения сотрудников";
			this.claimsManager = claimsManager;
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
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

		private bool showClosed;
		public bool ShowClosed {
			get => showClosed;
			set {
				SetField(ref showClosed, value);
				RefreshClaims();
			}
		}

		private string textMessage;
		[PropertyChangedAlso(nameof(SensitiveSend))]
		public string TextMessage {
			get => textMessage;
			set => SetField(ref textMessage, value);
		}
		
		private ProtectionTools protectionTools;
		[PropertyChangedAlso(nameof(SensitiveOpenProtectionTools))]
		[PropertyChangedAlso(nameof(ProtectionToolsTitle))]
		public ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}
		
		public string ClaimTitle => SelectClaim?.Title;
		public string ProtectionToolsTitle => ProtectionTools?.Name ?? "Неизвестная номенклатура нормы";

		public bool VisibleProtectionTools => (SelectClaim?.ProtectionToolsId ?? 0) > 0;
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
			Claims = claimsManager.GetClaims(sizePage, 0, ShowClosed).ToList();
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
		#endregion
		
		private void RefreshMessage() {
			if(SelectClaim != null)
				MessagesSelectClaims = claimsManager.GetMessages(SelectClaim.Id);
		}

		public bool UploadClaims() {
			var newClaims = claimsManager.GetClaims(sizePage, (uint)Claims.Count, ShowClosed).ToList();
			Claims.AddRange(newClaims);
			employeeNames = employeeRepository.GetFioByPhones(claims.Select(x => x.UserPhone).Where(x => !String.IsNullOrEmpty(x)).Distinct().ToArray());
			OnPropertyChanged(nameof(Claims));
			return newClaims.Count > 0;
		}
	}
}
