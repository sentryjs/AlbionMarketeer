using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

namespace AlbionMarketeer
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : Window
    {
        private static HttpClient client = new HttpClient();
        public static Log log_window;

        public Info(Log log)
        {
            InitializeComponent();
            log_window = log;

            GetServerStatusAsync();
        }

        private async void GetServerStatusAsync()
        {
            HttpResponseMessage response = await client.GetAsync("http://marketeer.vigilgaming.org/api/v1/status");
            if (response.IsSuccessStatusCode)
            {
                dynamic status = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                MainIp.Text = status.main_ip;
                Os.Text = status.os;
                Ram.Text = status.ram;
                Disk.Text = status.disk;
                VcpuCount.Text = status.vcpu_count;
                Location.Text = status.location;
                Status.Text = status.status;
                PendingCharges.Text = status.pending_charges;
                CostPerMonth.Text = status.cost_per_month;
                CurrentBandwidth.Text = status.current_bandwidth_gb;
                AllowedBandwidth.Text = status.allowed_bandwidth_gb;
                PowerStatus.Text = status.power_status;
                ServerState.Text = status.server_state;

                log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(response.ReasonPhrase.ToString())));
                return;
            }
            else
            {
                log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(response.ReasonPhrase.ToString())));
                return;
            }
        }
    }
}
