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
    /// Interaction logic for Details.xaml
    /// </summary>
    public partial class Details : Window
    {
        private List<ApiOrder> apiOrders;

        public Details(List<ApiOrder> apiOrders)
        {
            InitializeComponent();
            this.apiOrders = apiOrders;
            Draw();
        }

        private void Draw()
        {
            Title.Text = apiOrders.First().Name;
            string Uri = $"https://gameinfo.albiononline.com/api/gameinfo/items/{apiOrders.First().ItemId}";
            Image.Source = new ImageSourceConverter().ConvertFromString(Uri) as ImageSource;
            foreach (ApiOrder order in apiOrders)
            {
                if (order.BuyPriceMinDate != "0001-01-01T00:00:00Z")
                    Bid.Items.Add(new { Location = order.City, Price = order.BuyPriceMin, Date = order.BuyPriceMinDate });

                if (order.SellPriceMinDate != "0001-01-01T00:00:00Z")
                    Ask.Items.Add(new { Location = order.City, Price = order.SellPriceMin, Date = order.SellPriceMinDate });
            }
            
        }

    }
}