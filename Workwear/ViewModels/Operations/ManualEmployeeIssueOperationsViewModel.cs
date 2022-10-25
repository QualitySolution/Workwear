using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Repository.Operations;

namespace Workwear.ViewModels.Operations 
{
	public class ManualEmployeeIssueOperationsViewModel : UowDialogViewModelBase, IWindowDialogSettings 
	{
		private readonly ProtectionTools protectionTools;
		public ManualEmployeeIssueOperationsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			EmployeeIssueRepository repository,
			EmployeeCardItem cardItem = null,
			EmployeeIssueOperation selectOperation = null,
			IValidator validator = null) : base(unitOfWorkFactory, navigation, validator, "Редактирование ручных операций") 
		{
			Resizable = true;
			Deletable = true;
			if(cardItem != null) {
				Title = cardItem.ProtectionTools.Name;
				protectionTools = cardItem.ProtectionTools;
				EmployeeCardItem = UoW.GetById<EmployeeCardItem>(cardItem.Id);
				Operations = new GenericObservableList<EmployeeIssueOperation>(
					repository.GetAllManualIssue(UoW, EmployeeCardItem.EmployeeCard, EmployeeCardItem.ProtectionTools)
						.OrderBy(x => x.OperationTime)
						.ToList());
			}
			else if(selectOperation != null) {
				Title = selectOperation.ProtectionTools?.Name;
				protectionTools = selectOperation.ProtectionTools;
				Operations = new GenericObservableList<EmployeeIssueOperation>(
					repository.GetAllManualIssue(UoW, selectOperation.Employee, selectOperation.ProtectionTools)
						.OrderBy(x => x.OperationTime)
						.ToList());
			}
			else
				throw new ArgumentNullException(nameof(selectOperation) + nameof(cardItem));

			SelectOperation = selectOperation != null 
				? Operations.First(x => x.Id == selectOperation.Id) 
				: Operations.FirstOrDefault();
			
			//Исправляем ситуацию когда у операции пропала ссылка на норму, это может произойти в случает обновления нормы.
			if(EmployeeCardItem != null)
				foreach (var operation in Operations.Where(operation => operation.NormItem == null))
					operation.NormItem = EmployeeCardItem.ActiveNormItem;
		}

		#region PublicProperty

		private GenericObservableList<EmployeeIssueOperation> operations;
		public GenericObservableList<EmployeeIssueOperation> Operations {
			get => operations;
			private set => SetField(ref operations, value);
		}

		private EmployeeCardItem employeeCardItem;
		[PropertyChangedAlso(nameof(CanAddOperation))]
		public EmployeeCardItem EmployeeCardItem {
			get => employeeCardItem;
			private set => SetField(ref employeeCardItem, value);
		}

		private EmployeeIssueOperation selectOperation;
		[PropertyChangedAlso(nameof(CanEditOperation))]
		public EmployeeIssueOperation SelectOperation {
			get => selectOperation;
			set {
				if(SetField(ref selectOperation, value)) {
					if(value != null) {
						DateTime = value.OperationTime;
						Issued = value.Issued;
						OverrideBefore = value.OverrideBefore;
					}
					else
						Issued = 0;
				}
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

		private int issued;
		public int Issued {
			get => issued;
			set {
				if(SetField(ref issued, value))
					if(SelectOperation != null)
						SelectOperation.Issued = value;
			}
		}

		private bool overrideBefore;
		public bool OverrideBefore {
			get => overrideBefore;
			set {
				if(SetField(ref overrideBefore, value))
					if(SelectOperation != null)
						SelectOperation.OverrideBefore = value;
			}
		}
		public bool CanEditOperation => SelectOperation != null;
		public bool CanAddOperation => EmployeeCardItem != null;

		public event Action<ProtectionTools> SaveChanged; 

		#endregion

		public override bool Save() {
			Validations.Clear();
			Validations.AddRange(Operations.Select(x => new ValidationRequest(x)));
			if(!Validate())
				return false;
			foreach(var operation in Operations)
				UoW.Save(operation);
			UoW.Commit();
			SaveChanged?.Invoke(protectionTools);
			Close(false, CloseSource.Save);
			return true;
		}

		public void AddOnClicked() {
			if(EmployeeCardItem == null)
				throw new ArgumentNullException(nameof(EmployeeCardItem));
			var issue = new EmployeeIssueOperation {
				Employee = EmployeeCardItem.EmployeeCard,
				Issued = EmployeeCardItem.ActiveNormItem?.Amount ?? 1,
				ManualOperation = true,
				NormItem = EmployeeCardItem.ActiveNormItem,
				ProtectionTools = EmployeeCardItem.ProtectionTools,
				Returned = 0,
				WearPercent = 0m,
				UseAutoWriteoff = true,
				OperationTime = EmployeeCardItem.NextIssue ?? DateTime.Today 
			};
			if(!Operations.Any())
				issue.OverrideBefore = true;
			
			issue.ExpiryByNorm = issue.NormItem?.CalculateExpireDate(DateTime.Today);
			Operations.Add(issue);
		}

		public void DeleteOnClicked(EmployeeIssueOperation deleteOperation) {
			Operations.Remove(deleteOperation);
			UoW.Delete(deleteOperation);
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
