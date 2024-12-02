using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WindowsInput;
using WindowsInput.Native;

namespace OBS_Twitch_Challange_BoT.Services
{

	


	///END OF INPUT SIMULATOR
	///
	public class GameControlService
	{
		public InputSimulator inputSimulator;

		public GameControlService()
		{
			inputSimulator = new InputSimulator();
		}

		public void SimulateKeyPress(VirtualKeyCode key,int durationInMilliseconds)
		{
			inputSimulator.Keyboard.KeyDown(key); // Simulate key press
												  // Wait for the specified duration (in milliseconds)
			Thread.Sleep(durationInMilliseconds); // Wait for the specified duration (e.g., 5000ms = 5 seconds)

			// Simulate key release (KeyUp)
			inputSimulator.Keyboard.KeyUp(key);


		}
		// Simulate horizontal mouse movement (turning left or right)
		public void SimulateMouseTurn(float deltaX)
		{
			inputSimulator.Mouse.MoveMouseBy((int)deltaX, 0); // Horizontal turn (left or right)
		}

		// Simulate vertical mouse movement (looking up or down)
		public void SimulateMouseLook(float deltaY)
		{
			inputSimulator.Mouse.MoveMouseBy(0, (int)deltaY); // Vertical turn (up or down)
		}


		// Simulate a Left Mouse Click
		public void SimulateLeftClick()
		{
			inputSimulator.Mouse.LeftButtonClick();
		}

		// Simulate a Right Mouse Click
		public void SimulateRightClick()
		{
			inputSimulator.Mouse.RightButtonClick();
		
		}
		public void SimulateScrollDown()
		{
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