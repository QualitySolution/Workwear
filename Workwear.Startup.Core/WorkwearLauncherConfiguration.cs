using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using QS.Cloud.Client;
using QS.Launcher;
using QS.Project.Versioning;
using QS.Utilities.Extensions;

namespace Workwear.Startup
{
	/// <summary>
	/// Общая конфигурация лаунчера для приложения спецодежды.
	/// </summary>
	public static class WorkwearLauncherConfiguration
	{
		/// <summary>
		/// Добавляет общую конфигурацию лаунчера в ServiceCollection.
		/// </summary>
		/// <param name="services">Коллекция сервисов.</param>
		/// <param name="configureOptions">Опциональный action для донастройки LauncherOptions.</param>
		/// <returns>ServiceCollection для цепочки вызовов.</returns>
		public static IServiceCollection AddWorkwearLauncherConfiguration(
			this IServiceCollection services,
			Action<LauncherOptions> configureOptions = null)
		{
			var assembly = Assembly.GetExecutingAssembly();

			#region Типы подключений
			services.AddConnectionType(new QsCloudConnectionTypeBase());
			#endregion

			#region Настройки лаунчера
			var options = new LauncherOptions {
				AppTitle = "QS: Спецодежда",
				LogoImage = assembly.GetResourceByteArray("Workwear.Startup.Icons.logo.png"),
				LogoIcon = assembly.GetResourceByteArray("Workwear.Startup.Icons.logo128.ico"),
				ConnectionsJsonFileName = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"QS.Workwear",
					"connections.json"),
				OldConfigFilename = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"workwear.ini"),
				MakeDefaultConnections = () => new List<Dictionary<string, string>> {
					new Dictionary<string, string> {
						{"Title", "Демонстрационная(текущая)"},
						{"Type", "QSCloud"},
						{"Account", "demo"},
						{"Login", "demo"},
						{"Last", "true"},
					},
					new Dictionary<string, string> {
						{"Title", "Демонстрационная(стабильная)"},
						{"Type", "QSCloud"},
						{"Account", "demo"},
						{"Login", "demo"},
					}
				},
			};

			configureOptions?.Invoke(options);

			services.AddLauncherOptions(options);
			#endregion

			#region Application Info
			services.AddSingleton<IApplicationInfo, ApplicationInfo>(provider => new ApplicationInfo {
				ProductCode = 2,
			});
			#endregion

			return services;
		}
	}
}
