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
    public partial class MainWindow : INotifyPropertyChanged
    {
        char[] coord = { '0', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
        int nbColumns = 11;
        int nbRows = 10;
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            DataTable dt = new DataTable();
            DataTable secondDt = new DataTable();
            PlayerClient player = new PlayerClient();
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
            BoundNumber = "Green";
        }

        private void Button_Shoot(object sender, RoutedEventArgs e)
        {
            BoundNumber = "Green";
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRow ro = (enemyDataGrid.SelectedItem as DataRowView)?.Row;
            String co = (enemyDataGrid.SelectedItem as DataColumn)?.ColumnName;
            DataGridCellInfo cellInfo = enemyDataGrid.CurrentCell;
            DataGridColumn column = cellInfo.Column;
            var docid = ro["0"];
            BoundText = column.Header + docid.ToString();
            BoundNumber = "Black";
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
                    dr[col] = theBoard[row, col - 1];
                }
                dt.Rows.Add(dr);
            }
            theGrid.Items.Clear();
            theGrid.ItemsSource = dt.DefaultView;
        }
    }
}
