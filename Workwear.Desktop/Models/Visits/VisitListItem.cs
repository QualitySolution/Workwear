using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;

namespace Workwear.Models.Visits {

	// Интервал/номерок/посещение склада
	public class VisitListItem : PropertyChangedBase {

		//Создать пустой интервал 
		public VisitListItem(DateTime visitTime) {
			this.VisitTime = visitTime;
		}

		//Создать из записи
		public VisitListItem(Visit visit) {
			if(visit != null) {
				this.Visit = visit;
				this.VisitTime = visit.VisitTime;
				this.Employee = visit.Employee;
			}
			else
				throw new NullReferenceException(nameof(visit));
		}

		public DateTime VisitTime { get; }
		public Visit Visit { get; }
		public DateTime? CreateTime => Visit?.CreateDate;
		public string Comment => Visit?.Comment;

		public string DocumentsString => string.Join("\n", Documents.Select(x => x.label));
		public bool SensitiveActionButtons => Visit.SensitiveActionButtons;
		public bool SensitiveDoneAndCanceledButtons => Visit.SensitiveDoneAndCanceledButtons;
		public bool SensitiveElement => Visit.SensitiveActionButtons;

		//Список привязанных документов
		public List<(StockDocumentType doc, int id, string label)> Documents {
			get {
				var result = new List<(StockDocumentType, int, string)>();
				if(Visit?.ExpenseDocuments != null)
					result.AddRange(Visit.ExpenseDocuments.Select(x =>
						(StockDocumentType.ExpenseEmployeeDoc, x.Id,
							$"{(x.IssueDate != null ? "Выдача" : "Черновик")} {x.DocNumberText}")));
				if(Visit?.WriteoffDocuments != null)
					result.AddRange(Visit.WriteoffDocuments.Select(x =>
						(StockDocumentType.WriteoffDoc, x.Id, $"Списание {x.DocNumberText}")));
				if(Visit?.ReturnDocuments != null)
					result.AddRange(Visit.ReturnDocuments.Select(x =>
						(StockDocumentType.Return, x.Id, $"Возврат {x.DocNumberText}")));
				return result;
			}
		}

		public EmployeeCard Employee { get; }
		public string FIO => Employee?.FullName ?? "";

	}
}
