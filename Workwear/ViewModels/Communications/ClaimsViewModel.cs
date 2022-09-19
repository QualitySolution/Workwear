using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace Workwear.ViewModels.Communications 
{
	public class ClaimsViewModel : UowDialogViewModelBase 
	{
		
		private readonly ClaimsManagerService claimsManager;
		private readonly uint sizePage = 300;

		public ClaimsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			ClaimsManagerService claimsManager,
			IValidator validator = null, 
			string UoWTitle = "Обращения сотрудников"
			) : base(unitOfWorkFactory, navigation, validator, UoWTitle) 
		{
			Title = "Обращения сотрудников";
			this.claimsManager = claimsManager;
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

		public TranslateClaimState? SelectClaimState {
			get {
				if(SelectClaim != null)
					return (TranslateClaimState?)SelectClaim.ClaimState;
				return null;
			}
			set {
				if(SelectClaim != null && value != null) {
					SelectClaim.ClaimState = (ClaimState)value.Value;
					ChangeStatusClaim();
				}
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
			Claims = claimsManager.GetClaims(sizePage, 0, ShowClosed).ToList();
		}

		public void Send(object sender, EventArgs eventArgs) {
			claimsManager.Send(SelectClaim.Id, TextMessage);
			RefreshMessage();
			TextMessage = String.Empty;
		}

		private void ChangeStatusClaim() {
			claimsManager.SetChanges(SelectClaim);
		}
		
		#endregion

		private void RefreshMessage() {
			if(SelectClaim != null)
				MessagesSelectClaims = claimsManager.GetMessages(SelectClaim.Id);
		}

		public bool UploadClaims() {
			var newClaims = claimsManager.GetClaims(sizePage, (uint)Claims.Count, ShowClosed).ToList();
			Claims.AddRange(newClaims);
			OnPropertyChanged(nameof(Claims));
			return newClaims.Count > 0;
		}

		public enum TranslateClaimState {
			[Display(Name = "Закрыто")]
			Closed,
			[Display(Name = "Ожидает ответа")]
			WaitSupport,
			[Display(Name = "В ожидании сотрудника")]
			WaitUser
		}
	}
}
