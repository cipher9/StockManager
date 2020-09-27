using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using YahooFinanceApi;
using System.Windows.Controls;

namespace CipherStocks
{
    public delegate Point GetDragDropPosition(IInputElement theElement);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StartClock();
        }

        private void StartClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TickEvent;
            timer.Start();
        }

        private void TickEvent(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            LocalClockTB.Text = currentTime.ToString(@"G");
            NYClockTB.Text = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentTime, TimeZoneInfo.Local.Id, "Eastern Standard Time").ToString(@"T");
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
            Security company;
            try
            {
                company = securities[stock];
                ChangeColor(company.RegularMarketChangePercent);

            }
            catch (Exception)
            {
                MessageBox.Show($"The stock symbol \"{stock}\" was not found");
                return;
            }

            // Rows 4-5
            StockNameTB.Text = company.ShortName;
            StockPriceTB.Text = company.RegularMarketPrice.ToString("C2");
            PriceChangeTB.Text = company.RegularMarketChange.ToString("C2");
            PriceChangePercentTB.Text = $"({company.RegularMarketChangePercent.ToString("N2")}%)";
            ExchangeTB.Text = company.FullExchangeName;
            
            // Column 0
            OpenTB.Text = String.Format("{0:n}", company[Field.RegularMarketOpen].ToString("C2"));
            TodayHighTB.Text = company[Field.RegularMarketDayHigh].ToString("C2");
            TodayLowTB.Text = company[Field.RegularMarketDayLow].ToString("C2");
            FTWeekHighTB.Text = company[Field.FiftyTwoWeekHigh].ToString("C2");
            FTWeekLowTB.Text = company[Field.FiftyTwoWeekLow].ToString("C2");

            // Column 2
            VolumeTB.Text = Numbers.FormatNumber(company[Field.RegularMarketVolume], false);
            try
            {
                AverageVolTB.Text = Numbers.FormatNumber(company[Field.AverageDailyVolume10Day], false);
            }
            catch(KeyNotFoundException)
            {
                AverageVolTB.Text = "N/A";
            }

            MarketCapTB.Text = Numbers.FormatNumber(company[Field.MarketCap], true);

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
                DivTB.Text = company[Field.TrailingAnnualDividendYield].ToString("P2");
            }
            catch(KeyNotFoundException)
            {
                DivTB.Text = "N/A";
            }

            // Column 3
            try
            {
                PriceToBookTB.Text = company[Field.PriceToBook].ToString("N2");
            }
            catch(KeyNotFoundException)
            {
                PriceToBookTB.Text = "N/A";
            }
            
            try
            {
                BookValueTB.Text = company[Field.BookValue].ToString("C2");
            }
            catch(KeyNotFoundException)
            {
                BookValueTB.Text = "N/A";
            }
            
            FiftyDayChangeTB.Text = company[Field.FiftyDayAverageChange].ToString("C2");
            FiftyDayPercentChangeTB.Text = company[Field.FiftyDayAverageChangePercent].ToString("P2");

            try
            {
                EPSTB.Text = company[Field.EpsTrailingTwelveMonths].ToString("C2");
            }
            catch(KeyNotFoundException)
            {
                EPSTB.Text = "N/A";
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
            string dividendDate;
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

            try
            {
                dividendDate = DateTimeOffset.FromUnixTimeSeconds(company.DividendDate).ToString("MM/dd/yyyy");
            }
            catch (KeyNotFoundException)
            {
                dividendDate = "N/A";
            }

            StockListDG.Items.Add(new Stock()
            {
                Symbol = company.Symbol,
                Company = company.ShortName,
                Price = company[Field.RegularMarketPrice],
                TrailingPE = Math.Round(pe,2),
                RegularMarketVolume = company.RegularMarketVolume,
                RegularMarketChangePercent = Math.Round(company.RegularMarketChangePercent,2),
                TrailingAnnualDividendYield = Math.Round(dividendRate,2),
                DividendDate = dividendDate
            });
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string stock = StockTBx.Text.ToUpper();
            // Prevent duplicates in StockListDG
            if(StockList.Contains(stock))
            {
                return;
            }
            StockList.Add(stock);
            GetData(stock);
        }

        private void deleteRow_Click(object sender, RoutedEventArgs e)
        {
            string stock = (StockListDG.SelectedCells[1].Column.GetCellContent(StockListDG.SelectedItem) as TextBlock).Text;
            StockList.Remove(stock);

            StockListDG.Items.RemoveAt(StockListDG.SelectedIndex);
        }
    }
}
