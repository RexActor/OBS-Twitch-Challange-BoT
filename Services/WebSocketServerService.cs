using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WebSocketSharp.Server;
using WebSocketSharp;
using System.Diagnostics;
using Newtonsoft.Json;
using OBS_Twitch_Challange_BoT.Models;
using System.Text.Json;
using System.Windows.Media.Animation;

namespace OBS_Twitch_Challange_BoT.Services
{
	public class ChallengeWebSocketBehavior : WebSocketBehavior
	{
		private ObsService _obsService;
		private TwitchService _twitchService;
		private LogService _logService;
		public ChallengeWebSocketBehavior(ObsService obsService, TwitchService twitchService, LogService logService)
		{
			// Inject the ObsService to handle OBS-related actions
			_obsService = obsService;
			_twitchService = twitchService;
			_logService = logService;
		}

		private static bool isValidJson(string message)
		{
			try
			{
				JsonDocument.Parse(message);
				return true;
			}
			catch (System.Text.Json.JsonException ex)
			{
				return false;
			}
			catch (ArgumentNullException)
			{
				return false;
			}
		}

		protected override async void OnMessage(MessageEventArgs e)
		{
			// Handle incoming message from client

			_logService.Log($"[WebSocket-Service][Message] Received from client: {e.Data}", Brushes.Orange);


#if DEBUG
			Debug.WriteLine($"Received from client: {e.Data}");
#endif


			if (_obsService._obsIsConnected)
			{

				if (e.Data.Equals("rolling challange"))
				{
					_obsService.DeActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceTitle);
					_obsService.DeActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceDesc);

					_logService.Log($"[WebSocket-Service][Action] We are rolling for new Challange:", Brushes.Orange);
					_twitchService.SendMessage("We are rolling for new Challange");
				}

				if (!isValidJson(e.Data))
				{
					return; //if received message is not JSON format. we don't continue
				}

				var challange = JsonConvert.DeserializeObject<Challange>(e.Data);


				_obsService.UpdateTextSource(Properties.Settings.Default.ObsSourceTitle, challange.Title);
				_obsService.UpdateTextSource(Properties.Settings.Default.ObsSourceDesc, challange.Desc);
				// Respond to client with the message (optional)
				Send($"Server received: {e.Data}");

				_logService.Log($"[WebSocket-Service][Action] WChallange Selected Title: {challange.Title} Description: {challange.Desc}", Brushes.Orange);
				_twitchService.SendMessage($"We will do {challange.Title} challange");


				await Task.Delay(5000);

				_obsService.ActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceTitle);
				_obsService.ActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceDesc);


				_obsService.DeActivateSource(Properties.Settings.Default.ObsOverlayScene, Properties.Settings.Default.ObsSourceOverlay);

			}

		}
	}
	class WebSocketServerService
	{
		private WebSocketServer _server;
		private readonly LogService _logService;

		public WebSocketServerService(ObsService obsService, TwitchService twitchService, LogService logService)
		{
			// Initialize WebSocket server on port 9090
			_server = new WebSocketServer($"{Properties.Settings.Default.WebsocketAddress}:{Properties.Settings.Default.WebsocketPort}");

			// Add the custom behavior for handling incoming WebSocket connections
			_server.AddWebSocketService("/challenge", () => new ChallengeWebSocketBehavior(obsService, twitchService, logService));
			_logService = logService;
		}

		public void Start()
		{
			_server.Start();

			_logService.Log($"[WebSocket-Service][CONNECTION] WebSocket server started on {Properties.Settings.Default.WebsocketAddress}:{Properties.Settings.Default.WebsocketPort}", Brushes.Orange);
#if DEBUG
			Debug.WriteLine($"Websocket Server Started on {Properties.Settings.Default.WebsocketAddress}:{Properties.Settings.Default.WebsocketPort}");
#endif
		}

		public void Stop()
		{
			_server.Stop();
			_logService.Log($"[WebSocket-Service][CONNECTION] WebSocket server stopped.", Brushes.Orange);
#if DEBUG
			Debug.WriteLine("WebSocket server stopped.");
#endif
		}
	}
}
