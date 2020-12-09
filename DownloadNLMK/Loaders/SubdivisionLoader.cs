using System;
using System.Collections.Generic;
using System.Linq;
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
		public Dictionary<string, Subdivision> ByID;
		public HashSet<Subdivision> UsedSubdivision = new HashSet<Subdivision>();
		public HashSet<Subdivision> ChangedSubdivision = new HashSet<Subdivision>();

		public SubdivisionLoader(IUnitOfWork uow)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public void Load(OracleConnection connection)
		{
			logger.Info("Загружаем имеющиеся подразделения");
			ByID = uow.GetAll<Subdivision>().ToDictionary(x => x.Code, x => x);

			logger.Info("Загружаем SKLAD.SGRPOL");
			var sgrpol = connection.Query("SELECT * FROM SKLAD.SGRPOL");
			logger.Info("Обработка SKLAD.SGRPOL");
			foreach(var row in sgrpol) {
				Subdivision subdivision;
				if(ByID.TryGetValue(row.KGRPOL, out subdivision)) {
					if(subdivision.Name != row.NGRPOL) {
						subdivision.Name = row.NGRPOL;
						ChangedSubdivision.Add(subdivision);
					}
				}
				else
				{
					subdivision = new Subdivision {
						Name = row.NGRPOL,
						Code = row.KGRPOL,
					};
					ByID.Add(row.KGRPOL, subdivision);
					ChangedSubdivision.Add(subdivision);
				}

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
			var toSave = ChangedSubdivision.Where(x => UsedSubdivision.Contains(x)).ToList();
			foreach(var item in toSave) {
				uow.Save(item);
				i++;
				if(i % 100 == 0) {
					uow.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / toSave.Count:P}]... ");
				}
			}
			uow.Commit();
			Console.WriteLine($"Обновили {toSave.Count} подразделений.");
		}
	}
}
