using System;
using System.Collections.Generic;
using System.Linq;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using workwear.Domain.Stock;
using Workwear.Measurements;

namespace workwear.Models.Import
{
	public class NomenclatureTypes
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public List<ItemsType> ItemsTypes = new List<ItemsType>();
		public Dictionary<string, TypeDescription> KeyWords = new Dictionary<string, TypeDescription>();
		private readonly SizeService sizeService;
		private readonly IUnitOfWork uow;

		public NomenclatureTypes(IUnitOfWork uow, SizeService sizeService, bool tryLoad = false) {
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			if(tryLoad)
				ItemsTypes = uow.GetAll<ItemsType>().ToList();
			makeTypes(uow);
		}

		#region Создание типов
		private void makeTypes(IUnitOfWork uow) {
			var units = uow.GetAll<MeasurementUnits>();
			var sht = units.First(x => x.OKEI == "796");
			var pair = units.First(x => x.OKEI == "715");

			AddType("Халаты", СlothesType.Wear, sht, new[] { "ХАЛАТ" });
			AddType("Костюмы", СlothesType.Wear, sht, new[] { "КОСТЮМ", "ГИДРОКОСТЮМ" });
			AddType("Куртки", СlothesType.Wear, sht, new[] { "КУРТКА" });
			AddType("Брюки", СlothesType.Wear, sht, new[] { "БРЮКИ" });
			AddType("Жилеты", СlothesType.Wear, sht, new[] { "ЖИЛЕТ" });
			AddType("Футболки", СlothesType.Wear, sht, new[] { "ФУТБОЛКА" });
			AddType("Блузки", СlothesType.Wear, sht, new[] { "БЛУЗКА" });
			AddType("Плащи", СlothesType.Wear, sht, new[] { "ПЛАЩ" });
			AddType("Белье", СlothesType.Wear, sht, new[] { "БЕЛЬЕ", "КАЛЬСОНЫ" });
			AddType("Рубашки", СlothesType.Wear, sht, new[] { "СОРОЧКА", "Рубашка" });
			AddType("Фуфайки", СlothesType.Wear, sht, new[] { "Фуфайка" });
			AddType("Свитеры", СlothesType.Wear, sht, new[] { "СВИТЕР" });
			AddType("Комбинезоны", СlothesType.Wear, sht, new[] { "КОМБИНЕЗОН", "ПОЛУКОМБИНЕЗОН", "ГИДРОКОМБИНЕЗОН" });
			AddType("Комплекты", СlothesType.Wear, sht, new[] { "КОМПЛЕКТ" });
			AddType("Подшлемники", СlothesType.Headgear, sht, new[] { "ПОДШЛЕМНИК" });
			AddType("Головные уборы", СlothesType.Headgear, sht, new[] { "ШАПКА", "Кепка", "ФЕСКА", "ШЛЯПА", "ФУРАЖКА", "БЕЙСБОЛКА", "БЕРЕТ", "КОЛПАК", "КЕПИ", "ПИЛОТКА", "Головной" });
			AddType("Каски", СlothesType.Headgear, sht, new[] { "КАСКА",  });
			AddType("Полуботинки", СlothesType.Shoes, pair, new[] { "ПОЛУБОТИНКИ" });
			AddType("Сапоги", СlothesType.Shoes, pair, new[] { "САПОГИ", "ПОЛУСАПОГИ" });
			AddType("Ботинки", СlothesType.Shoes, pair, new[] { "БОТИНКИ", "ЧУВЯКИ", "боты" });
			AddType("Туфли", СlothesType.Shoes, pair, new[] { "Туфли" });
			AddType("Тапочки", СlothesType.Shoes, pair, new[] { "Тапки,", "Тапочки" });
			AddType("Сандалии", СlothesType.Shoes, pair, new[] { "Сабо", "Сандалии", "Сандали" });
			AddType("Обувь", СlothesType.Shoes, pair, new[] { "Обувь" });
			AddType("Валенки", СlothesType.WinterShoes, pair, new[] { "ВАЛЕНКИ" });
			AddType("Галоши", СlothesType.WinterShoes, pair, new[] { "ГАЛОШИ" });
			AddType("Перчатки", СlothesType.Gloves, pair, new[] { "ПЕРЧАТКИ", "КРАГИ", "перчаточные" });
			AddType("Рукавицы", СlothesType.Mittens, pair, new[] { "РУКАВИЦЫ", "Вачеги" });
			AddType("Аксесуары", СlothesType.PPE, sht, new[] { "ШАРФ", "ГАЛСТУК", "ШЕВРОН", "РЕМЕНЬ", "ЗНАЧОК", "Кокарда", "БЕЙДЖ", "ПОЯС", "Наколка" });
			AddType("Водолазный инвентарь", СlothesType.PPE, sht, new[] { "ВОДОЛАЗНАЯ", "ПОДВОДНАЯ", "ГРУЗ", "ВОДОЛАЗНЫЙ", "ШЛАНГ", "СПИНКА", "БАЛЛОН" });
			AddType("Носки", СlothesType.PPE, pair, new[] { "НОСКИ" });
			AddType("Чулки", СlothesType.PPE, pair, new[] { "ЧУЛКИ" });
			AddType("Портянки", СlothesType.PPE, pair, new[] { "ПОРТЯНКИ" });
			AddType("Фартуки", СlothesType.PPE, sht, new[] { "ФАРТУК" });
			AddType("Косынки", СlothesType.PPE, sht, new[] { "КОСЫНКА" });
			AddType("Повязка", СlothesType.PPE, sht, new[] { "ПОВЯЗКА" });
			AddType("Привязи", СlothesType.PPE, sht, new[] { "привязь" });
			AddType("Полотенца", СlothesType.PPE, sht, new[] { "Полотенце" });
			AddType("Щитки", СlothesType.PPE, sht, new[] { "ЩИТОК" });
			AddType("СИЗОД", СlothesType.PPE, sht, new[] { "СИЗОД", "Респиратор", "РЕСПИРАТОРЫ", "РЕСПИРАТОРА", "ПАТРОН", "ПРЕДФИЛЬТР", "КОРОБКА", "Капюшон", "дыхания", "Противогаз" });
			AddType("Беруши", СlothesType.PPE, pair, new[] { "Беруши", "противошумные" });
			AddType("Очки", СlothesType.PPE, sht, new[] { "ОЧКИ" });
			AddType("Наушники", СlothesType.PPE, sht, new[] { "НАУШНИКИ" });
			AddType("Маски", СlothesType.PPE, sht, new[] { "МАСКА", "ШЛЕМ", "МАСКЕ", "СТЕКЛА", "Полумаска", "накомарник" });
			AddType("Аптечки", СlothesType.PPE, sht, new[] { "АПТЕЧКА" });
			AddType("Моющее средства", СlothesType.PPE, sht, new[] { "Моющее", "очищающая" });
			AddType("Крема", СlothesType.PPE, sht, new[] { "крем", "МАЗЬ", "Паста" });
			AddType("Защита", СlothesType.PPE, pair, new[] { "НАРУКАВНИКИ", "Наколенники" });
			AddType("Защита от поражения электрическим током", СlothesType.PPE, sht, new[] { "Диэлектрический" });
			AddType("Имущество", sht, new[] { "ФИЛЬТР", "Фильтра", "ЗНАК", "ЭТИКЕТКА", "РЕДУКТОР", "УТЕПЛИТЕЛЬ", "ЛЕНТА", "ШТОРЫ", 
				"ФЛАГ", "ЧЕХОЛ", "РУКАВ", "ПЛОМБА", "КОЖА", "ПОЛОГ", "СУКНО" });
			AddType("Инструмент", sht, new[] { "НОЖ", "НОЖНИЦЫ", "СИГНАЛЬНЫЙ", "МАНОМЕТР", "Щетка" });
			AddType("Неизвестный тип", sht, new string[] { });
		}
		private void AddType(string name, СlothesType category, MeasurementUnits units, string[] keyWords, СlothesType? category2 = null, string[] keywords2 = null)
		{
			var type = ItemsTypes.FirstOrDefault(x => x.Name == name);
			if(type == null) {
				type = new ItemsType {
					Name = name,
					Category = ItemTypeCategory.wear,
					WearCategory = category,
					Units = units,
				};
				switch (category) {
					case СlothesType.Wear:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер одежды");
						type.HeightType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Рост");
						break;
					case СlothesType.Shoes:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер обуви");
						break;
					case СlothesType.WinterShoes:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер зимней обуви");
						break;
					case СlothesType.Headgear:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер головного убора");
						break;
					case СlothesType.Gloves:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер перчаток");
						break;
					case СlothesType.Mittens:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер рукавиц");
						break;
				}
				ItemsTypes.Add(type);
			}
			TypeDescription desc;
			if(category2 != null) {
				var type2 = new ItemsType {
					Name = name,
					Category = ItemTypeCategory.wear,
					WearCategory = category2.Value,
					Units = units
				};
				switch (category) {
					case СlothesType.Wear:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер одежды");
						type2.HeightType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Рост");
						break;
					case СlothesType.Shoes:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер обуви");
						break;
					case СlothesType.WinterShoes:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер зимней обуви");
						break;
					case СlothesType.Headgear:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер головного убора");
						break;
					case СlothesType.Gloves:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер перчаток");
						break;
					case СlothesType.Mittens:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер рукавиц");
						break;
				}
				desc = new TypeDescription(type, type2, keywords2.Select(x => x.ToLower()).ToArray());
			}
			else
				desc = new TypeDescription(type); 

			foreach(var word in keyWords) {
				KeyWords.Add(word.ToLower(), desc);
			}
		}

