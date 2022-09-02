using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Repository.Operations;

namespace workwear.ViewModels.Operations 
{
	public class ManualEmployeeIssueOperationsViewModel : UowDialogViewModelBase, IWindowDialogSettings 
	{
		public ManualEmployeeIssueOperationsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			EmployeeIssueRepository repository,
			EmployeeCardItem cardItem = null,
			EmployeeIssueOperation selectOperation = null,
			IValidator validator = null, 
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle) 
		{
			if(cardItem != null) {
				Title = cardItem.ProtectionTools.Name;
			}
			else if(selectOperation != null) {
				Title = selectOperation.ProtectionTools.Name;
			}
			Resizable = true;
			if(cardItem != null)
				EmployeeCardItem = UoW.GetById<EmployeeCardItem>(cardItem.Id);
			Operations = repository.GetAllManualIssue(UoW, EmployeeCardItem.EmployeeCard, EmployeeCardItem.ProtectionTools).ToList();
			SelectOperation = selectOperation != null 
				? Operations.First(x => x.Id == selectOperation.Id) 
				: Operations.FirstOrDefault();
			
			//Исправляем ситуацию когда у операции пропала ссылка на норму, это может произойти в случает обновления нормы.
			if(EmployeeCardItem != null)
				foreach (var operation in Operations.Where(operation => operation.NormItem == null))
					operation.NormItem = EmployeeCardItem.ActiveNormItem;
		}

		#region PublicProperty

		private List<EmployeeIssueOperation> operations;
		public List<EmployeeIssueOperation> Operations {
			get => operations;
			set => SetField(ref operations, value);
		}

		private EmployeeCardItem employeeCardItem;
		[PropertyChangedAlso(nameof(CanAddOperation))]
		public EmployeeCardItem EmployeeCardItem {
			get => employeeCardItem;
			set => SetField(ref employeeCardItem, value);
		}

		private EmployeeIssueOperation selectOperation;
		[PropertyChangedAlso(nameof(DateTime))]
		[PropertyChangedAlso(nameof(Issued))]
		[PropertyChangedAlso(nameof(CanEditOperation))]
		public EmployeeIssueOperation SelectOperation {
			get => selectOperation;
			set {
				if(SetField(ref selectOperation, value)) 
					DateTime = value.OperationTime;
			}
		}

		private DateTime dateTime;
		public DateTime DateTime {
			get => dateTime;
			set {
				if(SetField(ref dateTime, value)) {
					SelectOperation.OperationTime = value;
					SelectOperation.StartOfUse = value;
					SelectOperation.ExpiryByNorm = SelectOperation.NormItem.CalculateExpireDate(value);
					SelectOperation.AutoWriteoffDate = SelectOperation.ExpiryByNorm;
				}
			}
		}

		public int Issued => SelectOperation.Issued;
		public bool CanEditOperation => SelectOperation != null;
		public bool CanAddOperation => EmployeeCardItem != null;

		#endregion

		public void CancelOnClicked() => Close(false, CloseSource.Cancel);

		public void SaveOnClicked() {
			foreach(var operation in Operations) 
				UoW.Save(operation);
			UoW.Commit();
			SaveAndClose();
			Close(false, CloseSource.Save);
		}

		public void AddOnClicked() {
			
			if(EmployeeCardItem == null)
				throw new ArgumentNullException(nameof(EmployeeCardItem));
			
			var issue = new EmployeeIssueOperation {
				Employee = employeeCardItem.EmployeeCard,
				Issued = employeeCardItem.ActiveNormItem?.Amount ?? 1,
				OverrideBefore = true,
				NormItem = employeeCardItem.ActiveNormItem,
				ProtectionTools = employeeCardItem.ProtectionTools,
				Returned = 0,
				WearPercent = 0m,
				UseAutoWriteoff = true,
				OperationTime = employeeCardItem.NextIssue ?? DateTime.Today
			};
			UoW.Save(issue);
			Operations.Add(issue);
			SelectOperation = issue;
		}

		public void DeleteOnClicked() {
			UoW.Delete(SelectOperation);
			Operations.Remove(SelectOperation);
			SelectOperation = Operations.FirstOrDefault();
		}
		
		#region Windows Settings

		public bool IsModal { get; }
		public bool EnableMinimizeMaximize { get; }
		public bool Resizable { get; }
		public bool Deletable { get; }
		public WindowGravity WindowPosition { get; }
		
		#endregion
	}
}
