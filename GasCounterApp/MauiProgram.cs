using Microsoft.Extensions.Logging;
using GasCounterApp.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace GasCounterApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register services
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<LocationService>();
		builder.Services.AddSingleton<BackupService>();

		// Initialize SQLite
		SQLitePCL.Batteries_V2.Init();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
