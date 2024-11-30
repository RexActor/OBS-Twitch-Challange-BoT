using OBS_Twitch_Challange_BoT.Services;

using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using OBS_Twitch_Challange_BoT.Models;
using Newtonsoft.Json;

namespace OBS_Twitch_Challange_BoT
{
	/// <summary>
	/// Interaction logic for OptionsPage.xaml
	/// </summary>
	public partial class OptionsPage : UserControl
	{

		private readonly ObsService _obsService;
		private readonly HtmlService _htmlService;
		private readonly LogService _logService;


		//Setting variables for Twitch
		public string TwitchUserName { get; set; }
		public string TwitchAuth { get; set; }
		public string TwitchChannel { get; set; }


		//Setting variables for OBS source and Scene which one will be manipulated
		public string ObsScene { get; set; }
		public string ObsSourceTitle { get; set; }
		public string ObsSourceDesc { get; set; }

		public string ObsSourceOverlay { get; set; }

		public string ObsSceneOverlay { get; set; }

		public string ObsBrowserSource { get; set; }

		public string ChallangeList { get; set; }


		//Setting variables for OBS
		public string ObsAddress { get; set; }
		public int ObsPort { get; set; }
		public string ObsPassword { get; set; }


		//Setting variables for WebSocket

		public string WebsocketAddress { get; set; }
		public int WebsocketPort { get; set; }


		public string ChallangeTitle { get; set; }
		List<Challange> SelectedChallangeList { get; set; }

		public OptionsPage(ObsService obsService, HtmlService htmlService, LogService logService)
		{

			_obsService = obsService;
			_obsService.ObsConnectionChanged += OnObsConnectionChanged;
			_htmlService = htmlService;
			_logService = logService;

			InitializeComponent();
			ReloadSettings();

		}

		private void OnObsConnectionChanged(bool isConnected)
		{


			_logService.Log($"[SETTINGS][OBS] OBS CONNECTION CHANGED --- RELOADING SETTINGS FOR OBS...", Brushes.Yellow);
			Dispatcher.Invoke(() =>
			{// Update UI or perform actions based on the connection status
				if (isConnected)
				{
					// If OBS is connected, you might want to update or reload the scenes
					GetScenes();
				}
				else
				{
					// If OBS is disconnected, you might want to disable certain UI elements or show a message
					SceneComboBox.Items.Clear();
					SceneComboBox.Items.Add("-- Connect OBS --");
					TitleSourceComboBox.Items.Clear();
					DescSourceComboBox.Items.Clear();
					TitleSourceComboBox.IsEnabled = false;
					DescSourceComboBox.IsEnabled = false;
				}

				// You can also reload the settings based on the new connection status
				ReloadSettings();
			});
		}

		public void ReloadSettings()
		{
			if (_obsService?.ObsIsConnected == true) // Check if OBS is connected
			{
				GetScenes();
				GetAllBrowserSources();
				

			}
			GetChallangeFiles();
			LoadSettings();

		}

		private void GetChallangeFiles()
		{
			var _filePath = Path.Combine($"{Directory.GetCurrentDirectory()}\\Other Files\\");
			string[]files = Directory.GetFiles( _filePath );

			ChallangeListBox.Items.Clear();
			foreach (string file in files) {
				ChallangeListBox.Items.Add(Path.GetFileName(file));
			}
		}

		private void GetAllBrowserSources()
		{
			var browserSource = _obsService.GetAllBrowserSources();

			browserSource.ForEach(browserSource =>
			{

				OverlayBrowserSourceComboBox.Items.Add(browserSource);

			});

		}

		// Make sure to unsubscribe when the page is unloaded
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			_obsService.ObsConnectionChanged -= OnObsConnectionChanged;
		}

