using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace Workwear.Domain.Postomats {
	public class PostomatDocument : PropertyChangedBase {
		#region Cвойства	
		public virtual int Id { get; set; }

		private DateTime createTime = DateTime.Now;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreateTime {
			get => createTime;
			set => SetField(ref createTime, value);
		}
		
		private uint terminalId;
		[Display(Name = "Постомат")]
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
		
		private IObservableList<PostomatDocumentItem> items = new ObservableList<PostomatDocumentItem>();
		[Display(Name = "Строки документа")]
		public virtual IObservableList<PostomatDocumentItem> Items {
			get => items;
			set => SetField(ref items, value);
		}
		#endregion
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
