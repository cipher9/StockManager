using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using YahooFinanceApi;

namespace CipherStocks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StockListDG.IsReadOnly = true;
        }

        private void ChangeColor(double n)
        {
            if(n < 0)
            {
                PriceChangeTB.Foreground = new SolidColorBrush(Colors.Red);
                PriceChangePercentTB.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                PriceChangeTB.Foreground = new SolidColorBrush(Colors.LimeGreen);
                PriceChangePercentTB.Foreground = new SolidColorBrush(Colors.LimeGreen);
            }
        }

        private async void StockData(string stock)
        {
            var securities = await Yahoo.Symbols(stock).QueryAsync();
            var company = securities[stock];
            
            ChangeColor(company.RegularMarketChangePercent);

            // Rows 4-5
            StockNameTB.Text = company.ShortName;
            StockPriceTB.Text = company.RegularMarketPrice.ToString("C2");
            PriceChangeTB.Text = company.RegularMarketChange.ToString("C2");
            PriceChangePercentTB.Text = company.RegularMarketChangePercent.ToString("N2");

            // Column 0
            OpenTB.Text = String.Format("{0:n}", company[Field.RegularMarketOpen].ToString("C2"));
            TodayHighTB.Text = company[Field.RegularMarketDayHigh].ToString("C2");
            TodayLowTB.Text = company[Field.RegularMarketDayLow].ToString("C2");
            FTWeekHighTB.Text = company[Field.FiftyTwoWeekHigh].ToString("C2");
            FTWeekLowTB.Text = company[Field.FiftyTwoWeekLow].ToString("C2");
            

            // Column 2
            VolumeTB.Text = company[Field.RegularMarketVolume].ToString("N0");
            AverageVolTB.Text = company[Field.AverageDailyVolume3Month].ToString("N0");
            MarketCapTB.Text = company[Field.MarketCap].ToString("C0");
            try
            {
                PERatioTB.Text = company[Field.TrailingPE].ToString("N0");
            }
            catch(KeyNotFoundException)
            {
                PERatioTB.Text = "N/A";
            }

            try
            {
                DivTB.Text = company[Field.TrailingAnnualDividendYield].ToString();
            }
            catch(KeyNotFoundException)
            {
                DivTB.Text = "N/A";
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(StockTBx.Text))
            {
                StockTBx.Focus();
                return;
            }
            string stock = StockTBx.Text.Trim().ToUpper();
            StockData(stock);

        }

        private void StockTBx_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                SearchButton_Click(sender, e);
            }
        }

        List<string> StockList = new List<string>();

        private async void GetData(string stock)
        {
            var securities = await Yahoo.Symbols(stock).QueryAsync();
            var company = securities[stock];
            

            double pe, dividendRate;
            try
            {
                pe = company[Field.TrailingPE];
            }
            catch(KeyNotFoundException)
            {
                pe = 0;
            }

            try
            {
                dividendRate = company[Field.TrailingAnnualDividendRate];
            }
            catch(KeyNotFoundException)
            {
                dividendRate = 0;
            }

            StockListDG.Items.Add(new Stock()
            {
                Symbol = company.Symbol,
                Company = company.ShortName,
                Price = company[Field.RegularMarketPrice],
                TrailingPE = Math.Round(pe,2),
                RegularMarketVolume = company.RegularMarketVolume,
                RegularMarketChangePercent = Math.Round(company.RegularMarketChangePercent,2),
                TrailingAnnualDividendYield = Math.Round(dividendRate,2)
            });
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string stock = StockTBx.Text.ToUpper();
            // Prevent duplicates in StockListDG
            if(StockList.Contains(stock))
                return;
            StockList.Add(stock);
            GetData(stock);
        }
    }
}
