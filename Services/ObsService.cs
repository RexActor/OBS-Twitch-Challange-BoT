using Newtonsoft.Json.Linq;

using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Net.Mime.MediaTypeNames;

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

        private void ObsSocket_VendorEvent(object? sender, OBSWebsocketDotNet.Types.Events.VendorEventArgs e)
        {
            Debug.WriteLine($"{e.eventData}");
        }

        private void ObsSocket_Disconnected(object? sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e)
        {
            ObsIsConnected = obsSocket.IsConnected;
            Debug.WriteLine($"Disconected from OBS websocket - Reason: {e.DisconnectReason}");
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

        public void UpdateTextSource(string sourceName, string text)
        {

            JObject message = new JObject
            {
                { "text", text }
            };


            try
            {
                obsSocket.SetInputSettings(sourceName, message, false);

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Error ocured while trying to update text source: {ex.Message}");
            }
        }

        public void ActivateSource(string sceneName, string sourceName)
        {

            try
            {
                var itemID = obsSocket.GetSceneItemId(sceneName, sourceName, 0);

                obsSocket.SetSceneItemEnabled(sceneName, itemID, true);
                Debug.WriteLine($"{sourceName} activated in {sceneName} scene");

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Error ocured while trying to update text source: {ex.Message}");
            }

        }

        public List<SceneItemDetails> GetSourceNames(string sceneName)
        {
            var SceneItems = new List<SceneItemDetails>();

            try
            {
                SceneItems= obsSocket.GetSceneItemList(sceneName);

            }
            catch (Exception ex)
            {

                
                Debug.WriteLine($"Error ocured while trying to update text source: {ex.Message}");
            
            }
            return SceneItems;
        }
        public List<string> GetSceneNames()
        {
            var SceneItems = new List<string>();


            try
            {
                 var sceneList = obsSocket?.GetSceneList();
                SceneItems= sceneList?.Scenes.Select(scene=>scene.Name).ToList();
            }
            catch (Exception ex)
            {


                Debug.WriteLine($"Error ocured while trying to update text source: {ex.Message}");

            }
            return SceneItems;
        }

    }
}
