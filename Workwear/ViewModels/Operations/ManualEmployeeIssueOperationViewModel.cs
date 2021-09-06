using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using workwear.Domain.Company;
using workwear.Domain.Operations;

namespace workwear.ViewModels.Operations
{
	public class ManualEmployeeIssueOperationViewModel : EntityDialogViewModelBase<EmployeeIssueOperation>, IWindowDialogSettings
	{
		public ManualEmployeeIssueOperationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			EmployeeCardItem employeeCardItem = null,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			if(UoW.IsNew) {
				if(employeeCardItem == null)
					throw new ArgumentNullException(nameof(employeeCardItem));
				Entity.Employee = employeeCardItem.EmployeeCard;
				Entity.Issued = employeeCardItem.ActiveNormItem?.Amount ?? 1;
				Entity.ManualOperation = true;
				Entity.NormItem = employeeCardItem.ActiveNormItem;
				Entity.ProtectionTools = employeeCardItem.ProtectionTools;
				Entity.Returned = 0;
				Entity.WearPercent = 0m;
				Entity.UseAutoWriteoff = true;
				Entity.OperationTime = employeeCardItem.NextIssue ?? DateTime.Today;
			}
			if(!Entity.ManualOperation)
				throw new NotSupportedException("Этот диалог предназначен только для ручных операций");
			IssueDate = Entity.OperationTime;
		}

		#region Windows Settings
		public bool IsModal => true;
		public bool EnableMinimizeMaximize => false;
		public WindowGravity WindowPosition => WindowGravity.Center;
		#endregion

		#region Propeties
		private DateTime issueDate;
		public virtual DateTime IssueDate {
			get => issueDate;
			set {
				if(SetField(ref issueDate, value)) {
					Entity.OperationTime = value;
					Entity.StartOfUse = value;
					Entity.ExpiryByNorm = Entity.NormItem.CalculateExpireDate(value);
					Entity.AutoWriteoffDate = Entity.ExpiryByNorm;
				}
			}
		}

		public string Units => Entity.ProtectionTools?.Type?.Units?.Name;
		#endregion
	}
}
