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
		
		public virtual EmployeeCard Employee { get => operationIssue.Employee; }
		public virtual Nomenclature Nomenclature { get => operationIssue.Nomenclature; }
		public virtual int Amount { get => operationIssue.Issued; }
		public virtual DateTime IssuedDate { get => operationIssue.OperationTime; }

		private decimal wearPercentBefore;
		[Display (Name = "Изос до оценки")]	
		public virtual decimal WearPercentBefore {
			get => wearPercentBefore;
			set { SetField (ref wearPercentBefore, value, () => WearPercentBefore); }
		}
		
		private DateTime writeOfDateBefore;
		[Display (Name = "Дата списания до оценки")]	
		public virtual DateTime WriteOfDateBefore {
			get => writeOfDateBefore;
			set { SetField (ref writeOfDateBefore, value, () => WriteOfDateBefore); }
		}

		private decimal wearPercentAfter;
		[Display (Name = "Изос после оценки")]	
		public virtual decimal WearPercentAfter {
			get => wearPercentAfter;
			set { SetField (ref wearPercentAfter, value, () => WearPercentAfter); }
		}
		
		private DateTime writeOfDateAfter;
		[Display (Name = "Дата списания после оценки")]	
		public virtual DateTime WriteOfDateAfter {
			get => writeOfDateAfter;
			set { SetField (ref writeOfDateAfter, value, () => WriteOfDateAfter); }
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
