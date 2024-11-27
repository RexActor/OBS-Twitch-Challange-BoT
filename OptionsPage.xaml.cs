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

        public string TwitchUserName { get; set; }
        public string TwitchAuth { get; set; }
        public string TwitchChannel { get; set; }


        public string ObsAddress { get; set; }
        public int ObsPort { get; set; }
        public string ObsPassword { get; set; }


        public OptionsPage(ObsService obsService)
        {

            _obsService = obsService;

            InitializeComponent();

            //Reading Settings
            TwitchUserName = Properties.Settings.Default.TwitchUsername;
            TwitchAuth = Properties.Settings.Default.TwitchAuth;
            TwitchChannel = Properties.Settings.Default.TwitchChannel;

            ObsAddress = Properties.Settings.Default.ObsIP;
            ObsPort = Properties.Settings.Default.ObsPort;
            ObsPassword = Properties.Settings.Default.ObsPassword;



            TwitchUserNameTextBox.Text = TwitchUserName;
            TwitchAuthTextBox.Password = TwitchAuth;
            TwitchChannelTextBox.Text = TwitchChannel;

            ObsAddressTextBox.Text = ObsAddress;
            ObsPortTextBox.Text = ObsPort.ToString();
            ObsPasswordTextBox.Password = ObsPassword;

            GetScenes();
          
            

        }

        private void GetScenes()
        {

            if (!_obsService.ObsIsConnected)
            {
                SceneComboBox.Items.Add("-- Connect OBS --");
                return;
            }


            if (SceneComboBox.Items.Count > 0)
            {
                SceneComboBox.Items.Clear();
            }


            _obsService.GetSceneNames().ForEach(sceneName =>
            {
                SceneComboBox.Items.Add(sceneName);

            });
        }

        private void GetSceneItems(string sceneName)
        {
            if(SourceComboBox is null)
            {
                return;
            }

            SourceComboBox.IsEnabled = true;  // Enable the SourceComboBox

            if (SourceComboBox.Items.Count > 0) {
                SourceComboBox.Items.Clear();
               
            }
           
            var SourceNames = _obsService.GetSourceNames(sceneName);
            foreach (var SourceName in SourceNames)
            {
                SourceComboBox.Items.Add($"{SourceName.SourceName}");
            }

        }

        private void SaveTwitchSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TwitchUsername = TwitchUserNameTextBox.Text;
            Properties.Settings.Default.TwitchAuth = TwitchAuthTextBox.Password;
            Properties.Settings.Default.TwitchChannel = TwitchChannelTextBox.Text;

            Properties.Settings.Default.ObsIP = ObsAddressTextBox.Text;
            Properties.Settings.Default.ObsPort = Convert.ToInt32(ObsPortTextBox.Text);
            Properties.Settings.Default.ObsPassword = ObsPasswordTextBox.Password;


            Properties.Settings.Default.Save();
        }

        private void SourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void SceneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure the "Please Select" item is ignored in the event
            if (SceneComboBox.SelectedItem != null && SceneComboBox.SelectedItem.ToString() != "Please Select")
            {
                // Perform logic for valid selection (e.g., update another UI element)
                string selectedScene = SceneComboBox.SelectedItem.ToString();
                GetSceneItems(selectedScene);  // Example function call
                
            }
            else
            {
                // Handle case where "Please Select" is still selected (do nothing or disable options)
                SourceComboBox.IsEnabled = false;
            }

        }
    }
}