		private void LoadSettings()
		{//Reading Settings


#if DEBUG
			Debug.WriteLine("Loading settings....");
#endif

			_logService.Log($"[SETTINGS][LOAD] Loading Saved Settings...", Brushes.Yellow);

			TwitchUserName = Properties.Settings.Default.TwitchUsername;
			TwitchAuth = Properties.Settings.Default.TwitchAuth;
			TwitchChannel = Properties.Settings.Default.TwitchChannel;

			ObsAddress = Properties.Settings.Default.ObsIP;
			ObsPort = Properties.Settings.Default.ObsPort;
			ObsPassword = Properties.Settings.Default.ObsPassword;

			ObsScene = Properties.Settings.Default.ObsScene;
			ObsSourceTitle = Properties.Settings.Default.ObsSourceTitle;
			ObsSourceDesc = Properties.Settings.Default.ObsSourceDesc;
			ObsSourceOverlay = Properties.Settings.Default.ObsSourceOverlay;
			ObsSceneOverlay = Properties.Settings.Default.ObsOverlayScene;
			ObsBrowserSource = Properties.Settings.Default.ObsBrowserSource;

			WebsocketAddress = Properties.Settings.Default.WebsocketAddress;
			WebsocketPort = Convert.ToInt32(Properties.Settings.Default.WebsocketPort);


			ChallangeTitle = Properties.Settings.Default.ChallangeTitle;
			ChallangeList = Properties.Settings.Default.ChallangeList;

			Dispatcher.Invoke(() =>
			{

				TitleSourceComboBox.SelectedItem = ObsSourceTitle;
				SceneComboBox.SelectedItem = ObsScene;
				DescSourceComboBox.SelectedItem = ObsSourceDesc;
				OverlaySourceComboBox.SelectedItem = ObsSourceOverlay;
				OverlaySourceSceneComboBox.SelectedItem = ObsSceneOverlay;
				OverlayBrowserSourceComboBox.SelectedItem = ObsBrowserSource;


				TwitchUserNameTextBox.Text = TwitchUserName;
				TwitchAuthTextBox.Password = TwitchAuth;
				TwitchChannelTextBox.Text = TwitchChannel;

				ObsAddressTextBox.Text = ObsAddress;
				ObsPortTextBox.Text = ObsPort.ToString();
				ObsPasswordTextBox.Password = ObsPassword;

				WebsocketPortTextBox.Text = WebsocketPort.ToString();
				WebsocketAddressTextBox.Text = WebsocketAddress;

				ChallangeTitleTextBox.Text = ChallangeTitle;
				ChallangeListBox.SelectedItem = ChallangeList;
			});


			_logService.Log($"[SETTINGS][LOAD] Settings loaded", Brushes.Yellow);

#if DEBUG
			Debug.WriteLine("Settings loaded");
#endif

		}

		private void GetScenes()
		{

			// Ensure the update is done on the UI thread
			Dispatcher.Invoke(() =>
			{
				if (!_obsService.ObsIsConnected)
				{
					if (!SceneComboBox.Items.Contains("-- Connect OBS --"))
					{
						SceneComboBox.Items.Add("-- Connect OBS --");
					}
					return;
				}

				// Clear previous items before adding the new ones
				SceneComboBox.Items.Clear();
				OverlaySourceSceneComboBox.Items.Clear();

				// Add scenes retrieved from OBS
				var scenes = _obsService.GetSceneNames();
				foreach (var scene in scenes)
				{
					SceneComboBox.Items.Add(scene);
					OverlaySourceSceneComboBox.Items.Add(scene);
				}

				if (ObsScene != string.Empty) { SceneComboBox.SelectedItem = ObsScene; }
				if (ObsSceneOverlay != string.Empty) { OverlaySourceSceneComboBox.SelectedItem = ObsSceneOverlay; }

			});
		}

		private void GetSceneItems(string sceneName)
		{
			if (TitleSourceComboBox is null)
			{
				return;
			}

			TitleSourceComboBox.IsEnabled = true;  // Enable the SourceComboBox
			DescSourceComboBox.IsEnabled = true;

			if (TitleSourceComboBox.Items.Count > 0)
			{
				TitleSourceComboBox.Items.Clear();
			}

			if (DescSourceComboBox.Items.Count > 0)
			{
				DescSourceComboBox.Items.Clear();
			}

			var SourceNames = _obsService.GetSourceNames(sceneName);
			foreach (var SourceName in SourceNames)
			{

				TitleSourceComboBox.Items.Add($"{SourceName.SourceName}");
				DescSourceComboBox.Items.Add($"{SourceName.SourceName}");
			}
			if (ObsSourceTitle != string.Empty) { TitleSourceComboBox.SelectedItem = ObsSourceTitle; }
		}

