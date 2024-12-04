using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WindowsInput;
using WindowsInput.Native;

namespace OBS_Twitch_Challange_BoT.Services
{




    ///END OF INPUT SIMULATOR
    ///
    public class GameControlService
    {
        public InputSimulator inputSimulator;
        private Dictionary<string, DateTime> _actionCooldowns;
        private readonly int _defaultCooldownMilliSeconds;

        public GameControlService(int defaultCooldownMilliSeconds = 2000)
        {
            inputSimulator = new InputSimulator();
            _actionCooldowns = new Dictionary<string, DateTime>();
            _defaultCooldownMilliSeconds = defaultCooldownMilliSeconds;
        }

        private bool CanExecuteAction(string action, int cooldownMilliseconds)
        {
            if (_actionCooldowns.TryGetValue(action, out DateTime cooldownUntil))
            {
                if (DateTime.Now < cooldownUntil)
                {
                    return false; //Still on Cooldown
                }
            }
            _actionCooldowns[action] = DateTime.Now.AddMicroseconds(cooldownMilliseconds);
            return true;
        }

        public void SimulateKeyPress(VirtualKeyCode key, int durationInMilliseconds,int cooldownMilliseconds=-1)
        {

            string actionKey = $"KeyPress-{key}";
            // Use default cooldown if none specified
            cooldownMilliseconds = cooldownMilliseconds > 0 ? cooldownMilliseconds : _defaultCooldownMilliSeconds;

            if (!CanExecuteAction(actionKey, cooldownMilliseconds))
            {
                return;
            }

            inputSimulator.Keyboard.KeyDown(key); // Simulate key press
                                                  // Wait for the specified duration (in milliseconds)
            Thread.Sleep(durationInMilliseconds); // Wait for the specified duration (e.g., 5000ms = 5 seconds)

            // Simulate key release (KeyUp)
            inputSimulator.Keyboard.KeyUp(key);


        }
        // Simulate horizontal mouse movement (turning left or right)
        public void SimulateMouseTurn(float deltaX,int cooldownMilliseconds=-1)
        {

            string actionKey = $"MouseTurn-{deltaX}";
            // Use default cooldown if none specified
            cooldownMilliseconds = cooldownMilliseconds > 0 ? cooldownMilliseconds : _defaultCooldownMilliSeconds;

            if (!CanExecuteAction(actionKey, cooldownMilliseconds))
            {
                return;
            }


            inputSimulator.Mouse.MoveMouseBy((int)deltaX, 0); // Horizontal turn (left or right)
        }

        // Simulate vertical mouse movement (looking up or down)
        public void SimulateMouseLook(float deltaY, int cooldownMilliseconds = -1)
        {

            string actionKey = $"MouseTurn-{deltaY}";
            // Use default cooldown if none specified
            cooldownMilliseconds = cooldownMilliseconds > 0 ? cooldownMilliseconds : _defaultCooldownMilliSeconds;

            if (!CanExecuteAction(actionKey, cooldownMilliseconds))
            {
                return;
            }

            inputSimulator.Mouse.MoveMouseBy(0, (int)deltaY); // Vertical turn (up or down)
        }


        // Simulate a Left Mouse Click
        public void SimulateLeftClick(int cooldownMilliseconds = -1)
        {
            string actionKey = $"LeftClick";
            // Use default cooldown if none specified
            cooldownMilliseconds = cooldownMilliseconds > 0 ? cooldownMilliseconds : _defaultCooldownMilliSeconds;

            if (!CanExecuteAction(actionKey, cooldownMilliseconds))
            {
                return;
            }

            inputSimulator.Mouse.LeftButtonClick();
        }

        // Simulate a Right Mouse Click
        public void SimulateRightClick(int cooldownMilliseconds=-1)
        {

            string actionKey = $"RightClick";
            // Use default cooldown if none specified
            cooldownMilliseconds = cooldownMilliseconds > 0 ? cooldownMilliseconds : _defaultCooldownMilliSeconds;

            if (!CanExecuteAction(actionKey, cooldownMilliseconds))
            {
                return;
            }
            inputSimulator.Mouse.RightButtonClick();

        }
        public void SimulateScrollDown(int cooldownMilliseconds=-1 )
        {

            string actionKey = $"SimulateScrollDown";
            // Use default cooldown if none specified
            cooldownMilliseconds = cooldownMilliseconds > 0 ? cooldownMilliseconds : _defaultCooldownMilliSeconds;

            if (!CanExecuteAction(actionKey, cooldownMilliseconds))
            {
                return;
            }
            inputSimulator.Mouse.VerticalScroll(-5);
        }
        // Simulate Mouse Down and Mouse Up (for custom clicks)
        public void SimulateMouseDownAndUp()
        {
            inputSimulator.Mouse.LeftButtonDown();   // Mouse down (press)
            inputSimulator.Mouse.LeftButtonUp();     // Mouse up (release)
        }



    }





}