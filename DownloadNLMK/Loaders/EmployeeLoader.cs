using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
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
		public Dictionary<string, EmployeeCard> ByID = new Dictionary<string, EmployeeCard>();

		public HashSet<EmployeeCard> UsedEmployees = new HashSet<EmployeeCard>();

		public EmployeeLoader(IUnitOfWork uow, NormLoader norms, ProtectionToolsLoader protectionTools)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.norms = norms ?? throw new ArgumentNullException(nameof(norms));
			this.protectionTools = protectionTools ?? throw new ArgumentNullException(nameof(protectionTools));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Загружаем PERSONAL_CARD");
			var PERSONAL_CARD = connection.Query("SELECT * FROM SKLAD.PERSONAL_CARD c WHERE c.TN IN(SELECT TN FROM KIT.EXP_HUM_SKLAD)"); //FIXME Ускоряем не грузим карточки без сотрудника.
			logger.Info($"Загружено {PERSONAL_CARD.Count()} PERSONAL_CARD");

			logger.Info("Загружаем EXP_HUM_SKLAD");
			var EXP_HUM_SKLAD = connection.Query("SELECT * FROM KIT.EXP_HUM_SKLAD t WHERE t.TN IN (SELECT TN FROM SKLAD.PERSONAL_CARD)")//FIXME Ускоряем не грузим сотрудников без карточек.
				.ToDictionary<dynamic, decimal>(x => x.TN);
			logger.Info($"Загружено {EXP_HUM_SKLAD.Count()} EXP_HUM_SKLAD");

			logger.Info("Загружаем RELAT_PERS_PROFF");
			var RELAT_PERS_PROFF = connection.Query("SELECT * FROM SKLAD.RELAT_PERS_PROFF")
				.ToDictionary<dynamic, string>(x => x.PERSONAL_CARD_ID);
			logger.Info($"Загружено {RELAT_PERS_PROFF.Count()} RELAT_PERS_PROFF");

			logger.Info("Обработка PERSONAL_CARD");
			var dicPERSONAL_CARD = new Dictionary<string, EmployeeCard>();
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
				dicPERSONAL_CARD.Add(row.PERSONAL_CARD_ID, card);

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
			logger.Info($"Обработано {dicPERSONAL_CARD.Count()} личных карточек.");
			logger.Info($"C профессией {withNorm}.");
			PERSONAL_CARD = null;
			EXP_HUM_SKLAD = null;
			RELAT_PERS_PROFF = null;

			logger.Info("Загружаем PERSONAL_CARDS");
			var PERSONAL_CARDS = connection.Query("SELECT * FROM SKLAD.PERSONAL_CARDS r " +
				"WHERE r.PROTECTION_ID IS NOT NULL AND r.PERSONAL_CARD_ID IN (SELECT c.PERSONAL_CARD_ID FROM SKLAD.PERSONAL_CARD c WHERE c.TN IN(SELECT TN FROM KIT.EXP_HUM_SKLAD))"); //FIXME Ускоряем не загружаем строки для сотрудников которых не грузили...
			logger.Info($"Загружено {PERSONAL_CARDS.Count()} PERSONAL_CARDS");

			logger.Info("Обработка строк карточек...");
			totalRows = PERSONAL_CARDS.Count();
			int cardSkippedRows = 0;
			processed = 0;
			foreach(var item in PERSONAL_CARDS) {
				processed++;
				if(processed % 100 == 0)
					Console.Write($"\r\tОбработано строк {processed} [{processed / totalRows:P}]... ");
				if(!dicPERSONAL_CARD.ContainsKey(item.PERSONAL_CARD_ID)) {
					logger.Warn($"Строка для карточки PERSONAL_CARD_ID={item.PERSONAL_CARD_ID} пропущена");
					cardSkippedRows++;
					continue;
				}
				EmployeeCard card = dicPERSONAL_CARD[item.PERSONAL_CARD_ID];
				EmployeeCardItem cardItem;
				if(item.NORMA_ROW_ID != null) {
					NormItem normRow = norms.RowsByID[item.NORMA_ROW_ID];
					cardItem = new EmployeeCardItem(card, normRow);
					if(!card.UsedNorms.Contains(normRow.Norm)) {
						card.UsedNorms.Add(normRow.Norm);
						withNorm++;
					}
					//if(!DomainHelper.EqualDomainObjects(cardItem.Item, dicProtectionTools[item.PROTECTION_ID])) {
					//	logger.Warn($"По в норме {cardItem.Item.Name}!={dicProtectionTools[item.PROTECTION_ID].Name}");
					//}
				}
				else {
					cardItem = new EmployeeCardItem {
						EmployeeCard = card,
						Item = protectionTools.ByID[item.PROTECTION_ID]
					};
				}
				card.WorkwearItems.Add(cardItem);
			}
			Console.Write("Готово\n");
			logger.Info($"Пропущено {skipCards} строк карточек");
			logger.Info($"В итоге {withNorm} карточек с нормами.");
			logger.Info($"Карточек без строк: {dicPERSONAL_CARD.Values.Count(x => !x.WorkwearItems.Any())}");
			PERSONAL_CARDS = null;
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
		}
	}
}
