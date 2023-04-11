﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты оценки",
		Nominative = "акт оценки",
		Genitive = "акта оценки"
	)]
	public class Inspection: StockDocument//, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public virtual string Title{
			get{ return String.Format ("Акт оценки износа №{0} от {1:d}", Id, Date);}
		}

		private IList<InspectionItem> items = new List<InspectionItem>();
		[Display (Name = "Строки документа")]
		public virtual IList<InspectionItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		GenericObservableList<InspectionItem> observableItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<InspectionItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<InspectionItem> (Items);
				return observableItems;
			}
		}

		public virtual void RemoveItem(InspectionItem item) {
			ObservableItems.Remove (item);
		}
		public virtual void AddItem(EmployeeIssueOperation operation, decimal wearPercent) {
			var item = (new InspectionItem() {
				Document = this,
				OperationIssue = operation,
				WearPercentBefore = wearPercent
			});

			item.NewOperationIssue.UpdateIssueOperation(operation, Date);
			item.NewOperationIssue.IssuedOperation = operation;
			item.NewOperationIssue.Returned = operation.Issued;
			item.NewOperationIssue.UseAutoWriteoff = false;
			item.WriteOffDateAfter = operation.AutoWriteoffDate;
			
			operation.EmployeeOperationIssueOnWriteOff = item.NewOperationIssue;
			ObservableItems.Add(item);
		}
	}
}