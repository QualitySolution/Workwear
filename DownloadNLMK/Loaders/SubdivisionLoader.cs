using System;
using System.Collections.Generic;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using workwear.Domain.Company;

namespace DownloadNLMK.Loaders
{
	public class SubdivisionLoader
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWork uow;
		public Dictionary<string, Subdivision> ByID = new Dictionary<string, Subdivision>();
		public HashSet<Subdivision> UsedSubdivision = new HashSet<Subdivision>();

		public SubdivisionLoader(IUnitOfWork uow)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Загружаем SKLAD.SGRPOL");
			var sgrpol = connection.Query("SELECT * FROM SKLAD.SGRPOL");
			logger.Info("Обработка SKLAD.SGRPOL");
			foreach(var row in sgrpol) {
				var subdivision = new Subdivision {
					Name = row.NGRPOL,
					Code = row.KGRPOL,
				};
				if(ByID.ContainsKey(row.KGRPOL)) {
					logger.Error($"Дубль строки для SGRPOL {row.KGRPOL}\n >>{ByID[row.KGRPOL].Name}\n >>{subdivision.Name}");
					continue;
				}
				ByID.Add(row.KGRPOL, subdivision);
				if(subdivision.Name == null) {
					subdivision.Name = $"Без названия";
					logger.Error($"Для подразделение {subdivision.Code} нет названия.");
					continue;
				}
			}
		}

		public void MarkAsUsed(Subdivision subdivision)
		{
			UsedSubdivision.Add(subdivision);
		}

		public void Save()
		{
			logger.Info($"Сохраняем подразделения...");
			int i = 0;
			foreach(var item in UsedSubdivision) {
				uow.Save(item, orUpdate: false);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / UsedSubdivision.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.Write("Завершено\n");
		}
	}
}
