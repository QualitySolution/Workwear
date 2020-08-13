using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DownloadNLMK.Loaders.DTO;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Regulations;

namespace DownloadNLMK.Loaders
{
	public class EmployeeLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWork uow;
		private readonly NormLoader norms;
		private readonly ProtectionToolsLoader protectionTools;
		private readonly NomenclatureLoader nomenclature;
		private readonly Dictionary<EmployeeCard, List<EmployeeOperation>> Operations = new Dictionary<EmployeeCard, List<EmployeeOperation>>();
		public Dictionary<string, EmployeeCard> ByID = new Dictionary<string, EmployeeCard>();

		public HashSet<EmployeeCard> UsedEmployees = new HashSet<EmployeeCard>();

		public EmployeeLoader(IUnitOfWork uow, NormLoader norms, ProtectionToolsLoader protectionTools, NomenclatureLoader nomenclature)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.norms = norms ?? throw new ArgumentNullException(nameof(norms));
			this.protectionTools = protectionTools ?? throw new ArgumentNullException(nameof(protectionTools));
			this.nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Загружаем PERSONAL_CARD");
			var PERSONAL_CARD = connection.Query("SELECT c.TN, c.PERSONAL_CARD_ID, " +
				"t.SURNAME, t.NAME, t.SECNAME, t.E_SEX, t.DUVOL, t.DHIRING, t.E_PROF, t.PARENT_DEPT_CODE, t.ID_DEPT, t.ID_WP " +
				"FROM SKLAD.PERSONAL_CARD c " +
				"INNER JOIN KIT.EXP_HUM_SKLAD t ON t.TN = c.TN " +
				"WHERE c.TN IS NOT NULL ", buffered: false);

			logger.Info("Загружаем RELAT_PERS_PROFF");
			var RELAT_PERS_PROFF = connection.Query("SELECT * FROM SKLAD.RELAT_PERS_PROFF")
				.ToDictionary<dynamic, string>(x => x.PERSONAL_CARD_ID);
			logger.Info($"Загружено {RELAT_PERS_PROFF.Count()} RELAT_PERS_PROFF");

			logger.Info("Обработка PERSONAL_CARD");
			int skipCards = 0;
			int processed = 0;
			double totalRows = PERSONAL_CARD.Count();
			foreach(var row in PERSONAL_CARD) {
				processed++;
				if(processed % 100 == 0)
					Console.Write($"\r\tОбработано карточек {processed} [{processed / totalRows:P}]... ");
				if(row.TN == null) {
					//FIXME Пока не загружаем карточки без TN, возможно в будущем надо реализовать.
					skipCards++;
					continue;
				}
				
				EmployeeCard card = new EmployeeCard();
				card.PersonnelNumber = row.TN.ToString();
				card.LastName = row.SURNAME;
				card.FirstName = row.NAME;
				card.Patronymic = row.SECNAME;
				card.Sex = row.E_SEX == 2 ? Sex.F : (row.E_SEX == 1 ? Sex.M : Sex.None); ;
				card.DismissDate = row.DUVOL;
				card.HireDate = row.DHIRING;

				card.ProfessionId = (int?)row.E_PROF;
				card.SubdivisionId = (int?)row.PARENT_DEPT_CODE;
				card.DepartmentId = (int?)row.ID_DEPT;
				card.PostId = (int?)row.ID_WP;
				ByID.Add(row.PERSONAL_CARD_ID, card);
				Operations[card] = new List<EmployeeOperation>();

				//Связываем с нормой
				if(RELAT_PERS_PROFF.ContainsKey(row.PERSONAL_CARD_ID)) {
					var proff = RELAT_PERS_PROFF[row.PERSONAL_CARD_ID];
					if(norms.ByProf.ContainsKey(proff.PROFF_ID)) {
						var norm = norms.ByProf[proff.PROFF_ID];
						card.UsedNorms.Add(norm);
						}
				}
				if(card.PostId.HasValue && norms.ByCodeStaff.ContainsKey(card.PostId.Value) 
					&& !card.UsedNorms.Contains(norms.ByCodeStaff[card.PostId.Value])) {
					card.UsedNorms.Add(norms.ByCodeStaff[card.PostId.Value]);
				}
				if(card.UsedNorms.Any()) {
					if(card.UsedNorms.Count > 1) {
						logger.Warn($"У TN={card.PersonnelNumber} более одной нормы.");
						card.UpdateWorkwearItems();
					}
					else {
						foreach(var normItem in card.UsedNorms.First().Items) {
							card.WorkwearItems.Add(new EmployeeCardItem(card, normItem));
						}
					}
				}
			}
			Console.Write("Готово\n");
			logger.Info($"Пропущено {skipCards} карточек без ТН.");
			logger.Info($"Обработано {ByID.Count()} личных карточек.");
			logger.Info($"C профессией {ByID.Values.Count(x => x.UsedNorms.Any())}.");
			PERSONAL_CARD = null;
			RELAT_PERS_PROFF = null;

