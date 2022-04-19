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

		public NomenclatureTypes(IUnitOfWork uow, bool tryLoad = false) {
			if(tryLoad)
				ItemsTypes = uow.GetAll<ItemsType>().ToList();
			makeTypes(uow);
		}

		#region Создание типов
		private void makeTypes(IUnitOfWork uow) {
			var units = uow.GetAll<MeasurementUnits>();
			var sht = units.First(x => x.OKEI == "796");
			var pair = units.First(x => x.OKEI == "715");

			AddType("Халаты", sht, new[] { "ХАЛАТ" });
			AddType("Костюмы", sht, new[] { "КОСТЮМ", "ГИДРОКОСТЮМ" });
			AddType("Куртки", sht, new[] { "КУРТКА" });
			AddType("Брюки", sht, new[] { "БРЮКИ" });
			AddType("Жилеты", sht, new[] { "ЖИЛЕТ" });
			AddType("Футболки", sht, new[] { "ФУТБОЛКА" });
			AddType("Блузки", sht, new[] { "БЛУЗКА" });
			AddType("Плащи",  sht, new[] { "ПЛАЩ" });
			AddType("Белье",  sht, new[] { "БЕЛЬЕ", "КАЛЬСОНЫ" });
			AddType("Рубашки",  sht, new[] { "СОРОЧКА", "Рубашка" });
			AddType("Фуфайки",  sht, new[] { "Фуфайка" });
			AddType("Свитеры",  sht, new[] { "СВИТЕР" });
			AddType("Комбинезоны",  sht, new[] { "КОМБИНЕЗОН", "ПОЛУКОМБИНЕЗОН", "ГИДРОКОМБИНЕЗОН" });
			AddType("Комплекты",  sht, new[] { "КОМПЛЕКТ" });
			AddType("Подшлемники",  sht, new[] { "ПОДШЛЕМНИК" });
			AddType("Головные уборы",  sht, new[] 
				{ "ШАПКА", "Кепка", "ФЕСКА", "ШЛЯПА", "ФУРАЖКА", "БЕЙСБОЛКА", "БЕРЕТ", "КОЛПАК", "КЕПИ", "ПИЛОТКА", "Головной" });
			AddType("Каски",  sht, new[] { "КАСКА",  });
			AddType("Полуботинки",  pair, new[] { "ПОЛУБОТИНКИ" });
			AddType("Сапоги",  pair, new[] { "САПОГИ", "ПОЛУСАПОГИ" });
			AddType("Ботинки",  pair, new[] { "БОТИНКИ", "ЧУВЯКИ", "боты" });
			AddType("Туфли",  pair, new[] { "Туфли" });
			AddType("Тапочки", pair, new[] { "Тапки,", "Тапочки" });
			AddType("Сандалии", pair, new[] { "Сабо", "Сандалии", "Сандали" });
			AddType("Валенки",  pair, new[] { "ВАЛЕНКИ" });
			AddType("Галоши", pair, new[] { "ГАЛОШИ" });
			AddType("Перчатки", pair, new[] { "ПЕРЧАТКИ", "КРАГИ" });
			AddType("Рукавицы", pair, new[] { "РУКАВИЦЫ", "Вачеги" });
			AddType("Аксесуары", sht, new[] 
				{ "ШАРФ", "ГАЛСТУК", "ШЕВРОН", "РЕМЕНЬ", "ЗНАЧОК", "Кокарда", "БЕЙДЖ", "ПОЯС", "Наколка" });
			AddType("Водолазный инвентарь", sht, new[] 
				{ "ВОДОЛАЗНАЯ", "ПОДВОДНАЯ", "ГРУЗ", "ВОДОЛАЗНЫЙ", "ШЛАНГ", "СПИНКА", "БАЛЛОН" });
			AddType("Носки", pair, new[] { "НОСКИ" });
			AddType("Чулки", pair, new[] { "ЧУЛКИ" });
			AddType("Портянки", pair, new[] { "ПОРТЯНКИ" });
			AddType("Фартуки", sht, new[] { "ФАРТУК" });
			AddType("Косынки", sht, new[] { "КОСЫНКА" });
			AddType("Повязка", sht, new[] { "ПОВЯЗКА" });
			AddType("Привязи", sht, new[] { "привязь" });
			AddType("Полотенца", sht, new[] { "Полотенце" });
			AddType("Щитки", sht, new[] { "ЩИТОК" });
			AddType("СИЗОД",  sht, new[] 
				{ "СИЗОД", "Респиратор", "РЕСПИРАТОРЫ", "РЕСПИРАТОРА", "ПАТРОН", "ПРЕДФИЛЬТР", "КОРОБКА", "Капюшон" });
			AddType("Беруши",  pair, new[] { "Беруши", "противошумные" });
			AddType("Очки", sht, new[] { "ОЧКИ" });
			AddType("Наушники", sht, new[] { "НАУШНИКИ" });
			AddType("Маски", sht, new[] { "МАСКА", "ШЛЕМ", "МАСКЕ", "СТЕКЛА", "Полумаска" });
			AddType("Аптечки",sht, new[] { "АПТЕЧКА" });
			AddType("Моющее средства", sht, new[] { "Моющее", "очищающая" });
			AddType("Крема",  sht, new[] { "крем", "МАЗЬ", "Паста" });
			AddType("Защита", pair, new[] { "НАРУКАВНИКИ", "Наколенники" });
			AddType("Имущество", sht, new[] 
			{ "ФИЛЬТР", "ЗНАК", "ФИЛЬТРДЛЯ", "ЭТИКЕТКА", "РЕДУКТОР", "УТЕПЛИТЕЛЬ", "ЛЕНТА", "ШТОРЫ", 
				"ФЛАГ", "ЧЕХОЛ", "РУКАВ", "ПЛОМБА", "КОЖА", "ПОЛОГ", "СУКНО" });
			AddType("Инструмент", sht, new[] { "НОЖ", "НОЖНИЦЫ", "СИГНАЛЬНЫЙ", "МАНОМЕТР", "Щетка" });
			AddType("Неизвестный тип", sht, new string[] { });
		}
		private void AddType(string name, MeasurementUnits units, string[] keyWords) {
			var type = ItemsTypes.FirstOrDefault(x => x.Name == name);
			if(type == null) {
				type = new ItemsType {
					Name = name,
					Category = ItemTypeCategory.property,
					Units = units
				};
				ItemsTypes.Add(type);
			}
			var desc = new TypeDescription(type);

			foreach(var word in keyWords) {
				KeyWords.Add(word.ToLower(), desc);
			}
		}
		#endregion
		public ItemsType ParseNomenclatureName(string name) {
			var parts = name.ToLower().Split(' ', '-', '_', '~', '"', '(', ')');
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

	public class TypeDescription {
		public ItemsType ItemsType;
		public ItemsType ItemsType2;
		public string[] keyWords2;
		public TypeDescription(ItemsType itemsType) {
			ItemsType = itemsType;
		}
	}
}
