using OBS_Twitch_Challange_BoT.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OBS_Twitch_Challange_BoT
{
	/// <summary>
	/// Interaction logic for ConsolePage.xaml
	/// </summary>
	public partial class ConsolePage : UserControl
	{

		private readonly LogService _logService;

		public ConsolePage(LogService logService)
		{
			_logService = logService;

			InitializeComponent();

			
			_logService.LogUpdated += OnLogUpdated;
		}

		

		private void OnLogUpdated(string logMessage, Brush color)
		{
			Dispatcher.Invoke(() =>
			{
				// Move the caret to the end of the document
				LogTextBox.Selection.Select(LogTextBox.Document.ContentEnd, LogTextBox.Document.ContentEnd);
				LogTextBox.ScrollToEnd(); // Ensure it scrolls to the bottom
										  // Create a new Run with the log message
				var run = new Run(logMessage)
				{
					Foreground = color
				};

				// Add it to the RichTextBox
				LogTextBox.Document.Blocks.Add(new Paragraph(run));

				// Scroll to the bottom
				LogTextBox.ScrollToEnd();
			});
		}
	}
}
