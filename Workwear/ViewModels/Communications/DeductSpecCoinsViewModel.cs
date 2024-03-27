using System;
using QS.Cloud.WearLk.Client;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.ViewModels.Dialog;

namespace Workwear.ViewModels.Communications 
{
	public class DeductSpecCoinsViewModel : WindowDialogViewModelBase {
		private const string DeductOperationErrorMessage = "Операция невозможна: недостаточно спецкойнов для списания";
		private readonly IUnitOfWork unitOfWork;
		private readonly IInteractiveMessage interactive;
		private readonly IDataBaseInfo dataBaseInfo;
		private readonly SpecCoinManagerService specCoinManagerService;
		private readonly string employeePhone;
		private readonly int specCoinsBalance;
		private readonly Action specCoinsBalanceUpdated;

		public DeductSpecCoinsViewModel(IUnitOfWorkFactory unitOfWorkFactory, string employeePhone,
			int specCoinsBalance, Action specCoinsBalanceUpdated, IDataBaseInfo dataBaseInfo,
			SpecCoinManagerService specCoinManagerService, IInteractiveMessage interactive,
			INavigationManager navigation) : base(navigation) 
		{
			this.specCoinManagerService = specCoinManagerService ?? throw new ArgumentNullException(nameof(specCoinManagerService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			if (string.IsNullOrWhiteSpace(employeePhone)) throw new ArgumentNullException(nameof(this.employeePhone));
			this.employeePhone = employeePhone;
			this.specCoinsBalance = specCoinsBalance;
			this.specCoinsBalanceUpdated = specCoinsBalanceUpdated;
			this.dataBaseInfo = dataBaseInfo;
			Title = "Списание спецкойнов";
			
			unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
		}

		#region Sensetive
		public bool SensitiveDeductButton => deductCoinsAmount > 0 && !String.IsNullOrWhiteSpace(Description);
		#endregion
		
		#region Properties
		private int deductCoinsAmount;
		[PropertyChangedAlso(nameof(SensitiveDeductButton))]
		public int DeductCoinsAmount
		{
			get => deductCoinsAmount;
			set => SetField(ref deductCoinsAmount, value);
		}
		
		private string description;
		[PropertyChangedAlso(nameof(SensitiveDeductButton))]
		public string Description 
		{
			get => description;
			set => SetField(ref description, value);
		}
		#endregion

		#region Commands
		public void DeductCoins() 
		{
			if (specCoinsBalance - DeductCoinsAmount < 0) 
			{
				interactive.ShowMessage(ImportanceLevel.Error, DeductOperationErrorMessage);
				return;
			}

			string result = specCoinManagerService.DeductCoins(employeePhone, DeductCoinsAmount, Description);
			interactive.ShowMessage(ImportanceLevel.Info, result);
			specCoinsBalanceUpdated?.Invoke();
			Close(false, CloseSource.Self);
		}
		#endregion
	}
}
