using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Postomats 
{
	[Appellative(Gender = GrammaticalGender.Feminine, 
		NominativePlural = "строки документа постамата на забор",
		Nominative = "строка документа постамата на забор")]
	[HistoryTrace]
	public class PostomatDocumentWithdrawItem : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Db Properties

		public virtual int Id { get; set; }
		
		[IgnoreHistoryTrace]
		public virtual PostomatDocumentWithdraw DocumentWithdraw { get; set; }	
		
		private EmployeeCard employee;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee 
		{
			get => employee;
			set => SetField(ref employee, value);
		}
		
		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature 
		{
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		private Barcode barcode;
		[Display(Name = "Штрихкод")]
		public virtual Barcode Barcode
		{
			get => barcode;
			set => SetField(ref barcode, value);
		}
		
		private uint terminalId;
		[Display(Name = "Код постамата")]
		public virtual uint TerminalId
		{
			get => terminalId;
			set => SetField(ref terminalId, value);
		}
		
		[Display(Name = "Размещение постамата")]
		public virtual string TerminalLocation
		{
			get => Postomat?.Location; 
			set => _ = value;
		}
		#endregion
		
		#region Runtime Properties
		
		public virtual PostomatInfo Postomat { get; set; }
		
		#endregion

		#region Вычисляемые
		public virtual string Title => $"Строка ведомости забора {Barcode.Title} из постамата {TerminalId}:{TerminalLocation}";
		#endregion

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) 
		{
			if (TerminalId == 0) 
			{
				yield return new ValidationResult("Не выбран постамат.", new[] { nameof(TerminalId) });
			}
		}
	}
}
