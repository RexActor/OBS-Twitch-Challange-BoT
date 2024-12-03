using System.Windows.Media;

using TwitchLib.Client;
using TwitchLib.Client.Models;

using WindowsInput.Native;

namespace OBS_Twitch_Challange_BoT.Services
{
    public class SeaOfThievesControl
    {
        private readonly GameControlService _gameControlService;
        private readonly LogService _logService;

        public SeaOfThievesControl(GameControlService gameControlService,LogService logService)
        {
            _gameControlService = gameControlService;
            _logService = logService;
        }

        public void SelectItem(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ITEMS] Selecting Items [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {

                "right" => VirtualKeyCode.RIGHT,
                "left" => VirtualKeyCode.LEFT,
                "up" => VirtualKeyCode.UP,
                "down" => VirtualKeyCode.DOWN,
                _ => VirtualKeyCode.ESCAPE  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 0);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[SOT-SERVICE][ITEMS] Invalid argument received.", Brushes.Wheat);
            }
        }

        public void SimulateEat(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][EAT] Simulating eating [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {

                _ => VirtualKeyCode.XBUTTON1  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 2000);  // Directly pass the non-nullable VirtualKeyCode
                _gameControlService.SimulateLeftClick();
            }
            else
            {
                _logService.Log("[SOT-SERVICE][EAT] Invalid movement argument received.", Brushes.Wheat);
            }

        }

        public void SimulateItemDrop(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ITEM] Drop Item [ {arguments} ]  ", Brushes.Wheat);


            _gameControlService.SimulateScrollDown();  // Directly pass the non-nullable VirtualKeyCode


        }

        public void SimulateJump(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][JUMP] Jumping [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {


                _ => VirtualKeyCode.SPACE  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 0);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[Twitch-Service][CHANNEL] Invalid argument received.", Brushes.Wheat);
            }
        }
        public void SimulateLook(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][LOOK] Looking direction [ {arguments} ]  ", Brushes.Wheat);
            // Example of chat input handling
            var direction = arguments.Trim().ToLowerInvariant() switch
            {

                "up" => -450,      // Look up (negative Y value)
                "down" => 450,     // Look down (positive Y value)
                _ => 0           // No movement if invalid
            };

            // Simulate the mouse turn based on the input
            if (direction != 0)
            {
                _gameControlService.SimulateMouseLook(direction);  // Simulate horizontal turn
                            }
            else
            {
                _logService.Log("[SOT-SERVICE][LOOK] Invalid Look direction argument received.", Brushes.Wheat);
            }

        }

        public void SimulatePickup(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ITEM] Picking Up Items [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {

                _ => VirtualKeyCode.VK_F  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 0);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[SOT-SERVICE][ITEM] Invalid argument received.", Brushes.Wheat);
            }
        }

        public void SimulateTurn(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ROTATION] Simulating Rotation [ {arguments} ]  ", Brushes.Wheat);
            // Example of chat input handling
            var direction = arguments.Trim().ToLowerInvariant() switch
            {
                "left" => -450,    // Move mouse left (negative value)
                "right" => 450,    // Move mouse right (positive value)

                _ => 0           // No movement if invalid
            };

            // Simulate the mouse turn based on the input
            if (direction != 0)
            {


                _gameControlService.SimulateMouseTurn(direction);  // Simulate horizontal turn

            }
            else
            {
                _logService.Log("[SOT-SERVICE][ROTATION] Invalid movement argument received.", Brushes.Wheat);
            }

        }


        public void Simulateuse(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ITEM] Using Items [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {

                _ => VirtualKeyCode.VK_F  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 0);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[SOT-SERVICE][ITEM] Invalid argument received.", Brushes.Wheat);
            }
        }

        public void SimulateWalk(TwitchClient client, ChatMessage chatMessage, string arguments)
        {


            _logService.Log($"[SOT-SERVICE][WALK] Simulating Walk [ {arguments} ]  ", Brushes.Wheat);

            var direction = arguments.Trim().ToLowerInvariant() switch
            {
                "left" => VirtualKeyCode.VK_A,
                "right" => VirtualKeyCode.VK_D,
                "forward" => VirtualKeyCode.VK_W,
                "back" => VirtualKeyCode.VK_S,
                _ => VirtualKeyCode.ESCAPE  // Use a fallback key (e.g., VK_NONE)
            };

            if (direction != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(direction, 500);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[SOT-SERVICE][WALK] Invalid movement argument received.", Brushes.Wheat);
            }


        }

        public void Attack(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ITEM] M1 spam [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {

                _ => VirtualKeyCode.LBUTTON  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 0);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[SOT-SERVICE][ITEM] Invalid argument received.", Brushes.Wheat);
            }
        }

        public void TakePlank(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ITEM] Grab plank [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {

                _ => VirtualKeyCode.MBUTTON  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 0);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[SOT-SERVICE][ITEM] Invalid argument received.", Brushes.Wheat);
            }
        }

        public void TakeSniper(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ITEM] Grab sniper [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {

                _ => VirtualKeyCode.VK_2  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 0);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[SOT-SERVICE][ITEM] Invalid argument received.", Brushes.Wheat);
            }
        }

        public void TakeSword(TwitchClient client, ChatMessage chatMessage, string arguments)
        {
            _logService.Log($"[SOT-SERVICE][ITEM] Grab sword [ {arguments} ]  ", Brushes.Wheat);

            var action = arguments.Trim().ToLowerInvariant() switch
            {

                _ => VirtualKeyCode.VK_1  // Use a fallback key (e.g., VK_NONE)
            };

            if (action != VirtualKeyCode.ESCAPE)
            {
                _gameControlService.SimulateKeyPress(action, 0);  // Directly pass the non-nullable VirtualKeyCode
            }
            else
            {
                _logService.Log("[SOT-SERVICE][ITEM] Invalid  argument received.", Brushes.Wheat);
            }
        }
    }
}