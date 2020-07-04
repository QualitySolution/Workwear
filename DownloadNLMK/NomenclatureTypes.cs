using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using workwear.Domain.Stock;
using workwear.Measurements;

namespace DownloadNLMK
{
	public class NomenclatureTypes
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public List<ItemsType> ItemsTypes = new List<ItemsType>();
		public Dictionary<string, TypeDescription> KeyWords = new Dictionary<string, TypeDescription>();

		public MeasurementUnits KitUnits;

		public NomenclatureTypes(IUnitOfWork uow)
		{
			makeTypes(uow);
		}

		#region Создание типов
		private void makeTypes(IUnitOfWork uow)
		{
			var units = uow.GetAll<MeasurementUnits>();
			var sht = units.First(x => x.OKEI == "796");
			var pair = units.First(x => x.OKEI == "715");
			KitUnits = units.First(x => x.OKEI == "839");

			AddType("ХАЛАТ", СlothesType.Wear, sht, new string[] { "ХАЛАТ" });
			AddType("КОСТЮМ", СlothesType.Wear, sht, new string[] { "КОСТЮМ", "ГИДРОКОСТЮМ" });
			AddType("КУРТКА", СlothesType.Wear, sht, new string[] { "КУРТКА" });
			AddType("БРЮКИ", СlothesType.Wear, sht, new string[] { "БРЮКИ" });
			AddType("ЖИЛЕТ", СlothesType.Wear, sht, new string[] { "ЖИЛЕТ" });
			AddType("ФУТБОЛКА", СlothesType.Wear, sht, new string[] { "ФУТБОЛКА" });
			AddType("БЛУЗКА", СlothesType.Wear, sht, new string[] { "БЛУЗКА" });
			AddType("ПЛАЩ", СlothesType.Wear, sht, new string[] { "ПЛАЩ" });
			AddType("БЕЛЬЕ", СlothesType.Wear, sht, new string[] { "БЕЛЬЕ", "КАЛЬСОНЫ" });
			AddType("СОРОЧКА", СlothesType.Wear, sht, new string[] { "СОРОЧКА", "Рубашка" });
			AddType("Фуфайка", СlothesType.Wear, sht, new string[] { "Фуфайка" });
			AddType("СВИТЕР", СlothesType.Wear, sht, new string[] { "СВИТЕР" });
			AddType("КОМБИНЕЗОН", СlothesType.Wear, sht, new string[] { "КОМБИНЕЗОН", "ПОЛУКОМБИНЕЗОН", "ГИДРОКОМБИНЕЗОН" });
			AddType("ПОДШЛЕМНИК", СlothesType.Headgear, sht, new string[] { "ПОДШЛЕМНИК" });
			AddType("ШАПКА", СlothesType.Headgear, sht, new string[] { "ШАПКА", "Кепка", "ФЕСКА", "ШЛЯПА", "ФУРАЖКА", "БЕЙСБОЛКА", "БЕРЕТ", "КОЛПАК", "КЕПИ" });
			AddType("ПИЛОТКА", СlothesType.Headgear, sht, new string[] { "ПИЛОТКА" });
			AddType("КАСКА", СlothesType.Headgear, sht, new string[] { "КАСКА",  });
			AddType("ПОЛУБОТИНКИ", СlothesType.Shoes, pair, new string[] { "ПОЛУБОТИНКИ" });
			AddType("САПОГИ", СlothesType.Shoes, pair, new string[] { "САПОГИ", "ПОЛУСАПОГИ" });
			AddType("БОТИНКИ", СlothesType.Shoes, pair, new string[] { "БОТИНКИ", "ЧУВЯКИ" });
			AddType("Туфли", СlothesType.Shoes, pair, new string[] { "Туфли" });
			AddType("Сабо", СlothesType.Shoes, pair, new string[] { "Сабо" });
			AddType("Тапочки", СlothesType.Shoes, pair, new string[] { "Тапочки" });
			AddType("ВАЛЕНКИ", СlothesType.WinterShoes, pair, new string[] { "ВАЛЕНКИ" });
			AddType("ГАЛОШИ", СlothesType.WinterShoes, pair, new string[] { "ГАЛОШИ" });
			AddType("ПЕРЧАТКИ", СlothesType.Gloves, pair, new string[] { "ПЕРЧАТКИ", "КРАГИ" });
			AddType("РУКАВИЦЫ", СlothesType.Gloves, pair, new string[] { "РУКАВИЦЫ" });
			AddType("АКСЕСУАРЫ", СlothesType.PPE, sht, new string[] { "ШАРФ", "ГАЛСТУК", "ШЕВРОН", "РЕМЕНЬ", "ЗНАЧОК", "Кокарда", "БЕЙДЖ", "ПОЯС", "Наколка" });
			AddType("ВОДОЛАЗНЫЙ ИНВЕНТАРЬ", СlothesType.PPE, sht, new string[] { "ВОДОЛАЗНАЯ", "ПОДВОДНАЯ", "ГРУЗ", "ВОДОЛАЗНЫЙ", "ШЛАНГ", "СПИНКА", "БАЛЛОН" });
			AddType("НОСКИ", СlothesType.PPE, pair, new string[] { "НОСКИ" });
			AddType("ЧУЛКИ", СlothesType.PPE, pair, new string[] { "ЧУЛКИ" });
			AddType("ПОРТЯНКИ", СlothesType.PPE, pair, new string[] { "ПОРТЯНКИ" });
			AddType("ФАРТУК", СlothesType.PPE, sht, new string[] { "ФАРТУК" });
			AddType("КОСЫНКА", СlothesType.PPE, sht, new string[] { "КОСЫНКА" });
			AddType("ПОВЯЗКА", СlothesType.PPE, sht, new string[] { "ПОВЯЗКА" });
			AddType("ЩИТОК", СlothesType.PPE, sht, new string[] { "ЩИТОК" });
			AddType("КОМПЛЕКТ", СlothesType.PPE, sht, new string[] { "КОМПЛЕКТ" });
			AddType("Респиратор", СlothesType.PPE, sht, new string[] { "Респиратор", "РЕСПИРАТОРЫ", "РЕСПИРАТОРА", "ПАТРОН", "ПРЕДФИЛЬТР", "КОРОБКА" });
			AddType("Беруши", СlothesType.PPE, pair, new string[] { "Беруши" });
			AddType("ОЧКИ", СlothesType.PPE, sht, new string[] { "ОЧКИ" });
			AddType("НАУШНИКИ", СlothesType.PPE, sht, new string[] { "НАУШНИКИ" });
			AddType("МАСКА", СlothesType.PPE, sht, new string[] { "МАСКА", "ШЛЕМ", "МАСКЕ", "СТЕКЛА" });
			AddType("АПТЕЧКА", СlothesType.PPE, sht, new string[] { "АПТЕЧКА" });
			AddType("ЗАЩИТА", СlothesType.PPE, pair, new string[] { "НАРУКАВНИКИ", "Наколенники" });
			AddType("ИМУЩЕСТВО", sht, new string[] { "ФИЛЬТР", "ЗНАК", "ФИЛЬТРДЛЯ", "ЭТИКЕТКА", "РЕДУКТОР", "УТЕПЛИТЕЛЬ", "ЛЕНТА", "ШТОРЫ", 
				"ФЛАГ", "ЧЕХОЛ", "РУКАВ", "ПЛОМБА", "КОЖА", "ПОЛОГ", "СУКНО" });
			AddType("ИНСТРУМЕНТ", sht, new string[] { "МАЗЬ", "НОЖ", "НОЖНИЦЫ", "СИГНАЛЬНЫЙ", "МАНОМЕТР" });
		}

