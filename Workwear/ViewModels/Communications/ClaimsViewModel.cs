using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
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
		}
		

		#region View

		public IList<Claim> Claims { get; set; }
		public Claim SelectClaim { get; set; }
		public IList<ClaimMessage> MessagesSelectClaims { get; set; }
		public ClaimState SelectClaimState { get; set; }
		public bool ShowClosed { get; set; } 
		public string TextMessage { get; set; }

		public bool SensitiveSend => SensitiveChangeState && String.IsNullOrEmpty(TextMessage);
		public bool SensitiveChangeState => SelectClaim != null;

		#endregion

		#region ViewMethods

		public void RefreshClaims() {
			Claims = claimsManager.GetClaims(sizePage, 0, ShowClosed);
		}

		public void AddRangeClaims() {
			Claims
				.ToList()
				.AddRange(claimsManager.GetClaims(sizePage, (uint)Claims.Count, ShowClosed));
		}

		public void Send(object sender, EventArgs eventArgs) {
			claimsManager.Send(SelectClaim.Id, TextMessage);
		}

		public void ChangeStatusClaim(object sender, EventArgs e) {
			claimsManager.SetСhanges(SelectClaim);
		}

		#endregion
	}
}
