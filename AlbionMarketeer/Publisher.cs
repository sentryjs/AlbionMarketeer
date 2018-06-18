using NATS.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbionMarketeer
{
    class Publisher
    {
        Dictionary<string, string> parsedArgs = new Dictionary<string, string>();

        int count = 2000000;
        string url = Defaults.Url;
        string subject = "foo";
        byte[] payload = null;
        public Log log_window;

        public Publisher(Log log)
        {
            log_window = log;
        }

        public void Run(string[] args)
        {
            Stopwatch sw = null;

            parseArgs(args);
            banner();

            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = url;

            using (IConnection c = new ConnectionFactory().CreateConnection(opts))
            {
                sw = Stopwatch.StartNew();

                for (int i = 0; i < count; i++)
                {
                    c.Publish(subject, payload);
                }
                c.Flush();

                sw.Stop();

                log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(string.Format("Published {0} orders in {1} seconds ", count, sw.Elapsed.TotalSeconds))));
                System.Console.Write("Published {0} orders in {1} seconds ", count, sw.Elapsed.TotalSeconds);
                log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(string.Format("({0} orders/second).", (int)(count / sw.Elapsed.TotalSeconds)))));
                System.Console.WriteLine("({0} orders/second).",
                    (int)(count / sw.Elapsed.TotalSeconds));
                
                System.Console.WriteLine("{0}", parsedArgs["-payload"]);
                printStats(c);
            }
        }

        private void printStats(IConnection c)
        {
            IStatistics s = c.Stats;
            //log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(string.Format("Statistics:  "))));
            System.Console.WriteLine("Statistics:  ");
            //log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(string.Format("   Outgoing Payload Bytes: {0}", s.OutBytes))));
            System.Console.WriteLine("   Outgoing Payload Bytes: {0}", s.OutBytes);
            //log_window.Dispatcher.Invoke(new Action(() => log_window.AddLog(string.Format("   Outgoing Messages: {0}", s.OutMsgs))));
            System.Console.WriteLine("   Outgoing Messages: {0}", s.OutMsgs);
        }

        private void usage()
        {
            System.Console.Error.WriteLine(
                "Usage:  Publish [-url url] [-subject subject] " +
                "-count [count] [-payload payload]");

            System.Environment.Exit(-1);
        }

        private void parseArgs(string[] args)
        {
            if (args == null)
                return;

            for (int i = 0; i < args.Length; i++)
            {
                if (i + 1 == args.Length)
                    usage();

                parsedArgs.Add(args[i], args[i + 1]);
                i++;
            }

            if (parsedArgs.ContainsKey("-count"))
                count = Convert.ToInt32(parsedArgs["-count"]);

            if (parsedArgs.ContainsKey("-url"))
                url = parsedArgs["-url"];

            if (parsedArgs.ContainsKey("-subject"))
                subject = parsedArgs["-subject"];

            if (parsedArgs.ContainsKey("-payload"))
                payload = Encoding.UTF8.GetBytes(parsedArgs["-payload"]);
        }

        private void banner()
        {
            //frm.Invoke(new Action(() => AddOutput(string.Format("Publishing {0} messages on subject {1}", count, subject))));
            System.Console.WriteLine("Publishing {0} messages on subject {1}", count, subject);
            //frm.Invoke(new Action(() => AddOutput(string.Format("  Url: {0}", url))));
            System.Console.WriteLine("  Url: {0}", url);
            //frm.Invoke(new Action(() => AddOutput(string.Format("  Payload is {0} bytes.", payload != null ? payload.Length : 0))));
            System.Console.WriteLine("  Payload is {0} bytes.", payload != null ? payload.Length : 0);
        }
    }
}
