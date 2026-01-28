using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Repository.Stock;
using Workwear.Tools;

namespace Workwear.Domain.Stock {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "номенклатура",
		Nominative = "номенклатура",
		Genitive = "номенклатуры",
		GenitivePlural = "номенклатур"
		)]
	[HistoryTrace]
	public class Nomenclature: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
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
		private ClothesSex sex;
		[Display (Name = "Пол одежды")]
		public virtual ClothesSex Sex {
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
			set => SetField(ref number, value?.Trim());
		}

		private string additionalInfo;
		[Display(Name = "Модель, марка, артикул, класс защиты СИЗ, дерматологических СИЗ")]
		public virtual string AdditionalInfo {
			get => additionalInfo;
			set => SetField(ref additionalInfo, value);
		}
		private string catalogId;
		[Display(Name = "ID каталога")]
		[StringLength(24, ErrorMessage = "Максимальная длинна идентификатора в каталоге - 24 символа.")]
		public virtual string CatalogId {
			get => catalogId;
			set => SetField(ref catalogId, value);
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
		[Display(Name ="Использовать маркировку")]
		public virtual bool UseBarcode {
			get => useBarcode;
			set => SetField(ref useBarcode, value);
		}
		
		private bool washable;
		[Display(Name ="Можно стирать")]
		public virtual bool Washable {
			get => washable;
			set => SetField(ref washable, value);
		}
		#endregion
		#region Рассчетные
		public virtual string TypeName => Type.Name;
		public virtual string GetAmountAndUnitsText(int amount) => 
			Type?.Units?.MakeAmountShortStr(amount) ?? amount.ToString();
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
			if(Sex == ClothesSex.Universal)
				return true;
			switch(employeeSex) {
				case Workwear.Domain.Company.Sex.F:
					return Sex == ClothesSex.Women;
				case Workwear.Domain.Company.Sex.M:
					return Sex == ClothesSex.Men;
				default:
					return false;
			}
		}
		
		public virtual void CopyFrom(Nomenclature nomenclature) {
			Name = nomenclature.Name;
			Type = nomenclature.Type;
			Number = nomenclature.Number;
			Sex = nomenclature.Sex;
			Comment = nomenclature.Comment;
			Archival = nomenclature.Archival;
			SaleCost = nomenclature.SaleCost;
			UseBarcode = nomenclature.UseBarcode;

			foreach(var pt in nomenclature.ProtectionTools) 
				ProtectionTools.Add(pt);
		}
		#endregion
		
		#region ProtectionTools
		private IObservableList<ProtectionTools> protectionTools = new ObservableList<ProtectionTools>();
		[Display(Name = "Номенклатура нормы")]
		public virtual IObservableList<ProtectionTools> ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}

		public virtual void AddProtectionTools(ProtectionTools protectionTools)
		{
			if(ProtectionTools.Any(p => DomainHelper.EqualDomainObjects(p, protectionTools))) {
				logger.Warn("Номеклатура нормы уже добавлена. Пропускаем...");
				return;
			}
			ProtectionTools.Add(protectionTools);
		}

		public virtual void RemoveProtectionTools(ProtectionTools protectionTools)
		{
			ProtectionTools.Remove(protectionTools);
		}
		#endregion
		
		#region Обслуживание одежды
		private IObservableList<Service> useServices = new ObservableList<Service>();
		[Display(Name = "Услуги обслуживания")]
		public virtual IObservableList<Service> UseServices {
			get => useServices;
			set => SetField(ref useServices, value);
		}

		public virtual void AddService(Service service) {
			if(UseServices.Any(p => DomainHelper.EqualDomainObjects(p, service))) {
				logger.Warn("Услуга уже добавлен. Пропускаем...");
				return;
			}
			UseServices.Add(service);
		}

		public virtual void RemoveService(Service service) => UseServices.Remove(service);

		#endregion

	}
}

