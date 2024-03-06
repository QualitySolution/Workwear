using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Project.Domain;
using Workwear.Domain.ClothingService;

namespace Workwear.Domain.Postomats {
	[Appellative(Gender = GrammaticalGender.Masculine, 
		NominativePlural = "документы постамата",
		Nominative = "документ постамата",
		Genitive = "документ постамата")]
	[HistoryTrace]
	public class PostomatDocument : PropertyChangedBase, IDomainObject, IValidatableObject {
		#region Cвойства	
		public virtual int Id { get; set; }

		private DateTime createTime = DateTime.Now;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreateTime {
			get => createTime;
			set => SetField(ref createTime, value);
		}
		
		private uint terminalId;
		[Display(Name = "Постамат")]
		public virtual uint TerminalId {
			get => terminalId;
			set => SetField(ref terminalId, value);
		}

		private DocumentStatus status;
		[Display(Name = "Статус")]
		public virtual DocumentStatus Status {
			get => status;
			set => SetField(ref status, value);
		}
		
		private DocumentType type;
		[Display(Name = "Тип документа")]
		public virtual DocumentType Type {
			get => type;
			set => SetField(ref type, value);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		
		private IObservableList<PostomatDocumentItem> items = new ObservableList<PostomatDocumentItem>();
		[Display(Name = "Строки документа")]
		public virtual IObservableList<PostomatDocumentItem> Items {
			get => items;
			set => SetField(ref items, value);
		}
		#endregion

		#region Расчетные
		public virtual string Title => $"{Type.GetEnumTitle()} постамата №{Id} от {CreateTime:d}";

		#endregion

		#region Рантайм онли
		public virtual PostomatInfo Postomat { get; set; }
		#endregion

		#region Строки документа
		public virtual void AddItem(ServiceClaim claim, CellLocation location, UserBase user) {
			var newItem = new PostomatDocumentItem {
				Document = this,
				Employee = claim.Employee,
				Nomenclature = claim.Barcode.Nomenclature,
				Barcode = claim.Barcode,
				ServiceClaim = claim,
				Delta = 1,
				Location = location,
			};
			Items.Add(newItem);
			claim.ChangeState(ClaimState.InTransit, TerminalId, user, $"Перемещение в постамат {Postomat.Id}: {Postomat.Name}({Postomat.Location})");
		}

		#endregion
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if(TerminalId == 0)
				yield return new ValidationResult("Не выбран постамат.", new[] { nameof(TerminalId) });
			if(Items.Count == 0)
				yield return new ValidationResult("Не заполнены строки документа.", new[] { nameof(Items) });
			foreach(var item in Items) {
				if(item.Location.IsEmpty)
					yield return new ValidationResult($"Не заполнено место хранения для номенклатуры {item.Nomenclature.Name}.", new[] { nameof(Items) });
			}
		}
	}

	public enum DocumentType {
		[Display(Name = "Приход")]
		Income,
		[Display(Name = "Расход")]
		Outgo,
		[Display(Name = "Коррекция")]
		Correction
	}
	
	public enum DocumentStatus {
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Выполнен")]
		Done,
		[Display(Name = "Удален")]
		Deleted
	}
}
