using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using workwear.Domain.Regulations;

namespace DownloadNLMK.Loaders
{
	public class NormLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWork uow;
		private readonly ProtectionToolsLoader protectionTools;
		public Dictionary<string, Norm> ByID = new Dictionary<string, Norm>();
		public Dictionary<string, Norm> ByProf = new Dictionary<string, Norm>();
		public Dictionary<string, NormItem> RowsByID = new Dictionary<string, NormItem>();

		public HashSet<Norm> UsedNorms = new HashSet<Norm>();

		public NormLoader(IUnitOfWork uow, ProtectionToolsLoader protectionTools)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.protectionTools = protectionTools ?? throw new ArgumentNullException(nameof(protectionTools));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Загружаем NORMA");
			var dtNORMA = connection.Query("SELECT * FROM SKLAD.NORMA");
			logger.Info("Загружаем NORMA_ROW");
			var dtNORMA_ROW = connection.Query("SELECT * FROM SKLAD.NORMA_ROW");

			logger.Info("Загружаем PROFF_STAFF");
			var PROFF_STAFF = connection.Query("SELECT * FROM SKLAD.PROFF_STAFF")
				.ToDictionary<dynamic, string>(x => x.PROFF_ID);

			logger.Info("Обработка норм...");

			foreach(var rowNorma in dtNORMA) {
				if(!PROFF_STAFF.ContainsKey(rowNorma.PROFF_ID)) {
					logger.Warn($"Профессия NORMA.PROFF_ID={rowNorma.PROFF_ID} не найдена в PROFF_STAFF.");
					continue;
				}

				Norm norm = new Norm();
				norm.DateFrom = rowNorma.DATE_BEGIN;
				norm.DateTo = rowNorma.DATE_END;
				norm.Name = PROFF_STAFF[rowNorma.PROFF_ID].NAME_PROFF;
				ByID[rowNorma.NORMA_ID] = norm;
				if(!ByProf.ContainsKey(rowNorma.PROFF_ID) || ByProf[(string)rowNorma.PROFF_ID].DateTo < norm.DateTo) {
					ByProf[rowNorma.PROFF_ID] = norm;
				}
			}
			logger.Info($"Загружено {ByID.Count()} норм.");

			logger.Info("Обработка строк норм...");
			int normRows = 0;
			int processed = 0;
			double totalRows = dtNORMA_ROW.Count();
			foreach(var rowNorma in dtNORMA_ROW) {
				processed++;
				if(processed % 100 == 0)
					Console.Write($"\r\tОбработано строк {processed} [{processed / totalRows:P}]... ");
				if(!protectionTools.ByID.ContainsKey(rowNorma.PROTECTION_ID)) {
					logger.Warn($"В норме NORMA_ID={rowNorma.NORMA_ID} {ByID[rowNorma.NORMA_ID].Name} есть ссылка на PROTECTION_ID={rowNorma.PROTECTION_ID} которой нет.");
					continue;
				}
				NormItem normItem = new NormItem();
				normItem.Amount = Convert.ToInt32(rowNorma.COUNT);
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.Item = protectionTools.ByID[rowNorma.PROTECTION_ID];
				normItem.PeriodCount = Convert.ToInt32(rowNorma.WEARING_PERIOD);
				normItem.Norm = ByID[rowNorma.NORMA_ID];
				ByID[rowNorma.NORMA_ID].Items.Add(normItem);
				RowsByID.Add(rowNorma.NORMA_ROW_ID, normItem);
				normRows++;

				if(normItem.PeriodCount > 127) {
					logger.Warn($"В норме {normItem.Norm.Name} - {normItem.Item.Name} указано {normItem.PeriodCount} что больше максимального значения поля. Период сокращне до 120.");
					normItem.PeriodCount = 120;
				}
			}
			Console.Write("Готово\n");
			logger.Info($"Загружено {normRows} из {dtNORMA_ROW.Count()} строк норм.");
		}

		public void MarkAsUsed(Norm norm)
		{
			if(UsedNorms.Add(norm)) {
				foreach(var item in norm.Items)
					protectionTools.MarkAsUsed(item.Item);
			}
		}

		public void Save()
		{
			logger.Info($"Сохраняем нормы...");
			int i = 0;
			foreach(var norm in UsedNorms) {
				uow.Save(norm);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / UsedNorms.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.Write("Завершено\n");
		}
	}
}
