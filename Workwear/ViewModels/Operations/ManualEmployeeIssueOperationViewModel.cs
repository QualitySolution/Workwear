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
				Entity.OverrideBefore = true;
				Entity.NormItem = employeeCardItem.ActiveNormItem;
				Entity.ProtectionTools = employeeCardItem.ProtectionTools;
				Entity.Returned = 0;
				Entity.WearPercent = 0m;
				Entity.UseAutoWriteoff = true;
				Entity.OperationTime = employeeCardItem.NextIssue ?? DateTime.Today;
			}
			if(!Entity.OverrideBefore)
				throw new NotSupportedException("Этот диалог предназначен только для ручных операций");
			//Исправляем ситуацию когда у операции пропала ссылка на норму, это может произойти в случает обновления нормы.
			if (Entity.NormItem == null && employeeCardItem != null)
				Entity.NormItem = employeeCardItem.ActiveNormItem;
			IssueDate = Entity.OperationTime;
		}

		#region Windows Settings
		public bool IsModal => true;
		public bool EnableMinimizeMaximize => false;
		public bool Resizable => true;
		public bool Deletable => true;
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

		#region Sensitive
		public bool SensitiveDeleteButton => !UoW.IsNew;
		#endregion

		#region Actions
		public void Delete() {
			Close(false, CloseSource.Self);
		}
		#endregion
	}
}
