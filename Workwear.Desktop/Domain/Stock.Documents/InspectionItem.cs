using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Tools;

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
		private EmployeeIssueOperation operationWriteoff = new EmployeeIssueOperation();
		[Display (Name = "Операция списания")]	
		public virtual EmployeeIssueOperation OperationWriteoff {
			get => operationWriteoff;
			set { SetField (ref operationWriteoff, value, () => OperationWriteoff); }
		}
		private EmployeeIssueOperation newOperationIssue = new EmployeeIssueOperation();
		[Display (Name = "Новая операция выдачи")]	
		public virtual EmployeeIssueOperation NewOperationIssue {
			get => newOperationIssue;
			set { SetField (ref newOperationIssue, value, () => NewOperationIssue); }
		}
		
		public virtual EmployeeCard Employee { get => operationIssue.Employee; }
		public virtual Nomenclature Nomenclature { get => operationIssue.Nomenclature; }
		public virtual int Amount { get => operationIssue.Issued; }

		[Display (Name = "Изос до оценки")]	
		public virtual decimal WearPercentBefore {
			get => operationWriteoff.WearPercent;
		}
		
		[Display (Name = "Дата списания до оценки")]	
		public virtual DateTime? WriteOffDateBefore {
			get => OperationIssue.AutoWriteoffDate;
		}

		[Display (Name = "Изос после оценки")]	
		public virtual decimal WearPercentAfter {
			get => newOperationIssue.WearPercent;
			set {
				if(newOperationIssue.WearPercent != value) {
					newOperationIssue.WearPercent = value;
					OnPropertyChanged();
				}
			}
		}
		private DateTime? writeOffDateAfter;
		[Display (Name = "Дата списания после оценки")]	
		public virtual DateTime? WriteOffDateAfter {
			get => newOperationIssue.AutoWriteoffDate;
			set {
				if(newOperationIssue.AutoWriteoffDate != value) {
					newOperationIssue.AutoWriteoffDate = value;
					OnPropertyChanged();
				}
			}
		}
		private string aktNumber;
		[Display(Name = "Номер акта")]
		public virtual string AktNumber {
			get => aktNumber;
			set => SetField(ref aktNumber, value);
		}
		
		private string cause;
		[Display(Name = "Причина износа")]
		public virtual string Cause {
			get => cause;
			set => SetField(ref cause, value);
		}
	}
}
