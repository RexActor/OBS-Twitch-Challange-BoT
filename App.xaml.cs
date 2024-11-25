using OBS_Twitch_Challange_BoT.Services;

using System.Configuration;
using System.Data;
using System.Windows;

namespace OBS_Twitch_Challange_BoT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create an instance of ObsService
            ObsService obsService = new ObsService();

            // Pass ObsService to MainWindow
            MainWindow mainWindow = new MainWindow(obsService);
            mainWindow.Show();
        }
    }

}
