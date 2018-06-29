using NATS.Client;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Threading;

namespace AlbionMarketeer
{
    public class Logic
    {

        //    FOREST = 1000,
        //    STEPPE = 2000,
        //    HIGHLAND = 3004,
        //    SWAMP = 0,
        //    MOUNTAIN = 4000,
        //    CAERLEON = 3005
        private string fullJSON = String.Empty;
        private List<string> History = new List<string>();
        private static int LocationID = -1;
        public static List<MarketOrder> Orders = new List<MarketOrder>();
        public static Log log_window;

        public Logic(Log log)
        {
            log_window = log;
        }

        public void StartPCAP()
        {
            Console.WriteLine("Starting up...");
            log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Starting up...")));

            // Retrieve the device list on the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("No interfaces found! Make sure WinPcap is installed.")));
                return;
            }

            NetworkInterface nia = null;

            Console.WriteLine("Waiting for Albion-Online process.", false);
            log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Waiting for Albion-Online process.")));

            do
            {
                Thread.Sleep(1000);
                nia = getAdapterUsedByProcess("Albion-Online");
            }
            while (nia == null);
            Console.WriteLine();

            // Select temp device
            PacketDevice selectedDevice = allDevices[0];

            foreach (PacketDevice device in allDevices)
            {
                if (String.Concat(@"rpcap://\Device\NPF_", nia.Id) == device.Name)
                {
                    selectedDevice = device;
                    break;
                }
            }

