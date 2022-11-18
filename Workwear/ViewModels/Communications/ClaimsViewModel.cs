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
