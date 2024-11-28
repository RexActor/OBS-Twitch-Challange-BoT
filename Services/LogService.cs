using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBS_Twitch_Challange_BoT.Services
{
	public class LogService
	{
		public event Action<string, Brush> LogUpdated;

		private readonly ConcurrentQueue<(string Message, Brush Color)> _logQueue = new();


		public void Log(string message, Brush color)
		{
			string timeStampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
			_logQueue.Enqueue((timeStampedMessage, color));
			LogUpdated?.Invoke(timeStampedMessage, color);

#if DEBUG
			Debug.WriteLine(timeStampedMessage);
#endif
		}


		public (string Message, Brush Color)[] GetAllLogs()
		{
			return _logQueue.ToArray();
		}

	}
}
