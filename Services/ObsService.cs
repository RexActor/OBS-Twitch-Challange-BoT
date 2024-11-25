using OBSWebsocketDotNet;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBS_Twitch_Challange_BoT.Services
{
    public class ObsService
    {
        OBSWebsocket obsSocket;
        public bool _obsIsConnected;

        public bool ObsIsConnected
        {
            get => _obsIsConnected;
            private set
            {
                if (_obsIsConnected != value)
                {
                    _obsIsConnected = value;
                    OnObsConnectionChanged(_obsIsConnected);
                }
            }
        }

        // Event for connection status changes
        public event Action<bool> ObsConnectionChanged;


        public void ConnectWebSocket(string url, int port, string password)
        {

            obsSocket = new OBSWebsocket();
            Debug.WriteLine("Connecting to obs....");
            obsSocket.ConnectAsync($"ws://{url}:{port}", password);
            obsSocket.Connected += ObsSocket_Connected;
            obsSocket.Disconnected += ObsSocket_Disconnected;

        }

        private void ObsSocket_Disconnected(object? sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e)
        {
            ObsIsConnected = obsSocket.IsConnected;
        }

        public void DisconnectWebSocket()
        {
            if (obsSocket.IsConnected)
            {
                obsSocket.Disconnect();
            }
            else
            {
                return;
            }
        }

        private void ObsSocket_Connected(object? sender, EventArgs e)
        {
            ObsIsConnected = obsSocket.IsConnected;
        }

        protected virtual void OnObsConnectionChanged(bool isConnected)
        {
            ObsConnectionChanged?.Invoke(isConnected);
        }


    }
}
