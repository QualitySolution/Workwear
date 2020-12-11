using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using workwear.Domain.Regulations;

namespace DownloadNLMK.Loaders
{
	public class ProtectionToolsLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWork uow;
		private readonly NomenclatureLoader nomenclatures;
		public Dictionary<string, ProtectionTools> ByID = new Dictionary<string, ProtectionTools>();
		public Dictionary<string, ProtectionTools> DicUniqNameProtectionTools;

		public HashSet<ProtectionTools> UsedProtectionTools = new HashSet<ProtectionTools>();
		public HashSet<ProtectionTools> ChangedProtectionTools = new HashSet<ProtectionTools>();

		public ProtectionToolsLoader(IUnitOfWork uow, NomenclatureLoader nomenclatures)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.nomenclatures = nomenclatures ?? throw new ArgumentNullException(nameof(nomenclatures));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Загружаем имеющиеся СИЗ");
			DicUniqNameProtectionTools = uow.GetAll<ProtectionTools>().ToDictionary(x => x.Name, x => x);
			foreach(var tools in DicUniqNameProtectionTools.Values) {
				if(!String.IsNullOrWhiteSpace(tools.NlmkIds)) {
					var ids = tools.NlmkIds.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					foreach(var id in ids) {
						ByID.Add(id, tools);
					}
				}
			}

			logger.Info("Загружаем PROTECTION_TOOL");
			var dtPROTECTION_TOOLS = connection.Query("SELECT * FROM SKLAD.PROTECTION_TOOL");
			logger.Info("Загружаем PROTECTION_REPLACEMENT");
			var dtPROTECTION_REPLACEMENT = connection.Query("SELECT * FROM SKLAD.PROTECTION_REPLACEMENT");

			logger.Info("Обработка СИЗ...");
			foreach(var row in dtPROTECTION_TOOLS) {
				if(ByID.ContainsKey(row.PROTECTION_ID))
					continue;

				if(row.NAME != null && DicUniqNameProtectionTools.ContainsKey(row.NAME)) {
					ByID[row.PROTECTION_ID] = DicUniqNameProtectionTools[row.NAME];
					DicUniqNameProtectionTools[row.NAME].NlmkIds += row.PROTECTION_ID + ";";
					ChangedProtectionTools.Add(ByID[row.PROTECTION_ID]);
					continue;
				}

				var item = new ProtectionTools();
				item.Name = row.NAME;
				item.NlmkIds = row.PROTECTION_ID + ";";
				item.Comment = "Загружена из ОМТР";
				if(String.IsNullOrWhiteSpace(item.Name)) {
					logger.Warn($"СИЗ с кодом {row.PROTECTION_ID} не имеет названия.");
					item.Name = $"Без названия PROTECTION_ID={row.PROTECTION_ID}";
				}
				item.Type = nomenclatures.NomenclatureTypes.ParseNomenclatureName(item.Name, false);

				if (row.NAME != null)
					DicUniqNameProtectionTools[row.NAME] = item;
				ByID[row.PROTECTION_ID] = item;
				ChangedProtectionTools.Add(item);
			}
			logger.Info($"Загружено {ByID.Count} СИЗ-ов.");

			logger.Info($"Загружено уникальных {DicUniqNameProtectionTools.Count} СИЗ-ов.");

			logger.Info("Обработка Аналогов СИЗ...");
			int analogCount = 0;
			int analogNotFound = 0;
			foreach(var row in dtPROTECTION_REPLACEMENT) {
				if(!ByID.ContainsKey(row.PROTECTION_ID)) {
					logger.Warn($"Аналог PROTECTION_REPLACEMENT.PROTECTION_ID={row.PROTECTION_ID} не найден в загруженных СИЗ-ах.");
					analogNotFound++;
					continue;
				}
				if(!ByID.ContainsKey(row.PROTECTION_ID_ANALOG)) {
					logger.Warn($"Аналог PROTECTION_REPLACEMENT.PROTECTION_ID_ANALOG={row.PROTECTION_ID_ANALOG} не найден в загруженных СИЗ-ах.");
					analogNotFound++;
					continue;
				}

				if(!ByID[row.PROTECTION_ID].Analogs.Contains(ByID[row.PROTECTION_ID_ANALOG])) {
					ByID[row.PROTECTION_ID].Analogs.Add(ByID[row.PROTECTION_ID_ANALOG]);
					analogCount++;
				}
			}

			logger.Info($"Загружено {analogCount} аналогов СИЗ-ов.");
			logger.Info($"Не найдено {analogNotFound} аналогов СИЗ-ов.");

			logger.Info("Загружаем ANALOQUE_PROTECTION");
			var dtANALOQUE_PROTECTION = connection.Query("SELECT * FROM SKLAD.ANALOQUE_PROTECTION");
			logger.Info("Связываем СИЗ с номеклатурой.");
			foreach(var link in dtANALOQUE_PROTECTION) {
				ProtectionTools protection = ByID[link.PROTECTION_ID];
				if(!nomenclatures.ByID.ContainsKey(link.MAT)) {
					logger.Error($"Номеклатура {link.MAT} не найдена");
					continue;
				}
				var nomenclature = nomenclatures.ByID[link.MAT];
				protection.AddNomeclature(nomenclature);
				if(protection.Type == null) {
					protection.Type = nomenclature.Type;
				}
			}
			logger.Info($"Добавлено {dtANALOQUE_PROTECTION.Count()} связей.");
		}

		public void MarkAsUsed(ProtectionTools tools)
		{
			if(UsedProtectionTools.Add(tools)) {
				if(tools.Type == null) { //Чтобы иметь возможность сохранить
					tools.Type = nomenclatures.NomenclatureTypes.GetUnknownType();
					ChangedProtectionTools.Add(tools);
				}

				foreach(var item in tools.Nomenclatures)
					nomenclatures.MarkAsUsed(item);
				foreach(var tool in tools.Analogs)
					MarkAsUsed(tool);
			}
		}

		public void Save()
		{
			logger.Info($"Сохраняем СИЗ...");
			int i = 0;
			var toSave = ChangedProtectionTools.Where(x => UsedProtectionTools.Contains(x)).ToList();
			foreach(var item in toSave) {
				uow.Save(item);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / toSave.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.WriteLine($"Обновили {toSave.Count} СИЗ.");
		}
	}
}
