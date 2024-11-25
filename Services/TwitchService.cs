using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace OBS_Twitch_Challange_BoT.Services
{
    public class TwitchService
    {

        public bool _twitchIsConnected;
        TwitchClient twitchClient;

        public bool TwitchIsConnected
        {
            get => _twitchIsConnected;
            private set
            {
                if (_twitchIsConnected != value)
                {
                    _twitchIsConnected = value;
                    OnTwitchConnectionChanged(_twitchIsConnected);

                }
            }
        }

        public event Action<bool> TwitchConnectionChanged;


        protected virtual void OnTwitchConnectionChanged(bool connected)
        {
            TwitchConnectionChanged?.Invoke(connected);
        }

        public void ConnectToTwitch()
        {
            string userName = Properties.Settings.Default.TwitchUsername;
            string oauthToken = Properties.Settings.Default.TwitchAuth;
            string channelName = Properties.Settings.Default.TwitchChannel;

            ConnectionCredentials credentials = new ConnectionCredentials(userName, oauthToken);

            twitchClient = new TwitchClient();
            twitchClient.Initialize(credentials, channelName);

            twitchClient.OnMessageReceived += Client_OnMessageReceived;
            twitchClient.OnConnected += Client_OnConnected;
            twitchClient.OnDisconnected += Client_OnDisconnected;
            twitchClient.Connect();


        }

        public void Disconnect()
        {
            twitchClient?.Disconnect();
        }

        private void Client_OnDisconnected(object? sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            TwitchIsConnected = twitchClient.IsConnected;
        }

        private void Client_OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            TwitchIsConnected = twitchClient.IsConnected;
           
        }

        private void Client_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            Debug.WriteLine($"{e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
            // Respond to a specific message (e.g., '!hello')
            if (e.ChatMessage.Message.Equals("!hello", StringComparison.InvariantCultureIgnoreCase))
            {
                ((TwitchClient)sender).SendMessage(e.ChatMessage.Channel, "Hello, " + e.ChatMessage.DisplayName + "!");
            }
        }
    }
}
