using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты оценки",
		Nominative = "акт оценки",
		Genitive = "акта оценки"
	)]
	public class Inspection: StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства
		public virtual string Title{
			get{ return String.Format ("Акт оценки износа №{0} от {1:d}", DocNumberText, Date);}
		}

		private IObservableList<InspectionItem> items = new ObservableList<InspectionItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<InspectionItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		private Leader director;
		[Display (Name = "Утверждающее лицо")]
		public virtual Leader Director {
			get { return director; }
			set { SetField (ref director, value, () => Director); }
		}
		
		private Leader chairman;
		[Display (Name = "Председатель комиссии")]
		public virtual Leader Chairman {
			get { return chairman; }
			set { SetField (ref chairman, value, () => Chairman); }
		}
		
		private Organization organization;
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField(ref organization, value, () => Organization); }
		}

		private IObservableList<Leader> members = new ObservableList<Leader>();
		[Display(Name = "Члены комиссии")]
		public virtual IObservableList<Leader> Members {
			get { return members; }
			set { SetField(ref members, value, () => Members); }
		}
		#endregion

        #region Методы
		public virtual void RemoveItem(InspectionItem item) {
			Items.Remove (item);
		}
		public virtual void AddItem(EmployeeIssueOperation operation, decimal wearPercent) {
			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.OperationIssue, operation))) {
				logger.Warn("Эта операция уже есть в документе. Пропускаем...");
				return;
			}

			var item = (new InspectionItem() {
				Document = this,
				OperationIssue = operation,
			});
			item.NewOperationIssue.FixedOperation = true;
			item.NewOperationIssue.UpdateIssueOperation(operation, Date);
			item.NewOperationIssue.IssuedOperation = operation;
			item.NewOperationIssue.Returned = operation.Issued;
			item.ExpiryByNormAfter = operation.ExpiryByNorm;
			item.WearPercentAfter = wearPercent; 
			
			Items.Add(item);
		}
		
		public virtual void RemoveMember(Leader member) {
			Members.Remove (member);
		}
		
		public virtual void AddMember(Leader member) {
			if(Members.Any(p => DomainHelper.EqualDomainObjects(p, member))) {
				logger.Warn("Этот член комиссии уже добавлен. Пропускаем...");
				return;
			}
			Members.Add(member); 
		}
		#endregion
		
		#region Валидатор
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { nameof(Date)});
			
			if (DocNumber != null && DocNumber.Length > 15)
				yield return new ValidationResult ("Номер документа должен быть не более 15 символов", 
					new[] {nameof(DocNumber)});
			
			foreach(var item in Items) {
				if(!item.Writeoff && !item.ExpiryByNormAfter.HasValue)
					yield return new ValidationResult ($"По строке {item.Nomenclature.Name} - {item.Employee.ShortName} " +
					                                   $"не принято решение. Проставьте списание или дату продления.", 
                		new[] { nameof(Items)});
			}
		}
		#endregion
	}
}
