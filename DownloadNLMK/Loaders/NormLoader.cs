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
		public Dictionary<string, Norm> ByID;
		public Dictionary<string, Norm> ByProf = new Dictionary<string, Norm>();
		public Dictionary<int, Norm> ByCodeStaff = new Dictionary<int, Norm>();
		public Dictionary<string, NormItem> RowsByID = new Dictionary<string, NormItem>();

		public HashSet<Norm> UsedNorms = new HashSet<Norm>();
		public HashSet<Norm> ChangedNorms = new HashSet<Norm>();

		public NormLoader(IUnitOfWork uow, ProtectionToolsLoader protectionTools)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.protectionTools = protectionTools ?? throw new ArgumentNullException(nameof(protectionTools));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Загружаем имеющиеся нормы");
			ByID = uow.GetAll<Norm>().Where(x => x.NlmkNormId != null).ToDictionary(x => x.NlmkNormId, x => x);
			foreach(var norm in ByID.Values)
				AddNormToProfDic(norm.NlmkProffId, norm);

			logger.Info("Загружаем NORMA");
			var dtNORMA = connection.Query("SELECT * FROM SKLAD.NORMA norma");
			logger.Info("Загружаем NORMA_ROW");
			var dtNORMA_ROW = connection.Query("SELECT * FROM SKLAD.NORMA_ROW ", buffered: false);

			logger.Info("Загружаем PROFF_STAFF");
			var PROFF_STAFF = connection.Query("SELECT * FROM SKLAD.PROFF_STAFF")
				.ToDictionary<dynamic, string>(x => x.PROFF_ID);

			logger.Info("Обработка норм...");

			foreach(var rowNorma in dtNORMA) {
				if(!PROFF_STAFF.ContainsKey(rowNorma.PROFF_ID)) {
					logger.Warn($"Профессия NORMA.PROFF_ID={rowNorma.PROFF_ID} не найдена в PROFF_STAFF.");
					continue;
				}
				Norm norm;
				if(!ByID.TryGetValue(rowNorma.NORMA_ID, out norm)) {
					norm = new Norm();
					norm.NlmkNormId = rowNorma.NORMA_ID;
					ByID[rowNorma.NORMA_ID] = norm;
				}
				norm.PropertyChanged += (sender, e) => ChangedNorms.Add(norm);
				norm.DateFrom = rowNorma.DATE_BEGIN;
				norm.DateTo = rowNorma.DATE_END;
				norm.Name = PROFF_STAFF[rowNorma.PROFF_ID].NAME_PROFF;
				norm.NlmkProffId = rowNorma.PROFF_ID;
				AddNormToProfDic(rowNorma.PROFF_ID, norm);
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
				Norm norm = ByID[rowNorma.NORMA_ID];
				NormItem normItem = norm.Items.FirstOrDefault(x => x.NlmkId == rowNorma.NORMA_ROW_ID);
				if(normItem == null) {
					normItem = new NormItem();
					normItem.Norm = norm;
					normItem.NlmkId = rowNorma.NORMA_ROW_ID;
					norm.Items.Add(normItem);
				}
				normItem.PropertyChanged += (sender, e) => ChangedNorms.Add(norm);
				normItem.Amount = Convert.ToInt32(rowNorma.COUNT);
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.ProtectionTools = protectionTools.ByID[rowNorma.PROTECTION_ID];
				normItem.PeriodCount = Convert.ToInt32(rowNorma.WEARING_PERIOD);
				RowsByID.Add(rowNorma.NORMA_ROW_ID, normItem);
				normRows++;

				if(normItem.PeriodCount > 127) {
					logger.Warn($"В норме {normItem.Norm.Name} - {normItem.ProtectionTools.Name} указано {normItem.PeriodCount} что больше максимального значения поля. Период сокращне до 120.");
					normItem.PeriodCount = 120;
				}
			}
			Console.Write("Готово\n");
			logger.Info($"Загружено {normRows} из {dtNORMA_ROW.Count()} строк норм.");

			logger.Info("Загружаем RELAT_STAFF_PROFF");
			var RELAT_STAFF_PROFF = connection.Query("SELECT * FROM SKLAD.RELAT_STAFF_PROFF ", buffered: true);

			logger.Info("Обработка связей с профессией...");
			foreach(var item in RELAT_STAFF_PROFF) {
				if(!ByProf.ContainsKey(item.PROFF_ID)) {
					logger.Warn($"Для PROFF_ID={item.PROFF_ID} не найдено нормы.");
					continue;
				}
				ByCodeStaff.Add((int)item.CODE_STAFF, ByProf[item.PROFF_ID]);
				//FIXME Здесь неплохо было бы организовывать связь с профессией в кадровой базе.
			}
		}

		public void MarkAsUsed(Norm norm)
		{
			if(UsedNorms.Add(norm)) {
				foreach(var item in norm.Items)
					protectionTools.MarkAsUsed(item.ProtectionTools);
			}
		}

		public void Save()
		{
			logger.Info($"Сохраняем нормы...");
			int i = 0;
			var toSave = ChangedNorms.Where(x => UsedNorms.Contains(x)).ToList();
			foreach(var norm in UsedNorms) {
				uow.Save(norm);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / toSave.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.WriteLine($"Обновили {toSave.Count} норм.");
		}

		private void AddNormToProfDic(string proffId, Norm norm)
		{
			if(!ByProf.ContainsKey(proffId) || ByProf[proffId].DateTo < norm.DateTo) {
				ByProf[proffId] = norm;
			}
		}
	}
}
