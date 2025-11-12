using GasCounterApp.Views;

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
		return new Window(new NavigationPage(new MapPage()));
	}
}