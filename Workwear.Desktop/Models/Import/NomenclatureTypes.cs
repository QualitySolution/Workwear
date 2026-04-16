using System;
using System.Collections.Generic;
using System.Linq;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Tools.Sizes;

namespace Workwear.Models.Import
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

			AddType("Халаты", ClothesType.Wear, sht, new[] { "ХАЛАТ" });
			AddType("Костюмы", ClothesType.Wear, sht, new[] { "КОСТЮМ", "ГИДРОКОСТЮМ" });
			AddType("Куртки", ClothesType.Wear, sht, new[] { "КУРТКА" });
			AddType("Брюки", ClothesType.Wear, sht, new[] { "БРЮКИ" });
			AddType("Жилеты", ClothesType.Wear, sht, new[] { "ЖИЛЕТ" });
			AddType("Футболки", ClothesType.Wear, sht, new[] { "ФУТБОЛКА" });
			AddType("Блузки", ClothesType.Wear, sht, new[] { "БЛУЗКА" });
			AddType("Плащи", ClothesType.Wear, sht, new[] { "ПЛАЩ" });
			AddType("Белье", ClothesType.Wear, sht, new[] { "БЕЛЬЕ", "КАЛЬСОНЫ" });
			AddType("Рубашки", ClothesType.Wear, sht, new[] { "СОРОЧКА", "Рубашка" });
			AddType("Фуфайки", ClothesType.Wear, sht, new[] { "Фуфайка" });
			AddType("Свитеры", ClothesType.Wear, sht, new[] { "СВИТЕР" });
			AddType("Комбинезоны", ClothesType.Wear, sht, new[] { "КОМБИНЕЗОН", "ПОЛУКОМБИНЕЗОН", "ГИДРОКОМБИНЕЗОН" });
			AddType("Комплекты", ClothesType.Wear, sht, new[] { "КОМПЛЕКТ" });
			AddType("Подшлемники", ClothesType.Headgear, sht, new[] { "ПОДШЛЕМНИК" });
			AddType("Головные уборы", ClothesType.Headgear, sht, new[] { "ШАПКА", "Кепка", "ФЕСКА", "ШЛЯПА", "ФУРАЖКА", "БЕЙСБОЛКА", "БЕРЕТ", "КОЛПАК", "КЕПИ", "ПИЛОТКА", "Головной", "Каскетка"});
			AddType("Каски", ClothesType.Headgear, sht, new[] { "КАСКА",  });
			AddType("Полуботинки", ClothesType.Shoes, pair, new[] { "ПОЛУБОТИНКИ" });
			AddType("Сапоги", ClothesType.Shoes, pair, new[] { "САПОГИ", "ПОЛУСАПОГИ" });
			AddType("Ботинки", ClothesType.Shoes, pair, new[] { "БОТИНКИ", "ЧУВЯКИ", "боты" });
			AddType("Туфли", ClothesType.Shoes, pair, new[] { "Туфли" });
			AddType("Тапочки", ClothesType.Shoes, pair, new[] { "Тапки,", "Тапочки" });
			AddType("Сандалии", ClothesType.Shoes, pair, new[] { "Сабо", "Сандалии", "Сандали" });
			AddType("Обувь", ClothesType.Shoes, pair, new[] { "Обувь" });
			AddType("Валенки", ClothesType.WinterShoes, pair, new[] { "ВАЛЕНКИ" });
			AddType("Галоши", ClothesType.WinterShoes, pair, new[] { "ГАЛОШИ" });
			AddType("Перчатки", ClothesType.Gloves, pair, new[] { "ПЕРЧАТКИ", "КРАГИ", "перчаточные" });
			AddType("Рукавицы", ClothesType.Mittens, pair, new[] { "РУКАВИЦЫ", "Вачеги" });
			AddType("Аксесуары", ClothesType.PPE, sht, new[] { "ШАРФ", "ГАЛСТУК", "ШЕВРОН", "РЕМЕНЬ", "ЗНАЧОК", "Кокарда", "БЕЙДЖ", "ПОЯС", "Наколка" });
			AddType("Водолазный инвентарь", ClothesType.PPE, sht, new[] { "ВОДОЛАЗНАЯ", "ПОДВОДНАЯ", "ГРУЗ", "ВОДОЛАЗНЫЙ", "ШЛАНГ", "СПИНКА", "БАЛЛОН" });
			AddType("Носки", ClothesType.PPE, pair, new[] { "НОСКИ" });
			AddType("Чулки", ClothesType.PPE, pair, new[] { "ЧУЛКИ" });
			AddType("Портянки", ClothesType.PPE, pair, new[] { "ПОРТЯНКИ" });
			AddType("Фартуки", ClothesType.PPE, sht, new[] { "ФАРТУК" });
			AddType("Косынки", ClothesType.PPE, sht, new[] { "КОСЫНКА" });
			AddType("Повязка", ClothesType.PPE, sht, new[] { "ПОВЯЗКА" });
			AddType("Привязи", ClothesType.PPE, sht, new[] { "привязь" });
			AddType("Полотенца", ClothesType.PPE, sht, new[] { "Полотенце" });
			AddType("Щитки", ClothesType.PPE, sht, new[] { "ЩИТОК" });
			AddType("СИЗОД", ClothesType.PPE, sht, new[] { "СИЗОД", "Респиратор", "РЕСПИРАТОРЫ", "РЕСПИРАТОРА", "ПАТРОН", "ПРЕДФИЛЬТР", "КОРОБКА", "Капюшон", "дыхания", "Противогаз", "Фильтр", "Фильтра" });
			AddType("Беруши", ClothesType.PPE, pair, new[] { "Беруши", "противошумные" });
			AddType("Очки", ClothesType.PPE, sht, new[] { "ОЧКИ" });
			AddType("Наушники", ClothesType.PPE, sht, new[] { "НАУШНИКИ" });
			AddType("Маски", ClothesType.PPE, sht, new[] { "МАСКА", "ШЛЕМ", "МАСКЕ", "СТЕКЛА", "Полумаска", "накомарник" });
			AddType("Аптечки", ClothesType.PPE, sht, new[] { "АПТЕЧКА" });
			AddType("Моющее средства", ClothesType.PPE, sht, new[] { "Моющее", "очищающая" });
			AddType("Крема", ClothesType.PPE, sht, new[] { "крем", "МАЗЬ", "Паста" });
			AddType("Защита", ClothesType.PPE, pair, new[] { "НАРУКАВНИКИ", "Наколенники" });
			AddType("Защита от поражения электрическим током", ClothesType.PPE, sht, new[] { "Диэлектрический" });
			AddType("Неизвестный тип", ClothesType.PPE, sht, new string[] { });
		}
		private void AddType(string name, ClothesType category, MeasurementUnits units, string[] keyWords, ClothesType? category2 = null, string[] keywords2 = null)
		{
			var type = ItemsTypes.FirstOrDefault(x => x.Name == name);
			if(type == null) {
				type = new ItemsType {
					Name = name,
					WearCategory = category,
					Units = units,
				};
				switch (category) {
					case ClothesType.Wear:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер одежды");
						type.HeightType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Рост");
						break;
					case ClothesType.Shoes:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер обуви");
						break;
					case ClothesType.WinterShoes:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер зимней обуви");
						break;
					case ClothesType.Headgear:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер головного убора");
						break;
					case ClothesType.Gloves:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер перчаток");
						break;
					case ClothesType.Mittens:
						type.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер рукавиц");
						break;
				}
				ItemsTypes.Add(type);
			}
			TypeDescription desc;
			if(category2 != null) {
				var type2 = new ItemsType {
					Name = name,
					WearCategory = category2.Value,
					Units = units
				};
				switch (category) {
					case ClothesType.Wear:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер одежды");
						type2.HeightType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Рост");
						break;
					case ClothesType.Shoes:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер обуви");
						break;
					case ClothesType.WinterShoes:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер зимней обуви");
						break;
					case ClothesType.Headgear:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер головного убора");
						break;
					case ClothesType.Gloves:
						type2.SizeType = sizeService.GetSizeType(uow).FirstOrDefault(x => x.Name == "Размер перчаток");
						break;
					case ClothesType.Mittens:
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

		public ClothesSex ParseSex(string name) {
			var parts = name.ToLower().Split(' ', '-');
			foreach(var word in parts) {
				if(SexKeywords.TryGetValue(word, out var sex))
					return sex;
			}
			return ClothesSex.Universal;
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
