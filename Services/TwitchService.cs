using Newtonsoft.Json;

using OBS_Twitch_Challange_BoT.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
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
		private int _challangeCommandRequestCount = 0;

		public bool _twitchIsConnected;
		TwitchClient twitchClient;
		private readonly ObsService _obsService;
		private readonly LogService _logService;

		public TwitchService(ObsService obsService, LogService logService)
		{
			_obsService = obsService;
			_logService = logService;

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
				_logService.Log($"[Twitch-Service][Action] Bot Commands reloaded. Total {_commands.Count} commands found", Brushes.LightBlue);
#if DEBUG
				Debug.WriteLine($"Bot Commands reloaded. Total {_commands.Count} commands found");
#endif
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
			string isConnected = twitchClient.IsConnected ? "Yes" : "No";
			_logService.Log($"[Twitch-Service][CONNECTION] Am I connected to Twitch? {isConnected}", Brushes.LightBlue);
#if DEBUG
			Debug.WriteLine($"I'm connected? {twitchClient.IsConnected}");
#endif
			TwitchConnectionChanged?.Invoke(connected);
		}

		public void ConnectToTwitch()
		{
			if (twitchClient is not null)
			{
				_logService.Log($"[Twitch-Service][CONNECTION] There is already bot active. I'm not attempting to connect", Brushes.LightBlue);
#if DEBUG
				Debug.WriteLine("There is already bot active. I'm not attempting to connect");
#endif
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

			_logService.Log($"[Twitch-Service][CONNECTION] Connecting to Twitch...", Brushes.LightBlue);

			twitchClient.Connect();


		}

		private void TwitchClient_OnConnectionError(object? sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
		{
			if (twitchClient.IsConnected)
			{
				return;
			}
			_logService.Log($"[Twitch-Service][CONNECTION] Some error happened {e.Error.Message}", Brushes.LightBlue);
			_logService.Log($"[Twitch-Service][CONNECTION] Trying to reconnect....", Brushes.LightBlue);
#if DEBUG

			Debug.WriteLine($"Some error happened {e.Error.Message}");
			Debug.WriteLine($"Trying to reconnect....");
#endif

			ConnectToTwitch();
		}

		public void Disconnect()
		{
			if (twitchClient.IsConnected)
			{
				twitchClient.SendMessage(channelName, PickLeaveMessage());

				_logService.Log($"[Twitch-Service][CONNECTION] I'm disconnecting from {channelName}", Brushes.LightBlue);

#if DEBUG
				Debug.WriteLine($"I'm disconnecting from {channelName}");
#endif
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

		public void SendMessage(string message)
		{
			twitchClient.SendMessage(channelName, message);
		}

		private void Client_OnDisconnected(object? sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
		{

			_logService.Log($"[Twitch-Service][CONNECTION] I left {channelName}", Brushes.LightBlue);
#if DEBUG
			Debug.WriteLine($"I left {channelName}");
#endif

			TwitchIsConnected = false;
		}

		private void Client_OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
		{

			_logService.Log($"[Twitch-Service][CONNECTION] Connection Successfull", Brushes.LightBlue);
			TwitchIsConnected = twitchClient.IsConnected;
			HandleJoinMessage(twitchClient, channelName);

		}

		private void HandleJoinMessage(TwitchClient twitchClient, string channelName)
		{

			_logService.Log($"[Twitch-Service][CHANNEL] I joined {channelName}", Brushes.LightBlue);
			twitchClient.SendMessage(channelName, PickJoinMessage());
		}

		private void Client_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
		{


			if (sender is not TwitchClient client)
			{
				return;
			}


			_logService.Log($"[Twitch-Service][CHANNEL] {e.ChatMessage.DisplayName} : {e.ChatMessage.Message}", Brushes.LightBlue);
#if DEBUG
			Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {e.ChatMessage.DisplayName}:{e.ChatMessage.Message}");
#endif

			if (e.ChatMessage.Message.StartsWith("!"))
			{

				var messageParts = e.ChatMessage.Message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (messageParts.Length == 0) { return; } //ignoring empty messages

				var command = messageParts[0].ToLowerInvariant();//first word is the command

				var arguments = messageParts.Length > 1 ? messageParts[1] : string.Empty; // remaining text is argument

				HandleCommand(client, e.ChatMessage, command, arguments);
			}


		}


		private void HandleCommand(TwitchClient client, ChatMessage chatMessage, string command, string arguments)
		{

			//check if command exists in commands.json file
			_logService.Log($"[Twitch-Service][CHANNEL] Command {command} received... ", Brushes.LightBlue);

			var mathcedCommand = _commands.FirstOrDefault(c => c.CommandText.Equals(command, StringComparison.InvariantCultureIgnoreCase));
			if (mathcedCommand != null)
			{

				client.SendMessage(chatMessage.Channel, mathcedCommand.Response);
				return;
			}

			switch (command)
			{

				case "!roll":

					HandleChallangeCommand(client, chatMessage);
					break;
				case "!leave":
					HandleLeaveCommand(client, chatMessage);
					break;
			}


		}

		private void HandleChallangeCommand(TwitchClient client, ChatMessage chatMessage)
		{
			if (chatMessage.IsModerator || chatMessage.IsBroadcaster)
			{
				if (!_obsService.ObsIsConnected)
				{
					_logService.Log($"[Twitch-Service][ERROR] OBS is not connected. Can't execute ROLL command Command have TryCount of [{_challangeCommandRequestCount}]", Brushes.LightBlue);
					client.SendMessage(chatMessage.Channel, PickOBSConnectionErroMessage(chatMessage.Username, _challangeCommandRequestCount));
					_challangeCommandRequestCount++;
					return;
				}

				_logService.Log($"[Twitch-Service][ACTION] Handling Challange Command", Brushes.LightBlue);

				//Deactivating Text and Description sources
				_obsService.DeActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceTitle);

				_obsService.DeActivateSource(Properties.Settings.Default.ObsScene, Properties.Settings.Default.ObsSourceDesc);


				//Activating Overlay Source
				_obsService.ActivateSource(Properties.Settings.Default.ObsOverlayScene, Properties.Settings.Default.ObsSourceOverlay);
				_challangeCommandRequestCount = 0;
			}
		}

		private void HandleLeaveCommand(TwitchClient client, ChatMessage chatMessage)
		{


			if (chatMessage.Message.Equals("!leave", StringComparison.InvariantCultureIgnoreCase))
			{

				_logService.Log($"[Twitch-Service][ACTION] Handling Leave Command", Brushes.LightBlue);

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

		private string PickOBSConnectionErroMessage(string username, int tryCount)
		{
			var messageList = new JsonArray {
			  $"Hey, {username}, OBS isn’t connected! I feel like a balloon with no string—just floating aimlessly.",

$"Uh-oh, {username}, no connection! I’m like a car with no fuel. Let’s fix this before I stall out!",

$"OBS is missing its connection, {username}. I’m basically popcorn in a cold pan—no pop, just sadness.",

$"Hey, {username}, looks like the link’s missing! I’m like a TV remote with dead batteries—no control here!",

$"No connection, {username}? I feel like a bird trying to fly without wings. Let’s get reconnected!",

$"OBS isn’t connected, {username}. I’m out here like a laptop with no Wi-Fi—completely useless!",

$"Oops, {username}! No link detected. I’m like a sandwich without bread—just a pile of confusion.",

$"OBS is disconnected, {username}, and I’m feeling like a GPS with no signal—lost and dramatic!",

$"Connection missing, {username}! I’m like a rocket without fuel—ready to launch, but stuck on the ground.",

$"OBS says nope, {username}. I’m like a pen that’s out of ink—can’t write, can’t work, just sitting here.",

$"Hey, {username}, where’s my link? I’m like a lightbulb with no electricity—dim and disappointed.",

$"OBS isn’t connected, {username}? I feel like a pizza with no cheese—this is not okay.",

$"No connection detected, {username}. I’m basically a plane without a pilot—grounded and confused.",

$"OBS is disconnected, {username}. I’m like a marathon runner with no shoes—going nowhere fast.",

$"Oops, {username}, the link’s gone! I’m like a joke with no punchline—awkward and unproductive.",

$"OBS isn’t connected, {username}? Great, now I’m just a toaster with no bread—pointless!",

$"Connection missing, {username}? I feel like a phone with no signal—staring into the void.",

$"OBS says ‘no connection,’ {username}. I’m like a kite with no wind—just stuck here!",

$"Hey, {username}, OBS isn’t linked! I’m like a DJ with no turntables—what am I even doing here?",

$"OBS isn’t connected, {username}? I feel like a movie with no ending—suspenseful and incomplete."
			};

			var messageListAnnoyed = new JsonArray
			{
				$"Seriously, {username}? OBS isn’t connected again! I’m starting to feel like a balloon with no string—help me help you!",

$"Uh-oh, {username}, still no connection? I’m like a car with no fuel, and I’ve asked before! Plug me in already!",

$"OBS is missing its connection, {username}. This is the {_challangeCommandRequestCount} time I’m saying it—I’m basically popcorn in a cold pan. Do something!",

$"Hey, {username}, OBS is still not connected? I’m like a remote with dead batteries, and it’s getting old. Fix it!",

$"No connection yet, {username}? I feel like a bird trying to fly without wings... and this bird is getting annoyed.",

$"OBS isn’t connected, {username}. Didn’t I mention this before? I’m like a laptop with no Wi-Fi—still useless, still waiting!",

$"Oops, {username}! No link detected—again. I’m like a sandwich without bread, and now I’m getting hangry!",

$"OBS is disconnected, {username}. How many times do I have to say it? I’m a GPS with no signal—still lost, still dramatic!",

$"Connection missing, {username}! I’m like a rocket without fuel—ready to launch, but I’ve been on the ground way too long now.",

$"OBS says nope, {username}. I’m like a pen that’s out of ink, and at this point, I’ve run out of patience too!",

$"Hey, {username}, where’s the link? I’ve been asking! I’m like a lightbulb with no electricity—dim, disappointed, and now frustrated.",

$"OBS isn’t connected, {username}? For real? I’m a pizza with no cheese, and this situation is extra crusty now!",

$"No connection detected, {username}. I’m basically a plane without a pilot, and I’ve been grounded for way too long!",

$"OBS is still disconnected, {username}? I’m like a marathon runner with no shoes... and I’m running out of patience!",

$"Oops, {username}, the link’s still gone! I’m a joke with no punchline—and at this point, the joke’s on me.",

$"OBS isn’t connected, {username}? Great. I’m just a toaster with no bread, and I’m officially over it now.",

$"Connection missing, {username}? I’m like a phone with no signal, and I’ve been waiting on bars for ages!",

$"OBS says ‘no connection,’ {username}. AGAIN.’ I’m a kite with no wind—and my string is about to snap!",

$"Hey, {username}, OBS still isn’t linked? I’m a DJ with no turntables... and now I’m just standing here annoyed!",

$"OBS isn’t connected, {username}? Really? I feel like a movie with no ending, and now I’m just stuck in a bad sequel."
			};

			var messageListAngry = new JsonArray
			{
				$"Okay, {username}, OBS STILL isn’t connected? I’ve been asking, begging, pleading. Do I need to send a carrier pigeon or what?!",

$"SERIOUSLY, {username}? No connection AGAIN? I’m about to become a balloon with no helium—and explode. Fix it already!",

$"OBS is STILL not connected, {username}? I’m out here like a popcorn kernel in a cold pan—ready to pop, but now I’m just STEAMING.",

$"Oh, come on, {username}. How many times do I need to ask? I’m like a car with no fuel, except now I’m about to burst into flames.",

$"OBS isn’t connected, {username}? For real?! I’m a bird without wings—except now I’m more like a bird ready to peck someone. Get it done!",

$"Hey, {username}, where’s my link? I’ve been polite. I’ve been patient. But I’m DONE. I’m a lightbulb, and I’m about to shatter.",

$"OBS is STILL not connected, {username}? I’m over it. I’m a sandwich with no bread—except now I’m just crumbs of rage!",

$"No connection, {username}? AGAIN?! I’m a rocket without fuel, and I’m ready to explode. Plug me in RIGHT NOW.",

$"OBS says nope AGAIN, {username}? I’m a pen out of ink, but now I’m writing complaints in permanent marker. FIX. IT.",

$"Hey, {username}, this is the last time I’m asking. I’m a kite without wind—but now I’m a storm cloud ready to rain fury.",

$"OBS still isn’t connected, {username}? I’m a pizza with no cheese, and now I’m flipping the table and throwing the crust at you.",

$"No connection detected, {username}. How is this STILL a problem? I’m about to ground this plane permanently. DO SOMETHING.",

$"OBS isn’t connected, {username}. AGAIN. I’m a marathon runner with no shoes, but now I’m stomping in frustration. Get it DONE.",

$"Oops, {username}, no link—again! No. More. Excuses. I’m a joke with no punchline, and now I’m the one laughing—maniacally.",

$"OBS isn’t connected, {username}? I’m a toaster with no bread, and now I’m just toast. Burnt, fiery, angry toast.",

$"Connection missing AGAIN, {username}? I’m a phone with no signal—but now I’m ready to throw the phone out the window.",

$"OBS says ‘no connection,’ {username}. STILL?! I’m a kite with no wind, and now I’m ready to cut the string altogether.",

$"Hey, {username}, OBS isn’t linked? AGAIN? I’m a DJ with no turntables, but now I’m about to smash the whole booth.",

$"OBS isn’t connected, {username}? I’m a movie with no ending, but now it’s just a horror film—and you’re the main character. Fix it.",

$"Okay, {username}, OBS still isn’t connected?! I’m a volcano now, and I’m about to erupt if you don’t sort this out RIGHT NOW!"

			};


			Random rand = new Random();


			var selectedMessageList = tryCount switch
			{
				> 5 => messageListAngry,
				> 3 => messageListAnnoyed,
				_ => messageList,
			};


			return selectedMessageList[rand.Next(0, selectedMessageList.Count - 1)]?.ToString();

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
