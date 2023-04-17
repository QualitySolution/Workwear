using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
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
			get{ return String.Format ("Акт оценки износа №{0} от {1:d}", Id, Date);}
		}

		private IList<InspectionItem> items = new List<InspectionItem>();
		[Display (Name = "Строки документа")]
		public virtual IList<InspectionItem> Items {
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
		[Display (Name = "Председатель комисии")]
		public virtual Leader Chairman {
			get { return chairman; }
			set { SetField (ref chairman, value, () => Chairman); }
		}
		
		private IList<InspectionMember> members = new List<InspectionMember>();
		[Display(Name = "Члены комисии")]
		public virtual IList<InspectionMember> Members {
			get { return members; }
			set { SetField(ref members, value, () => Members); }
		}
		#endregion
		
		GenericObservableList<InspectionItem> observableItems;
        //FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<InspectionItem> ObservableItems {
			get {
				if(observableItems == null)
					observableItems = new GenericObservableList<InspectionItem>(Items);
				return observableItems;
	        }
		}
        
        GenericObservableList<InspectionMember> observableMembers;
        //FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<InspectionMember> ObservableMembers {
	        get {
		        if(observableMembers == null)
			        observableMembers = new GenericObservableList<InspectionMember>(Members);
		        return observableMembers;
	        }
        }

        #region Методы
		public virtual void RemoveItem(InspectionItem item) {
			ObservableItems.Remove (item);
		}
		public virtual void AddItem(EmployeeIssueOperation operation, decimal wearPercent) {
			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.OperationIssue, operation))) {
				logger.Warn("Эта операция уже есть в документе. Пропускаем...");
				return;
			}

			var item = (new InspectionItem() {
				Document = this,
				OperationIssue = operation,
				WearPercentBefore = wearPercent
			});

			item.NewOperationIssue.UpdateIssueOperation(operation, Date);
			item.NewOperationIssue.IssuedOperation = operation;
			item.NewOperationIssue.Returned = operation.Issued;
			item.WriteOffDateAfter = operation.AutoWriteoffDate;
			item.WearPercentAfter = wearPercent; 
			
			ObservableItems.Add(item);
		}
		
		public virtual void RemoveMember(InspectionMember member) {
			ObservableMembers.Remove (member);
		}
		
		public virtual void AddMember(Leader member)
		{
			if(Members.Any(p => DomainHelper.EqualDomainObjects(p, member))) {
				logger.Warn("Этот член комисии уже добавлен. Пропускаем...");
				return;
			}
			ObservableMembers.Add(new InspectionMember(){Member = member, Document = this});
		}
		#endregion
		
		#region Валидатор
//todo Надо Сделать валидацию		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if(false) yield return new ValidationResult("Валидация не реализована");
		}
		#endregion
	}
}
