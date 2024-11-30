using Newtonsoft.Json;
using OBS_Twitch_Challange_BoT.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OBS_Twitch_Challange_BoT.Services
{
	public class HtmlService
	{

		readonly static string fileDirectory = $"{Directory.GetCurrentDirectory()}/Html/";

		public void GenerateHTMLFile(string htmlFile,string title)
		{

		   
			GenerateCSSFile("style.css");

			//check if file exists, if not then generate file

			FileExists(htmlFile);

			//write content into file

			string htmlContent = $@"
			<!DOCTYPE html>
<html lang=""en"">

<head>
	<meta charset=""UTF-8"">
	<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
	<title>Challange</title>
	<link rel=""stylesheet"" href=""style.css"">
</head>

<body>
	<div class=""container"">
		<div class=""title"">
			<h1>{title}</h1>

		</div>
		<div id=""targets-container""></div>
		<div id=""challenge-result"" class=""hidden"">
			<div>
				<h2 id=""selected-challenge""></h2>
			</div>
			<h3 id=""selected-challenge-description"">
			</h3>
		</div>
	</div>

	<script src=""main.js""></script>
</body>

</html>

";
			string filePath = Path.Combine(fileDirectory, htmlFile);
			try
			{
				using (StreamWriter writer = new StreamWriter(filePath))
				{
					writer.WriteLine(htmlContent);

				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"An Error occured while writing to the file :{ex.Message}");
			}
		}

		public void GenerateJavaScriptFile(string jsFileName,string WebSocketAddress,int webSocketPort,List<Challange> challanges,int challangeCount)
		{

		  

			// Generate the challenges list dynamically
			string challengeJson = JsonConvert.SerializeObject(challanges);

			string jsContent = $@"
	// Sound effect when choosing targets
var challengeCount = {challangeCount};
	// List of challenges
	const challenges = {challengeJson};

	// WebSocket to connect to WPF application
	class WebSocketClient {{
		constructor(url) {{
			this.url = url;
			this.socket = null;
		}}

		// Method to establish WebSocket connection
		connect() {{
			this.socket = new WebSocket(this.url); // Create a new WebSocket connection

			// Define event handlers
			this.socket.onopen = this.onOpen.bind(this);
			this.socket.onmessage = this.onMessage.bind(this);
			this.socket.onerror = this.onError.bind(this);
			this.socket.onclose = this.onClose.bind(this);
		}}

		// Called when the WebSocket connection is open
		onOpen() {{
			console.log('Connection with WPF is open');
		}}

		// Called when a message is received from the WebSocket server
		onMessage(event) {{
			console.log('Received from WPF:', event.data);
		}}

		// Called when an error occurs with the WebSocket connection
		onError(error) {{
			console.error('WebSocket error:', error);
		}}

		// Called when the WebSocket connection is closed
		onClose() {{
			console.log('WebSocket connection closed');
		}}

		// Method to send a message through the WebSocket connection
		sendMessage(message) {{
			if (this.socket && this.socket.readyState === WebSocket.OPEN) {{
				this.socket.send(message);
			}} else {{
				console.error('WebSocket is not open. Cannot send message.');
			}}
		}}

		// Method to close the WebSocket connection
		close() {{
			if (this.socket) {{
				this.socket.close();
			}}
		}}
	}}

	// WebSocket Client initialization with dynamic WebSocket address and port
	const wsClient = new WebSocketClient('{WebSocketAddress}:{webSocketPort}/challenge');
	wsClient.connect();

	// Select challenge count from available challenges
	var selectedChallenges = [];

	while (selectedChallenges.length < challengeCount) {{
		var chosenChallenge = challenges[Math.floor(Math.random() * challenges.length)];
		if (!selectedChallenges.includes(chosenChallenge)) {{
			selectedChallenges.push(chosenChallenge);
		}}
	}}

	// Create targets dynamically
	const targetsContainer = document.getElementById('targets-container');

	// Function to create targets
	function createTargets() {{
		for (let i = 0; i < selectedChallenges.length; i++) {{
			const target = document.createElement('div');
			target.classList.add('target');
			target.setAttribute('data-index', i); // Store index in data attribute
			targetsContainer.appendChild(target);
		}}
	}}

	// Function to simulate randomly shooting a target
	function shootRandomTarget() {{
		wsClient.sendMessage('rolling challenge');
		const targets = document.querySelectorAll('.target');
		const randomTargetIndex = Math.floor(Math.random() * targets.length);
		const randomTarget = targets[randomTargetIndex];

		// Clearing TEXT SOURCES before applying new challenge
		let selectionInProgress = true;
		let counter = 0;
		const highlightInterval = setInterval(() => {{
			const randomTargetIndex = Math.floor(Math.random() * targets.length); // Select random target
			const target = targets[randomTargetIndex];

			target.classList.add('target-highlight'); // Highlight the target

			// Reset previous target highlight after a short time
			setTimeout(() => {{
				target.classList.remove('target-highlight');
			}}, 250);

			counter++;
			if (counter >= 10) {{
				clearInterval(highlightInterval); // Stop the random highlight process
				selectionInProgress = false;
				target.classList.remove('target-highlight');
				randomTarget.classList.add('target-shot'); // Shot effect on the selected target

				// Select a random challenge
				const randomChallenge = challenges[Math.floor(Math.random() * challenges.length)];

				// Display the selected challenge
				const challengeResult = document.getElementById('challenge-result');
				const selectedChallengeElement = document.getElementById('selected-challenge');
				const selectedChallengeDescElement = document.getElementById('selected-challenge-description');
				const textDisplayInterval = setInterval(() => {{
					selectedChallengeElement.textContent = randomChallenge.Title;
					selectedChallengeDescElement.textContent = randomChallenge.Desc;
				   
					wsClient.sendMessage(JSON.stringify(randomChallenge));
					clearInterval(textDisplayInterval);
				}}, 500);

				challengeResult.classList.remove('hidden');
			}}
		}}, 500); // Change target every 500ms
	}}

	// Initialize the game by creating targets and automatically shooting a random target
	createTargets();

	// Add a delay before starting the target selection process
	setTimeout(() => {{
		shootRandomTarget();
	}}, 1500); // 1.5 second delay before the selection process starts
	";

			string filePath = Path.Combine(fileDirectory, jsFileName);
			try
			{
				using (StreamWriter writer = new StreamWriter(filePath))
				{
					writer.WriteLine(jsContent);

				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"An Error occured while writing to the file :{ex.Message}");
			}

		}


		public void GenerateCSSFile(string cssFileName)
		{


			//check if file exists, if not then generate file
			FileExists(cssFileName);

			string cssContent = @"/* General styles */
body {
	font-family: 'Arial', sans-serif;
	display: flex;
	background-color: green;
	justify-content: center;
	align-items: center;
	height: 100vh;
	margin: 0;

}

.container {
	text-align: center;
	padding: 20px;
}

.title {
	color: red;
}

.title>h1 {
	font-family: cursive;
	font-size: 70px;
	font-weight: bolder;
	animation: colorchange 3s infinite;
}


@keyframes colorchange {
	0% {
		color: red;
	}

	50% {
		color: blue
	}

	100% {
		color: green
	}

}



h1 {
	font-size: 2rem;
}

#targets-container {
	display: flex;
	justify-content: center;
	flex-wrap: wrap;
	gap: 20px;
	margin-top: 20px;
}

.target {
	width: 80px;
	height: 80px;
	background-color: #fff;
	border-radius: 50%;
	border: 5px solid #333;
	position: relative;
	cursor: pointer;
	box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
	transition: transform 0.2s ease;
}

.target:hover {
	transform: scale(1.1);
}

.target:active {
	transform: scale(0.9);
}

.target:before,
.target:after {
	content: """";
	position: absolute;
	top: 50%;
	left: 50%;
	width: 50%;
	height: 50%;
	border-radius: 50%;
	background-color: transparent;
	border: 2px solid #333;
	transform: translate(-50%, -50%);
}

.target:before {
	width: 70%;
	height: 70%;
}

.target:after {
	width: 40%;
	height: 40%;
}

.target-shot {
	animation: shotEffect 0.5s forwards;
}

@keyframes shotEffect {
	0% {
		transform: scale(1);
		background-color: #fff;
	}

	50% {
		transform: scale(1.2);
		background-color: #FF6347;
		/* Red color to show shot */
	}

	100% {
		transform: scale(1);
		background-color: #FF6347;
		/* Red color to indicate hit */
	}
}

.target-highlight {
	animation: highlightEffect 0.3s alternate infinite;
}

@keyframes highlightEffect {
	0% {
		transform: scale(1);
		border-color: #3498db;
		box-shadow: 0 0 10px rgba(52, 152, 219, 0.8);
	}

	100% {
		transform: scale(1.2);
		border-color: #e74c3c;
		box-shadow: 0 0 15px rgba(231, 76, 60, 0.8);
	}
}

#challenge-result {
	margin-top: 40px;
	font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	color: red;
}

#selected-challenge {
	font-size: 50px;
}

#selected-challenge-description {
	font-size: 30px;
}

#challenge-result.hidden {
	display: none;
}

#retry-btn {
	margin-top: 20px;
	padding: 10px 20px;
	background-color: #3498db;
	color: white;
	border: none;
	border-radius: 5px;
	cursor: pointer;
}

#retry-btn:hover {
	background-color: #2980b9;
}";

			string filePath = Path.Combine(fileDirectory, cssFileName);
			try
			{
				using (StreamWriter writer = new StreamWriter(filePath))
				{
					writer.WriteLine(cssContent);

				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"An Error occured while writing to the file :{ex.Message}");
			}

		}


		private static void FileExists(string fileName)
		{


			var result = File.Exists($"{fileDirectory}{fileName}");

			if (!result)
			{
				File.Create($"{fileDirectory}{fileName}");
			}

		}


	}
}
