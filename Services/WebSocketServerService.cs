﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public ChallengeWebSocketBehavior(ObsService obsService, TwitchService twitchService)
		{
			// Inject the ObsService to handle OBS-related actions
			_obsService = obsService;
			_twitchService = twitchService;
			
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
			Debug.WriteLine($"Received from client: {e.Data}");

			if (e.Data.Equals("rolling challange"))
			{
				_obsService.DeActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceTitle);
				_obsService.DeActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceDesc);
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


			_twitchService.SendMessage($"We will do {challange.Title} challange");


			await Task.Delay(5000);

			_obsService.ActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceTitle);
			_obsService.ActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceDesc);


			_obsService.DeActivateSource(Properties.Settings.Default.ObsOverlayScene, Properties.Settings.Default.ObsSourceOverlay);



		}
	}
	class WebSocketServerService
	{
		private WebSocketServer _server;

		public WebSocketServerService(ObsService obsService,TwitchService twitchService)
		{
			// Initialize WebSocket server on port 8080
			_server = new WebSocketServer("ws://localhost:9090");

			// Add the custom behavior for handling incoming WebSocket connections
			_server.AddWebSocketService("/challenge", () => new ChallengeWebSocketBehavior(obsService,twitchService));

		}

		public void Start()
		{
			_server.Start();
			Debug.WriteLine("WebSocket server started on ws://localhost:9090");
		}

		public void Stop()
		{
			_server.Stop();
			Debug.WriteLine("WebSocket server stopped.");
		}
	}
}
