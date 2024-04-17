using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Analytics 
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Категории номенклатуры нормы для аналитики",
		Nominative = "Категория номенклатуры нормы для аналитики",
		Genitive = "Категории номенклатуры нормы для аналитики"
	)]
	[HistoryTrace]
	public class ProtectionToolsCategory : PropertyChangedBase, IDomainObject 
	{
		#region Properties

		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название категории не должно быть пустым.")]
		[StringLength(100)]
		public virtual string Name 
		{
			get => name; 
			set => SetField(ref name, value?.Trim());
		}
		
		private string comment;
		[Display(Name = "Описание")]
		public virtual string Comment 
		{
			get => comment; 
			set => SetField(ref comment, value?.Trim());
		}

		private IObservableList<ProtectionTools> protectionTools = new ObservableList<ProtectionTools>();
		[Display(Name = "Типы номенклатуры нормы")]
		public virtual IObservableList<ProtectionTools> ProtectionTools 
		{
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}		
		#endregion

		#region Protection Tools

		public virtual void AddProtectionTools(ProtectionTools tool)
		{
			if(tool == null) throw new ArgumentNullException(nameof(tool));
			tool.CategoryForAnalytic = this;
			if(!ProtectionTools.Any(p => DomainHelper.EqualDomainObjects(p, tool))) 
			{
				ProtectionTools.Add(tool);
			}
		}

		public virtual void RemoveProtectionTools(ProtectionTools tool) 
		{
			if(tool == null) throw new ArgumentNullException(nameof(tool));
			tool.CategoryForAnalytic = null;
			ProtectionTools.Remove(tool);
		}
		
		#endregion
	}
}
