
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
					if(nomenclature.Type.WearCategory.HasValue) {
						nomenclature.Sex = nomeclatureTypes.ParseSex(nomenclature.Name);
						if(SizeHelper.HasClothesSex(nomenclature.Type.WearCategory.Value)) {
							if(nomenclature.Sex == null)
								logger.Warn($"Не найден пол для [{nomenclature.Name}]");
						}
						else {
							if(nomenclature.Sex != null)
								logger.Warn($"Пол найден в [{nomenclature.Name}], но тип {nomenclature.Type.Name} без пола.");
						}
					}

					if(zmat.EDIZ != null && zmat.EDIZ.ToString() != nomenclature.Type.Units.OKEI)
						logger.Error($"Единица измерения не соответсвует {zmat.EDIZ} != {nomenclature.Type.Units.OKEI} для [{nomenclature.Name}]");
				}
				logger.Warn($"Для {categoryFail} номеклатур, не найдено категорий.");

				logger.Info("Загружаем PROTECTION_TOOL");
				var dtPROTECTION_TOOLS = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.PROTECTION_TOOL");
				logger.Info("Загружаем PROTECTION_REPLACEMENT");
				var dtPROTECTION_REPLACEMENT = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.PROTECTION_REPLACEMENT");

				var dicProtectionTools = new Dictionary<string, ProtectionTools>();
				var dicSkipedProtectionTools = new Dictionary<string, dynamic>();
				logger.Info("Обработка СИЗ...");
				foreach(var row in dtPROTECTION_TOOLS) {
					if(String.IsNullOrWhiteSpace(row.NAME)) {
						logger.Warn($"СИЗ с кодом {row.PROTECTION_ID} не имеет названия. Пропускаем...");
						dicSkipedProtectionTools[row.PROTECTION_ID] = row;
						continue;
					}
					var item = new ProtectionTools();
					item.Name = row.NAME;
					dicProtectionTools[row.PROTECTION_ID] = item;
				}
				logger.Info($"Загружено {dicProtectionTools.Count} СИЗ-ов.");
				logger.Info("Обработка Аналогов СИЗ...");
				int analogCount = 0;
				int analogNotFound = 0;
				foreach(var row in dtPROTECTION_REPLACEMENT) {
					if(!dicProtectionTools.ContainsKey(row.PROTECTION_ID)) {
						logger.Warn($"Аналог PROTECTION_REPLACEMENT.PROTECTION_ID={row.PROTECTION_ID} не найден в загруженных СИЗ-ах.");
						analogNotFound++;
						continue;
					}
					if(!dicProtectionTools.ContainsKey(row.PROTECTION_ID_ANALOG)) {
						logger.Warn($"Аналог PROTECTION_REPLACEMENT.PROTECTION_ID_ANALOG={row.PROTECTION_ID_ANALOG} не найден в загруженных СИЗ-ах.");
						analogNotFound++;
						continue;
					}
					dicProtectionTools[row.PROTECTION_ID].Analogs.Add(dicProtectionTools[row.PROTECTION_ID_ANALOG]);
					analogCount++;
				}
				logger.Info($"Загружено {analogCount} аналогов СИЗ-ов.");
				logger.Info($"Не найдено {analogNotFound} аналогов СИЗ-ов.");

				logger.Info("Загружаем ANALOQUE_PROTECTION");
				var dtANALOQUE_PROTECTION = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.ANALOQUE_PROTECTION");
				logger.Info("Связываем СИЗ с номеклатурой.");
				var noUsedNomenclature = dicSAP_ZMAT.Values.ToList();
				foreach(var link in dtANALOQUE_PROTECTION) {
					ProtectionTools protection = dicProtectionTools[link.PROTECTION_ID];
					if(!dicSAP_ZMAT.ContainsKey(link.MAT)) {
						logger.Error($"Номеклатура {link.MAT} не найдена");
						continue;
					}
					var nomenclature = dicSAP_ZMAT[link.MAT];
					protection.AddNomeclature(nomenclature);
					noUsedNomenclature.Remove(nomenclature);
				}
				logger.Info($"Добавлено {dtANALOQUE_PROTECTION.Count()} связей.");
				if(noUsedNomenclature.Count > 0) {
					logger.Warn($"Не использовано {noUsedNomenclature.Count} из {dicSAP_ZMAT.Count} номеклатур:\n"
						+ String.Join("\n", noUsedNomenclature.Select(x => x.Name)));
				}
