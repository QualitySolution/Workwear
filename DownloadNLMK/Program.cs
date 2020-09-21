
using System;
using System.Linq;
using DownloadNLMK.Loaders;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Services.Interactive;
using QS.Services;
using QSMachineConfig;
using workwear.Tools.Oracle;

namespace DownloadNLMK
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
				var start = DateTime.Now;

				var subdivisions = new SubdivisionLoader(uow);
				subdivisions.Load(NLMKOracle.Connection);

				var nomenclatures = new NomenclatureLoader(uow);
				nomenclatures.Load(NLMKOracle.Connection);

				var protectionTools = new ProtectionToolsLoader(uow, nomenclatures);
				protectionTools.Load(NLMKOracle.Connection);

				var norms = new NormLoader(uow, protectionTools);
				norms.Load(NLMKOracle.Connection);

				var employees = new EmployeeLoader(uow, norms, protectionTools, nomenclatures, subdivisions);
				employees.Load(NLMKOracle.Connection);

				//Помечаем какие сохранять.
				foreach(var employee in employees.ByID.Values) {
					employees.MarkAsUsed(employee);
				}

				//Добавляем неактивные нормы тем у кого совсем нет норм.
				foreach(var employee in employees.ByID.Values.Where(x => !x.UsedNorms.Any() && x.WorkwearItems.Any())) {
					var norm = employee.WorkwearItems.Select(x => x.ActiveNormItem.Norm).Distinct().OrderByDescending(x => x.DateTo).First();
					employee.UsedNorms.Add(norm);
				}

				logger.Info($"Использовано {subdivisions.UsedSubdivision.Count} из {subdivisions.ByID.Count} подразделений.");
				logger.Info($"Использовано {nomenclatures.UsedNomenclatures.Count} из {nomenclatures.ByID.Count} номенклатур.");
				logger.Info($"Использовано {protectionTools.UsedProtectionTools.Count} из {protectionTools.ByID.Count} СИЗ-ов.");
				logger.Info($"Использовано {norms.UsedNorms.Count} из {norms.ByID.Count} норм.");
				logger.Info($"Использовано {employees.UsedEmployees.Count} из {employees.ByID.Count} сотрудников.");

				logger.Info($"Сотрудников без норм {employees.UsedEmployees.Count(x => x.UsedNorms.Count == 0)}");
				logger.Info($"Сотрудников с истекшими норами {employees.UsedEmployees.Count(x => x.UsedNorms.Any(n => !n.IsActive))}");
				logger.Info($"Сотрудников без выдач {employees.UsedEmployees.Count(x => x.WorkwearItems.Count == 0)}");
				logger.Info($"Сотрудников без подразделения {employees.UsedEmployees.Count(x => x.Subdivision == null)}");
#if !NOSAVE
				subdivisions.Save();
				nomenclatures.Save();
				protectionTools.Save();
				norms.Save();
				employees.Save();
#endif
				logger.Info($"Задача выполнена за {DateTime.Now - start}");
			}
			logger.Info("Работа завершена. Нажмите любую кнопку для закрытия консоли...");
			Console.ReadLine();
		}
	}
}
