﻿using System;
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
using System.Windows.Shapes;

namespace AlbionMarketeer
{
    /// <summary>
    /// Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        public Log()
        {
            InitializeComponent();
        }

        public void AddLog(string log, bool newline = true)
        {
            DateTime time = DateTime.Now;
            //string format = "yyyy MMM ddd d HH:mm ";
            if (newline == true) this.log.AppendText(time.ToString("s") + ": " + log + "\r");
            else this.log.AppendText(log);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void log_TextChanged(object sender, TextChangedEventArgs e)
        {
            log.ScrollToEnd();
        }
    }
}