            Console.WriteLine(String.Concat("Selected network device ", selectedDevice.Description));
            log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(String.Concat("Selected network device ", selectedDevice.Description))));
            // Open the device
            using (PacketCommunicator communicator =
                selectedDevice.Open(65536, // portion of the packet to capture
                                           // 65536 guarantees that the whole packet will be captured on all the link layers
                                    PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                    1000)) // read timeout
            {
                if (communicator.DataLink.Kind != DataLinkKind.Ethernet)
                {
                    Console.WriteLine("This program only works on Ethernet networks.");
                    log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("This program only works on Ethernet networks.")));
                    return;
                }

                using (BerkeleyPacketFilter filter = communicator.CreateFilter("ip and udp"))
                {
                    communicator.SetFilter(filter);
                }

                communicator.ReceivePackets(0, DispatcherHandler);
            }
        }

        private void DispatcherHandler(Packet packet)
        {
            string line = String.Empty;

            for (int i = 0; i != packet.Length; ++i)
            {
                var Hexbyte = (packet[i]).ToString("X2");

                // Add new letter onto line each iteration
                line += ConvertHexToString(Hexbyte);

                // Reached the end of the packet
                if (i == packet.Length - 1)
                {
                    //ThetfordMarket Location = 0
                    //LymhurstMarket Location = 1000
                    //BridgewatchMarket Location = 2000
                    //BlackMarket Location = 3003
                    //MartlockMarket Location = 3004
                    //CaerleonMarket Location = 3005
                    //FortSterlingMarket Location = 4000

                    //SwampCrossMarket Location = 4
                    //ForestCrossMarket Location = 1006
                    //SteppeCrossMarket Location = 2002
                    //HighlandCrossMarket Location = 3002
                    //MountainCrossMarket Location = 4006
                    if (line.Contains("SWAMP_GREEN_MARKETPLACE") && LocationID != 0)
                    {
                        LocationID = 0;
                        log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Location set to Thetford")));
                    }
                    else if (line.Contains("FOREST_GREEN_MARKETPLACE") && LocationID != 1000)
                    {
                        LocationID = 1000;
                        log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Location set to Lymhurst")));
                    }
                    else if (line.Contains("STEPPE_GREEN_MARKETPLACE") && LocationID != 2000)
                    {
                        LocationID = 2000;
                        log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Location set to Bridgewatch")));
                    }
                    else if (line.Contains("HIGHLAND_GREEN_MARKETPLACE") && LocationID != 3004)
                    {
                        LocationID = 3004;
                        log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Location set to Martlock")));
                    }
                    else if (line.Contains("MOUNTAIN_GREEN_MARKETPLACE") && LocationID != 4000)
                    {
                        LocationID = 4000;
                        log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Location set to Fort Sterling")));
                    }
                    else if (line.Contains("HIGHLAND_DEAD_MARKETPLACE_CENTERCITY") && LocationID != 3005)
                    {
                        LocationID = 3005;
                        log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Location set to Caerleon")));
                    }
                    else if (line.Contains("HIGHLAND_DEAD_T8_MELDBUILDING_CENTERCITY") && LocationID != 3003)
                    {
                        LocationID = 3003;
                        log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Location set to BlackMarket")));
                    }
                    
                    //if (!line.Contains("MSG marketorders.deduped"))
                    //{
                    fullJSON += line;
                    line = String.Empty;
                    //}
                }
            }

            fullJSON = JSONParser(fullJSON, packet);
        }

        private string JSONParser(string json, Packet packet)
        {
            string pattern = @"{(""([\w\d\n-_:.]*)"":((""([\w\d\n-_:.]*)"")|([\w\d\n-_:.]*)),)*""([\w\d\n-_:.]*)"":((""([\w\d\n-_:.]*)"")|([\w\d\n-_:.]*))}";

            foreach (Match m in Regex.Matches(json, pattern))
            {
                MarketOrder marketOrder = JsonConvert.DeserializeObject<MarketOrder>(m.Value);

                if (m.Value.StartsWith("{\"Id\":") && !m.Value.StartsWith("{\"Orders\":") && !History.Contains(marketOrder.ID.ToString()))
                {
                    marketOrder.LocationID = LocationID;

                    if (!History.Contains(marketOrder.ID.ToString()) && marketOrder.LocationID != -1)
                    {
                        Orders.Add(marketOrder);
                        History.Add(marketOrder.ID.ToString());
                    }
                }
                string retval = (fullJSON.Length - 1 >= m.Index ? fullJSON.Substring(0, m.Index) : fullJSON);
                return retval;
            }

            int numorders = Orders.Count;
            if (Orders.Count > 0)
            {
                string payload_start = "{\"Orders\":[";
                string payload_list = "";
                var i = 0;
                foreach (var order in Orders)
                {
                    i++;
                    if (i < Orders.Count) payload_list = String.Concat(payload_list, JsonConvert.SerializeObject(order), ",");
                    else payload_list = String.Concat(payload_list, JsonConvert.SerializeObject(order));
                }
                var payload = String.Concat(payload_start, payload_list, "]}");

                try
                {
                    Task.Factory.StartNew(() => {
                        MarketOrders_Publish(payload, numorders);
                    });
                }
                catch (Exception ex)
                {
                    System.Console.Error.WriteLine("Exception: " + ex.Message);
                    log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog("Exception: " + ex.Message)));
                    System.Console.Error.WriteLine(ex);
                }

                Orders.Clear();
            }


            if (History.Count > 100) History.RemoveAt(0);

            if (fullJSON.Length > 11000) return fullJSON.Substring(3000, 7000);
            else return fullJSON;
        }

        private void MarketOrders_Publish(string payload, int count)
        {
            new Publisher(log_window).Run(new string[] { "-url", "nats://public:thenewalbiondata@www.albion-online-data.com:4222", "-subject", "marketorders.ingest", "-count", count.ToString(), "-payload", payload });
        }

        public static string ConvertHexToString(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            return StrValue;
        }

        private static NetworkInterface getAdapterUsedByProcess(string pName)
        {
            Process[] candidates = Process.GetProcessesByName(pName);
            if (candidates.Length == 0)
                return null;

            IPAddress localAddr = null;
            using (Process p = candidates[0])
            {
                TcpTable table = ManagedIpHelper.GetExtendedTcpTable(true);
                foreach (TcpRow r in table)
                    if (r.ProcessId == p.Id)
                    {
                        localAddr = r.LocalEndPoint.Address;
                        break;
                    }
            }

            if (localAddr == null)
                return null;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProps = nic.GetIPProperties();
                if (ipProps.UnicastAddresses.Any(new Func<UnicastIPAddressInformation, bool>((u) => { return u.Address.ToString() == localAddr.ToString(); })))
                    return nic;
            }
            return null;
        }
    }
}
