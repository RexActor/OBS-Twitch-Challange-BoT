using OBS_Twitch_Challange_BoT.Services;

using System.Diagnostics;
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
		private readonly ObsService _obsService;
		private readonly HtmlService _htmlService;
		private readonly TwitchService _twitchService;
		
		// Parameterless constructor for WPF
		public MainWindow() : this(new ObsService(), new HtmlService(), null)
		{
			_twitchService = new TwitchService(_obsService);

		}

		public MainWindow(ObsService obsService, HtmlService htmlService, TwitchService twitchService  )
		{

			InitializeComponent();
			_htmlService = htmlService;
			_obsService = obsService;
			_twitchService = twitchService;
			

			_obsService.ObsConnectionChanged += _obsService_ObsConnectionChanged;
			_twitchService.TwitchConnectionChanged += _twitchService_TwitchConnectionChanged;
			_htmlService.GenerateHTMLFile("Index.html");
			
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
					ObsConnectBtn.Click -= ObsConnectBtn_Click;
					ObsConnectBtn.Click += ObsConnectBtn_Disconnect;
				}
				else
				{
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



		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MainContentControl.Content = new OptionsPage(_obsService);
		}



		private void CommandsBtn_Click(object sender, RoutedEventArgs e)
		{
			MainContentControl.Content=new CommandPage();
		}
	}
}