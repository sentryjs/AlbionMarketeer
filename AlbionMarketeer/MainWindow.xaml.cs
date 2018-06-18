using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Newtonsoft.Json;

namespace AlbionMarketeer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Logic logic;
        public event Action<string> MyEvent = delegate { };
        private static HttpClient client = new HttpClient();
        public static Window window;
        public static Log log_window;
        public static DataSet itemsds = new DataSet();

        public MainWindow()
        {
            InitializeComponent();
            window = this;

            log_window = new Log();
            logic = new Logic(log_window);
            //log_window.Hide();
            Task.Run(() => { logic.StartPCAP(); });
        }

        private static List<string> FindItems(string search)
        {
            string xmlLocalization = "localization.xml";

            DataSet itemsds = new DataSet();
            itemsds.ReadXml(xmlLocalization, XmlReadMode.InferSchema);

            DataTableCollection tables = itemsds.Tables;

            List<string> found = new List<string>();

            foreach (DataTable table in tables)
            {
                if (table.TableName != "tuv") continue;

                foreach (var row in table.AsEnumerable())
                {
                    if (row[0].ToString().ToLower().Contains(search.ToLower().Trim()))
                    {
                        foreach (var parent in tables["tu"].AsEnumerable())
                        {
                            if (row[2].ToString() == parent[0].ToString() && parent[1].ToString().StartsWith("@ITEMS_")) found.Add(parent[1].ToString().Substring(7));
                        }
                    }
                }
            }

            if (found.Count > 0) Console.WriteLine(found.First().ToString());
            return found;
        }

        public static string GetAppLocation()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private async void SearchAsync(object sender, RoutedEventArgs e)
        {
            if (string.Empty == search.Text) return;

            search.IsEnabled = false;
            search_button.IsEnabled = false;
            loading.Content = "Loading...";

            List<string> Items = FindItems(search.Text);
            List<string> Locations = new List<string>();

            if ((bool)caerleon.IsChecked)
            {
                Locations.Add("Caerleon Market");
            }
            else if ((bool)bridgewatch.IsChecked)
            {
                Locations.Add("Bridgewatch Market");
            }
            else if ((bool)martlock.IsChecked)
            {
                Locations.Add("Martlock Market");
            }
            else if ((bool)thetford.IsChecked)
            {
                Locations.Add("Thetford Market");
            }
            else if ((bool)sterling.IsChecked)
            {
                Locations.Add("Fort Sterling Market");
            }
            else if ((bool)lymhurst.IsChecked)
            {
                Locations.Add("Lymhurst Market");
            }

            string Locations_joined = string.Join(",", Locations);
            string Items_joined = string.Join(",", Items);
            string result = "";

            string url = $"https://www.albion-online-data.com/api/v1/stats/prices/{Items_joined}?locations={Locations_joined}";
            result = await GetAPIData(url);

            dynamic obj = JsonConvert.DeserializeObject<dynamic>(result);

            grid.ItemsSource = obj;
        }

        private async Task<string> GetAPIData(string path)
        {
            HttpResponseMessage response = await client.GetAsync(path);
            log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(response.ReasonPhrase.ToString())));
            if (response.IsSuccessStatusCode)
            {
                loading.Content = "";
                search.IsEnabled = true;
                search_button.IsEnabled = true;
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "{\"Error\": \"Request failed!\"}";
            }
        }

        private void log_button_Click(object sender, RoutedEventArgs e)
        {
            log_window.Show();
        }

        
    }
}
