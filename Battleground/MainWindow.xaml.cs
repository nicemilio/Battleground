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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data;
using System.Threading;

namespace Battleground { 
    public partial class MainWindow : INotifyPropertyChanged, IObserver
    {
        char[] coord = { '0', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
        readonly string[] bad_input = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "010" };
        int nbColumns = 11;
        int nbRows = 10;
        PlayerClient player;
        BotClient bot;
        delegate void Update_Fields_callback();
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            DataTable dt = new DataTable();
            DataTable secondDt = new DataTable();
            player = new PlayerClient();
            bot = new BotClient();
            player.Register(this);
            myDataGrid.HorizontalContentAlignment = HorizontalAlignment.Center;
            UpdateDataGrid (myDataGrid, player.myBoard );
            UpdateDataGrid (enemyDataGrid, player.enemyBoard);
            BoundNumber = "Red";
        }

        private String _boundNumber;
        public String BoundNumber
        { 
            get { return _boundNumber; }
            set
            {
                _boundNumber = value;
                OnPropertyChanged();
            }
        }

        private String _boundText;
        public String BoundText
        {
            get { return _boundText; }
            set
            {
                _boundText = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Connect(object sender, RoutedEventArgs e)
        {
            if (IPAddress.Text == "Bot" || IPAddress.Text == "bot")
            {
                bot.StartClient("127.0.0.1");
            }
            else
            {
                if (player.StartClient(IPAddress.Text))
                    BoundNumber = "Green";
                else
                {
                    BoundNumber = "Red";
                }
            }
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRow ro = (enemyDataGrid.SelectedItem as DataRowView)?.Row;
            String co = (enemyDataGrid.SelectedItem as DataColumn)?.ColumnName;
            DataGridCellInfo cellInfo = enemyDataGrid.CurrentCell;
            DataGridColumn column = cellInfo.Column;
            var docid = ro["0"];
            String row = docid.ToString();
            String coords = column.Header + row;
            if (! bad_input.Any(coords.Contains))
            {
                player.Shoot (coords);
                UpdateDataGrid(myDataGrid, player.myBoard);
                UpdateDataGrid(enemyDataGrid, player.enemyBoard);
            }
        }

        private void UpdateDataGrid (DataGrid theGrid, board theData)
        {
            DataTable dt = new DataTable();
            var theBoard = theData.mBoard;
            foreach (var character in coord)
            {
                dt.Columns.Add(character.ToString(), typeof(String));
            }

            for (int row = 0; row < nbRows; row++)
            {
                DataRow dr = dt.NewRow();
                dr[0] = row + 1;
                for (int col = 1; col < nbColumns; col++)
                {
                    dr[col] = theBoard[col - 1, row];
                }
                dt.Rows.Add(dr);
            }
            theGrid.ItemsSource = null;
            theGrid.ItemsSource = dt.DefaultView;
        }

        public void BoardChanged(bool myTurn)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => BoardChanged(myTurn));
            } else
            {
                UpdateDataGrid(myDataGrid, player.myBoard);
                UpdateDataGrid(enemyDataGrid, player.enemyBoard);
            }
            
        }
    }
}