		private void AddType(string name, СlothesType category, MeasurementUnits units, string[] keyWords, СlothesType? category2 = null, string[] keywords2 = null)
		{
			var type = new ItemsType {
				Name = name,
				Category = ItemTypeCategory.wear,
				WearCategory = category,
				Units = units
			};
			ItemsTypes.Add(type);
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
			var type = new ItemsType {
				Name = name,
				Category = ItemTypeCategory.property,
				Units = units
			};
			ItemsTypes.Add(type);
			TypeDescription desc = new TypeDescription(type);

			foreach(var word in keyWords) {
				KeyWords.Add(word.ToLower(), desc);
			}
		}

		#endregion

		public ItemsType ParseNomenclatureName(string name, bool kit)
		{
			var parts = name.ToLower().Split(' ', '-', '_', '~', '"');
			foreach(var word in parts) {
				if(KeyWords.TryGetValue(word, out TypeDescription desc)) {
					if(desc.keyWords2 != null) {
						foreach(var word2 in parts) {
							if(desc.keyWords2.Contains(word2))
								return desc.ItemsType2;
						}
					}
					return kit ? desc.GetKit(KitUnits) : desc.ItemsType;
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
	}

	public class TypeDescription
	{
		public ItemsType ItemsType;
		public ItemsType ItemsTypeKit;

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

		public ItemsType GetKit(MeasurementUnits komplect)
		{
			if(ItemsTypeKit == null) {
				ItemsTypeKit = new ItemsType {
					Name = ItemsType.Name + " КОМПЛЕКТ",
					Category = ItemTypeCategory.wear,
					WearCategory = ItemsType.WearCategory,
					Units = komplect
				};
			}
			return ItemsTypeKit;
		}
	}
}
