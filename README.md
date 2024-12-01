
# OBS Twitch Challenge Bot

OBS Twitch Challenge Bot is a versatile tool designed to integrate Twitch functionalities with OBS (Open Broadcaster Software). 
This project aims to enhance the streaming experience by automating commands, managing interactions, and adding customization options.

## Features

- **Twitch Command** Integration: Automate Twitch chat commands and interactions.
- **OBS Control** Manage OBS text sources directly from the bot, updating them with real-time information.
- **Challenge System** Select challenges from a predefined list and display them on stream.
- **HTML Overlay Generation** Create HTML overlay files for enhanced stream visuals.
- **Interactive Console** Monitor real-time logs and bot activities.
- **Customizable Options** Tailor the bot to your specific streaming needs.
## Installation

1. Clone or download this repository:
   ```bash
   git clone https://github.com/your-username/OBS-Twitch-Challange-BoT.git
   ```
2. Open the solution file (`OBS Twitch Challange BoT.sln`) in Visual Studio.
3. Restore NuGet packages if prompted.
4. Build the project to ensure all dependencies are resolved.
5. Run the application.

## Usage

1. Configure your Twitch credentials in the `App.config` file.
2. Start the bot and log in using your Twitch account.
3.Connect to OBS by using OBS websocket login details
4. Monitor activity in the `Application Console`.


```
!challenge
```
Function: Randomly selects a challenge from the predefined list, updates the OBS text source with the challenge, and generates an overlay in the HTML file.

Example Usage:

```
User: !challenge
Bot: Today's challenge is: "No HUD for 5 minutes!"
```


## File and Directory Overview

- **Html/**: Contains HTML overlay for OBS.
- **Models/**: Defines data structures and objects for the bot.
- **Other Files/**: Challange Lists.
- **Properties/**: Contains application settings and resources.
- **Services/**: Core logic and services for Twitch and OBS integration.
- **App.config**: Configuration file for setting up application variables.
- **MainWindow.xaml**: The primary user interface.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork this repository.
2. Create a new branch for your feature or bug fix:
   ```bash
   git checkout -b feature-name
   ```
3. Commit your changes:
   ```bash
   git commit -m "Add feature-name"
   ```
4. Push to your branch:
   ```bash
   git push origin feature-name
   ```
5. Submit a pull request.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.

## Acknowledgments

- [OBS Studio](https://obsproject.com/) for providing an open-source broadcasting platform.
- [Twitch Developers](https://dev.twitch.tv/) for APIs and integration support.
