using Newtonsoft.Json;

using OBS_Twitch_Challange_BoT.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OBS_Twitch_Challange_BoT
{
    /// <summary>
    /// Interaction logic for CommandPage.xaml
    /// </summary>
    public partial class CommandPage : UserControl
    {

        private List<BotCommand>commands = new List<BotCommand>();
        private string fileName = "commands.json";
        public CommandPage()
        {
            InitializeComponent();
            LoadCommands(fileName);
        }

        private void LoadCommands(string file)
        {
            
            if (File.Exists(file)) { 
            
            string botCommandsJson = File.ReadAllText(file);
                commands = JsonConvert.DeserializeObject<List<BotCommand>>(botCommandsJson);
                foreach(var command in commands)
                {
                    CommandListBox.Items.Add(command.CommandText);
                }
            
            }
        }

        private void AddCommandBtn_Click(object sender, RoutedEventArgs e)
        {
            string commandText= CommandTextBox.Text;
            string response = ResponseTextBox.Text;


            if (!string.IsNullOrEmpty(commandText) && !string.IsNullOrEmpty(response)) {

                BotCommand newCommand = new BotCommand
                {
                    CommandText = commandText,
                    Response = response
                };
                commands.Add(newCommand);
                CommandListBox.Items.Add(newCommand.CommandText);
                SaveCommands(fileName);
                 CommandTextBox.Text=string.Empty;
                ResponseTextBox.Text =string.Empty;


            }
        }

        private void SaveCommands(string file)
        {
            string botComamndsJson = JsonConvert.SerializeObject(commands,Formatting.Indented);
            File.WriteAllText(file,botComamndsJson);
        }

        private void RemoveCommandBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CommandListBox.SelectedItem != null) { 
            
            string selectedCommand = CommandListBox.SelectedItem.ToString();
                var commandToRemove = commands.FirstOrDefault(c=>c.CommandText == selectedCommand);
                if (commandToRemove != null) { 
                    commands.Remove(commandToRemove);
                    CommandListBox.Items.Remove(commandToRemove);
                    SaveCommands(fileName);
                }
            }
        }
    }
}
