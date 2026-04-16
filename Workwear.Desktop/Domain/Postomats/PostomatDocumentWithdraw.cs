using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Project.Domain;
using Workwear.Domain.ClothingService;

namespace Workwear.Domain.Postomats 
{
	[Appellative(Gender = GrammaticalGender.Masculine, 
		NominativePlural = "документы постамата на забор",
		Nominative = "документ постамата на забор",
		Genitive = "документ постамата на забор",
		GenitivePlural = "документов постамата на забор")]
	[HistoryTrace]
	public class PostomatDocumentWithdraw : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Db Properties
		
		public virtual int Id { get; set; }
		
		private DateTime createTime = DateTime.Now;
		[Display(Name = "Дата создания")]
		public virtual DateTime CreateTime 
		{
			get => createTime;
			set => SetField(ref createTime, value);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment 
		{
			get => comment;
			set => SetField(ref comment, value);
		}
		
		private UserBase user;
		[Display(Name = "Пользователь")]
		public virtual UserBase User {
			get => user;
			set => SetField(ref user, value);
		}
		#endregion

		#region Additional Properties

		public virtual string Title => $"Забор из постамата №{Id} от {CreateTime:d}";
		
		private IObservableList<PostomatDocumentWithdrawItem> items = new ObservableList<PostomatDocumentWithdrawItem>();
		[Display(Name = "Строки документа на забор")]
		public virtual IObservableList<PostomatDocumentWithdrawItem> Items 
		{
			get => items;
			set => SetField(ref items, value);
		}

		#endregion
		
		public virtual void AddItem(ServiceClaim claim, PostomatInfo postomat) 
		{
			var newItem = new PostomatDocumentWithdrawItem 
			{
				DocumentWithdraw = this,
				Employee = claim.Employee,
				Nomenclature = claim.Barcode.Nomenclature,
				Barcode = claim.Barcode,
				TerminalId = postomat.Id,
				Postomat = postomat
			};
			
			Items.Add(newItem);
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) 
		{
			if(!Items.Any()) 
			{
				yield return new ValidationResult("Не заполнены строки документа.", new[] { nameof(Items) });
			}
		}
	}
}
