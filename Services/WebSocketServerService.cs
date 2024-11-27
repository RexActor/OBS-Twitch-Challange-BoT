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

namespace OBS_Twitch_Challange_BoT.Services
{
	public class ChallengeWebSocketBehavior : WebSocketBehavior
	{
		private ObsService _obsService;

		public ChallengeWebSocketBehavior(ObsService obsService)
		{
			// Inject the ObsService to handle OBS-related actions
			_obsService = obsService;
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			// Handle incoming message from client
			Debug.WriteLine($"Received from client: {e.Data}");


			var challange = JsonConvert.DeserializeObject<Challange>(e.Data);


			_obsService.UpdateTextSource(Properties.Settings.Default.ObsSourceTitle, challange.Title);
			_obsService.UpdateTextSource(Properties.Settings.Default.ObsSourceDesc, challange.Desc);


			// Respond to client with the message (optional)
			Send($"Server received: {e.Data}");
		}
	}
	class WebSocketServerService
	{
		private WebSocketServer _server;

		public WebSocketServerService( ObsService obsService)
		{
			// Initialize WebSocket server on port 8080
			_server = new WebSocketServer("ws://localhost:9090");

			// Add the custom behavior for handling incoming WebSocket connections
			_server.AddWebSocketService<ChallengeWebSocketBehavior>("/challenge", () => new ChallengeWebSocketBehavior(obsService));

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
