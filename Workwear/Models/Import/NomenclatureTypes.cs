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

		public MeasurementUnits KitUnits;

		public NomenclatureTypes(IUnitOfWork uow, bool tryLoad = false)
		{
			if(tryLoad)
				ItemsTypes = uow.GetAll<ItemsType>().ToList();
			makeTypes(uow);
		}

		#region Создание типов
		private void makeTypes(IUnitOfWork uow)
		{
			var units = uow.GetAll<MeasurementUnits>();
			var sht = units.First(x => x.OKEI == "796");
			var pair = units.First(x => x.OKEI == "715");
			KitUnits = units.First(x => x.OKEI == "839");

			AddType("Халаты", СlothesType.Wear, sht, new string[] { "ХАЛАТ" });
			AddType("Костюмы", СlothesType.Wear, sht, new string[] { "КОСТЮМ", "ГИДРОКОСТЮМ" });
			AddType("Куртки", СlothesType.Wear, sht, new string[] { "КУРТКА" });
			AddType("Брюки", СlothesType.Wear, sht, new string[] { "БРЮКИ" });
			AddType("Жилеты", СlothesType.Wear, sht, new string[] { "ЖИЛЕТ" });
			AddType("Футболки", СlothesType.Wear, sht, new string[] { "ФУТБОЛКА" });
			AddType("Блузки", СlothesType.Wear, sht, new string[] { "БЛУЗКА" });
			AddType("Плащи", СlothesType.Wear, sht, new string[] { "ПЛАЩ" });
			AddType("Белье", СlothesType.Wear, sht, new string[] { "БЕЛЬЕ", "КАЛЬСОНЫ" });
			AddType("Рубашки", СlothesType.Wear, sht, new string[] { "СОРОЧКА", "Рубашка" });
			AddType("Фуфайки", СlothesType.Wear, sht, new string[] { "Фуфайка" });
			AddType("Свитеры", СlothesType.Wear, sht, new string[] { "СВИТЕР" });
			AddType("Комбинезоны", СlothesType.Wear, sht, new string[] { "КОМБИНЕЗОН", "ПОЛУКОМБИНЕЗОН", "ГИДРОКОМБИНЕЗОН" });
			AddType("Комплекты", СlothesType.Wear, sht, new string[] { "КОМПЛЕКТ" });
			AddType("Подшлемники", СlothesType.Headgear, sht, new string[] { "ПОДШЛЕМНИК" });
			AddType("Головные уборы", СlothesType.Headgear, sht, new string[] { "ШАПКА", "Кепка", "ФЕСКА", "ШЛЯПА", "ФУРАЖКА", "БЕЙСБОЛКА", "БЕРЕТ", "КОЛПАК", "КЕПИ", "ПИЛОТКА", "Головной" });
			AddType("Каски", СlothesType.Headgear, sht, new string[] { "КАСКА",  });
			AddType("Полуботинки", СlothesType.Shoes, pair, new string[] { "ПОЛУБОТИНКИ" });
			AddType("Сапоги", СlothesType.Shoes, pair, new string[] { "САПОГИ", "ПОЛУСАПОГИ" });
			AddType("Ботинки", СlothesType.Shoes, pair, new string[] { "БОТИНКИ", "ЧУВЯКИ", "боты" });
			AddType("Туфли", СlothesType.Shoes, pair, new string[] { "Туфли" });
			AddType("Тапочки", СlothesType.Shoes, pair, new string[] { "Тапки,", "Тапочки" });
			AddType("Сандалии", СlothesType.Shoes, pair, new string[] { "Сабо", "Сандалии", "Сандали" });
			AddType("Валенки", СlothesType.WinterShoes, pair, new string[] { "ВАЛЕНКИ" });
			AddType("Галоши", СlothesType.WinterShoes, pair, new string[] { "ГАЛОШИ" });
			AddType("Перчатки", СlothesType.Gloves, pair, new string[] { "ПЕРЧАТКИ", "КРАГИ" });
			AddType("Рукавицы", СlothesType.Mittens, pair, new string[] { "РУКАВИЦЫ", "Вачеги" });
			AddType("Аксесуары", СlothesType.PPE, sht, new string[] { "ШАРФ", "ГАЛСТУК", "ШЕВРОН", "РЕМЕНЬ", "ЗНАЧОК", "Кокарда", "БЕЙДЖ", "ПОЯС", "Наколка" });
			AddType("Водолазный инвентарь", СlothesType.PPE, sht, new string[] { "ВОДОЛАЗНАЯ", "ПОДВОДНАЯ", "ГРУЗ", "ВОДОЛАЗНЫЙ", "ШЛАНГ", "СПИНКА", "БАЛЛОН" });
			AddType("Носки", СlothesType.PPE, pair, new string[] { "НОСКИ" });
			AddType("Чулки", СlothesType.PPE, pair, new string[] { "ЧУЛКИ" });
			AddType("Портянки", СlothesType.PPE, pair, new string[] { "ПОРТЯНКИ" });
			AddType("Фартуки", СlothesType.PPE, sht, new string[] { "ФАРТУК" });
			AddType("Косынки", СlothesType.PPE, sht, new string[] { "КОСЫНКА" });
			AddType("Повязка", СlothesType.PPE, sht, new string[] { "ПОВЯЗКА" });
			AddType("Привязи", СlothesType.PPE, sht, new string[] { "привязь" });
			AddType("Полотенца", СlothesType.PPE, sht, new string[] { "Полотенце" });
			AddType("Щитки", СlothesType.PPE, sht, new string[] { "ЩИТОК" });
			AddType("СИЗОД", СlothesType.PPE, sht, new string[] { "СИЗОД", "Респиратор", "РЕСПИРАТОРЫ", "РЕСПИРАТОРА", "ПАТРОН", "ПРЕДФИЛЬТР", "КОРОБКА", "Капюшон" });
			AddType("Беруши", СlothesType.PPE, pair, new string[] { "Беруши", "противошумные" });
			AddType("Очки", СlothesType.PPE, sht, new string[] { "ОЧКИ" });
			AddType("Наушники", СlothesType.PPE, sht, new string[] { "НАУШНИКИ" });
			AddType("Маски", СlothesType.PPE, sht, new string[] { "МАСКА", "ШЛЕМ", "МАСКЕ", "СТЕКЛА", "Полумаска" });
			AddType("Аптечки", СlothesType.PPE, sht, new string[] { "АПТЕЧКА" });
			AddType("Моющее средства", СlothesType.PPE, sht, new string[] { "Моющее", "очищающая" });
			AddType("Крема", СlothesType.PPE, sht, new string[] { "крем", "МАЗЬ", "Паста" });
			AddType("Защита", СlothesType.PPE, pair, new string[] { "НАРУКАВНИКИ", "Наколенники" });
			AddType("Имущество", sht, new string[] { "ФИЛЬТР", "ЗНАК", "ФИЛЬТРДЛЯ", "ЭТИКЕТКА", "РЕДУКТОР", "УТЕПЛИТЕЛЬ", "ЛЕНТА", "ШТОРЫ", 
				"ФЛАГ", "ЧЕХОЛ", "РУКАВ", "ПЛОМБА", "КОЖА", "ПОЛОГ", "СУКНО" });
			AddType("Инструмент", sht, new string[] { "НОЖ", "НОЖНИЦЫ", "СИГНАЛЬНЫЙ", "МАНОМЕТР", "Щетка" });
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
					Units = units
				};
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

		public ItemsType ParseNomenclatureName(string name)
		{
			var parts = name.ToLower().Split(' ', '-', '_', '~', '"', '(', ')');
			foreach(var word in parts) {
				if(KeyWords.TryGetValue(word, out TypeDescription desc)) {
					if(desc.keyWords2 != null) {
						foreach(var word2 in parts) {
							if(desc.keyWords2.Contains(word2))
								return desc.ItemsType2;
						}
					}
					return desc.ItemsType;
				}
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

		public ClothesSex? ParseSex(string name)
		{
			var parts = name.ToLower().Split(' ', '-');
			foreach(var word in parts) {
				if(SexKeywords.TryGetValue(word, out ClothesSex sex))
					return sex;
			}
			return null;
		}

		#region Получение специфичных типов.

		public ItemsType GetUnknownType()
		{
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
