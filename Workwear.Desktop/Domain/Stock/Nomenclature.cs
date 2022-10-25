using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Measurements;
using Workwear.Repository.Stock;
using Workwear.Tools;

namespace Workwear.Domain.Stock {
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

		private string name;
		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название номенклатуры должно быть заполнено.")]
		[StringLength(240)]
		public virtual string Name {
			get => name;
			set => SetField (ref name, value?.Trim());
		}

		private ItemsType type;
		[Display (Name = "Группа номенклатур")]
		[Required (ErrorMessage = "Номенклатурная группа должна быть указана.")]
		public virtual ItemsType Type {
			get => type;
			set => SetField (ref type, value);
		}
		private ClothesSex? sex;
		[Display (Name = "Пол одежды")]
		public virtual ClothesSex? Sex {
			get => sex;
			set => SetField (ref sex, value);
		}
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		private string number;
		[Display(Name = "Номенклатурный номер")]
		[StringLength(20)]
		public virtual string Number {
			get => number;
			set => SetField(ref number, value);
		}
		private bool archival;
		[Display(Name ="Архивная")]
		public virtual bool Archival {
			get => archival;
			set => SetField(ref archival, value);
		}
		
		private decimal? saleCost;
		[Display(Name = "Цена Продажи")]
		public virtual decimal? SaleCost {
			get => saleCost;
			set => SetField(ref saleCost, value);
		}
		
		private float? rating;
		[Display(Name ="Средняя оценка")]
		public virtual float? Rating {
			get => rating;
			set => SetField(ref rating, value);
		}

		private int? ratingCount;

		[Display(Name = "Количество оценок")]
		public virtual int? RatingCount {
			get => ratingCount;
			set => SetField(ref ratingCount, value);
		}

		private bool useBarcode;
		[Display(Name ="Использовать штрихкод")]
		public virtual bool UseBarcode {
			get => useBarcode;
			set => SetField(ref useBarcode, value);
		}
		
		#endregion
		#region Рассчетные
		public virtual string TypeName => Type.Name;
		public virtual string GetAmountAndUnitsText(int amount) => 
			Type?.Units?.MakeAmountShortStr(amount) ?? amount.ToString();
		#endregion
		#region Средства защиты
		private IList<ProtectionTools> protectionTools = new List<ProtectionTools>();
		[Display(Name = "Номенклатура нормы")]
		public virtual IList<ProtectionTools> ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}
		private GenericObservableList<ProtectionTools> observableProtectionTools;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ProtectionTools> ObservableProtectionTools =>
			observableProtectionTools ?? (observableProtectionTools =
				new GenericObservableList<ProtectionTools>(ProtectionTools));
		#endregion
		public Nomenclature () { }
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext) {
			var baseParameters = (BaseParameters)validationContext.Items[nameof(BaseParameters)];
			if (Archival && baseParameters.CheckBalances) {
				var repository = new StockRepository();
				var nomenclatures = new List<Nomenclature>() {this};
				var uow = (IUnitOfWork) validationContext.Items[nameof(IUnitOfWork)];
				var warehouses = uow.Query<Warehouse>().List();
				foreach (var warehouse in warehouses) {
					var anyBalance = repository.StockBalances(uow, warehouse, nomenclatures, DateTime.Now)
						.Where(x => x.Amount > 0);
					foreach (var position in anyBalance) {
						yield return new ValidationResult(
							"Архивная номенклатура не должна иметь остатков на складе" +
							$" склад {warehouse.Name} содержит {position.StockPosition.Title} в кол-ве {position.Amount} шт.");
					}
				}
			}
		}
		#endregion
		#region Функции
		public virtual bool MatchingEmployeeSex(Sex employeeSex) {
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

