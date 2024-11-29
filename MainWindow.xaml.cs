using OBS_Twitch_Challange_BoT.Services;

using System.Diagnostics;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OBS_Twitch_Challange_BoT
{




	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		private WebSocketServerService _webSocketServer;
		private readonly ObsService _obsService;
		private readonly HtmlService _htmlService;
		private readonly TwitchService _twitchService;
		private readonly LogService _logService;

		// Parameterless constructor for WPF
		public MainWindow() : this(null, new HtmlService(), null, null)
		{

			_logService = new LogService();
			_twitchService = new TwitchService(_obsService, _logService);
			_obsService = new ObsService(_logService);

		}

		public MainWindow(ObsService obsService, HtmlService htmlService, TwitchService twitchService, LogService logService)
		{

			InitializeComponent();



			_htmlService = htmlService;
			_obsService = obsService;
			_twitchService = twitchService;
			_logService = logService;


			_obsService.ObsConnectionChanged += _obsService_ObsConnectionChanged;
			_twitchService.TwitchConnectionChanged += _twitchService_TwitchConnectionChanged;


			InitializeCommandsTab();
			InitializeSettingsTab();
			InitializeConsoleTab();

		}



		private void StartWebSocketServer()
		{
			_webSocketServer = new WebSocketServerService(_obsService, _twitchService, _logService);
			_webSocketServer.Start();
		}

		// Ensure to stop the WebSocket server when the application is closed
		private void Window_Closed(object sender, EventArgs e)
		{
			_webSocketServer.Stop();
		}

		private void _twitchService_TwitchConnectionChanged(bool obj)
		{
			Dispatcher.Invoke(() =>
			{
				TwitchConnectionLbl.Content = obj ? "Connected" : "Disconnected";
				TwitchConnectBtn.Content = obj ? "Disconnect Twitch" : "Connect to Twitch";
				TwitchConnectionLbl.Foreground = new SolidColorBrush(obj ? Color.FromRgb(34, 139, 34) : Color.FromRgb(220, 20, 60));


				if (obj)
				{

					TwitchConnectBtn.Click -= TwitchConnectBtn_Click;
					TwitchConnectBtn.Click += TwitchDisconnectBtn_Click;
				}
				else
				{
					TwitchConnectBtn.Click += TwitchConnectBtn_Click;
					TwitchConnectBtn.Click -= TwitchDisconnectBtn_Click;
				}




			});
		}

		private void _obsService_ObsConnectionChanged(bool obj)
		{
			Dispatcher.Invoke(() =>
			{
				ObsConnectionLbl.Content = obj ? "Connected" : "Disconnected";
				ObsConnectBtn.Content = obj ? "Disconnect OBS" : "Connect to OBS";
				ObsConnectionLbl.Foreground = new SolidColorBrush(obj ? Color.FromRgb(34, 139, 34) : Color.FromRgb(220, 20, 60));

				if (obj)
				{

					StartWebSocketServer();
					ObsConnectBtn.Click -= ObsConnectBtn_Click;
					ObsConnectBtn.Click += ObsConnectBtn_Disconnect;
				}
				else
				{
					_webSocketServer.Stop();
					ObsConnectBtn.Click += ObsConnectBtn_Click;
					ObsConnectBtn.Click -= ObsConnectBtn_Disconnect;
				}


			});
		}

		private void ObsConnectBtn_Disconnect(object sender, RoutedEventArgs e)
		{

			_obsService.DisconnectWebSocket();

		}

		private void ObsConnectBtn_Click(object sender, RoutedEventArgs e)
		{

			_obsService.ConnectWebSocket(Properties.Settings.Default.ObsIP, Properties.Settings.Default.ObsPort, Properties.Settings.Default.ObsPassword);


		}

		private void TwitchConnectBtn_Click(object sender, RoutedEventArgs e)
		{
			_twitchService.ConnectToTwitch();



		}
		private void TwitchDisconnectBtn_Click(object sender, RoutedEventArgs e)
		{

			_twitchService.Disconnect();

		}


		private void InitializeSettingsTab()
		{
			SettingsContentControl.Content = new OptionsPage(_obsService, _htmlService, _logService);

		}

		private void InitializeCommandsTab()
		{
			CommandsContentControl.Content = new CommandPage();
		}
		private void InitializeConsoleTab()
		{
			ConsoleTab.Content = new ConsolePage(_logService);
		}
		private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			if (MainTabControl.SelectedItem is TabItem selectedTab && selectedTab.Header.ToString() == "Settings")
			{
				if (SettingsContentControl.Content is OptionsPage optionsPage)
				{



					//optionsPage.ReloadSettings();          // Reload settings


				}

			}
		}
	}
}