#if !NOSAVE
				logger.Info($"Сохраняем типы...");
				foreach(var item in nomeclatureTypes.ItemsTypes) {
					uow.Save(item);
				}
				uow.Commit();

				logger.Info($"Сохраняем номенклатуру...");
				int i = 0;
				foreach(var item in dicSAP_ZMAT.Values) {
					uow.Save(item);
					i++;
					if(i % 100 == 0) {
						uow.Commit();
						logger.Info($"Сохранили {(float)i/ dicSAP_ZMAT.Count:P}");
					}
				}
				uow.Commit();

				logger.Info($"Сохраняем СИЗ...");
				int i = 0;
				foreach(var item in dicProtectionTools.Values) {
					//uow.Save(item);
					i++;
					if(i % 100 == 0) {
						uow.Commit();
						logger.Info($"Сохранили {(float)i/dicProtectionTools.Count:P}");
					}
				}
				uow.Commit();
				logger.Info("Готово");
#endif
				logger.Info("Загружаем NORMA");
				var dtNORMA = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.NORMA");
				logger.Info("Загружаем NORMA_ROW");
				var dtNORMA_ROW = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.NORMA_ROW");

				logger.Info("Загружаем PROFF_STAFF");
				var PROFF_STAFF = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.PROFF_STAFF")
					.ToDictionary<dynamic, string>(x => x.PROFF_ID);

				//logger.Info("Загружаем RELAT_STAFF_PROFF");
				//var RELAT_STAFF_PROFF = NLMKOracle.Connection.Query("SELECT * FROM SKLAD.RELAT_STAFF_PROFF")
					//.ToDictionary<dynamic, string>(x => x.PROFF_ID);

				logger.Info("Обработка норм...");
				var dicNorms = new Dictionary<string, Norm>();
				var dicNorms_row = new Dictionary<string, NormItem>();

				foreach(var rowNorma in dtNORMA) {
					if(!PROFF_STAFF.ContainsKey(rowNorma.PROFF_ID)) {
						logger.Warn($"Профессия NORMA.PROFF_ID={rowNorma.PROFF_ID} не найдена в PROFF_STAFF.");
						continue;
					}

					//if(!RELAT_STAFF_PROFF.ContainsKey(rowNorma.PROFF_ID)) {
					//	logger.Warn($"Профессия NORMA.PROFF_ID={rowNorma.PROFF_ID} не найдена в RELAT_STAFF_PROFF.");
					//	continue;
					//}

					Norm norm = new Norm();
					norm.DateFrom = rowNorma.DATE_BEGIN;
					norm.DateTo = rowNorma.DATE_END;
					norm.Name = PROFF_STAFF[rowNorma.PROFF_ID].NAME_PROFF;
					dicNorms[rowNorma.NORMA_ID] = norm;
				}

				logger.Info($"Загружено {dicNorms.Count()} норм.");

				logger.Info("Обработка строк норм...");
				int normRows = 0;
				foreach(var rowNorma in dtNORMA_ROW) {
					if(!dicProtectionTools.ContainsKey(rowNorma.PROTECTION_ID)) {
						if(dicSkipedProtectionTools.ContainsKey(rowNorma.PROTECTION_ID))
							logger.Warn($"PROTECTION_ID={rowNorma.PROTECTION_ID} в норме NORMA_ID={rowNorma.NORMA_ID} [{dicNorms[rowNorma.NORMA_ID].Name}] была пропущена.");
						else
							logger.Warn($"В норме NORMA_ID={rowNorma.NORMA_ID} {dicNorms[rowNorma.NORMA_ID].Name} есть ссылка на PROTECTION_ID={rowNorma.PROTECTION_ID} которой нет.");
						continue;
					}
					NormItem normItem = new NormItem();
					normItem.Amount = Convert.ToInt32(rowNorma.COUNT);
					normItem.NormPeriod = NormPeriodType.Month;
					normItem.Item = dicProtectionTools[rowNorma.PROTECTION_ID];
					normItem.PeriodCount = Convert.ToInt32(rowNorma.WEARING_PERIOD);
					normItem.Norm = dicNorms[rowNorma.NORMA_ID];
					dicNorms[rowNorma.NORMA_ID].Items.Add(normItem);
					normRows++;
				}

				logger.Info($"Загружено {normRows} из {dtNORMA_ROW.Count()} строк норм.");

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
