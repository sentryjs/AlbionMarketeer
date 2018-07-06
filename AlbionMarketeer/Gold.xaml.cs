using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;


namespace AlbionMarketeer
{
    /// <summary>
    /// Interaction logic for Gold.xaml
    /// </summary>
    public partial class Gold : Window
    {
        private string[] _labels;
        private static HttpClient client = new HttpClient();

        public Gold()
        {
            InitializeComponent();

            string url = "http://marketeer.vigilgaming.org/api/v1/gold";
            GetGold(url).ContinueWith(t =>
            {
                ChartValues<double> Values = new ChartValues<double>();
                List<string> lLabels = new List<string>();
                List<GoldPoint> points = GoldPoint.FromJson(t.Result);

                Values.AddRange(points.Select(p => (double)p.Price).Reverse());
                lLabels.AddRange(points.Select(p => p.Timestamp.Replace('T', ' ').Replace(":00.000Z", "")).Reverse());

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    SeriesCollection = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Gold Prices (72 hours)",
                            Values = Values,
                            Fill = Brushes.Transparent,
                            LineSmoothness = .5, //0: straight lines, 1: really smooth lines
                            //PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                            PointGeometrySize = 10,
                            PointForeground = Brushes.Gold
                        }
                    };

                    Labels = lLabels.ToArray();

                    DataContext = this;
                });

            });
        }

        private async Task<string> GetGold(string path)
        {
            HttpResponseMessage response = await client.GetAsync(path);
            return await response.Content.ReadAsStringAsync();
        }

        public SeriesCollection SeriesCollection { get; set; }

        public string[] Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                OnPropertyChanged("Labels");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
