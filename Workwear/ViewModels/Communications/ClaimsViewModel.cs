using System;
using System.Collections.Generic;
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

		private IList<Claim> Claims { get; set; }
		public Claim SelectClaim { get; set; }
		public ClaimMessage MessagesSelectClaims { get; set; }
		public ClaimState SelectClaimState { get; set; }
		public bool ShowClosed { get; set; } 
		public string TextMessage { get; set; }

		#endregion

		#region ViewMethods

		public object GetClaims() {
			throw new System.NotImplementedException();
		}

		#endregion

		public void Send(object sender, EventArgs eventArgs) {
			throw new NotImplementedException();
		}

		public void ChangeStatusClaim(object sender, EventArgs e) {
			throw new NotImplementedException();
		}
	}
}
