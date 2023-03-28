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

		private decimal wearPercentBefore;
		[Display (Name = "Изос до оценки")]	
		public virtual decimal WearPercentBefore {
			get => wearPercentBefore;
			set { SetField (ref wearPercentBefore, value, () => WearPercentBefore); }
		}
		
		private DateTime? writeOffDateBefore;
		[Display (Name = "Дата списания до оценки")]	
		public virtual DateTime? WriteOffDateBefore {
			get => writeOffDateBefore;
			set { SetField (ref writeOffDateBefore, value, () => WriteOffDateBefore); }
		}

		private decimal wearPercentAfter;
		[Display (Name = "Изос после оценки")]	
		public virtual decimal WearPercentAfter {
			get => wearPercentAfter;
			set { SetField (ref wearPercentAfter, value, () => WearPercentAfter); }
		}
		
		private DateTime? writeOffDateAfter;
		[Display (Name = "Дата списания после оценки")]	
		public virtual DateTime? WriteOffDateAfter {
			get => writeOffDateAfter;
			set { SetField (ref writeOffDateAfter, value, () => WriteOffDateAfter); }
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
