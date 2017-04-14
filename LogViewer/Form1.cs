using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LogViewer
{
    public partial class Form1 : Form
    {
        public class LogDir
        {
            public Dictionary<String, List<String>> Logs = new Dictionary<string, List<string>>();
        }

        private static Dictionary<String, LogDir> _subdirs = new Dictionary<string, LogDir>();
        private static System.Timers.Timer _update_timer;
        private static String _selected_channel = String.Empty;
        private static String _selected_month = String.Empty;
        private static String _selected_day = String.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateDirectories();

            _update_timer = new System.Timers.Timer(10000);
            _update_timer.Elapsed += delegate
            {
                UpdateDirectories();
            };
            _update_timer.Start();
        }

        private void UpdateDirectories()
        {
            if (!Directory.Exists("./Storage/Logs"))
                return;

            String[] chans = Directory.GetDirectories("./Storage/Logs");

            foreach (String chan in chans)
            {
                LogDir dir = new LogDir();
                String _chan = Path.GetFileName(chan);

                if (ChannelDropdown.Items.Contains(_chan))
                    continue;

                ChannelDropdown.Items.Add(_chan);

                String[] months = Directory.GetDirectories("./Storage/Logs/" + _chan);

                foreach (String month in months)
                {
                    String _month = Path.GetFileName(month);

                    if (!dir.Logs.ContainsKey(_month))
                    {
                        List<String> days = new List<string>();

                        foreach (String day in Directory.GetFiles("./Storage/Logs/" + _chan + "/" + _month))
                        {
                            days.Add(Path.GetFileName(day));
                        }

                        dir.Logs.Add(_month, days);
                    }
                }

                _subdirs.Add(_chan, dir);
            }
        }

        private void ChannelDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selected_channel = (String)ChannelDropdown.SelectedItem;

            MonthDropdown.Enabled = false;
            DayDropdown.Enabled = false;
            LogOutput.Enabled = false;

            MonthDropdown.Items.Clear();
            DayDropdown.Items.Clear();

            MonthDropdown.Text = String.Empty;
            DayDropdown.Text = String.Empty;

            _selected_month = String.Empty;
            _selected_day = String.Empty;

            if (!_selected_channel.StartsWith("#"))
            {
                return;
            }

            if (_subdirs.ContainsKey(_selected_channel))
            {
                foreach (String month in _subdirs[_selected_channel].Logs.Keys)
                {
                    MonthDropdown.Items.Add(month);
                }

                MonthDropdown.Enabled = true;
            }
        }

        private void MonthDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selected_month = (String)MonthDropdown.SelectedItem;

            DayDropdown.Enabled = false;
            LogOutput.Enabled = false;

            DayDropdown.Items.Clear();

            _selected_day = String.Empty;

            if (String.IsNullOrWhiteSpace(_selected_month))
            {
                return;
            }

            if (_subdirs.ContainsKey(_selected_channel))
            {
                if (_subdirs[_selected_channel].Logs.ContainsKey(_selected_month))
                {
                    foreach (String day in _subdirs[_selected_channel].Logs[_selected_month])
                    {
                        DayDropdown.Items.Add(day);
                    }

                    DayDropdown.Enabled = true;
                }
            }
        }

        private void DayDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selected_day = (String)DayDropdown.SelectedItem;

            LogOutput.Clear();
            LogOutput.Enabled = false;

            if (String.IsNullOrWhiteSpace(_selected_day))
            {
                return;
            }

            if (_subdirs.ContainsKey(_selected_channel))
            {
                if (_subdirs[_selected_channel].Logs.ContainsKey(_selected_month))
                {
                    if (_subdirs[_selected_channel].Logs[_selected_month].Contains(_selected_day))
                    {
                        LoadCurrentFile();
                    }
                }
            }
        }

        private void LoadCurrentFile()
        {
            String filename = String.Format("./Storage/Logs/{0}/{1}/{2}", _selected_channel, _selected_month, _selected_day);
            if (File.Exists(filename))
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    LogOutput.Text = reader.ReadToEnd();
                    LogOutput.Enabled = true;
                }
            }
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            LoadCurrentFile();
        }
    }
}