		private void GetSceneItemsForOverlay(string sceneName)
		{
			if (OverlaySourceComboBox is null)
			{
				return;
			}

			OverlaySourceComboBox.IsEnabled = true;

			if (OverlaySourceComboBox.Items.Count > 0)
			{
				OverlaySourceComboBox.Items.Clear();
			}

			var SourceNames = _obsService.GetSourceNames(sceneName);
			foreach (var SourceName in SourceNames)
			{
				OverlaySourceComboBox.Items.Add($"{SourceName.SourceName}");

			}
			if (ObsSourceOverlay != string.Empty) { OverlaySourceComboBox.SelectedItem = ObsSourceOverlay; }
		}




		private void SaveTwitchSettingsBtn_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.TwitchUsername = TwitchUserNameTextBox.Text;
			Properties.Settings.Default.TwitchAuth = TwitchAuthTextBox.Password;
			Properties.Settings.Default.TwitchChannel = TwitchChannelTextBox.Text;

			_logService.Log($"[OPTIONS][SAVE] Saving Twitch settings", Brushes.Yellow);

#if (DEBUG)
			Debug.WriteLine($"Saving Settings for Twitch Auth ");
#endif

			Properties.Settings.Default.Save();
		}


		private void SceneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			// Ensure there is a valid selected item
			if (SceneComboBox.SelectedItem != null)
			{
				string selectedScene = SceneComboBox.SelectedItem.ToString();
				GetSceneItems(selectedScene);  // Example function call

				// Enable the SourceComboBox only if a valid scene is selected
				TitleSourceComboBox.IsEnabled = true;
			}
			else
			{
				// If no item is selected, disable SourceComboBox
				//SourceComboBox.IsEnabled = false;
			}

		}

		private void OverlaySourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ObsSourceOverlay != string.Empty) { OverlaySourceComboBox.SelectedItem = ObsSourceOverlay; }
		}

		private void OverlaySourceSceneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Ensure there is a valid selected item
			if (OverlaySourceSceneComboBox.SelectedItem != null)
			{
				string selectedScene = OverlaySourceSceneComboBox.SelectedItem.ToString();
				GetSceneItemsForOverlay(selectedScene);  // Example function call

				// Enable the SourceComboBox only if a valid scene is selected
				OverlaySourceComboBox.IsEnabled = true;
			}
			else
			{
				// If no item is selected, disable SourceComboBox
				//SourceComboBox.IsEnabled = false;
			}
		}

		private void SaveObsWebsocket_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.ObsIP = ObsAddressTextBox.Text;
			Properties.Settings.Default.ObsPort = Convert.ToInt32(ObsPortTextBox.Text);
			Properties.Settings.Default.ObsPassword = ObsPasswordTextBox.Password;


			_logService.Log($"[OPTIONS][SAVE] Saving OBS Websocket settings", Brushes.Yellow);

#if (DEBUG)
			Debug.WriteLine($"Saving Settings for OBS WebSocket  ");
