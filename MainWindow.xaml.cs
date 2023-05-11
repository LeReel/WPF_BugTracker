using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace BugTracker
{
    public partial class MainWindow : Window
    {
        const string SECURITY_JSON_PATH = "../../../security_info.json"; //Downloadable in GoogleAPI Dashboard. Needs to be placed in main folder (where .sln is)

        private bool m_hasConnect = false;
        private bool m_isPopupOpen = false;

        private string m_sheetID = ""; //Found in the Sheets' URL
        private string m_sheetName = ""; //The tab name

        private DateTime m_time1, m_time2;

        private int refreshRate = 1;

        public MainWindow()
        {
            InitializeComponent();

            m_time1 = DateTime.Now;
            m_time2 = DateTime.Now;

            OnOpenConnectPopup(null, null);
        }

        //TODO:
        // - AutoScroll
        // - Prevent UI micro-freeze when fetching

        async void Fetch()
        {
            while (true)
            {
                if (!m_isPopupOpen)
                {
                    FetchBugs();

                    await Task.Delay(refreshRate * 1000);
                }
            }
        }

        void FetchBugs()
        {
            bugsList.Items.Clear();

            if (string.IsNullOrEmpty(m_sheetID) ||
                string.IsNullOrEmpty(m_sheetName))
            {
                return;
            }

            GoogleSheetsHelper.GoogleSheetsHelper _gsh = new GoogleSheetsHelper.GoogleSheetsHelper(SECURITY_JSON_PATH, m_sheetID);

            GoogleSheetsHelper.GoogleSheetParameters _gsp = new GoogleSheetsHelper.GoogleSheetParameters()
            {
                RangeColumnStart = 1,
                RangeRowStart = 1,
                RangeColumnEnd = 4,
                RangeRowEnd = 1000,
                FirstRowIsHeaders = true,
                SheetName = m_sheetName
            };

            var _rowValues = _gsh.GetDataFromSheet(_gsp);

            SolidColorBrush _brush = new SolidColorBrush(Colors.Transparent);

            foreach (dynamic _rowValue in _rowValues)
            {
                if (!IsRowValid(_rowValue))
                {
                    break;
                }

                string _state = _rowValue.State;

                switch (_state)
                {
                    case "UNTREATED":
                        _brush = Brushes.IndianRed;
                        break;
                    case "PROGRESS":
                        _brush = Brushes.Orange;
                        break;
                    case "FINISHED":
                        _brush = Brushes.Green;
                        break;
                }

                ListViewItem _item = new ListViewItem();
                _item.Content = new TrackedBug(_rowValue);
                _item.Foreground = new SolidColorBrush(Colors.Black);
                _item.Background = _brush;
                _item.HorizontalContentAlignment = HorizontalAlignment.Center;

                bugsList.Items.Add(_item);
            }
        }

        private void OnOpenConnectPopup(object sender, RoutedEventArgs e)
        {
            m_isPopupOpen = true;

            Window _w = new Window();
            _w.ResizeMode = ResizeMode.NoResize;
            _w.Width = 500;
            _w.Height = 250;

            Grid _grid = new Grid();
            _grid.Width = _w.Width;
            _grid.Height = _w.Height;

            Button _button = new Button();
            _button.Width = 75;
            _button.Height = 35;
            _button.Margin = new Thickness(0, -100, 0, 0);
            _button.Content = "Connect";
            _grid.Children.Add(_button);

            TextBox _sheetIDBox = new TextBox();
            _sheetIDBox.Background = new SolidColorBrush(Colors.AliceBlue);
            _sheetIDBox.Foreground = new SolidColorBrush(Colors.Black);
            _sheetIDBox.Width = 250;
            _sheetIDBox.Height = 25;
            _sheetIDBox.Margin = new Thickness(10, 0, 10, 0);
            _grid.Children.Add(_sheetIDBox);

            TextBox _sheetNameBox = new TextBox();
            _sheetNameBox.Background = new SolidColorBrush(Colors.AliceBlue);
            _sheetNameBox.Foreground = new SolidColorBrush(Colors.Black);
            _sheetNameBox.Width = 250;
            _sheetNameBox.Height = 25;
            _sheetNameBox.Margin = new Thickness(10, 75, 10, 0);
            _grid.Children.Add(_sheetNameBox);

            _button.Click += (s, e) =>
            {
                m_sheetID = _sheetIDBox.Text;
                m_sheetName = _sheetNameBox.Text;
                _w.Close();
                m_isPopupOpen = false;
                Fetch();
            };

            _w.Content = _grid;

            _w.ShowDialog();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double _columnWidth = Width / 4;

            DateCol.Width = CateCol.Width = DescCol.Width = StateCol.Width = _columnWidth;
        }

        private float DeltaTime()
        {
            m_time2 = DateTime.Now;
            float _dT = (m_time2.Ticks - m_time1.Ticks) / 10000000f;
            m_time1 = m_time2;

            return _dT;
        }

        private bool IsRowValid(dynamic _rowValue)
        {
            IDictionary<String, object> _row = (IDictionary<String, object>)_rowValue;

            return _row.ContainsKey("Date")
                && _row.ContainsKey("Categorie")
                && _row.ContainsKey("Description")
                && _row.ContainsKey("State");
        }
    }

    public class TrackedBug
    {
        public string Date { get; set; } = "";
        public string Categorie { get; set; } = "";
        public string Description { get; set; } = "";
        public string State { get; set; } = "";

        public TrackedBug(dynamic _rowValue)
        {
            Date = _rowValue.Date;
            Categorie = _rowValue.Categorie;
            Description = _rowValue.Description;
            State = _rowValue.State;
        }
    }
}
