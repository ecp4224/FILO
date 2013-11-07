/*
    This file is part of FILO.

    FILO is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FILO is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FILO.  If not, see <http://www.gnu.org/licenses/>. 
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FILO.Core;
using FILO.Core.Log;
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;

namespace FILO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private delegate void UpdateId(string id);

        private delegate void UpdateButton(Button b, string content);

        private delegate void UpdateLabel(Label l, string content);

        private delegate void UpdateControlState(Control b, bool value);

        private delegate void MoveControl(Control b, int x, int y);

        private delegate void UpdateProgress(ProgressBar b, double progress);
  
        private Connection connection;
        private bool sending;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var testServer = new Connection(ConnectionType.Sender);
            testServer.OnConnectionMade += testServer_OnConnectionMade;
            testServer.OnPortFowardFailed += testServer_OnPortFowardFailed;
            testServer.PrepareConnection();
        }

        void testServer_OnPortFowardFailed(Connection connection)
        {
            MessageBox.Show("Failed to open port!", "Port Foward Failed");
        }

        void testServer_OnConnectionMade(Connection connection)
        {
            new Thread(new ThreadStart(() => progressSend(connection))).Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var testClient = new Connection(ConnectionType.Reciever, "127.0.0.1");
            testClient.OnConnectionMade += testClient_OnConnectionMade;
            testClient.PrepareConnection();
        }

        void testClient_OnConnectionMade(Connection connection)
        {
            new Thread(new ThreadStart(() => progressRecieve(connection))).Start();
        }

        private void progressSend(Connection connection)
        {
            connection.SendFile("testfile.txt");
        }

        private void progressRecieve(Connection connection)
        {
            connection.RecieveFile("testing.txt");
        }

        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = e.NewValue as TreeViewItem;
            if (item == null || item.Tag == null)
                return;
            SendButton.IsEnabled = File.Exists(item.Tag as string);
        }

        private object dummyNode = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RecieveProgressBar.Visibility = Visibility.Hidden;
            StatusLabel.Visibility = Visibility.Hidden;
            CancelButton.Visibility = Visibility.Hidden;
            SendProgressBar.Visibility = Visibility.Hidden;
            SendStatusLabel.Visibility = Visibility.Hidden;
            SendCancelButton.Visibility = Visibility.Hidden;
            SendButton.IsEnabled = false;
            IdTextBox.Text = "Enter Id or IP";
            IdBox.Text = "Please Wait..";
            IdBox.ToolTip = _idHelp;
            new Thread(GetId).Start();
            foreach (var item in Directory.GetLogicalDrives().Select(s => new TreeViewItem {Header = s, Tag = s, FontWeight = FontWeights.Normal}))
            {
                item.Items.Add(dummyNode);
                item.Expanded += item_Expanded;
                FoldersItem.Items.Add(item);
            }
        }

        private void GetId()
        {
            string id;
            using (var web = new WebClient())
            {
                try
                {
                    id = web.DownloadString("http://www.hypereddie.com/utils/filo.php?action=create");
                }
                catch
                {
                    id = "Failed";
                }
            }
            ChangeId(id);
        }

        private void ChangeId(string id)
        {
            if (!IdBox.Dispatcher.CheckAccess())
            {
                IdBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new UpdateId(ChangeId), id);
                return;
            }
            IdBox.Text = id;
        }

        private void ChangeProgressBar(ProgressBar b, double value)
        {
            if (!b.Dispatcher.CheckAccess())
            {
                b.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new UpdateProgress(ChangeProgressBar), b, value);
                return;
            }
            b.Value = value;
        }

        private void ChangeButtonContent(Button b, string id)
        {
            if (!b.Dispatcher.CheckAccess())
            {
                b.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UpdateButton(ChangeButtonContent), b, id);
                return;
            }
            b.Content = id;
        }

        private void SetEnabled(Control b, bool value)
        {
            if (!b.Dispatcher.CheckAccess())
            {
                b.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UpdateControlState(SetEnabled), b, value);
                return;
            }
            b.IsEnabled = value;
            CommandManager.InvalidateRequerySuggested();
        }

        private void SetLabelContent(Label l, string content)
        {
            if (!l.Dispatcher.CheckAccess())
            {
                l.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UpdateLabel(SetLabelContent), l, content);
                return;
            }
            l.Content = content;
        }

        private void SetVisible(Control b, bool value)
        {
            if (!b.Dispatcher.CheckAccess())
            {
                b.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UpdateControlState(SetVisible), b, value);
                return;
            }
            b.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        }

        void item_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (item.Items.Count != 1 || item.Items[0] != dummyNode) return;
            item.Items.Clear();
            try
            {
                foreach (var subitem in Directory.GetDirectories(item.Tag.ToString()).Select(s => new TreeViewItem
                {
                    Header = s.Substring(s.LastIndexOf("\\", System.StringComparison.Ordinal) + 1),
                    Tag = s,
                    FontWeight = FontWeights.Normal
                }))
                {
                    subitem.Items.Add(dummyNode);
                    subitem.Expanded += item_Expanded;
                    item.Items.Add(subitem);
                }
                foreach (var subitem in Directory.GetFiles(item.Tag.ToString()).Select(file => new TreeViewItem()
                {
                    Header = Path.GetFileName(file),
                    Tag = file,
                    FontWeight = FontWeights.Normal
                }))
                {
                    item.Items.Add(subitem);
                }
            }
            catch
            { }
        }

        private void tbControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(IdTextBox.Text))
                IdTextBox.Text = "Enter Id or IP";
        }

        private readonly ToolTip _idHelp = new ToolTip { Content = "Your personal ID hides your IP from other users by masking it.\n Give this ID to the person recieving your file so they can connect." };
        private readonly ToolTip _idCopy = new ToolTip {Content = "Your personal ID has been copied to your clipboard!"};
        private void IdBox_OnMouseEnter(object sender, MouseEventArgs e)
        {
            _idHelp.IsOpen = true;
        }

        private void IdBox_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _idHelp.IsOpen = false;
            _idCopy.IsOpen = false;
        }

        private void IdBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IdBox.Text == "Please Wait.." || IdBox.Text == "Failed")
                return;
            Clipboard.SetText(IdBox.Text);
            _idCopy.IsOpen = true;
            _idHelp.IsOpen = false;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var item = FoldersItem.SelectedItem as TreeViewItem;
            if (item == null)
                return;
            string filePath = item.Tag.ToString();
            SendButton.IsEnabled = false;
            SendCancelButton.IsEnabled = true;
            SendButton.Content = "Preparing";
            FoldersItem.IsEnabled = false;
            RecieveItem.IsEnabled = false;
            SendProgressBar.Maximum = 200;
            new Thread(new ThreadStart(delegate
            {
                int dots = 0;
                var timer = new Timer(delegate
                {
                    dots++;
                    if (dots > 4)
                        dots = 0;
                    string s = "Preparing";
                    for (int i = 0; i < dots; i++)
                    {
                        s += ".";
                    }

                    ChangeButtonContent(SendButton, s);

                }, null, 0, 500);

                var c = new Connection(ConnectionType.Sender);
                c.PrepareConnection();
                c.OnConnectionMade += connection1 => new Thread(new ThreadStart(delegate
                {
                    connection1.SendFile(filePath);
                    Logger.Info("Done!");
                    ChangeButtonContent(SendCancelButton, "Finish");
                    ChangeButtonContent(SendButton, "Send");
                    SetEnabled(SendCancelButton, true);
                    SetEnabled(FoldersItem, true);
                    SetEnabled(SendButton, true);
                    SetEnabled(RecieveItem, true);
                    done = true;
                    timer.Dispose();
                    ChangeProgressBar(SendProgressBar, connection1.Progress);
                })).Start();
                timer.Dispose();
                SetVisibilitySendTabControls(false);
                SetEnabled(SendCancelButton, true);
                SetEnabled(SendButton, true);
                timer = new Timer(delegate
                {
                    ChangeProgressBar(SendProgressBar, c.Progress);
                    SetLabelContent(SendStatusLabel, Logger.LastMessage);
                }, null, 0, 20);
            })).Start();

        }

        private void SetVisibilitySendTabControls(bool value)
        {
            SetVisible(SendButton, value);
            SetVisible(FoldersItem, value);
            SetVisible(Group, value);
            SetVisible(lbl1, value);
            SetVisible(IdBox, value);


            SetVisible(SendProgressBar, !value);
            SetVisible(SendStatusLabel, !value);
            SetVisible(SendCancelButton, !value);
        }

        private void SetVisibilityRecieveTabControl(bool value)
        {
            SetVisible(RecieveButton, value);
            SetVisible(IdTextBox, value);


            SetVisible(RecieveProgressBar, !value);
            SetVisible(StatusLabel, !value);
            SetVisible(CancelButton, !value);
        }

        private void RecieveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder to save the file.";
            dialog.UseDescriptionForTitle = true;
            var showDialog = dialog.ShowDialog(this);
            bool val = showDialog != null && (bool)showDialog;
            if (!val)
                return;
            string filePath = dialog.SelectedPath;
            RecieveButton.IsEnabled = false;
            RecieveButton.Content = "Preparing";
            CancelButton.IsEnabled = true;
            RecieveProgressBar.Maximum = 200;
            string ip;
            string id = IdTextBox.Text;
            IdTextBox.IsEnabled = false;
            if (!id.Contains("."))
            {
                int dots = 0;
                var timer = new Timer(delegate
                {
                    dots++;
                    if (dots > 4)
                        dots = 0;
                    string s = "Preparing";
                    for (int i = 0; i < dots; i++)
                    {
                        s += ".";
                    }

                    ChangeButtonContent(RecieveButton, s);
                    
                }, null, 0, 500);
                new Thread(new ThreadStart(delegate
                {
                    Thread.Sleep(1500);
                    using (var web = new WebClient())
                    {
                        ip =
                            web.DownloadString("http://www.hypereddie.com/utils/filo.php?action=get&id=" +
                                              id);
                    }
                    timer.Dispose();
                    if (ip == "No ID found..")
                    {
                        SetEnabled(RecieveButton, true);
                        SetEnabled(IdTextBox, true);
                        ChangeButtonContent(RecieveButton, "Connect and Recieve");
                        MessageBox.Show("The personal ID supplied could not be resolved!", "Error resolving ID");
                        return;
                    }
                    PrepareRecieve(ip, filePath);
                })).Start();
            }
            else
            {
                ip = id;
                new Thread(() => PrepareRecieve(ip, filePath)).Start();
            }
        }

        private bool done;
        private void PrepareRecieve(string ip, string filePath)
        {
            SetEnabled(SendItem, false);
            ChangeButtonContent(RecieveButton, "Connecting");
            connection = new Connection(ConnectionType.Reciever, ip);
            int dots = 0;
            var timer = new Timer(delegate
            {
                dots++;
                if (dots > 4)
                    dots = 0;
                string s = "Connecting";
                for (int i = 0; i < dots; i++)
                {
                    s += ".";
                }

                ChangeButtonContent(RecieveButton, s);

            }, null, 0, 500);
            try
            {
                connection.PrepareConnection();
                if (!connection.IsConnected)
                {
                    MessageBox.Show("Could not make a connection!", "Error connecting..");
                    ChangeButtonContent(RecieveButton, "Connect and Recieve");
                    SetEnabled(RecieveButton, true);
                    SetEnabled(IdTextBox, true);
                    SetEnabled(SendItem, true);
                    timer.Dispose();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("An error occured while attempting to connect!", "Error connecting..");
                ChangeButtonContent(RecieveButton, "Connect and Recieve");
                SetEnabled(RecieveButton, true);
                SetEnabled(IdTextBox, true);
                SetEnabled(SendItem, true);
                timer.Dispose();
                return;
            }

            SetVisibilityRecieveTabControl(false);
            SetEnabled(CancelButton, true);
            SetEnabled(RecieveButton, true);
            timer = new Timer(delegate
            {
                ChangeProgressBar(RecieveProgressBar, connection.Progress);
                SetLabelContent(StatusLabel, Logger.LastMessage);
            }, null, 0, 20);

            connection.RecieveFile(filePath);

            Logger.Info("Done!");
            ChangeButtonContent(CancelButton, "Finish");
            SetLabelContent(StatusLabel, "Done!");
            SetEnabled(CancelButton, true);
            SetEnabled(RecieveButton, true);
            done = true;
            timer.Dispose();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IdTextBox.Text.Length > 15)
            {
                IdTextBox.Text = IdTextBox.Text.Substring(0, 15);
                IdTextBox.SelectionStart = 15;
                IdTextBox.SelectionLength = 0;
            }

            RecieveButton.IsEnabled = IdTextBox.Text.Length > 0 || IdTextBox.Text == "Enter Id or IP";
        }

        private void IdTextBox_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (IdTextBox.Text == "Enter Id or IP")
                IdTextBox.Text = "";
        }

        private void SendCancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (done)
            {
                SetVisibilitySendTabControls(true);
                SetEnabled(RecieveItem, true);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (done)
            {
                SetVisibilityRecieveTabControl(true);
                SetEnabled(SendItem, true);
            }
        }
    }
}
