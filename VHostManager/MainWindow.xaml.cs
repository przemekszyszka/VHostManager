using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using MaxMind.GeoIP;
using Microsoft.Win32;

namespace VHostManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IList<Host> _testoweHosty; 
        private IList<string> _restults;
        private IList<string> _vHosts;
        private readonly LookupService _lService;
        private readonly LookupService _lServiceDomain;


        public MainWindow()
        {
            InitializeComponent();
            var projectPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            _lService = new LookupService(projectPath + "/GeoIPDatabase/GeoLiteCity.dat", LookupService.GEOIP_STANDARD);
            _lServiceDomain = new LookupService(projectPath + "/GeoIPDatabase/GeoIPDomain.dat", LookupService.GEOIP_STANDARD);
            _restults = new List<string>();
        }

        private void ListCountries()
        {
            
            if (_testoweHosty.Count < 1)
                return;

            var sb = new StringBuilder("");

            foreach (var host in _testoweHosty)
            {

                var domainTxt = host.DomainName != null ? ", Domena: " + host.DomainName : "";
                sb.Append("Adres IP: " + host.AdresIp + ", kraj: " + host.Country + domainTxt + "\n");
            }

            var lines = sb.ToString().Split('\n');
            for (var i = 0; i < lines.Length; i += 1)
            {
                lines[i] = lines[i].TrimEnd('\n');
                _restults.Add(lines[i]);
            }


            TxtResult.Text = sb.ToString();

        }

        private void wczytaj_btn_click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {DefaultExt = ".txt", Filter = "TXT Files (*.txt)|*.txt"};

            var result = dlg.ShowDialog();


            if (result != true) 
                return;
           
            var filename = dlg.FileName;
            _testoweHosty = new List<Host>();

            try
            {
                using (var sr = new StreamReader(filename))
                {
                    var line = sr.ReadToEnd();
                    
                    var lines = line.Split('\n');
                    for (var i = 0; i < lines.Length; i += 1)
                    {
                        lines[i] = lines[i].TrimEnd('\r');
                        _testoweHosty.Add(new Host() 
                        { 
                            AdresIp = lines[i], 
                            DomainName = GetDomainName(lines[i]),
                            Country = GetCountry(lines[i])
                        });
                    }
                       

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem z odczytaniem pliku.");
                Console.WriteLine(ex.Message);
            }
            
            ListCountries();
            if (_testoweHosty.Count > 0)
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
                var sb = new StringBuilder("");

                foreach (var line in _restults)
                {
                    sb.Append(line + Environment.NewLine);
                }

                File.WriteAllText(saveFileDlg.FileName, sb.ToString());
            }
        }

        private void dane_ip_btn_click(object sender, RoutedEventArgs e)
        {
            var reader = new WebPageReader();

            foreach (var host in _testoweHosty)
            {
                _vHosts = reader.findVirtualHostsByIp(host.AdresIp);
                host.VirtualHosts = new List<Host>();
                foreach (var vhost in _vHosts)
                {
                    
                    host.VirtualHosts.Add(
                        new Host()
                        {
                            AdresIp = host.AdresIp, 
                            DomainName = vhost, 
                            Country = host.Country
                        
                        }
                    );
                }
            }
           

            //var sb = new StringBuilder("");

            //foreach (var vhost in _vHosts)
            //{
            //    sb.Append("VHost: " + VisualBitmapScalingModehost + "\n");
            //}

            TxtResult.Text = TxtResult.Text + "\nBadanie skończone, można przejść do wyników";
            WynikiMenuItem.IsEnabled = true;
        }

        private void skala_wirtualizacji_click(object sender, RoutedEventArgs e)
        {
            TxtResult.Text = "Średnia ilość adresów dns na jednego hosta:";

        }

        private void najwieksza_wirtualizacja_click(object sender, RoutedEventArgs e)
        {
            TxtResult.Text = "Hosty o najwiekszej wirtualizacji:\n\n\n";

            var sb = new StringBuilder("");

            var najVirtHosts = _testoweHosty.OrderByDescending(x => x.VirtualHosts.Count).Take(3);
            foreach (var host in najVirtHosts)
            {
                var domainTxt = host.DomainName != null ? ", Domena: " + host.DomainName : "";
                sb.Append("Adres IP: " + host.AdresIp + domainTxt + "\n");
                sb.Append("Liczba writualnych hostów: " + host.VirtualHosts.Count + "\n\n");
            }

            TxtResult.Text = TxtResult.Text + sb;
        }

        private void lista_wszystkich_click(object sender, RoutedEventArgs e)
        {
            TxtResult.Text = "Lista wszystkich znalezionych:\n\n\n";

            var sb = new StringBuilder("");

            foreach (var host in _testoweHosty)
            {
                var domainTxt = host.DomainName != null ? ", Domena: " + host.DomainName : "";
                sb.Append("Adres IP: " + host.AdresIp + domainTxt + "\n");
                sb.Append("Liczba writualnych hostów: " + host.VirtualHosts.Count + "\n\n");
            }

            TxtResult.Text = TxtResult.Text + sb;

        }
    }
}
