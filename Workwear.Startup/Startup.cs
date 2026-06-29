using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using QS.Launcher;
using QS.Project;
using ReactiveUI.Avalonia;

namespace Workwear.Startup;

internal class Startup
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		ConfigureLauncherApp().StartWithClassicDesktopLifetime(args);
	}

	public static AppBuilder ConfigureLauncherApp()
	{
		ServiceCollection services = new();

		#region Настройки лаунчера
		services.AddWorkwearLauncherConfiguration(options => {
			options.IsStandalone = true;
		});
		#endregion

		#region Вычисляем путь к исполняемому файлу
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string exePath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "Application", "workwear.exe"));
		#endregion

		#region Лаунчер
		services
			.AddLauncherDependencies()
			.AddPages()
			.AddLauncherViewModels()
			.UseNewProcessRunner(exePath)
			.AddInteractive();
		#endregion

		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// You can't get any view from DI before Avalonia initialization.
		return AppBuilder.Configure(() => new LauncherApp {
				MainWindowGetter = () => serviceProvider.GetRequiredService<QS.Launcher.Views.MainWindow>()
			})
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();
	}
}
