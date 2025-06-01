using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using System.Collections;
using System.Net.Http;
using System.Xml.Linq;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace mapy
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Koordynaty : Page
    {

        readonly string BingKey;

        public Koordynaty()
        {
            this.InitializeComponent();
            GdzieJaNaMapie();
            BingKey = DaneGeograficzne.BingKey;
        }

        private async void GdzieJaNaMapie()
        {
            try
            {
                Geolocator mojGPS = new Geolocator
                {
                    DesiredAccuracy = PositionAccuracy.High
                };

                Geoposition mojeZGPS = await mojGPS.GetGeopositionAsync();

                double szerokosc = mojeZGPS.Coordinate.Point.Position.Latitude;
                double dlugosc = mojeZGPS.Coordinate.Point.Position.Longitude;
                
                tbGPS.Text = $"Szerokość: {szerokosc:F6}°, Długość: {dlugosc:F6}°";

                DaneGeograficzne.pktStartowy = new BasicGeoposition
                {
                    Latitude = szerokosc,
                    Longitude = dlugosc
                };
            }
            catch (Exception ex)
            {
                tbGPS.Text = "Błąd GPS: " + ex.Message;
            }
        }

        private async void szukajCelu(object sender, RoutedEventArgs e)
        {
            try
            {
                string szukanyAdres = AdresTextBox.Text;

                if (string.IsNullOrWhiteSpace(szukanyAdres))
                {
                    tbWynikSzukania.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                    tbWynikSzukania.Text = "Wprowadź adres celu!";
                    return;
                }

                string apiKey = DaneGeograficzne.BingKey;
                string encodedAddress = Uri.EscapeDataString(szukanyAdres);
                string url = $"https://dev.virtualearth.net/REST/v1/Locations?query={encodedAddress}&key={apiKey}&output=xml";

                var listonosz = new HttpClient();
                var result = await listonosz.GetAsync(url);

                if (result != null && result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var contentXml = XDocument.Parse(content);

                    XNamespace ns = "http://schemas.microsoft.com/search/local/ws/rest/v1";

                    var location = contentXml.Descendants(ns + "Location").FirstOrDefault();

                    if (location != null)
                    {
                        var nameElement = location.Descendants(ns + "Name").FirstOrDefault();
                        string adres = nameElement?.Value ?? szukanyAdres;

                        var pointElement = location.Descendants(ns + "Point").FirstOrDefault();
                        if (pointElement != null)
                        {
                            var latElement = pointElement.Descendants(ns + "Latitude").FirstOrDefault();
                            var lonElement = pointElement.Descendants(ns + "Longitude").FirstOrDefault();

                            if (latElement != null && lonElement != null &&
                                double.TryParse(latElement.Value, out double latitude) &&
                                double.TryParse(lonElement.Value, out double longitude))
                            {
                                DaneGeograficzne.opisCelu = adres;
                                DaneGeograficzne.pktDocelowy = new BasicGeoposition
                                {
                                    Latitude = latitude,
                                    Longitude = longitude
                                };

                                tbDlugosc.Text = $"Długość geogr.: {longitude:F6}°";
                                tbSzerokosc.Text = $"Szerokość geogr.: {latitude:F6}°";
                                tbWynikSzukania.Foreground = new SolidColorBrush(Windows.UI.Colors.Green);
                                tbWynikSzukania.Text = $"Znaleziono: {adres}";
                            }
                            else
                            {
                                tbWynikSzukania.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                                tbWynikSzukania.Text = "Błąd parsowania współrzędnych.";
                            }
                        }
                        else
                        {
                            tbWynikSzukania.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                            tbWynikSzukania.Text = "Brak danych o współrzędnych.";
                        }
                    }
                    else
                    {
                        tbWynikSzukania.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                        tbWynikSzukania.Text = "Nie znaleziono adresu.";

                        DaneGeograficzne.pktDocelowy = new BasicGeoposition();
                        DaneGeograficzne.opisCelu = null;
                        tbDlugosc.Text = "Długość geogr.: -";
                        tbSzerokosc.Text = "Szerokość geogr.: -";
                    }
                }
                else
                {
                    tbWynikSzukania.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                    tbWynikSzukania.Text = $"Błąd API: {result?.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                tbWynikSzukania.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                tbWynikSzukania.Text = $"Błąd: {ex.Message}";
            }
        }

        private void powrot(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}