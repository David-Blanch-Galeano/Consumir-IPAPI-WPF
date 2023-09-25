using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
using RestSharp;
using static System.Net.WebRequestMethods;
using static WpfApp1.GeoData;

namespace WpfApp1
{



    public class GeoData
    {
        public string geoplugin_request { get; set; }
        public string geoplugin_city { get; set; }
        public string geoplugin_region { get; set; }
        public string geoplugin_regionName { get; set; }
        public string geoplugin_countryCode { get; set; }
        public string geoplugin_countryName { get; set; }
        public int geoplugin_inEU { get; set; }
        public string geoplugin_continentCode { get; set; }
        public string geoplugin_continentName { get; set; }
        public string geoplugin_latitude { get; set; }
        public string geoplugin_longitude { get; set; }
        public string geoplugin_timezone { get; set; }
        public string geoplugin_currencyCode { get; set; }
        public string geoplugin_currencySymbol { get; set; }
        public double geoplugin_currencyConverter { get; set; }
    }

    public class CountryViewModel : INotifyPropertyChanged
        {
            private string countryCode;

            public string CountryCode
            {
                get { return countryCode; }
                set
                {
                    if (countryCode != value)
                    {
                        countryCode = value;
                        OnPropertyChanged("CountryCode");
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public partial class MainWindow : Window
        {

        private string ipRecuest;
        private bool myIP = false;
        public GeoData ipData;
        private string web = "http://www.geoplugin.net/json.gp";
        static readonly HttpClient client = new HttpClient();

        public string IpRecuest
        { get { return ipRecuest; } set { ipRecuest = value; } }

        public bool MyIP
        { get { return myIP; } set { myIP = value; } }

        public GeoData IpData
        { get { return ipData; } set { } }

        public MainWindow()
            {
                InitializeComponent();
            }

        public void Clean()
        {
            var newStyle = new Style(typeof(Paragraph));
            newStyle.Setters.Add(new Setter(Block.LineHeightProperty, 2.0));
            var newParagraph = new Paragraph();
            newParagraph.Style = newStyle;
            var content = richBox.Document.Blocks.ToList();
            richBox.Document.Blocks.Clear();
            richBox.Document.Blocks.Add(newParagraph);
        }

        private void TextBoxThreeDigits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            LimitToThreeDigits(textBox, e);
        }

        private void LimitToThreeDigits(TextBox textBox, TextCompositionEventArgs e)
        {
            string newText = textBox.Text + e.Text;
            if (newText.Length > 3)
            {
                e.Handled = true;
            }

            if (!int.TryParse(newText, out _))
            {
                e.Handled = true;
            }
        }

        public async void deserializeAsync(bool myIP)
        {

            bool error = false;

            Clean();

            if (myIP)
            {
                IpRecuest = "";
                error = false;
            }
            else
            {
                IpRecuest = "?ip=" + TB1.Text + "." + TB2.Text + "." + TB3.Text + "." + TB4.Text;

                if (!IsNumeric(TB1.Text) || !IsNumeric(TB2.Text) || !IsNumeric(TB3.Text) || !IsNumeric(TB4.Text))
                {
                    error = true;
                }

                if (int.Parse(TB1.Text) >= 255 || int.Parse(TB2.Text) >= 255 || int.Parse(TB3.Text) >= 255 || int.Parse(TB4.Text) >= 255 || int.Parse(TB1.Text) < 0 || int.Parse(TB2.Text) < 0 || int.Parse(TB3.Text) < 0 || int.Parse(TB4.Text) < 0)
                {
                    error = true;
                }
            }

            if (!error)
            {
                try
                {
                    using HttpResponseMessage response = await client.GetAsync(web + ipRecuest);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var geoData = JsonSerializer.Deserialize<GeoData>(responseBody);

                    richBox.AppendText("IP: " + geoData.geoplugin_request + "\n");
                    richBox.AppendText("Continent: " + geoData.geoplugin_continentName + "\n");
                    richBox.AppendText("Continent Code: " + geoData.geoplugin_continentCode + "\n");
                    richBox.AppendText("Country: " + geoData.geoplugin_countryName + "\n");
                    richBox.AppendText("Country Code: " + geoData.geoplugin_countryCode + "\n");
                    richBox.AppendText("Region: " + geoData.geoplugin_regionName + "\n");
                    richBox.AppendText("City: " + geoData.geoplugin_city + "\n");
                    richBox.AppendText("Latitude: " + geoData.geoplugin_latitude + "\n");
                    richBox.AppendText("Longitude: " + geoData.geoplugin_longitude + "\n");
                    richBox.AppendText("Currency: " + geoData.geoplugin_currencyCode + "\n");
                    richBox.AppendText("Currency Symbol: " + geoData.geoplugin_currencySymbol + "\n");
                    richBox.AppendText("1" + geoData.geoplugin_currencySymbol + " = " + geoData.geoplugin_currencyConverter + "$" + "");
                    CountryViewModel countryViewModel = new CountryViewModel();
                    countryViewModel.CountryCode = geoData.geoplugin_countryCode;
                    this.DataContext = countryViewModel;

                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    MessageBox.Show("Message :{0} ", e.Message);

                }
            }
            else
            {
                MessageBox.Show("That values are incorrect for IPs. Remember that IPs range from 255 to 0.","Error in IP address",MessageBoxButton.OK, MessageBoxImage.Error);
            }
            

        }

        private bool IsNumeric(string text)
        {
            return int.TryParse(text, out _);
        }


        private void by_ip(object sender, RoutedEventArgs e)
        {
                deserializeAsync(false);
        }

        private void my_ip(object sender, RoutedEventArgs e)
        {
            deserializeAsync(true);
        }
    }
    }
