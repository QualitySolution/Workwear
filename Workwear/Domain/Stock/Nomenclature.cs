using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Measurements;
using Workwear.Measurements;
using QS.HistoryLog;
using workwear.Repository.Stock;
using workwear.Tools;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "номенклатура",
		Nominative = "номенклатура",
		Genitive = "номенклатуры"
		)]
	[HistoryTrace]
	public class Nomenclature: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название номенклатуры должно быть заполнено.")]
		[StringLength(240)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value?.Trim()); }
		}

		ItemsType type;

		[Display (Name = "Группа номенклатур")]
		[Required (ErrorMessage = "Номенклатурная группа должна быть указана.")]
		public virtual ItemsType Type {
			get { return type; }
			set { SetField (ref type, value, () => Type); }
		}

		ClothesSex? sex;

		[Display (Name = "Пол одежды")]
		public virtual ClothesSex? Sex {
			get { return sex; }
			set { SetField (ref sex, value, () => Sex); }
		}

		string sizeStd;

		[Display (Name = "Стандарт размера")]
		public virtual string SizeStd {
			get { return sizeStd; }
			set { SetField (ref sizeStd, value, () => SizeStd); }
		}

		private string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		private uint? number;

		[Display(Name = "Номенклатурный номер")]
		public virtual uint? Number {
			get { return number; }
			set { SetField(ref number, value); }
		}

		private bool archival;
		[Display(Name ="Архивная")]
		public virtual bool Archival {
			get => archival;
			set => SetField(ref archival, value);
		}

		#endregion

		#region Рассчетные

		public virtual string TypeName => Type.Name;

		public virtual string GetAmountAndUnitsText(int amount)
		{
			return this.Type?.Units?.MakeAmountShortStr(amount) ?? amount.ToString();
		}

		#endregion

		#region Средства защиты

		private IList<ProtectionTools> protectionTools = new List<ProtectionTools>();

		[Display(Name = "Номенаклатуры нормы")]
		public virtual IList<ProtectionTools> ProtectionTools {
			get { return protectionTools; }
			set { SetField(ref protectionTools, value, () => ProtectionTools); }
		}

		GenericObservableList<ProtectionTools> observableProtectionTools;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ProtectionTools> ObservableProtectionTools {
			get {
				if(observableProtectionTools == null)
					observableProtectionTools = new GenericObservableList<ProtectionTools>(ProtectionTools);
				return observableProtectionTools;
			}
		}

		#endregion

		public Nomenclature ()
		{
			
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext) {
			if (Type != null && Type.WearCategory != null && Sex.HasValue 
				&& Sex == ClothesSex.Universal && SizeHelper.HasСlothesSizeStd(Type.WearCategory.Value) && !SizeHelper.IsUniversalСlothes (Type.WearCategory.Value))
				yield return new ValidationResult ("Данный вид одежды не имеет универсальных размеров.", 
					new[] { this.GetPropertyName (o => o.Sex) });

			if(Type != null && Type.WearCategory != null && Type.WearCategory != СlothesType.PPE && String.IsNullOrWhiteSpace(SizeStd))
				yield return new ValidationResult("Необходимо указать стандарт размера спецодежды.",
					new[] { this.GetPropertyName(o => o.SizeStd) });
			
			var baseParameters = (BaseParameters)validationContext.Items[nameof(BaseParameters)];
			if (Archival && baseParameters.CheckBalances) {
				var repository = new StockRepository();
				var nomenclatures = new List<Nomenclature>() {this};
				var uow = (IUnitOfWork)validationContext.Items[nameof(IUnitOfWork)];
				var warehouses = uow.Query<Warehouse>().List();
				foreach (var warehouse in warehouses) {
					var anyBalance = repository.StockBalances(uow, warehouse, nomenclatures, DateTime.Now)
						.Where(x => x.Amount > 0);
					foreach (var position in anyBalance) {
						yield return new ValidationResult("Архивная номенклатура не должна иметь остатков на складе"+
						                                  $" склад {warehouse.Name} содержит {Name} в кол-ве {position.Amount} шт.");
					}
				}
			}
		}
		#endregion

		#region Функции
		public virtual bool MatchingEmployeeSex(Sex employeeSex)
		{
			if(Sex == null)
				return true;

			switch(employeeSex) {
				case Workwear.Domain.Company.Sex.F:
					return Sex == ClothesSex.Women || Sex == ClothesSex.Universal;
				case Workwear.Domain.Company.Sex.M:
					return Sex == ClothesSex.Men || Sex == ClothesSex.Universal;
				default:
					return false;
			}
		}
		#endregion
	}
}

