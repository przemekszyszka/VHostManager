using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using MaxMind.GeoIP;
using Microsoft.Win32;

namespace VHostDetector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IList<Host> _testoweHosty1;
        private IList<Host> _testoweHosty2; 
        private IList<string> _vHosts;
        private readonly LookupService _lService;
        private readonly LookupService _lServiceDomain;


        public MainWindow()
        {
            InitializeComponent();
            var projectPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            _lService = new LookupService(projectPath + "/GeoIPDatabase/GeoLiteCity.dat", LookupService.GEOIP_STANDARD);
            _lServiceDomain = new LookupService(projectPath + "/GeoIPDatabase/GeoIPDomain.dat", LookupService.GEOIP_STANDARD);
            ProgressTxt.Text = "Wczytaj plik z adresami IP, aby rozpocząć badanie.";
            ProgressTxt.Visibility = Visibility.Visible;

        }

        private void AdressesList()
        {

            if (_testoweHosty1.Count < 1 || _testoweHosty2.Count < 1)
                return;

            var sb = new StringBuilder("");
            sb.Append("Wczytane adresy IP: \n\n");
            foreach (var host in _testoweHosty1)
            {

                var domainTxt = host.DomainName != null ? ", Domena: " + host.DomainName : "";
                sb.Append(host.AdresIp + domainTxt + "\n");
            }

            AllChartCollapse();
            WynikiMenuItem.IsEnabled = false;
            ZapiszBtn.IsEnabled = false;
            BadaniaMenuItem.IsEnabled = false;
            TxtResult.Text = sb.ToString();

        }

        private void wczytaj_btn_click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {DefaultExt = ".txt", Filter = "TXT Files (*.txt)|*.txt"};

            var result = dlg.ShowDialog();


            if (result != true) 
                return;
           
            var filename = dlg.FileName;
            _testoweHosty1 = new List<Host>();
            _testoweHosty2 = new List<Host>();

            try
            {
                using (var sr = new StreamReader(filename))
                {
                    var line = sr.ReadToEnd();
                    
                    var lines = line.Split('\n');
                    for (var i = 0; i < lines.Length; i += 1)
                    {
                        lines[i] = lines[i].TrimEnd('\r');
                        _testoweHosty1.Add(new Host
                        { 
                            AdresIp = lines[i], 
                            DomainName = GetDomainName(lines[i]),
                            Country = GetCountry(lines[i]),
                            VirtualHosts = new List<Host>()
                        });

                        _testoweHosty2.Add(new Host
                        {
                            AdresIp = lines[i],
                            DomainName = GetDomainName(lines[i]),
                            Country = GetCountry(lines[i]),
                            VirtualHosts = new List<Host>()
                        });
                    }
                       

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem z odczytaniem pliku.");
                Console.WriteLine(ex.Message);
            }
            ProgressTxt.Visibility = Visibility.Collapsed;
            AdressesList();
            if (_testoweHosty1.Count > 0 && _testoweHosty1.Count > 0)
               BadaniaMenuItem.IsEnabled = true;
          
            

        }

        private string GetDomainName(string adresIp)
        {
            return _lServiceDomain.getOrg(adresIp);
        }

        private string GetCountry(string adresIp)
        {
            return _lService.getLocation(adresIp).countryName;
        }


        private void zapisz_btn_click(object sender, RoutedEventArgs e)
        {
            var saveFileDlg = new SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                RestoreDirectory = true,
                FileName = "result"
            };


            if (saveFileDlg.ShowDialog() == true && saveFileDlg.FileName != "")
            {
                var sb = new StringBuilder("Wyniki badań 1 narzędzia:");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                foreach (var host in _testoweHosty1)
                {
                    sb.Append(host.AdresIp + ": "+ host.VirtualHosts.Count);
                    sb.Append(Environment.NewLine);
                }
                sb.Append(Environment.NewLine);
                sb.Append("Wyniki badań 2 narzędzia:");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                foreach (var host in _testoweHosty2)
                {
                    sb.Append(host.AdresIp + ": " + host.VirtualHosts.Count);
                    sb.Append(Environment.NewLine);
                }
                

                File.WriteAllText(saveFileDlg.FileName, sb.ToString());
            }
        }


        private void dane_ip_btn_click(object sender, RoutedEventArgs e)
        {
            AllChartCollapse();
            WynikiMenuItem.IsEnabled = false;
            TxtResult.Text = "";
            ProgressBarActivate();
            using (var worker = new BackgroundWorker())
            {
                worker.WorkerReportsProgress = true;
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;

                worker.RunWorkerAsync();
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;

            if (ProgressBar.Value >= 100)
            {
                ProgressBarDeactivate();
                WynikiMenuItem.IsEnabled = true;
                ZapiszBtn.IsEnabled = true;
            }
            

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            var backgroundWorker = sender as BackgroundWorker;

            if (backgroundWorker == null)
                return;

            Thread.Sleep(1000);
            backgroundWorker.ReportProgress(10);
            Thread.Sleep(1000);
            backgroundWorker.ReportProgress(10);
            ReadFromFirstWebsite();
            backgroundWorker.ReportProgress(40);
            ReadFromSecondWebsite();
            backgroundWorker.ReportProgress(80);
            Thread.Sleep(1000);
            backgroundWorker.ReportProgress(100);

           
            
        }

        private void ReadFromFirstWebsite()
        {
            var reader = new WebPageReader();

            foreach (var host in _testoweHosty1)
            {
                _vHosts = reader.FindVirtualHostsByIp(host.AdresIp);

                foreach (var vhost in _vHosts)
                {
                    host.VirtualHosts.Add(
                        new Host
                        {
                            AdresIp = host.AdresIp,
                            DomainName = vhost,
                            Country = host.Country

                        }
                    );
                }
            }
        }

        private void ReadFromSecondWebsite()
        {
            var reader = new WebPageReader();

            foreach (var host in _testoweHosty2)
            {
                _vHosts = reader.FindOtherVirtualHostsByIp(host.AdresIp);

                foreach (var vhost in _vHosts)
                {
                    host.VirtualHosts.Add(
                        new Host
                        {
                            AdresIp = host.AdresIp,
                            DomainName = vhost,
                            Country = host.Country
                        }
                    );
                }
            }
        }

        #region statystyki

        private void skala_wirtualizacji_click(object sender, RoutedEventArgs e)
        {
            AllChartCollapse();
            if (_testoweHosty1.Count < 1 || _testoweHosty2.Count < 1)
                return;

            var liczbaVhostow1 = _testoweHosty1.Select(host => host.VirtualHosts.Count).ToList();
            var result1 = liczbaVhostow1.AsEnumerable().Average(o => o);

            var liczbaVhostow2 = _testoweHosty2.Select(host => host.VirtualHosts.Count).ToList();
            var result2 = liczbaVhostow2.AsEnumerable().Average(o => o);

            var valueList = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("1 narzędzie", Convert.ToInt32(result1)),
                new KeyValuePair<string, int>("2 narzędzie", Convert.ToInt32(result2))
            };

            ShowChart(valueList, "Średnia ilość adresów dns na jednego hosta", "Średnia wartość", ChartEnum.ColumnChart1);
        }

        private void najwieksza_wirtualizacja_click(object sender, RoutedEventArgs e)
        {
            AllChartCollapse();

            if (_testoweHosty1.Count < 1 || _testoweHosty2.Count < 1)
                return;

            var najVirtHosts1 = _testoweHosty1.OrderByDescending(x => x.VirtualHosts.Count).Take(3);
            var najVirtHosts2 = _testoweHosty2.OrderByDescending(x => x.VirtualHosts.Count).Take(3);

            var valueList1 = najVirtHosts1.Select(host => new KeyValuePair<string, int>(host.DomainName, host.VirtualHosts.Count)).ToList();
            var valueList2 = najVirtHosts2.Select(host => new KeyValuePair<string, int>(host.DomainName, host.VirtualHosts.Count)).ToList();

            ShowChart(valueList1, "Hosty o najwiekszej wirtualizacji 1 narzedzie", "Liczba vhostów", ChartEnum.ColumnChart1);
            ShowChart(valueList2, "Hosty o najwiekszej wirtualizacji 2 narzedzie", "Liczba vhostów", ChartEnum.ColumnChart2);
        }

        private void lista_wszystkich_click(object sender, RoutedEventArgs e)
        {
            AllChartCollapse();

            if (_testoweHosty1.Count < 1 || _testoweHosty2.Count < 1)
                return;

            var valueList1 = _testoweHosty1.Select(host => new KeyValuePair<string, int>(host.DomainName, host.VirtualHosts.Count)).ToList();
            var valueList2 = _testoweHosty2.Select(host => new KeyValuePair<string, int>(host.DomainName, host.VirtualHosts.Count)).ToList();

            ShowChart(valueList1, "Znalezione wirtualne hosty 1 narzędzie", "Liczba vhostów", ChartEnum.PieChart1);
            ShowChart(valueList2, "Znalezione wirtualne hosty 2 narzędzie", "Liczba vhostów", ChartEnum.PieChart2);
        }

        private void rozlozenie_geo_click(object sender, RoutedEventArgs e)
        {
            AllChartCollapse();

            if (_testoweHosty1.Count < 1)
                return;

            var valueList1 = _testoweHosty1.Select(host => new KeyValuePair<string, int>(host.Country, _testoweHosty1.Count)).ToList();

            var groupedHosts = from c in valueList1
                       group c by c.Key into grouped
                       select new CountryCount
                       {
                           Key = grouped.Key,
                           Value = grouped.Count()
                       }.ToKeyValuePair();

            ShowChart(groupedHosts.ToList(), "Rozłożenie geograficzne", "Państwa", ChartEnum.PieChart1);
        }

        #endregion

        #region charts

        private void ShowChart(List<KeyValuePair<string, int>> valueList, string chartTitle, string columnSeriesTitle, ChartEnum chart)
        {
            switch (chart)
            {
                case ChartEnum.ColumnChart1:
                {
                    ColumnChart1.DataContext = null;
                    ColumnChart1.DataContext = valueList;
                    ColumnChart1.Title = chartTitle;
                    ColumnSeries1.Title = columnSeriesTitle;
                    ColumnChart1.Visibility = Visibility.Visible;
                    break;
                }
                case ChartEnum.ColumnChart2:
                {
                    ColumnChart2.DataContext = null;
                    ColumnChart2.DataContext = valueList;
                    ColumnChart2.Title = chartTitle;
                    ColumnSeries2.Title = columnSeriesTitle;
                    ColumnChart2.Visibility = Visibility.Visible;
                    break;
                }
                case ChartEnum.PieChart1:
                {
                    PieChart1.DataContext = null;
                    PieChart1.DataContext = valueList;
                    PieChart1.Title = chartTitle;
                    PieSeries1.Title = columnSeriesTitle;
                    PieChart1.Visibility = Visibility.Visible; 
                    break;
                }
                case ChartEnum.PieChart2:
                {
                    PieChart2.DataContext = null;
                    PieChart2.DataContext = valueList;
                    PieChart2.Title = chartTitle;
                    PieSeries2.Title = columnSeriesTitle;
                    PieChart2.Visibility = Visibility.Visible;
                    break;
                }

            }
            
        }
        
        private void AllChartCollapse()
        {
            TxtResult.Text = "";
            ProgressTxt.Visibility = Visibility.Collapsed;
            ColumnChart1.Visibility = Visibility.Collapsed;
            ColumnChart2.Visibility = Visibility.Collapsed;
            PieChart1.Visibility = Visibility.Collapsed;
            PieChart2.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region progressBar
        private void ProgressBarActivate()
        {

            ProgressBar.Visibility = Visibility.Visible;
            ProgressTxt.Visibility = Visibility.Visible;
            ProgressTxt.Text = "Trwa wykrywanie wirtualnych hostów, proszę czekać.";

        }

        private void ProgressBarDeactivate()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressTxt.Text = "Badanie skończone, można przejść do wyników.";
        }

        #endregion

    }

    
}
