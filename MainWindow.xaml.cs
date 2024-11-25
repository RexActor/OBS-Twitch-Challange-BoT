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
        // Parameterless constructor for WPF
        public MainWindow() : this(new ObsService(),new HtmlService()) { }

        public  MainWindow(ObsService obsService,HtmlService htmlService)
        {

            InitializeComponent();
            _htmlService = htmlService;
           _obsService = obsService;
            _obsService.ObsConnectionChanged += _obsService_ObsConnectionChanged;
            _htmlService.GenerateHTMLFile("Index.html");
           
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