using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Regulations;

namespace Workwear.Domain.Company
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "должности",
		Nominative = "должность",
		Genitive ="должности"
		)]
	[HistoryTrace]
	public class Post : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[StringLength (180)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private Department department;
		[Display(Name = "Отдел")]
		public virtual Department Department {
			get => department;
			set => SetField(ref department, value);
		}

		private Profession profession;
		[Display(Name = "Профессия")]
		public virtual Profession Profession {
			get => profession;
			set {
				SetField(ref profession, value);
				if(Profession != null && String.IsNullOrWhiteSpace(Name))
					Name = Profession.Name;
			}
		}

		private CostCenter costCenter;

		[Display(Name = "Место возникновения затрат")]
		public virtual CostCenter CostCenter {
			get => costCenter;
			set => SetField(ref costCenter, value);
		}

		private string comments;
		[Display(Name = "Комментарии")]
		public virtual string Comments {
			get => comments;
			set => SetField(ref comments, value);
		}

		#endregion

		public virtual string Title {
			get {
				var list = (new [] { Subdivision?.Name, Department?.Name }).Where(x => x != null).ToList();
				if(list.Any())
					return Name + " [" + String.Join("›", list) + "]";
				else
					return Name;
			}
		}

		public Post ()
		{
		}
	}
}

