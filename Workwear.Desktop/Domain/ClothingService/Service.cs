using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Stock;

namespace Workwear.Domain.ClothingService {
	[HistoryTrace]
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "услуги",
		Nominative = "услуга",
		Genitive = "услуги",
		GenitivePlural = "услуг")]
	public class Service : PropertyChangedBase, IDomainObject{
		#region Свойства
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private decimal cost;
		[Display(Name = "Стоимость")]
		public virtual decimal Cost {
			get => cost;
			set => SetField(ref cost, value);
		}
		
		private string code;
		[Display (Name = "Сервисный код")]
		public virtual string Code {
			get => code;
			set => SetField(ref code, value);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		#endregion
		
		#region Номенклатуры

		private IObservableList<Nomenclature> nomenclatures = new ObservableList<Nomenclature>();

		[Display(Name = "Номенклатуры")]
		public virtual IObservableList<Nomenclature> Nomenclatures {
			get { return nomenclatures; }
			set { SetField(ref nomenclatures, value, () => Nomenclatures); }
		}

		public virtual void AddNomenclature(Nomenclature nomenclature) {
			if(Nomenclatures.Any(p => DomainHelper.EqualDomainObjects(p, nomenclature))) 
				return;
			Nomenclatures.Add(nomenclature);
		}
		public virtual void RemoveNomenclature(Nomenclature nomenclature) => Nomenclatures.Remove(nomenclature);
		#endregion
	}
}
