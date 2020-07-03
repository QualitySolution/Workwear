
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using NLog;
using Oracle.ManagedDataAccess.Client;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Services.Interactive;
using QS.Services;
using QSMachineConfig;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.Tools.Oracle;

namespace DonwloadNMLK
{
	class MainClass
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		static IInteractiveService interactive = new ConsoleInteractiveService();
		static NLMKOracle NLMKOracle;

		public static void Main(string[] args)
		{
			#region config
			MachineConfig.ConfigFileName = "workwear.ini";
			MachineConfig.ReloadConfigFile();

			NLMKOracle = new NLMKOracle();
			NLMKOracle.Connect(interactive);
			if(NLMKOracle.Connection == null)
				return;

			ConnectConfig.InitConnection(interactive);
			if(OrmConfig.NhConfig == null)
				return;

			#endregion

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				logger.Info("start");
				logger.Info("Создаем типы номеклатур");
				var nomeclatureTypes = new NomenclatureTypes(uow);

				logger.Info("Загружаем SKLAD.SAP_ZMAT");
				var dtSAP_ZMAT = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.SAP_ZMAT");
				logger.Info("Обработка SKLAD.SAP_ZMAT");
				var dicSAP_ZMAT = new Dictionary<string, Nomenclature>();
				int categoryFail = 0;
				foreach(var zmat in dtSAP_ZMAT) {
					var nomenclature = new Nomenclature {
						Name = zmat.NMAT ?? zmat.NMAT_,
						Ozm = uint.Parse(zmat.ZMAT), 
						Comment = "Выгружен из ОМТР",
					};
					if(dicSAP_ZMAT.ContainsKey(zmat.ZMAT)) {
						logger.Error($"Дубль строки для ОЗМ {zmat.ZMAT}\n >>{dicSAP_ZMAT[zmat.ZMAT].Name}\n >>{nomenclature.Name}");
						continue;
					}
					dicSAP_ZMAT.Add(zmat.ZMAT, nomenclature);
					if(nomenclature.Name == null) {
						logger.Error($"Для ОЗМ {nomenclature.Ozm} нет названия.");
						categoryFail++;
						continue;
					}

					nomenclature.Type = nomeclatureTypes.ParseNomenclatureName(nomenclature.Name, zmat.EDIZ == 839);

					if(nomenclature.Type == null) {
						categoryFail++;
						continue;
					}
					nomenclature.Sex = nomeclatureTypes.ParseSex(nomenclature.Name);
					if(SizeHelper.HasClothesSex(nomenclature.Type.WearCategory.Value)) {
						if(nomenclature.Sex == null)
							logger.Warn($"Не найден пол для [{nomenclature.Name}]");
					}
					else {
						if(nomenclature.Sex != null)
							logger.Warn($"Пол найден в [{nomenclature.Name}], но тип {nomenclature.Type.Name} без пола.");
					}

					if(zmat.EDIZ != null && zmat.EDIZ.ToString() != nomenclature.Type.Units.OKEI)
						logger.Error($"Единица измерения не соответсвует {zmat.EDIZ} != {nomenclature.Type.Units.OKEI} для [{nomenclature.Name}]");
				}
				logger.Warn($"Для {categoryFail} номеклатур, не найдено категорий.");
				return;
						if(nomenclature.Sex == null)
				logger.Info("Загружаем PROTECTION_TOOL");
				var dtPROTECTION_TOOLS = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.PROTECTION_TOOL");
				logger.Info("Загружаем PROTECTION_REPLACEMENT");
				var dtPROTECTION_REPLACEMENT = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.PROTECTION_REPLACEMENT");

				var dicItemsTypes = new Dictionary<string, ItemsType>();
				logger.Info("Обработка СИЗ...");
				foreach(var row in dtPROTECTION_TOOLS) {
					if(String.IsNullOrWhiteSpace(row.NAME)) {
						logger.Warn($"СИЗ с кодом {row.PROTECTION_ID} не имеет названия. Пропускаем...");
						continue;
					}
					var item = new ItemsType();
					item.Name = row.NAME;
					item.Category = ItemTypeCategory.wear;
					dicItemsTypes[row.PROTECTION_ID] = item;
				}
				logger.Info($"Загружено {dicItemsTypes.Count} СИЗ-ов.");
				logger.Info("Обработка Аналогов СИЗ...");
				int analogCount = 0;
				int analogNotFound = 0;
				foreach(var row in dtPROTECTION_REPLACEMENT) {
					if(!dicItemsTypes.ContainsKey(row.PROTECTION_ID)) {
						logger.Warn($"Аналог PROTECTION_REPLACEMENT.PROTECTION_ID={row.PROTECTION_ID} не найден в загруженных СИЗ-ах.");
						analogNotFound++;
						continue;
					}
					if(!dicItemsTypes.ContainsKey(row.PROTECTION_ID_ANALOG)) {
						logger.Warn($"Аналог PROTECTION_REPLACEMENT.PROTECTION_ID_ANALOG={row.PROTECTION_ID_ANALOG} не найден в загруженных СИЗ-ах.");
						analogNotFound++;
						continue;
					}
					dicItemsTypes[row.PROTECTION_ID].ItemsTypesAnalogs.Add(dicItemsTypes[row.PROTECTION_ID_ANALOG]);
					analogCount++;
				}
				logger.Info($"Загружено {analogCount} аналогов СИЗ-ов.");
				logger.Info($"Не найдено {analogNotFound} аналогов СИЗ-ов.");
				logger.Info($"Сохраняем...");
				int i = 0;
				foreach(var item in dicItemsTypes.Values) {
					uow.Save(item);
					i++;
					if(i % 100 == 0) {
						uow.Commit();
						logger.Info($"Сохранили {(float)i/dicItemsTypes.Count:P}");
					}
				}
				uow.Commit();
				logger.Info("Готово");

				return;

				//DataTable dtNORMA = getTableFromDataBase("SELECT * FROM SKLAD.NORMA");
				//DataTable dtNORMA_ROW = getTableFromDataBase("SELECT * FROM SKLAD.NORMA_ROW");

				//var dicNorms = new Dictionary<int, Norm>();
				//var dicNorms_row = new Dictionary<int, NormItem>();

				//foreach(DataRow rowNorma in dtNORMA.Rows) {
				//	Norm norm = new Norm();
				//	norm.DateFrom = DateTime.Parse(rowNorma["DATE_BEGIN"].ToString());
				//	norm.DateTo = DateTime.Parse(rowNorma["DATE_END"].ToString());

				//	dicNorms[int.Parse(rowNorma["NORMA_ID"].ToString())] = norm;

				//}

				//foreach(DataRow rowNorma_row in dtNORMA_ROW.Rows) {
				//	NormItem normItem = new NormItem();
				//	normItem.Amount = int.Parse(rowNorma_row["COUNT"].ToString());
				//	normItem.NormPeriod = NormPeriodType.Month;
				//	normItem.Item = dicItemsTypes[int.Parse(rowNorma_row["PROTECTION_ID"].ToString())];
				//	normItem.PeriodCount = int.Parse(rowNorma_row["WEARING_PERIOD"].ToString());
				//	normItem.Norm = dicNorms[int.Parse(rowNorma_row["NORMA_ID"].ToString())];

				//	dicNorms_row[int.Parse(rowNorma_row["NORMA_ROW_ID"].ToString())] = normItem;
				//	dicNorms[int.Parse(rowNorma_row["NORMA_ID"].ToString())].Items.Add(normItem);
				//}

				//foreach(var Norma in dicNorms.Values)
				//	uow.Save(Norma);

				//DataTable dtPERSONAL_CARDS = getTableFromDataBase("SELECT * FROM SKLAD.PERSONAL_CARD");
				//foreach (DataRow rowPers_cards in dtPERSONAL_CARDS.Rows) {
				//	EmployeeCard card = new EmployeeCard();
				//	card.PersonnelNumber = rowPers_cards["TN"].ToString();
				//	card.LastName = rowPers_cards["SURNAME"].ToString();
				//	card.FirstName = rowPers_cards["NAME"].ToString();
				//	card.Patronymic = rowPers_cards["SECNAME"].ToString();
				//	card.CardNumber = rowPers_cards["NUM_CARD"].ToString();
				//	card.Comment = rowPers_cards["COMM"].ToString();
				//	card.Sex = rowPers_cards["E_SEX"].ToString() == "0" ? Sex.M : Sex.F;
				//	uow.Save(card);
				//}

				uow.Commit();
			}

			Console.ReadLine();
		}
	}
}
