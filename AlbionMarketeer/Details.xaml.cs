using System;
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
        private ApiOrder apiOrder;

        public Details(ApiOrder apiOrder)
        {
            InitializeComponent();
            this.apiOrder = apiOrder;
            Draw();
        }

        private void Draw()
        {
            Title.Text = apiOrder.Name;
            Bid.Items.Add(new { Price = apiOrder.BuyPriceMin, Date = apiOrder.BuyPriceMinDate });
            Ask.Items.Add(new { Price = apiOrder.SellPriceMin, Date = apiOrder.SellPriceMinDate });
        }
    }
}
