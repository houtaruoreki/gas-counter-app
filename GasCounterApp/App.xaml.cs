using GasCounterApp.Views;
using GasCounterApp.Services;

namespace GasCounterApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Set app theme to follow system theme
		UserAppTheme = AppTheme.Unspecified;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Use dependency injection to create MapPage with services
		var databaseService = Handler.MauiContext?.Services.GetService<DatabaseService>() ?? new DatabaseService();
		var locationService = Handler.MauiContext?.Services.GetService<LocationService>() ?? new LocationService();
		var backupService = Handler.MauiContext?.Services.GetService<BackupService>() ?? new BackupService(databaseService);

		return new Window(new NavigationPage(new MapPage(databaseService, locationService, backupService)));
	}
}