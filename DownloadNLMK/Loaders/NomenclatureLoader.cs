using System;
using System.Collections.Generic;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using workwear.Domain.Stock;
using workwear.Measurements;

namespace DownloadNLMK.Loaders
{
	public class NomenclatureLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWork uow;
		public Dictionary<string, Nomenclature> ByID = new Dictionary<string, Nomenclature>();
		public NomenclatureTypes NomenclatureTypes;
		public HashSet<Nomenclature> UsedNomenclatures = new HashSet<Nomenclature>();

		public NomenclatureLoader(IUnitOfWork uow )
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Создаем типы номеклатур");
			NomenclatureTypes = new NomenclatureTypes(uow);

			logger.Info("Загружаем SKLAD.SAP_ZMAT");
			var dtSAP_ZMAT = connection.Query("SELECT * FROM SKLAD.SAP_ZMAT");
			logger.Info("Обработка SKLAD.SAP_ZMAT");
			int categoryFail = 0;
			foreach(var zmat in dtSAP_ZMAT) {
				var nomenclature = new Nomenclature {
					Name = zmat.NMAT ?? zmat.NMAT_,
					Ozm = uint.Parse(zmat.ZMAT),
					Comment = "Выгружен из ОМТР",
				};
				if(ByID.ContainsKey(zmat.ZMAT)) {
					logger.Error($"Дубль строки для ОЗМ {zmat.ZMAT}\n >>{ByID[zmat.ZMAT].Name}\n >>{nomenclature.Name}");
					continue;
				}
				ByID.Add(zmat.ZMAT, nomenclature);
				if(nomenclature.Name == null) {
					nomenclature.Name = $"Без названия ОЗМ={nomenclature.Ozm}";
					logger.Error($"Для ОЗМ {nomenclature.Ozm} нет названия.");
					categoryFail++;
					continue;
				}

				nomenclature.Type = NomenclatureTypes.ParseNomenclatureName(nomenclature.Name, zmat.EDIZ == 839);

				if(nomenclature.Type == null) {
					categoryFail++;
					continue;
				}
				if(nomenclature.Type.WearCategory.HasValue) {
					nomenclature.Sex = NomenclatureTypes.ParseSex(nomenclature.Name);
					if(SizeHelper.HasClothesSex(nomenclature.Type.WearCategory.Value)) {
						if(nomenclature.Sex == null) {
							logger.Warn($"Не найден пол для [{nomenclature.Name}]");
							nomenclature.Sex = ClothesSex.Universal;
						}
					}
					else {
						if(nomenclature.Sex != null)
							logger.Warn($"Пол найден в [{nomenclature.Name}], но тип {nomenclature.Type.Name} без пола.");
					}

					var sizeStd = SizeHelper.GetDefaultSizeStd(nomenclature.Type.WearCategory.Value, nomenclature.Sex ?? ClothesSex.Universal);
					if(sizeStd != null)
						nomenclature.SizeStd = SizeHelper.GetSizeStdCode(sizeStd);
					else
						logger.Warn($"Для {nomenclature.Name} стандарт размера не установлен.");
				}

				if(zmat.EDIZ != null && zmat.EDIZ.ToString() != nomenclature.Type.Units.OKEI)
					logger.Error($"Единица измерения не соответсвует {zmat.EDIZ} != {nomenclature.Type.Units.OKEI} для [{nomenclature.Name}]");
			}
			logger.Warn($"Для {categoryFail} номеклатур, не найдено категорий.");
		}

		public void MarkAsUsed(Nomenclature nomenclature)
		{
			UsedNomenclatures.Add(nomenclature);
		}

		public void Save()
		{
			logger.Info($"Сохраняем типы...");
			foreach(var item in NomenclatureTypes.ItemsTypes) {
				uow.Save(item, orUpdate: false);
			}
			uow.Commit();

			logger.Info($"Сохраняем номенклатуру...");
			int i = 0;
			foreach(var item in UsedNomenclatures) {
				uow.Save(item, orUpdate: false);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / UsedNomenclatures.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.Write("Завершено\n");
		}
	}
}