			logger.Info("Загружаем PERSONAL_CARDS");
			var PERSONAL_CARDS = connection.Query("SELECT r.PERSONAL_CARD_ID, r.NORMA_ROW_ID, sms.MAT, sms.DOTP, sms.KOLMOTP, sm.TYPE, ADD_MONTHS(sms.DOTP, WEARING_PERIOD) expiry " +
				"FROM SKLAD.PERSONAL_CARDS r " +
				"INNER JOIN SKLAD.NORMA_ROW ON SKLAD.NORMA_ROW.NORMA_ROW_ID = r.NORMA_ROW_ID " +
				"INNER JOIN SKLAD.NORMA norma ON norma.NORMA_ID = SKLAD.NORMA_ROW.NORMA_ID " +
				"INNER JOIN SKLAD.SMSFORMA sms ON sms.IDFORMS = r.IDFORMS " +
				"INNER JOIN SKLAD.smforma sm ON sms.idform = sm.idform " +
				"WHERE sms.KOLMOTP IS NOT NULL AND sysdate <= ADD_MONTHS(sms.DOTP, WEARING_PERIOD) " +
				"AND r.PERSONAL_CARD_ID IN (SELECT c.PERSONAL_CARD_ID FROM SKLAD.PERSONAL_CARD c WHERE c.TN IN(SELECT TN FROM KIT.EXP_HUM_SKLAD)) " +
				"ORDER BY sms.DOTP",
				buffered: false);