#endif

			Properties.Settings.Default.Save();

		}

		private void SaveOBSSettings_Click(object sender, RoutedEventArgs e)
		{

			// Ensure the SceneComboBox has a valid selection
			string selectedScene = SceneComboBox.SelectedItem as string; // Make sure it's a valid scene name

			// Ensure the TitleSourceComboBox has a valid selection
			string selectedSourceTitle = TitleSourceComboBox.SelectedItem as string; // Make sure it's a valid scene name

			// Ensure the DescSourceComboBox has a valid selection
			string selectedSourceDesc = DescSourceComboBox.SelectedItem as string; // Make sure it's a valid scene name

			// Ensure the OverlaySourceComboBox has a valid selection
			string selectedOverlaySource = OverlaySourceComboBox.SelectedItem as string; // Make sure it's a valid scene name

			// Ensure the OverlaySourceComboBox has a valid selection
			string selectedOverlayScene = OverlaySourceSceneComboBox.SelectedItem as string; // Make sure it's a valid scene name
																							 // Ensure the OverlaySourceComboBox has a valid selection
			string selectedOverlayBrowserSource = OverlayBrowserSourceComboBox.SelectedItem as string; // Make sure it's a valid scene name



			Properties.Settings.Default.ObsSourceTitle = selectedSourceTitle;
			Properties.Settings.Default.ObsSourceDesc = selectedSourceDesc;
			Properties.Settings.Default.ObsSourceOverlay = selectedOverlaySource;
			Properties.Settings.Default.ObsScene = selectedScene;
			Properties.Settings.Default.ObsOverlayScene = selectedOverlayScene;
			Properties.Settings.Default.ObsBrowserSource = selectedOverlayBrowserSource;
			_logService.Log($"[OPTIONS][SAVE] Saving OBS Settings for Scenes and Overlays", Brushes.Yellow);

#if (DEBUG)
			Debug.WriteLine($"Saving Settings for OBS Scenes  ");
#endif
			Properties.Settings.Default.Save();

		}

		private void SaveWebsocketSettings_Click(object sender, RoutedEventArgs e)
		{



			Properties.Settings.Default.WebsocketAddress = WebsocketAddressTextBox.Text;
			Properties.Settings.Default.WebsocketPort = Convert.ToInt32(WebsocketPortTextBox.Text);

			_logService.Log($"[OPTIONS][SAVE] Saving WebsocketSettings", Brushes.Yellow);

#if (DEBUG)
			Debug.WriteLine($"Saving Settings for OBS Scenes  ");
#endif
			Properties.Settings.Default.Save();
		}

		private void GenerateHTMLFileBtn_Click(object sender, RoutedEventArgs e)
		{
			string filePath = Path.Combine($"{Directory.GetCurrentDirectory()}\\Html\\Index.html");
			_logService.Log($"[OPTIONS][HTML] Generating HTML File....", Brushes.Yellow);

#if (DEBUG)
			Debug.WriteLine($"Generating HTML File");
#endif


			_htmlService.GenerateJavaScriptFile("main.js", Properties.Settings.Default.WebsocketAddress, Properties.Settings.Default.WebsocketPort, SelectedChallangeList,9);
			_htmlService.GenerateHTMLFile("Index.html", ChallangeTitleTextBox.Text);

			_obsService.UpdateOverlaySource(filePath, ObsBrowserSource);
			_logService.Log($"[OPTIONS][HTML] HTML File Generated", Brushes.Yellow);


		}

		private void SaveOverlaySettings_Click(object sender, RoutedEventArgs e)
		{

			_logService.Log($"[OPTIONS][SAVE] Saving HTML Settings....", Brushes.Yellow);
			Properties.Settings.Default.ChallangeTitle = ChallangeTitleTextBox.Text;
			ChallangeTitle = ChallangeTitleTextBox.Text;

			
			Properties.Settings.Default.ChallangeList = ChallangeList;
			ChallangeList = ChallangeListBox.SelectedItem.ToString();

			LoadChallanges(ChallangeList);



			Properties.Settings.Default.Save();


			_logService.Log($"[OPTIONS][SAVE] HTML Settings saved", Brushes.Yellow);


#if DEBUG
			Debug.Write("Saving Overlay Settings");
#endif
		}

		private void LoadChallanges(string challangeList)
		{
			var _filePath = Path.Combine($"{Directory.GetCurrentDirectory()}/Other Files/{challangeList}");

			try
			{
				var jsContent = File.ReadAllText(_filePath);
				 SelectedChallangeList = JsonConvert.DeserializeObject<List<Challange>>(jsContent);

				_logService.Log($"[OPTIONS][Challanges] Challanges loaded", Brushes.Yellow);
			}
			catch (Exception ex)
			{

				_logService.Log($"[OPTIONS][ERROR] Can't load challanges", Brushes.Yellow);

#if DEBUG
				Debug.WriteLine($"Can't Load Challanges {ex.Message}");
#endif
			}



		}


		private void OverlayBrowserSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{






		}
	}
}
