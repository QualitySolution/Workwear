using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents 
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки подменной выдачи",
		Nominative = "строка подменной выдачи",
		Genitive = "строки подменной выдачи"
	)]
	[HistoryTrace]
	public class SubstituteFundDocumentItem : PropertyChangedBase, IDomainObject
	{
		#region Properties
		public virtual int Id { get; set; }

		private SubstituteFundDocuments document;
		[Display(Name = "Документ подменной выдачи")]
		public virtual SubstituteFundDocuments Document 
		{
			get => document;
			set => SetField(ref document, value);
		}
		
		private SubstituteFundOperation substituteFundOperation;
		[Display(Name = "Оперерация на выдачу подменного фонда")]
		public virtual SubstituteFundOperation SubstituteFundOperation 
		{
			get => substituteFundOperation;
			set => SetField(ref substituteFundOperation, value);
		}
		#endregion

		public SubstituteFundDocumentItem() 
		{
		}
		
		public SubstituteFundDocumentItem(SubstituteFundDocuments document, SubstituteFundOperation operation) 
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
			SubstituteFundOperation = operation ?? throw new ArgumentNullException(nameof(operation));
		}
	}
}
