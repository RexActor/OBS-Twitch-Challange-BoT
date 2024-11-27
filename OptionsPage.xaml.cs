using OBS_Twitch_Challange_BoT.Services;

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace OBS_Twitch_Challange_BoT
{
	/// <summary>
	/// Interaction logic for OptionsPage.xaml
	/// </summary>
	public partial class OptionsPage : UserControl
	{

		private readonly ObsService _obsService;


		//Setting variables for Twitch
		public string TwitchUserName { get; set; }
		public string TwitchAuth { get; set; }
		public string TwitchChannel { get; set; }



		//Setting variables for OBS source and Scene which one will be manipulated
		public string ObsScene { get; set; }
		public string ObsSourceTitle { get; set; }
		public string ObsSourceDesc { get; set; }

		//Setting variables for OBS
		public string ObsAddress { get; set; }
		public int ObsPort { get; set; }
		public string ObsPassword { get; set; }


		public OptionsPage(ObsService obsService)
		{

			_obsService = obsService;
			_obsService.ObsConnectionChanged += OnObsConnectionChanged;
			InitializeComponent();




			ReloadSettings();
		}

		private void OnObsConnectionChanged(bool isConnected)
		{
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
			}
			LoadSettings();

		}
		// Make sure to unsubscribe when the page is unloaded
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			_obsService.ObsConnectionChanged -= OnObsConnectionChanged;
		}

		private void LoadSettings()
		{//Reading Settings

			TwitchUserName = Properties.Settings.Default.TwitchUsername;
			TwitchAuth = Properties.Settings.Default.TwitchAuth;
			TwitchChannel = Properties.Settings.Default.TwitchChannel;

			ObsAddress = Properties.Settings.Default.ObsIP;
			ObsPort = Properties.Settings.Default.ObsPort;
			ObsPassword = Properties.Settings.Default.ObsPassword;

			ObsScene = Properties.Settings.Default.ObsScene;
			ObsSourceTitle = Properties.Settings.Default.ObsSourceTitle;
			ObsSourceDesc = Properties.Settings.Default.ObsSourceDesc;

			Dispatcher.Invoke(() =>
			{

				TitleSourceComboBox.SelectedItem = ObsSourceTitle;
				SceneComboBox.SelectedItem = ObsScene;
				DescSourceComboBox.SelectedItem = ObsSourceDesc;


				TwitchUserNameTextBox.Text = TwitchUserName;
				TwitchAuthTextBox.Password = TwitchAuth;
				TwitchChannelTextBox.Text = TwitchChannel;

				ObsAddressTextBox.Text = ObsAddress;
				ObsPortTextBox.Text = ObsPort.ToString();
				ObsPasswordTextBox.Password = ObsPassword;
			});

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

				// Add scenes retrieved from OBS
				var scenes = _obsService.GetSceneNames();
				foreach (var scene in scenes)
				{
					SceneComboBox.Items.Add(scene);
				}

				if (ObsScene != string.Empty) { SceneComboBox.SelectedItem = ObsScene; }


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

		private void SaveTwitchSettingsBtn_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.TwitchUsername = TwitchUserNameTextBox.Text;
			Properties.Settings.Default.TwitchAuth = TwitchAuthTextBox.Password;
			Properties.Settings.Default.TwitchChannel = TwitchChannelTextBox.Text;

			Properties.Settings.Default.ObsIP = ObsAddressTextBox.Text;
			Properties.Settings.Default.ObsPort = Convert.ToInt32(ObsPortTextBox.Text);
			Properties.Settings.Default.ObsPassword = ObsPasswordTextBox.Password;

			// Ensure the SceneComboBox has a valid selection
			string selectedScene = SceneComboBox.SelectedItem as string; // Make sure it's a valid scene name
			Properties.Settings.Default.ObsScene = selectedScene;

			// Ensure the SceneComboBox has a valid selection
			string selectedSourceTitle = TitleSourceComboBox.SelectedItem as string; // Make sure it's a valid scene name
			Properties.Settings.Default.ObsSourceTitle = selectedSourceTitle;

			// Ensure the SceneComboBox has a valid selection
			string selectedSourceDesc = DescSourceComboBox.SelectedItem as string; // Make sure it's a valid scene name
			Properties.Settings.Default.ObsSourceDesc = selectedSourceDesc;
#if (DEBUG)
			Debug.WriteLine($"Saving Settings {selectedScene} Scene  with Title: {selectedSourceTitle} Desc: {selectedSourceDesc} sources");
#endif   

			Properties.Settings.Default.Save();
		}

		private void SourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

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
	}
}
