using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Project.Domain;

namespace Workwear.Domain.Stock.Documents {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "предполагаемые поставки",
		Nominative = "предполагаемая поставка",
		Genitive = "предполагаемой поставки"
	)]
	[HistoryTrace]
	public class Procurement: IValidatableObject 
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		#region Свойства

		public virtual int Id { get; set; }
		
		private DateTime startPeriod;
		[Display(Name="Начало периода")]
		public virtual DateTime StartPeriod {
			get=> startPeriod;
			set {startPeriod = value;}
		}
		
		private DateTime endPeriod;
		[Display(Name="Окончание периода")]
		public virtual DateTime EndPeriod {
			get=> endPeriod;
			set { endPeriod = value; }
		}
		
		private UserBase createdbyUser;
		[Display(Name = "Документ создал")]
		public virtual UserBase CreatedbyUser {
			get =>createdbyUser; 
			set { createdbyUser = value; }
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get =>comment;
			set { comment = value; }
		}
		
		private DateTime? creationDate = DateTime.Now;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreationDate {
			get => creationDate;
			set {creationDate = value;}
		}

		#endregion
		
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if (StartPeriod < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата начала периода должна быть указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.StartPeriod)});
			if(StartPeriod > EndPeriod)
				yield return new ValidationResult("Дата начала периода должна быть меньше его окончания",
					new[] { this.GetPropertyName(o => o.StartPeriod) });
			
		}
		#endregion
	}
}
