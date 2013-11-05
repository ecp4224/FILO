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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FILO.Core;
using Path = System.IO.Path;

namespace FILO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private delegate void UpdateId(string id);
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
        }

        private object dummyNode = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IdBox.Text = "Please Wait..";
            IdBox.ToolTip = idHelp;
            new Thread(GetId).Start();
            foreach (var item in Directory.GetLogicalDrives().Select(s => new TreeViewItem {Header = s, Tag = s, FontWeight = FontWeights.Normal}))
            {
                item.Items.Add(dummyNode);
                item.Expanded += item_Expanded;
                foldersItem.Items.Add(item);
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void tbControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        ToolTip idHelp = new ToolTip { Content = "Your personal ID hides your IP from other users by masking it.\n Give this ID to the person recieving your file so they can connect." };
        private ToolTip idCopy = new ToolTip {Content = "Your personal ID has been copied to your clipboard!"};
        private void IdBox_OnMouseEnter(object sender, MouseEventArgs e)
        {
            idHelp.IsOpen = true;
        }

        private void IdBox_OnMouseLeave(object sender, MouseEventArgs e)
        {
            idHelp.IsOpen = false;
            idCopy.IsOpen = false;
        }

        private void IdBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IdBox.Text == "Please Wait.." || IdBox.Text == "Failed")
                return;
            Clipboard.SetText(IdBox.Text);
            idCopy.IsOpen = true;
            idHelp.IsOpen = false;
        }
    }
}
