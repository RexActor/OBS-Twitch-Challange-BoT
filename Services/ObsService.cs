using Newtonsoft.Json.Linq;

using OBS_Twitch_Challange_BoT.Models;
using System.Windows.Media;
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
using Newtonsoft.Json;

namespace OBS_Twitch_Challange_BoT.Services
{
	public class ObsService
	{
		OBSWebsocket obsSocket;
		public bool _obsIsConnected;
		private readonly LogService _logService;

		// Event for connection status changes
		public event Action<bool> ObsConnectionChanged;

		public ObsService(LogService logService)
		{
			_logService = logService;
		}

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


		public void ConnectWebSocket(string url, int port, string password)
		{

			obsSocket = new OBSWebsocket();

			_logService.Log($"[OBS-Service][CONNECTION] Connecting to obs....", Brushes.Green);
#if DEBUG

			Debug.WriteLine("Connecting to obs....");
#endif
			obsSocket.ConnectAsync($"ws://{url}:{port}", password);
			obsSocket.Connected += ObsSocket_Connected;
			obsSocket.Disconnected += ObsSocket_Disconnected;


		}

		private void ObsSocket_Disconnected(object? sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e)
		{
			ObsIsConnected = obsSocket.IsConnected;

			_logService.Log($"[OBS-Service][CONNECTION] Disconected from OBS websocket - Reason: {e.DisconnectReason}", Brushes.Green);
#if DEBUG
			Debug.WriteLine($"Disconected from OBS websocket - Reason: {e.DisconnectReason}");
#endif
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

			_logService.Log($"[OBS-Service][CONNECTION] Connected to OBS websocket", Brushes.Green);
#if DEBUG
			Debug.WriteLine($"Connected to OBS");
#endif
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
				_logService.Log($"[OBS-Service][ERRROR] Error ocured while trying to update text source: {ex.Message}", Brushes.Green);
#if DEBUG
				Debug.WriteLine($"Error ocured while trying to update text source: {ex.Message}");
#endif
			}
		}

		public void ActivateSource(string sceneName, string sourceName)
		{

			try
			{
				var itemID = obsSocket.GetSceneItemId(sceneName, sourceName, 0);

				obsSocket.SetSceneItemEnabled(sceneName, itemID, true);
				_logService.Log($"[OBS-Service][EVENT] {sourceName} activated in {sceneName} scene", Brushes.Green);
#if DEBUG
				Debug.WriteLine($"{sourceName} activated in {sceneName} scene");
#endif
			}
			catch (Exception ex)
			{
				_logService.Log($"[OBS-Service][ERROR] Error ocured while trying to activate sources: {ex.Message}", Brushes.Green);
#if DEBUG
				Debug.WriteLine($"Error ocured while trying to activate sources: {ex.Message}");
#endif
			}

		}
		public void DeActivateSource(string sceneName, string sourceName)
		{

			try
			{
				var itemID = obsSocket.GetSceneItemId(sceneName, sourceName, 0);

				obsSocket.SetSceneItemEnabled(sceneName, itemID, false);


				_logService.Log($"[OBS-Service][EVENT] {sourceName} de-activated in {sceneName} scene", Brushes.Green);

#if DEBUG
				Debug.WriteLine($"{sourceName} de-activated in {sceneName} scene");

#endif
			}
			catch (Exception ex)
			{

				_logService.Log($"[OBS-Service][ERROR] Error ocured while trying to Deactivate sources : {ex.Message}", Brushes.Green);
#if DEBUG
				Debug.WriteLine($"Error ocured while trying to Deactivate sources: {ex.Message}");
#endif
			}

		}

		public List<SceneItemDetails> GetSourceNames(string sceneName)
		{
			var SceneItems = new List<SceneItemDetails>();

			try
			{
				SceneItems = obsSocket.GetSceneItemList(sceneName);

			}
			catch (Exception ex)
			{

				_logService.Log($"[OBS-Service][ERROR] Error ocured while trying to GetSourceNames : {ex.Message}", Brushes.Green);
#if DEBUG
				Debug.WriteLine($"Error ocured while trying to GetSourceNames: {ex.Message}");
#endif
			}
			return SceneItems;
		}
		public List<string> GetSceneNames()
		{
			var SceneItems = new List<string>();

			try
			{
				var sceneList = obsSocket?.GetSceneList();
				SceneItems = sceneList?.Scenes.Select(scene => scene.Name).ToList();
			}
			catch (Exception ex)
			{

				_logService.Log($"[OBS-Service][ERROR] Error ocured while trying to GetSceneNames : {ex.Message}", Brushes.Green);
#if DEBUG
				Debug.WriteLine($"Error ocured while trying to GetSceneNames: {ex.Message}");
#endif
			}
			return SceneItems;
		}

		public void UpdateOverlaySource(string filename,string sourceName)
		{





			string filePath = $"{filename.Replace("\\", "/")}";

#if DEBUG
			var settings = obsSocket.GetInputSettings(sourceName);
			Debug.WriteLine($"Settings for {sourceName}: {settings.Settings}");
			Debug.WriteLine(filePath);
#endif

			// Construct the new settings for the browser source
			JObject message = new JObject
		{
			{ "local_file", filePath}, // Convert file path to a URL format
			{ "width", 1920 }, // Set the width
			{ "height", 1080 }, // Set the height
			{ "is_local_file", true }, // Indicate that it's a local file
			{ "shutdown", true }, // Shutdown source when not visible
			{ "restart_when_active", true } // Refresh browser when scene becomes active
		};

			try
			{
				obsSocket.SetInputSettings(sourceName, message, false);
				_logService.Log($"[OBS-Service][OVERLAY] Overlay Source Updated in OBS", Brushes.Green);

			}
			catch (Exception ex)
			{
				_logService.Log($"[OBS-Service][ERRROR] Error ocured while trying to update Overlay source: {ex.Message}", Brushes.Green);
#if DEBUG
				Debug.WriteLine($"Error ocured while trying to update Overlay source: {ex.Message}");
#endif
			}


		}

		public List<string> GetAllBrowserSources()
		{
			try
			{
				// Retrieve all sources (inputs) from OBS
				var inputs = obsSocket.GetInputList();

				// Filter sources to find those of type "browser_source"
				var browserSources = inputs
					.Where(input => input.InputKind?.ToString() == "browser_source")
					.Select(input => input.InputName?.ToString())
					.ToList();

				// Log and return the list of browser source names
				_logService.Log($"[OBS-Service] Found {browserSources} browser sources: {string.Join(", ", browserSources)}", Brushes.Green);
				return browserSources;
			}
			catch (Exception ex)
			{
				_logService.Log($"[OBS-Service][ERROR] Error retrieving browser sources: {ex.Message}", Brushes.Red);
#if DEBUG
				Debug.WriteLine($"Error retrieving browser sources: {ex.Message}");
#endif
				return new List<string>();
			}
		}
	}
}
