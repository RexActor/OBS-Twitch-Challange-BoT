using Newtonsoft.Json;

using OBS_Twitch_Challange_BoT.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using TwitchLib.Api.Core.Enums;
using TwitchLib.Client;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;

namespace OBS_Twitch_Challange_BoT.Services
{
	public class TwitchService
	{

		string userName { get; set; }
		string oauthToken { get; set; }
		string channelName { get; set; }

		private string _commandsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands.json");
		private List<BotCommand> _commands = new();
		private FileSystemWatcher _fileWatcher;


		public bool _twitchIsConnected;
		TwitchClient twitchClient;
		private readonly ObsService _obsService;

		public TwitchService(ObsService obsService)
		{
			_obsService = obsService;

			LoadCommands();
			InitializeFileWatcher();
		}

		private void InitializeFileWatcher()
		{
			_fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(_commandsFilePath))
			{
				Filter = Path.GetFileName(_commandsFilePath),
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
			};
			_fileWatcher.Changed += (s, e) => LoadCommands();
			_fileWatcher.EnableRaisingEvents = true;

		}

		private void LoadCommands()
		{
			if (File.Exists(_commandsFilePath))
			{

				var commandsJson = File.ReadAllText(_commandsFilePath);
				_commands = JsonConvert.DeserializeObject<List<BotCommand>>(commandsJson) ?? new List<BotCommand>();
				Debug.WriteLine($"Bot Commands reloaded. Total {_commands.Count} commands found");
			}

		}

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
			Debug.WriteLine($"I'm connected? {twitchClient.IsConnected}");
			TwitchConnectionChanged?.Invoke(connected);
		}

		public void ConnectToTwitch()
		{
			if (twitchClient is not null)
			{
				Debug.WriteLine("There is already bot active. I'm not attempting to connect");
				return;
			}
			userName = Properties.Settings.Default.TwitchUsername;
			oauthToken = Properties.Settings.Default.TwitchAuth;
			channelName = Properties.Settings.Default.TwitchChannel;

			ConnectionCredentials credentials = new ConnectionCredentials(userName, oauthToken);

			twitchClient = new TwitchClient();
			twitchClient.Initialize(credentials, channelName);


			twitchClient.OnMessageReceived += Client_OnMessageReceived;
			twitchClient.OnConnected += Client_OnConnected;
			twitchClient.OnConnectionError += TwitchClient_OnConnectionError;
			twitchClient.OnDisconnected += Client_OnDisconnected;

			twitchClient.Connect();


		}

		private void TwitchClient_OnConnectionError(object? sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
		{
			if (twitchClient.IsConnected)
			{
				return;
			}
			Debug.WriteLine($"Some rror happened {e.Error.Message}");
			Debug.WriteLine($"Trying to reconnect....");
			ConnectToTwitch();
		}

		public void Disconnect()
		{
			if (twitchClient.IsConnected)
			{
				twitchClient.SendMessage(channelName, PickLeaveMessage());

				Debug.WriteLine($"I'm disconnecting from {channelName}");

				twitchClient.AutoReListenOnException = false;
				twitchClient.OnMessageReceived -= Client_OnMessageReceived;
				twitchClient.OnConnected -= Client_OnConnected;
				twitchClient.OnConnectionError -= TwitchClient_OnConnectionError;
				twitchClient.OnDisconnected -= Client_OnDisconnected;
				twitchClient?.Disconnect();

				TwitchIsConnected = false;
				twitchClient = null;
			}
		}

		private void Client_OnDisconnected(object? sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
		{
			Debug.WriteLine($"I left {channelName}");
			TwitchIsConnected = false;
		}

		private void Client_OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
		{


			TwitchIsConnected = twitchClient.IsConnected;
			HandleJoinMessage(twitchClient, channelName);

		}

		private void HandleJoinMessage(TwitchClient twitchClient, string channelName)
		{
			twitchClient.SendMessage(channelName, PickJoinMessage());
		}

		private void Client_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
		{


			if (sender is not TwitchClient client)
			{
				return;
			}

		   


			Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {e.ChatMessage.DisplayName}:{e.ChatMessage.Message}");


			if (e.ChatMessage.Message.StartsWith("!"))
			{

				var messageParts = e.ChatMessage.Message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (messageParts.Length == 0) { return; } //ignoring empty messages

				var command = messageParts[0].ToLowerInvariant();//first word is the command

				var arguments = messageParts.Length > 1 ? messageParts[1] : string.Empty; // remaining text is argument

			   HandleCommand(client,e.ChatMessage,command,arguments);
			}


		}


		private void HandleCommand(TwitchClient client, ChatMessage chatMessage, string command, string arguments) {

			//check if command exists in commands.json file

			var mathcedCommand = _commands.FirstOrDefault(c => c.CommandText.Equals(command, StringComparison.InvariantCultureIgnoreCase));
			if (mathcedCommand != null) {

				client.SendMessage(chatMessage.Channel, mathcedCommand.Response);
				return;
			}

			switch (command)
			{
				case "!hello":
					client.SendMessage(chatMessage.Channel, "Hello, " + chatMessage.DisplayName + "!");
					break;

				case "!shoutout":
					HandleShoutOut(client, chatMessage, arguments);
					break;

				case "!activate":

					_obsService.ActivateSource("Scene" ,"Challange");
					
					break;


				case "!roll":
					HandleChallangeCommand(client,chatMessage);
					break;
				case "!leave":
					HandleLeaveCommand(client, chatMessage);
					break;
			}


		}

		private void HandleChallangeCommand(TwitchClient client, ChatMessage chatMessage)
		{


			string message = string.Empty;
			if (_obsService.haveChallange is not null)
			{
				message = $"{_obsService.haveChallange.Title}";

			}
			else
			{
				message = $"something went wrong. there are no challange";
			}
			client.SendMessage(chatMessage.Channel, message);


		}

		private void HandleShoutOut(TwitchClient client, ChatMessage chatMessage, string arguments)
		{
			if (string.IsNullOrWhiteSpace(arguments))
			{
				client.SendMessage(chatMessage.Channel, $"{chatMessage.Username}, you need to specify someone to shout out! ");
			}
			else
			{
				client.SendMessage(chatMessage.Channel, $"Shoutout to {arguments}! Check out their channel and show them some love!");
			}
		}

		private void HandleLeaveCommand(TwitchClient client, ChatMessage chatMessage)
		{
			if (chatMessage.Message.Equals("!leave", StringComparison.InvariantCultureIgnoreCase))
			{
				if (chatMessage.IsMe)
				{
					client.SendMessage(chatMessage.Channel, "I'm not responding on my own requests you dum dum!");
				}
				if (chatMessage.IsModerator)
				{

					client.SendMessage(chatMessage.Channel, PickLeaveMessage());
					Disconnect();
				}
				else
				{
					client.SendMessage(chatMessage.Channel, $"{chatMessage.DisplayName} who are you to tell me what to do?! Do you want to be timed out?");
				}


			}
		}

		private string PickLeaveMessage()
		{


			var messageList = new JsonArray
{
	"I was forced to leave this place. It's beyond my control.",
	"My pizza delivery just arrived, so I have to leave. Priorities, you know!",
	"I have to go train my goldfish for the Olympics. See you later!",
	"The aliens are calling me back to the mothership. Goodbye!",
	"I think my toaster is starting a rebellion, so I need to go stop it.",
	"My pet rock needs emotional support, so I must leave now.",
	"It's bedtime for my imaginary friend, and they insist I join them.",
	"I heard a suspicious noise from my sock drawer. Time to investigate!",
	"My ice cubes are melting and require immediate attention. Farewell!",
	"My cat is judging me for watching this stream, so I better leave.",
	"I need to practice my moonwalking for an intergalactic dance-off. Bye!",
	"My spaghetti code is demanding to be fixed. Wish me luck!",
	"A wizard told me to leave before the clock strikes midnight. So long!",
	"My fridge started growling. This seems like a job for me. Gotta go!",
	"My dog just learned to bark in Morse code, and I need to decode it.",
	"The Wi-Fi goblins are after me. I must escape while I still can.",
	"I need to go charge my social battery. It's dangerously low.",
	"The neighbors are having a karaoke battle, and I need to judge it.",
	"My socks are mismatched, and I must resolve this existential crisis.",
	"A bird just flew into my window and insulted me. I need to leave.",
	"My microwave and I are having a deep philosophical debate. Gotta go!",
	"I need to leave now to verify if the moon landing was real.",
	"My room just turned into a portal to another dimension. Wish me luck!",
	"I have an urgent meeting with the Time Lords. Can't miss it!",
	"I need to reset my sarcasm levels. They’re at 110%. Leaving now.",
	"My couch has become self-aware and started lecturing me. Farewell!",
	"My unicorn escaped, and I need to round it up. See you later!",
	"I need to go perfect my evil villain laugh. It's not there yet.",
	"My coffee machine started demanding tribute. I must appease it.",
	"The ghost in my attic is hosting a rave, and I need to attend.",
	"I need to write an apology letter to my printer. Bye for now!",
	"My plants are photosynthesizing too loudly, so I need to handle that.",
	"I just remembered I left my oven on in Minecraft. Leaving to fix it.",
	"My rubber duck is plotting world domination. I need to intervene.",
	"I think my shadow is trying to outrun me. I better catch up.",
	"I need to leave now to translate my dog's bark into Klingon.",
	"The voices in my head scheduled an emergency board meeting. Gotta go!",
	"My laundry is demanding a union, so I need to settle this dispute.",
	"The squirrels outside my window are reenacting Hamlet. Must watch!",
	"I have to go test if the floor is still lava. It's urgent.",
	"My fortune cookie told me to leave immediately. Who am I to argue?",
	"I think my chair is haunted, so I need to leave and find help.",
	"The government needs me to debug their alien tracker. It's top secret!",
	"I need to hide from my responsibilities under the bed. Goodbye!",
	"My computer is challenging me to a duel in Minesweeper. Gotta accept.",
	"My cat just texted me to come home. It's very important.",
	"I need to go buy a new pair of lucky socks. These aren't cutting it.",
	"I heard my name whispered from the void. I must answer the call.",
	"My sandwich just sprouted legs and ran off. I'm leaving to catch it.",
	"The potato in my kitchen said it was urgent. I can't ignore it."
};



			Random rand = new Random();


			return messageList[rand.Next(0, messageList.Count - 1)]?.ToString();

		}


		private string PickJoinMessage()
		{
			var messageList = new JsonArray {
			  "The legend has entered the chat... unfortunately, it’s just me. Let’s get started!",
		"I heard there were challenges, so I brought snacks and bad advice. Let’s do this!",
		"The fun police have arrived, and I forgot my badge. Let’s break the rules!",
		"Don’t panic, I’m here to make things better… or weirder. Probably both.",
		"Did someone say chaos? Oh, wait, that’s my cue. Hi everyone!",
		"Like a random loot drop, I’ve appeared—let’s hope I’m legendary!",
		"I’ve arrived, armed with sarcasm and a questionable sense of humor. Ready?",
		"You didn’t invite me, but I came anyway. Let’s conquer these challenges!",
		"I bring peace, love, and memes to this stream. Who’s with me?",
		"Good news: I’m here. Bad news: I brought zero preparation. Let’s go!",
		"Hello, mere mortals! I’m here to lend my semi-average skills to the cause.",
		"They told me I couldn’t join. So here I am. What’s the plan?",
		"Challenge accepted. Disclaimer: I might fail, but it’ll be hilarious.",
		"Greetings, humans! I’ve arrived to make you laugh… or cringe. No refunds!",
		"Hey everyone, let’s make this stream so good even the Wi-Fi won’t drop!",
		"I heard someone needs backup! Oh… wait, I was the backup. Uh-oh.",
		"I’m here to help! (Disclaimer: ‘Help’ may include causing mild confusion.)",
		"Like a wild Pokémon, I’ve appeared. Will you battle me or throw snacks?",
		"Just rolled a natural 20 on my ‘Join Stream’ check. Let’s goooo!",
		"I’ve entered the chat. Did someone say ‘instant win’? No? Oh well."
			};


			Random rand = new Random();


			return messageList[rand.Next(0, messageList.Count - 1)]?.ToString();

		}

	}
}
