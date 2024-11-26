using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : UserControl
    {
        public string TwitchUserName { get;set; }
        public string TwitchAuth { get; set; }
        public string TwitchChannel { get; set; }


        public string ObsAddress { get; set; }
        public int ObsPort {  get; set; }
        public string ObsPassword {  get; set; }


        public OptionsPage()
        {
            InitializeComponent();

            //Reading Settings
            TwitchUserName = Properties.Settings.Default.TwitchUsername;
            TwitchAuth=Properties.Settings.Default.TwitchAuth;
            TwitchChannel=Properties.Settings.Default.TwitchChannel;

            ObsAddress = Properties.Settings.Default.ObsIP;
            ObsPort = Properties.Settings.Default.ObsPort;
            ObsPassword = Properties.Settings.Default.ObsPassword;



            TwitchUserNameTextBox.Text = TwitchUserName;
            TwitchAuthTextBox.Password = TwitchAuth;
            TwitchChannelTextBox.Text = TwitchChannel;

            ObsAddressTextBox.Text = ObsAddress;
            ObsPortTextBox.Text = ObsPort.ToString();
            ObsPasswordTextBox.Password = ObsPassword;
           


        }

        private void SaveTwitchSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TwitchUsername = TwitchUserNameTextBox.Text;
            Properties.Settings.Default.TwitchAuth = TwitchAuthTextBox.Password;
            Properties.Settings.Default.TwitchChannel = TwitchChannelTextBox.Text;

            Properties.Settings.Default.ObsIP = ObsAddressTextBox.Text;
            Properties.Settings.Default.ObsPort=Convert.ToInt32(ObsPortTextBox.Text);
            Properties.Settings.Default.ObsPassword = ObsPasswordTextBox.Password;


            Properties.Settings.Default.Save();
        }
    }
}
