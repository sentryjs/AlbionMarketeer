using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
using System.Windows.Threading;

using Newtonsoft.Json;
using AutoUpdaterDotNET;


namespace AlbionMarketeer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Logic logic;
        private static HttpClient client = new HttpClient();
        public static Window window;
        public static Log log_window;
        public static DataSet itemsds = new DataSet();

        public static Dictionary<string, string> Result;
        public static List<string> Item_keys = new List<string>();
        public static List<string> Item_values = new List<string>();
        public static List<ApiOrder> apiOrders = new List<ApiOrder>();

        public static string Version = AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();

        public MainWindow()
        {
            InitializeComponent();
            window = this;

            LoadingGif.Visibility = Visibility.Hidden;

            log_window = new Log();
            logic = new Logic(log_window);

            AutoUpdater.Start("http://marketeer.vigilgaming.org/download/updateInfo.xml");
            VersionControl.Text = string.Concat("v", Version);

            Task.Run(() => { logic.StartPCAP(); });
        }

        public static string GetAppLocation()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public Task<Dictionary<string, string>> FindItems(string search)
        {
            var taskCompletionSource = new TaskCompletionSource<Dictionary<string, string>>();
            Task<Dictionary<string, string>> task = Task.Factory.StartNew(() =>
            {
                string xmlLocalization = "localization.xml";

                DataSet itemsds = new DataSet();
                itemsds.ReadXml(xmlLocalization, XmlReadMode.InferSchema);

                DataTableCollection tables = itemsds.Tables;

                Dictionary<string, string> found = new Dictionary<string, string>();

                foreach (DataTable table in tables)
                {
                    if (table.TableName != "tuv") continue;

                    foreach (var row in table.AsEnumerable())
                    {
                        if (row[0].ToString().ToLower().Contains(search.ToLower().Trim()))
                        {
                            foreach (var parent in tables["tu"].AsEnumerable())
                            {
                                if (row[2].ToString() == parent[0].ToString() && parent[1].ToString().StartsWith("@ITEMS_") && !found.ContainsKey(parent[1].ToString().Substring(7))) found.Add(parent[1].ToString().Substring(7), row[0].ToString());
                            }
                        }
                    }
                }
                itemsds.Clear();
                itemsds = null;
                return found;
            });
            task.ContinueWith(t => taskCompletionSource.SetResult(t.Result));
            return taskCompletionSource.Task;
        }

        private void SearchAsync(object sender, RoutedEventArgs e)
        {
            if (string.Empty == search.Text) return;

            LoadingGif.Visibility = Visibility.Visible;
            blackmarket.IsEnabled = false;
            caerleon.IsEnabled = false;
            bridgewatch.IsEnabled = false;
            martlock.IsEnabled = false;
            thetford.IsEnabled = false;
            sterling.IsEnabled = false;
            lymhurst.IsEnabled = false;
            search.IsEnabled = false;
            search_button.IsEnabled = false;
            loading.Content = "Loading...";

            List<string> Locations = new List<string>();

            if ((bool)blackmarket.IsChecked)
            {
                Locations.Add("Black Market");
            }
            if ((bool)caerleon.IsChecked)
            {
                Locations.Add("Caerleon Market");
            }
            if ((bool)bridgewatch.IsChecked)
            {
                Locations.Add("Bridgewatch Market");
            }
            if ((bool)martlock.IsChecked)
            {
                Locations.Add("Martlock Market");
            }
            if ((bool)thetford.IsChecked)
            {
                Locations.Add("Thetford Market");
            }
            if ((bool)sterling.IsChecked)
            {
                Locations.Add("Fort Sterling Market");
            }
            if ((bool)lymhurst.IsChecked)
            {
                Locations.Add("Lymhurst Market");
            }

            string Locations_joined = string.Join(",", Locations);

            log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Searching in dictionary.")));
            var task = FindItems(search.Text);
            task.ContinueWith(async t =>
            {
                Result = t.Result;
                List<string> RemoveKeys = new List<string>();
                List<string> Level1Enchant = new List<string>();
                List<string> Level2Enchant = new List<string>();
                List<string> Level3Enchant = new List<string>();
                foreach (var key in Result.Keys)
                {
                    if (key.EndsWith("_DESC")) RemoveKeys.Add(key);
                    if (key.EndsWith("_LEVEL1")) Level1Enchant.Add(key);
                    if (key.EndsWith("_LEVEL2")) Level2Enchant.Add(key);
                    if (key.EndsWith("_LEVEL3")) Level3Enchant.Add(key);
                }
                foreach (var key in RemoveKeys)
                {
                    Result.Remove(key);
                }
                foreach (var key in Level1Enchant)
                {
                    Result.TryGetValue(key, out string value);
                    Result.Remove(key);
                    Result.Add(string.Concat(key, "@1"), value);
                }
                foreach (var key in Level2Enchant)
                {
                    Result.TryGetValue(key, out string value);
                    Result.Remove(key);
                    Result.Add(string.Concat(key, "@2"), value);
                }
                foreach (var key in Level3Enchant)
                {
                    Result.TryGetValue(key, out string value);
                    Result.Remove(key);
                    Result.Add(string.Concat(key, "@3"), value);
                }

                Item_keys.Clear();
                Item_values.Clear();
                Item_keys.AddRange(Result.Keys);
                Item_values.AddRange(Result.Values);

                string Items_joined = "";

                Items_joined = string.Join(",", Item_keys);

                string url = $"http://marketeer.vigilgaming.org/api/v1/market/{Items_joined}?locations={Locations_joined}";
                string result = "";

                log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Sending matched items to API.")));
                log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Waiting for response.")));
                result = await GetAPIData(url);

                apiOrders = ApiOrder.FromJson(result);

                foreach (var pair in Result)
                {
                    ApiOrder order = apiOrders.Find(o => o.ItemId == pair.Key);
                    if (order != null) order.Name = pair.Value;
                }

                Results_List.ItemsSource = apiOrders.Select(c => c.Name).Where(s => s != null).ToList();
                Result.Clear();
                Result = null;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            
        }

        private async Task<string> GetAPIData(string path)
        {
            HttpResponseMessage response = await client.GetAsync(path);
            log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(response.ReasonPhrase.ToString())));
            if (response.IsSuccessStatusCode)
            {
                LoadingGif.Visibility = Visibility.Hidden;
                Dispatcher.Invoke(new Action(() => loading.Content = ""));
                Dispatcher.Invoke(new Action(() => blackmarket.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => caerleon.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => bridgewatch.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => martlock.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => thetford.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => sterling.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => lymhurst.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => search.IsEnabled = true));
                Dispatcher.Invoke(new Action(() => search_button.IsEnabled = true));
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "{\"Error\": \"" + response.ReasonPhrase.ToString() + "\"}";
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchAsync(new object(), new RoutedEventArgs());
            }
        }

        private void Results_List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Results_List.Items.Count <= 0) return;

            int index = Item_values.FindIndex(i => i == Results_List.SelectedItem.ToString());
            ShowDetails(Item_keys.ElementAt(index));
        }

        private void ShowDetails(string key)
        {
            Details details_window = new Details(apiOrders.FindAll(o => o.ItemId == key));
            details_window.Show();
        }

        private void LogButton_Click(object sender, MouseButtonEventArgs e)
        {
            log_window.Show();
        }

        private void InfoButton_Click(object sender, MouseButtonEventArgs e)
        {
            Info info_window = new Info(log_window);
            info_window.Show();
        }

        private void PinButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (window.Topmost)
            {
                window.Topmost = false;
                PinIcon.Fill = new SolidColorBrush(Colors.Black);
            }
            else
            {
                window.Topmost = true;
                PinIcon.Fill = new SolidColorBrush(Colors.Blue);
            }
        }

        private void DiscordButton_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://discord.gg/GMGf5Zs");
        }
        
    }
}
