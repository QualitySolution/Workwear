using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Impl;

namespace Workwear.Domain.Stock.Documents 
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки подменной выдачи",
		Nominative = "строка подменной выдачи",
		Genitive = "строки подменной выдачи"
	)]
	[HistoryTrace]
	public class OverNormItem : PropertyChangedBase, IDomainObject
	{
		#region Properties
		public virtual int Id { get; set; }

		private OverNorm document;
		[Display(Name = "Документ подменной выдачи")]
		public virtual OverNorm Document 
		{
			get => document;
			set => SetField(ref document, value);
		}
		
		private OverNormOperation overNormOperation;
		[Display(Name = "Оперерация на выдачу подменного фонда")]
		public virtual OverNormOperation OverNormOperation 
		{
			get => overNormOperation;
			set => SetField(ref overNormOperation, value);
		}

		#endregion
		
		#region Not Mapped Propertis
		private OverNormParam param;
		public virtual OverNormParam Param 
		{
			get => param;
			set => SetField(ref param, value);
		}

		public virtual string Title => $"Подменная выдача {OverNormOperation.WarehouseOperation.Nomenclature.Name} в количестве {OverNormOperation.WarehouseOperation.Amount}";
		#endregion
		
		protected OverNormItem() 
		{
		}
		
		public OverNormItem(OverNorm document, OverNormOperation operation) 
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
			OverNormOperation = operation ?? throw new ArgumentNullException(nameof(operation));
		}
	}
}
