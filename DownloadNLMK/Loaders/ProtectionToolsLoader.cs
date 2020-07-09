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

		public HashSet<ProtectionTools> UsedProtectionTools = new HashSet<ProtectionTools>();

		public ProtectionToolsLoader(IUnitOfWork uow, NomenclatureLoader nomenclatures)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.nomenclatures = nomenclatures ?? throw new ArgumentNullException(nameof(nomenclatures));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Загружаем PROTECTION_TOOL");
			var dtPROTECTION_TOOLS = connection.Query("SELECT * FROM SKLAD.PROTECTION_TOOL");
			logger.Info("Загружаем PROTECTION_REPLACEMENT");
			var dtPROTECTION_REPLACEMENT = connection.Query("SELECT * FROM SKLAD.PROTECTION_REPLACEMENT");

			logger.Info("Обработка СИЗ...");
			foreach(var row in dtPROTECTION_TOOLS) {
				var item = new ProtectionTools();
				item.Name = row.NAME;
				item.Comment = "Загружена из ОМТР";
				if(String.IsNullOrWhiteSpace(item.Name)) {
					logger.Warn($"СИЗ с кодом {row.PROTECTION_ID} не имеет названия.");
					item.Name = $"Без названия PROTECTION_ID={row.PROTECTION_ID}";
				}
				ByID[row.PROTECTION_ID] = item;
			}
			logger.Info($"Загружено {ByID.Count} СИЗ-ов.");

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
				ByID[row.PROTECTION_ID].Analogs.Add(ByID[row.PROTECTION_ID_ANALOG]);
				analogCount++;
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
			}
			logger.Info($"Добавлено {dtANALOQUE_PROTECTION.Count()} связей.");
		}

		public void MarkAsUsed(ProtectionTools tools)
		{
			if(UsedProtectionTools.Add(tools)) {
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
			foreach(var item in UsedProtectionTools) {
				uow.Save(item);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / UsedProtectionTools.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.Write("Завершено\n");
		}
	}
}