		private void AddType(string name, MeasurementUnits units, string[] keyWords)
		{
			var type = ItemsTypes.FirstOrDefault(x => x.Name == name);
			if(type == null) {
				type = new ItemsType {
					Name = name,
					Category = ItemTypeCategory.property,
					Units = units
				};
				ItemsTypes.Add(type);
			}
			TypeDescription desc = new TypeDescription(type);

			foreach(var word in keyWords) {
				KeyWords.Add(word.ToLower(), desc);
			}
		}
		#endregion
		public ItemsType ParseNomenclatureName(string name) {
			var parts = name.ToLower().Split(' ', '-', '_', '~', '"', '(', ')', '.', ',');
			foreach(var word in parts) {
				if (!KeyWords.TryGetValue(word, out var desc)) continue;
				if (desc.keyWords2 == null) return desc.ItemsType;
				return parts.Any(word2 => desc.keyWords2.Contains(word2)) ? 
					desc.ItemsType2 : desc.ItemsType;
			}
			logger.Warn($"Не найдена категория для [{name}]");
			return null;
		}

		private Dictionary<string, ClothesSex> SexKeywords = new Dictionary<string, ClothesSex> {
			{"жен.", ClothesSex.Women},
			{"жен", ClothesSex.Women},
			{"женские", ClothesSex.Women},
			{"муж.", ClothesSex.Men},
			{"муж", ClothesSex.Men},
			{"мужские", ClothesSex.Men},
			{"мужской", ClothesSex.Men},
			{"мужская", ClothesSex.Men},
			{"универсальный", ClothesSex.Universal},
			{"универсальн.", ClothesSex.Universal},
			{"универс.", ClothesSex.Universal},
			{"универсальная", ClothesSex.Universal},
		};

		public ClothesSex? ParseSex(string name) {
			var parts = name.ToLower().Split(' ', '-');
			foreach(var word in parts) {
				if(SexKeywords.TryGetValue(word, out var sex))
					return sex;
			}
			return null;
		}
		#region Получение специфичных типов.
		public ItemsType GetUnknownType() {
			return ItemsTypes.First(x => x.Name == "Неизвестный тип");
		}
		#endregion
	}

	
	public class TypeDescription
	{
		public ItemsType ItemsType;

		public ItemsType ItemsType2;
		public string[] keyWords2;

		public TypeDescription(ItemsType itemsType)
		{
			ItemsType = itemsType;
		}

		public TypeDescription(ItemsType itemsType, ItemsType itemsType2, string[] keyWords2) : this(itemsType)
		{
			ItemsType2 = itemsType2;
			this.keyWords2 = keyWords2;
		}
	}
}
