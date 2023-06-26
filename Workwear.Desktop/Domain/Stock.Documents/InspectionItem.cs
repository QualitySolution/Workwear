using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки акта оценки",
		Nominative = "строка акта оценки",
		Genitive = "строки акта оценки"
	)]
	public class InspectionItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual string Title =>
			$"{Employee.ShortName} - переоценка {Nomenclature?.Name} в количестве {Amount} {Nomenclature?.Type?.Units?.Name}";
		
		private Inspection document;
		[Display(Name = "Акт оценки")]
		[IgnoreHistoryTrace]
		public virtual Inspection Document {
			get => document;
			set => SetField(ref document, value);
		}

		private EmployeeIssueOperation operationIssue;
		[Display (Name = "Исходная операция выдачи")]	
		public virtual EmployeeIssueOperation OperationIssue {
			get => operationIssue;
			set { SetField (ref operationIssue, value, () => OperationIssue); }
		}

		private EmployeeIssueOperation newOperationIssue = new EmployeeIssueOperation();
		[Display (Name = "Новая операция выдачи")]	
		public virtual EmployeeIssueOperation NewOperationIssue {
			get => newOperationIssue;
			set => SetField (ref newOperationIssue, value);
		}
		
		public virtual EmployeeCard Employee { get => operationIssue.Employee; }
		public virtual string EmployeeNumber { get => operationIssue.Employee?.CardNumber ?? operationIssue.Employee?.Id.ToString() ?? ""; }
		public virtual Nomenclature Nomenclature { get => operationIssue.Nomenclature; }
		public virtual int Amount { get => operationIssue.Issued; }
		
		[Display (Name = "Износ на дату выдачи")]	
		public virtual decimal WearPercentBefore { get => OperationIssue.WearPercent;}
		
		[Display (Name = "Дата выдачи")]
		public virtual DateTime? IssueDate { get => OperationIssue.OperationTime;}
		
		[Display (Name = "Выдано до даты")]	
		public virtual DateTime? ExpiryByNormBefore {
			get => OperationIssue.ExpiryByNorm;
		}

		[Display (Name = "Изос после оценки")]	
		public virtual decimal WearPercentAfter {
			get => NewOperationIssue.WearPercent;
			set {
				if(NewOperationIssue.WearPercent != value) {
					NewOperationIssue.WearPercent = value;
					OnPropertyChanged();
				}
			}
		}

		[Display (Name = "Продлить до")]	
		public virtual DateTime? ExpiryByNormAfter {
			get => NewOperationIssue.ExpiryByNorm;
			set {
				if(NewOperationIssue.ExpiryByNorm != value) {
					NewOperationIssue.ExpiryByNorm = value;
					if(value != null) {
						Writeoff = false;
					}
					OnPropertyChanged();
				}
			}
		}

		[Display(Name = "Списать")]
		public virtual bool Writeoff {
			get => NewOperationIssue.Issued == 0;
			set {
				if(value != (NewOperationIssue.Issued == 0)) {
					NewOperationIssue.Issued = value ? 0 : OperationIssue.Issued;
					if(value) ExpiryByNormAfter = null;
					OnPropertyChanged();
					OnPropertyChanged(nameof(ExpiryByNormAfter));
				}
			}
		}

		private string cause;
		[Display(Name = "Причина износа")]
		public virtual string Cause {
			get => cause;
			set => SetField(ref cause, value);
		}
	}
}
