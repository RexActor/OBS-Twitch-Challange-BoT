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
        // Parameterless constructor for WPF
        public MainWindow() : this(new ObsService()) { }

        public  MainWindow(ObsService obsService)
        {

            InitializeComponent();
           _obsService = obsService;
            _obsService.ObsConnectionChanged += _obsService_ObsConnectionChanged;
        }

        private void _obsService_ObsConnectionChanged(bool obj)
        {
            Dispatcher.Invoke(() =>
            {
                ObsConnectionLbl.Content = obj ? "Connected" : "Disconnected";
                ObsConnectBtn.Content = obj ? "Disconnect" : "Connect";
                ObsConnectBtn.Click -= ObsConnectBtn_Click;
                ObsConnectBtn.Click += ObsConnectBtn_Disconnect;
            });
        }

        private void ObsConnectBtn_Disconnect(object sender, RoutedEventArgs e)
        {
            _obsService.DisconnectWebSocket();
        }

        private void ObsConnectBtn_Click(object sender, RoutedEventArgs e)
        {
          
          _obsService.ConnectWebSocket("10.159.80.29",4455, "ykmSGUMi3UdrBKvW");

            
        }
    }
}