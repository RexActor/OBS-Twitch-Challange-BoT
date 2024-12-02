using Microsoft.Extensions.DependencyInjection;

using OBS_Twitch_Challange_BoT.Services;

using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace OBS_Twitch_Challange_BoT
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	   
	{

		public static ServiceProvider ServiceProvider { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
		  

			var services = new ServiceCollection();
			ConfigureServices(services);
			ServiceProvider=services.BuildServiceProvider();



			// Pass ObsService to MainWindow
			var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
			mainWindow.Show();

			base.OnStartup(e);
		}

		private void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<ObsService>();
			services.AddSingleton<HtmlService>();
			services.AddSingleton<TwitchService>();
			services.AddSingleton<GameControlService>();
			services.AddSingleton<LogService>();
			services.AddTransient<MainWindow>();
			services.AddTransient<OptionsPage>();		
			
		}

	}

}
