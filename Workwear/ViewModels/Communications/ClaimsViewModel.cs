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
using workwear.Repository.Company;

namespace workwear.ViewModels.Communications 
{
	public class ClaimsViewModel : UowDialogViewModelBase 
	{
		
		private readonly ClaimsManager claimsManager;
		private readonly EmployeeRepository employeeRepository;
		private readonly uint sizePage = 100;

		public ClaimsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			ClaimsManager claimsManager,
			EmployeeRepository employeeRepository,
			IValidator validator = null, 
			string UoWTitle = "Обращения сотрудников"
			) : base(unitOfWorkFactory, navigation, validator, UoWTitle) 
		{
			Title = "Обращения сотрудников";
			this.claimsManager = claimsManager;
			this.employeeRepository = employeeRepository;
			messagesSelectClaims = new List<ClaimMessage>();
			Claims = new List<Claim>();
		}
		

		#region View
		
		private IList<Claim> claims;
		public IList<Claim> Claims {
			get => claims;
			set => SetField(ref claims, value);
		}

		private Claim selectClaim;
		[PropertyChangedAlso(nameof(SelectClaimState))]
		[PropertyChangedAlso(nameof(SensitiveChangeState))]
		[PropertyChangedAlso(nameof(SensitiveSend))]
		public Claim SelectClaim {
			get => selectClaim;
			set { SetField(ref selectClaim, value);
				RefreshMessage(); }
		}

		private IList<ClaimMessage> messagesSelectClaims;
		public IList<ClaimMessage> MessagesSelectClaims {
			get => messagesSelectClaims;
			set => SetField(ref messagesSelectClaims, value);
		}

		public ClaimState? SelectClaimState {
			get => SelectClaim?.ClaimState;
			set {
				if(SelectClaim != null && value != null)
					SelectClaim.ClaimState = value.Value;
			}
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

		public bool SensitiveSend => SelectClaim != null && !String.IsNullOrEmpty(TextMessage);
		public bool SensitiveChangeState => SelectClaim != null;

		#endregion

		#region ViewMethods

		public void RefreshClaims() {
			Claims = new List<Claim>();
			Claims = claimsManager.GetClaims(sizePage, 0, ShowClosed);
		}

		public void AddRangeClaims() {
			Claims.ToList().AddRange(claimsManager.GetClaims(sizePage, (uint)Claims.Count, ShowClosed));
		}

		public void Send(object sender, EventArgs eventArgs) {
			claimsManager.Send(SelectClaim.Id, TextMessage);
			RefreshMessage();
			TextMessage = String.Empty;
		}

		public void ChangeStatusClaim(object sender, EventArgs e) {
			claimsManager.SetСhanges(SelectClaim);
		}
		#endregion

		private void RefreshMessage() {
			MessagesSelectClaims = new List<ClaimMessage>();
			if(SelectClaim != null)
				MessagesSelectClaims = claimsManager.GetMessages(SelectClaim.Id);
		}
	}
}