			logger.Info("Обработка строк карточек...");
			totalRows = PERSONAL_CARDS.Count();
			int cardSkippedRows = 0;
			processed = 0;
			foreach(var item in PERSONAL_CARDS) {
				processed++;
				if(processed % 500 == 0)
					Console.Write($"\r\tОбработано строк {processed} [{processed / totalRows:P}]... ");
				if(!ByID.ContainsKey(item.PERSONAL_CARD_ID) || item.NORMA_ROW_ID == null) {
					logger.Warn($"Строка для карточки PERSONAL_CARD_ID={item.PERSONAL_CARD_ID} пропущена");
					cardSkippedRows++;
					continue;
				}

				EmployeeCard card = ByID[item.PERSONAL_CARD_ID];
				NormItem normRow = norms.RowsByID[item.NORMA_ROW_ID];

				EmployeeCardItem cardItem = card.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem == normRow);
				if(cardItem == null) {
					cardItem = card.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem.ProtectionTools == normRow.ProtectionTools);
				}
				if(cardItem == null) {
					cardItem = new EmployeeCardItem(card, normRow);
					card.WorkwearItems.Add(cardItem);
				}
				if(item.TYPE == "2")
					cardItem.Amount += Convert.ToInt32(item.KOLMOTP);
				else
					cardItem.Amount -= Convert.ToInt32(item.KOLMOTP);

				if((cardItem.LastIssue ?? default(DateTime)) < item.DOTP ?? item.TYPE == "2") {
					cardItem.LastIssue = item.DOTP;
				}

				cardItem.NextIssue = normRow.CalculateExpireDate(cardItem.LastIssue.Value, cardItem.Amount);

				var issueNomenclature = nomenclature.ByID.ContainsKey(item.MAT)
					? nomenclature.ByID[item.MAT]
					: cardItem.ProtectionTools.MatchedNomenclatures.FirstOrDefault();

				var operation = new EmployeeOperation {
					Employee = card,
					NormItem = normRow,
					returned = item.TYPE == "1" ? Convert.ToInt32(item.KOLMOTP) : 0,
					issued = item.TYPE == "2" ? Convert.ToInt32(item.KOLMOTP) : 0,
					auto_writeoff_date = item.TYPE == "2" ? item.expiry : null,
					ProtectionTools = cardItem.ProtectionTools,
					ExpiryByNorm = item.TYPE == "2" ? item.expiry : null,
					Nomenclature = issueNomenclature,
					operation_time = item.DOTP,
					StartOfUse = item.TYPE == "2" ? item.DOTP : null,
				};
				Operations[card].Add(operation);
			}
			Console.Write("Готово\n");

			logger.Info("Проставляем пропущенные нормы...");
			int setNormByExpense = 0;
			int moreOneNorms = 0;
			int OneActiveNorms = 0;
			foreach(var employee in ByID.Values.Where(x => !x.UsedNorms.Any() && x.WorkwearItems.Any())) {
				setNormByExpense++;
				Norm selectNorm;
				var employeeNorms = employee.WorkwearItems.Select(x => x.ActiveNormItem.Norm).Distinct().ToList();
				var employeeActiveNorms = employeeNorms.Where(x => x.IsActive).ToList();
				if(employeeNorms.Count == 1)
					selectNorm = employeeNorms.First();
				else if(employeeActiveNorms.Count == 1) {
					OneActiveNorms++;
					selectNorm = employeeActiveNorms.First();
				}
				else {
					moreOneNorms++;
					selectNorm = employeeNorms.OrderByDescending(x => employee.WorkwearItems.Count(i => i.ActiveNormItem.Norm == x)).First();
				}

				employee.UsedNorms.Add(selectNorm);
				foreach(var item in selectNorm.Items) {
					var found = employee.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem == item);
					if(found == null)
						found = employee.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem.ProtectionTools == item.ProtectionTools);
					else
						continue;
					if(found == null)
						found = employee.WorkwearItems.FirstOrDefault(x => item.ProtectionTools.Analogs.Contains(x.ActiveNormItem.ProtectionTools));
					if(found == null) {
						employee.WorkwearItems.Add(new EmployeeCardItem(employee, item));
					}
					else
						found.ActiveNormItem = item;
				}
			}

			logger.Info($"В {setNormByExpense} карточках норма установлена по последним выдачам.");
			logger.Info($"В {OneActiveNorms} карточках в одна активная нормы.");
			logger.Info($"В {moreOneNorms} карточках в выдачах больше одной нормы.");
			logger.Info($"В итоге {ByID.Count(x => x.Value.UsedNorms.Any())} карточек с нормами.");
			logger.Info($"{ByID.Values.Sum(e => e.WorkwearItems.Count(x => !e.UsedNorms.Contains(x.ActiveNormItem.Norm)))} строк осталось без активной нормы.");
			logger.Info($"{ByID.Count(x => (x.Value.UsedNorms.FirstOrDefault()?.Items.Count ?? 0) != x.Value.WorkwearItems.Count )} карточек c отличным от нормы количеством строк.");
			logger.Info($"Карточек без строк: {ByID.Values.Count(x => !x.WorkwearItems.Any())}");
		}

		public void MarkAsUsed(EmployeeCard employee)
		{
			if(UsedEmployees.Add(employee)) {
				foreach(var norm in employee.UsedNorms)
					norms.MarkAsUsed(norm);
				foreach(var item in employee.WorkwearItems)
					norms.MarkAsUsed(item.ActiveNormItem.Norm);
				foreach(var operation in Operations[employee]) {
					nomenclature.MarkAsUsed(operation.Nomenclature);
					norms.MarkAsUsed(operation.NormItem.Norm);
				}
			}
		}

		public void Save()
		{
			logger.Info($"Сохраняем личные карточки...");
			int i = 0;
			foreach(var card in UsedEmployees) {
				uow.Save(card, orUpdate: false);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / UsedEmployees.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.Write("Завершено\n");
			var sql = "INSERT INTO operation_issued_by_employee " +
				"(`employee_id`, `operation_time`, `nomenclature_id`, `issued`, `returned`, `auto_writeoff`, `auto_writeoff_date`, `protection_tools_id`, `norm_item_id`, `StartOfUse`, `ExpiryByNorm`, wear_percent) " +
				"Values (@employee_id, @operation_time, @nomenclature_id, @issued, @returned, @auto_writeoff, @auto_writeoff_date, @protection_tools_id, @norm_item_id, @StartOfUse, @ExpiryByNorm, @wear_percent);";

			logger.Info($"Сохраняем операции выдачи...");
			var listToInsert = new List<EmployeeOperation>();
			i = 0;
			int amountSkip = 0;
			int processed = 0;
			foreach(var pair in Operations) {
				if(!UsedEmployees.Contains(pair.Key))
					continue;

				processed++;

				foreach(var item in pair.Value) {
					if(item.nomenclature_id == 0) {
						logger.Warn($"Выдачу {item.Title} пропускаем так как не нашли номеклатуру.");
						continue;
					}
					if(item.issued == 0 && item.returned == 0) {
						amountSkip++;
						continue;
					}

					listToInsert.Add(item);
					i++;
				}

				if(listToInsert.Count > 1000) {
					uow.Session.Connection.Execute(sql, listToInsert);	
					Console.Write($"\r\tСохранили {i} [{(float)processed / Operations.Count:P}]... ");
					listToInsert.Clear();
				}
			}
			if(listToInsert.Count > 0)
				uow.Session.Connection.Execute(sql, listToInsert);

			Console.Write("Завершено\n");

			logger.Info($"Сохранено {i} операций выдачи");
			logger.Info($"Пропущено {amountSkip} операций с нулевый количеством.");
		}
	}
}