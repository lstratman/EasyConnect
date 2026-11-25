using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EasyConnect.Protocols;
using EasyConnect.Protocols.Rdp;
using EasyTabs;

namespace EasyConnect
{
    public class FavoritesWindow : Form
    {
        private const int ProtocolIconSize = 128;

        private readonly MainForm _applicationForm;
        private readonly Dictionary<Type, int> _protocolIcons = new Dictionary<Type, int>();
        private readonly List<Bitmap> _protocolBitmaps = new List<Bitmap>();
        private readonly ImageList _largeImageList;
        private readonly ListView _favoritesListView;
        private readonly Font _recentConnectionFont;
        private readonly Label _emptyLabel;

        public FavoritesWindow(MainForm applicationForm)
        {
            _applicationForm = applicationForm;
            Text = "Favorites";
            Icon = applicationForm.Icon;
            BackColor = Color.FromArgb(248, 249, 250);
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);

            _largeImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(ProtocolIconSize, ProtocolIconSize),
                TransparentColor = Color.Transparent
            };

            _favoritesListView = new ListView
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0),
                HideSelection = false,
                LargeImageList = _largeImageList,
                MultiSelect = false,
                ShowItemToolTips = true,
                Sorting = SortOrder.Ascending,
                UseCompatibleStateImageBehavior = false,
                View = View.LargeIcon
            };
            _favoritesListView.DoubleClick += FavoritesListView_DoubleClick;
            _recentConnectionFont = new Font(_favoritesListView.Font, FontStyle.Bold);

            _emptyLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(112, 113, 114),
                Text = "No recent connections",
                TextAlign = ContentAlignment.MiddleCenter
            };

            Panel contentPanel = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(18)
            };
            contentPanel.Controls.Add(_favoritesListView);
            contentPanel.Controls.Add(_emptyLabel);

            Panel headerPanel = new Panel
            {
                BackColor = Color.FromArgb(51, 103, 214),
                Dock = DockStyle.Top,
                Height = 56
            };
            headerPanel.Controls.Add(new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0),
                ForeColor = Color.White,
                Location = new Point(12, 17),
                Text = "Recently connected computers"
            });

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);

            foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
            {
                using (Icon icon = new Icon(protocol.ProtocolIcon, ProtocolIconSize, ProtocolIconSize))
                {
                    Bitmap bitmap = icon.ToBitmap();
                    _protocolBitmaps.Add(bitmap);
                    _largeImageList.Images.Add(bitmap);
                }

                _protocolIcons[protocol.ConnectionType] = _largeImageList.Images.Count - 1;
            }

            RefreshFavorites();
            History.Instance.Connections.CollectionModified += Connections_CollectionModified;
        }

        private void Connections_CollectionModified(object sender, ListModificationEventArgs e)
        {
            RefreshFavorites();
        }

        private void RefreshFavorites()
        {
            _favoritesListView.BeginUpdate();
            try
            {
                _favoritesListView.Items.Clear();
                HashSet<string> seenConnections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                DateTime now = DateTime.Now;
                DateTime recentConnectionThreshold = now.AddHours(-24);
                DateTime staleConnectionThreshold = now.AddDays(-3);
                DateTime hiddenConnectionThreshold = now.AddDays(-7);

                foreach (HistoricalConnection historicalConnection in History.Instance.Connections.OrderByDescending(c => c.LastConnection))
                {
                    if (historicalConnection.LastConnection < hiddenConnectionThreshold)
                        continue;

                    IConnection connection = historicalConnection.Connection;
                    if (connection == null || String.IsNullOrWhiteSpace(connection.Host))
                        continue;

                    Type connectionType = connection.GetType() == typeof(LegacyHistoricalConnection)
                        ? typeof(RdpConnection)
                        : connection.GetType();
                    int imageIndex;
                    if (!_protocolIcons.TryGetValue(connectionType, out imageIndex))
                    {
                        System.Diagnostics.Debug.WriteLine("Skipping a recent connection with an unknown protocol: " + connectionType.FullName);
                        continue;
                    }

                    string key = connectionType.FullName + "\0" + connection.Host;
                    if (!seenConnections.Add(key))
                        continue;

                    _favoritesListView.Items.Add(new ListViewItem(connection.DisplayName, imageIndex)
                    {
                        Tag = connection,
                        ToolTipText = String.Format("{0}\nLast connected: {1}", connection.Host, historicalConnection.LastConnection.ToString("g")),
                        ForeColor = historicalConnection.LastConnection < staleConnectionThreshold ? Color.DarkGray : Color.Black,
                        Font = historicalConnection.LastConnection >= recentConnectionThreshold ? _recentConnectionFont : _favoritesListView.Font
                    });
                }

                _favoritesListView.Visible = _favoritesListView.Items.Count > 0;
                _emptyLabel.Visible = _favoritesListView.Items.Count == 0;
            }
            finally
            {
                _favoritesListView.EndUpdate();
            }
        }

        private async void FavoritesListView_DoubleClick(object sender, EventArgs e)
        {
            if (_favoritesListView.SelectedItems.Count == 0)
                return;

            IConnection connection = _favoritesListView.SelectedItems[0].Tag as IConnection;
            if (connection != null)
                await _applicationForm.Connect(connection, true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MainForm parent = ParentForm as MainForm;
            if (!e.Cancel && (parent == null || !parent.IsClosingTabs) &&
                MessageBox.Show(parent, "Close the Favorites tab?", "Close Favorites", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                e.Cancel = true;

            base.OnClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                History.Instance.Connections.CollectionModified -= Connections_CollectionModified;
                _largeImageList.Dispose();
                foreach (Bitmap bitmap in _protocolBitmaps)
                    bitmap.Dispose();
                _protocolBitmaps.Clear();
            }

            base.Dispose(disposing);

            if (disposing)
                _recentConnectionFont.Dispose();
        }
    }
}
