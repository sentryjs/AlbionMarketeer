﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static HttpClient client = new HttpClient();
        public static Window window;
        public static Log log_window;
        public static DataSet itemsds = new DataSet();

        public static Dictionary<string, string> Result;
        public static List<string> Item_keys = new List<string>();
        public static List<string> Item_values = new List<string>();
        public static List<ApiOrder> apiOrders = new List<ApiOrder>();

        public MainWindow()
        {
            InitializeComponent();
            window = this;

            log_window = new Log();
            logic = new Logic(log_window);

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
                                if (row[2].ToString() == parent[0].ToString() && parent[1].ToString().StartsWith("@ITEMS_")) found.Add(parent[1].ToString().Substring(7), row[0].ToString());
                            }
                        }
                    }
                }

                if (found.Count > 0) Console.WriteLine(found.First().ToString());
                return found;
            });
            task.ContinueWith(t => taskCompletionSource.SetResult(t.Result));
            return taskCompletionSource.Task;
        }

        private void SearchAsync(object sender, RoutedEventArgs e)
        {
            if (string.Empty == search.Text) return;

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

            var task = FindItems(search.Text);
            task.ContinueWith(async t =>
            {
                Result = t.Result;

                Item_keys.AddRange(Result.Keys);
                Item_values.AddRange(Result.Values);

                string Items_joined = string.Join(",", Item_keys);
                string url = $"https://www.albion-online-data.com/api/v1/stats/prices/{Items_joined}?locations={Locations_joined}";

                string result = "";
                result = await GetAPIData(url);

                //dynamic obj = JsonConvert.DeserializeObject<dynamic>(result);
                apiOrders = ApiOrder.FromJson(result);

                foreach (var pair in Result)
                {
                    ApiOrder order = apiOrders.Find(o => o.ItemId == pair.Key);
                    if (order != null) order.Name = pair.Value;
                }

                Results.ItemsSource = apiOrders.Select(c => c.Name).Where(s => s != null).ToList();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            
        }

        private async Task<string> GetAPIData(string path)
        {
            HttpResponseMessage response = await client.GetAsync(path);
            log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(response.ReasonPhrase.ToString())));
            if (response.IsSuccessStatusCode)
            {
                Dispatcher.Invoke(new Action(() => loading.Content = ""));
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

        private void log_button_Click(object sender, RoutedEventArgs e)
        {
            log_window.Show();
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

        private void Results_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Results.Items.Count <= 0) return;

            int index = Item_values.FindIndex(i => i == Results.SelectedItem.ToString());
            ShowDetails(Item_keys.ElementAt(index));
        }

        private void ShowDetails(string key)
        {
            Details details_window = new Details(apiOrders.Find(o => o.ItemId == key));
            details_window.Show();
        }
    }
}
