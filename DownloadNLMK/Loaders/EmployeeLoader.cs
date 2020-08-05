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
			var PERSONAL_CARD = connection.Query("SELECT c.TN, c.PERSONAL_CARD_ID FROM SKLAD.PERSONAL_CARD c WHERE c.TN IN(SELECT TN FROM KIT.EXP_HUM_SKLAD)"); //FIXME Ускоряем не грузим карточки без сотрудника.
			logger.Info($"Загружено {PERSONAL_CARD.Count()} PERSONAL_CARD");

			logger.Info("Загружаем EXP_HUM_SKLAD");
			var EXP_HUM_SKLAD = connection.Query(
				"SELECT t.TN, t.SURNAME, t.NAME, t.SECNAME, t.E_SEX, t.DUVOL, t.DHIRING, t.E_PROF, t.PARENT_DEPT_CODE, t.ID_DEPT, t.ID_WP " +
				"FROM KIT.EXP_HUM_SKLAD t " +
				"WHERE t.TN IN (SELECT TN FROM SKLAD.PERSONAL_CARD)")//FIXME Ускоряем не грузим сотрудников без карточек.
				.ToDictionary<dynamic, decimal>(x => x.TN);
			logger.Info($"Загружено {EXP_HUM_SKLAD.Count()} EXP_HUM_SKLAD");

			logger.Info("Загружаем RELAT_PERS_PROFF");
			var RELAT_PERS_PROFF = connection.Query("SELECT * FROM SKLAD.RELAT_PERS_PROFF")
				.ToDictionary<dynamic, string>(x => x.PERSONAL_CARD_ID);
			logger.Info($"Загружено {RELAT_PERS_PROFF.Count()} RELAT_PERS_PROFF");

			logger.Info("Обработка PERSONAL_CARD");
			int skipCards = 0;
			int withNorm = 0;
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
				if(!EXP_HUM_SKLAD.ContainsKey(row.TN)) {
					logger.Error($"Сотрудник с TN={row.TN} не найден.");
					continue;
				}
				var info = EXP_HUM_SKLAD[row.TN];
				EmployeeCard card = new EmployeeCard();
				card.PersonnelNumber = row.TN.ToString();
				card.LastName = info.SURNAME;
				card.FirstName = info.NAME;
				card.Patronymic = info.SECNAME;
				card.Sex = info.E_SEX == 2 ? Sex.F : (info.E_SEX == 1 ? Sex.M : Sex.None); ;
				card.DismissDate = info.DUVOL;
				card.HireDate = info.DHIRING;

				card.ProfessionId = (int?)info.E_PROF;
				card.SubdivisionId = (int?)info.PARENT_DEPT_CODE;
				card.DepartmentId = (int?)info.ID_DEPT;
				card.PostId = (int?)info.ID_WP;
				ByID.Add(row.PERSONAL_CARD_ID, card);
				Operations[card] = new List<EmployeeOperation>();

				//Связываем с нормой
				if(!RELAT_PERS_PROFF.ContainsKey(row.PERSONAL_CARD_ID)) {
					//logger.Warn($"Для {card.ShortName} TN={row.TN} связь с профессией ОМТР не найдена.");
					continue;
				}
				var proff = RELAT_PERS_PROFF[row.PERSONAL_CARD_ID];
				if(!norms.ByProf.ContainsKey(proff.PROFF_ID)) {
					logger.Warn($"Для {card.ShortName} TN={row.TN} PROFF_ID={proff.PROFF_ID} норма не найдена.");
					continue;
				}
				var norm = norms.ByProf[proff.PROFF_ID];
				card.UsedNorms.Add(norm);
				withNorm++;
			}
			Console.Write("Готово\n");
			logger.Info($"Пропущено {skipCards} карточек без ТН.");
			logger.Info($"Обработано {ByID.Count()} личных карточек.");
			logger.Info($"C профессией {withNorm}.");
			PERSONAL_CARD = null;
			EXP_HUM_SKLAD = null;
			RELAT_PERS_PROFF = null;

			logger.Info("Загружаем PERSONAL_CARDS");
			var PERSONAL_CARDS = connection.Query("SELECT r.PERSONAL_CARD_ID, r.NORMA_ROW_ID, sms.MAT, sms.DOTP, sms.KOLMOTP " +
				"FROM SKLAD.PERSONAL_CARDS r " +
				"INNER JOIN SKLAD.NORMA_ROW ON SKLAD.NORMA_ROW.NORMA_ROW_ID = r.NORMA_ROW_ID " +
				"INNER JOIN SKLAD.NORMA norma ON norma.NORMA_ID = SKLAD.NORMA_ROW.NORMA_ID " +
				"INNER JOIN SKLAD.SMSFORMA sms ON sms.IDFORMS = r.IDFORMS " +
				"WHERE sysdate BETWEEN norma.DATE_BEGIN AND norma.DATE_END " +
				"AND r.PERSONAL_CARD_ID IN (SELECT c.PERSONAL_CARD_ID FROM SKLAD.PERSONAL_CARD c WHERE c.TN IN(SELECT TN FROM KIT.EXP_HUM_SKLAD))");

			logger.Info("Обработка строк карточек...");
			totalRows = PERSONAL_CARDS.Count();
			int cardSkippedRows = 0;
			processed = 0;
			foreach(var item in PERSONAL_CARDS) {
				processed++;
				if(processed % 100 == 0)
					Console.Write($"\r\tОбработано строк {processed} [{processed / totalRows:P}]... ");
				if(!ByID.ContainsKey(item.PERSONAL_CARD_ID) || item.NORMA_ROW_ID == null) {
					logger.Warn($"Строка для карточки PERSONAL_CARD_ID={item.PERSONAL_CARD_ID} пропущена");
					cardSkippedRows++;
					continue;
				}

				EmployeeCard card = ByID[item.PERSONAL_CARD_ID];
				NormItem normRow = norms.RowsByID[item.NORMA_ROW_ID];
				if(normRow.Norm.IsActive && !card.UsedNorms.Contains(normRow.Norm)) {
					card.UsedNorms.Add(normRow.Norm);
					withNorm++;
				}
				EmployeeCardItem cardItem = card.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem == normRow);
				if(cardItem == null) {
					cardItem = new EmployeeCardItem(card, normRow);
					card.WorkwearItems.Add(cardItem);
				}
			 	if((cardItem.LastIssue ?? default(DateTime)) < item.DOTP) {
					cardItem.LastIssue = item.DOTP;
					cardItem.Amount = Convert.ToInt32(item.KOLMOTP);
					cardItem.NextIssue = normRow.CalculateExpireDate(cardItem.LastIssue.Value, cardItem.Amount);
				}

				var issueNomenclature = nomenclature.ByID.ContainsKey(item.MAT)
					? nomenclature.ByID[item.MAT]
					: cardItem.Item.MatchedNomenclatures.FirstOrDefault();

				var operation = new EmployeeOperation {
					Employee = card,
					NormItem = normRow,
					returned =0,
					issued = cardItem.Amount,
					auto_writeoff_date = cardItem.NextIssue,
					ProtectionTools = cardItem.Item,
					ExpiryByNorm = cardItem.NextIssue,
					Nomenclature = issueNomenclature,
					operation_time = item.DOTP,
					StartOfUse = cardItem.LastIssue,
				};
				Operations[card].Add(operation);
			}
			Console.Write("Готово\n");
			logger.Info($"Пропущено {skipCards} строк карточек");
			logger.Info($"В итоге {withNorm} карточек с нормами.");
			logger.Info($"Карточек без строк: {ByID.Values.Count(x => !x.WorkwearItems.Any())}");
		}

		public void MarkAsUsed(EmployeeCard employee)
		{
			if(UsedEmployees.Add(employee)) {
				foreach(var norm in employee.UsedNorms)
					norms.MarkAsUsed(norm);
			}
		}

		public void Save()
		{
			logger.Info($"Сохраняем личные карточки...");
			int i = 0;
			foreach(var card in UsedEmployees) {
				uow.Save(card);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / UsedEmployees.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.Write("Завершено\n");
			var sql = "INSERT INTO operation_issued_by_employee " +
				"(`employee_id`, `operation_time`, `nomenclature_id`, `issued`, `returned`, `auto_writeoff`, `auto_writeoff_date`, `protection_tools_id`, `norm_item_id`, `StartOfUse`, `ExpiryByNorm`) " +
				"Values (@employee_id, @operation_time, @nomenclature_id, @issued, @returned, @auto_writeoff, @auto_writeoff_date, @protection_tools_id, @norm_item_id, @StartOfUse, @ExpiryByNorm);";

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
					if(item.issued == 0) {
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