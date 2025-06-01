using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
namespace Waluty
{
    public sealed partial class MainPage : Page
    {
        const string daneNBP = "https://static.nbp.pl/dane/kursy/xml/LastA.xml";

        List<PozycjaTabeliA> kursyAktualne;

        public static PozycjaTabeliA AktualnaWalutaZrodlowa { get; set; }

        private string dataPublikacji = "";

        public MainPage()
        {
            this.InitializeComponent();
            kursyAktualne = new List<PozycjaTabeliA>();
        }

        private void oProgramieBtn_Click(object sender, RoutedEventArgs e)
        {
            // do pola statycznego
            AktualnaWalutaZrodlowa = IbxZWaluty.SelectedItem as PozycjaTabeliA;

            // jako parametr
            var walutaDocelowa = IbxNaWalute.SelectedItem as PozycjaTabeliA;

            ZapiszAktualnyStan();

            Frame.Navigate(typeof(OProgramie), new NavigationParameter
            {
                WalutaDocelowa = walutaDocelowa,
                DataPublikacji = dataPublikacji
            });
        }

        private async void grKalkulator_Loaded(object sender, RoutedEventArgs e)
        {
            var listonosz = new HttpClient();
            string daneKursoweXml = string.Empty;

            try
            {
                daneKursoweXml = await listonosz.GetStringAsync(daneNBP);
            }
            catch (HttpRequestException ex)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Błąd połączenia",
                    Content = "Nie udało się pobrać kursów walut. Sprawdź połączenie z internetem i spróbuj ponownie.\n\nSzczegóły: " + ex.Message,
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
            }

            var daneXml = XDocument.Parse(daneKursoweXml);

            // pobieranie daty publikacji pliku danych
            var tabela = daneXml.Element("tabela_kursow");
            if (tabela != null)
            {
                var dataElement = tabela.Element("data_publikacji");
                if (dataElement != null)
                {
                    dataPublikacji = dataElement.Value;
                }
            }

            var listaPozycji = from item in daneXml.Descendants("pozycja")
                               select new PozycjaTabeliA()
                               {
                                   nazwa_waluty = item.Element("nazwa_waluty").Value,
                                   kod_waluty = item.Element("kod_waluty").Value,
                                   kurs_sredni = item.Element("kurs_sredni").Value,
                                   przelicznik = item.Element("przelicznik").Value
                               };

            kursyAktualne = listaPozycji.ToList();

            kursyAktualne.Insert(0, new PozycjaTabeliA()
            {
                nazwa_waluty = "Polski zloty",
                kurs_sredni = "1,0000",
                kod_waluty = "PLN",
                przelicznik = "1"
            });

            IbxZWaluty.ItemsSource = kursyAktualne;
            IbxNaWalute.ItemsSource = kursyAktualne;

            OdczytajZapisanyStan();
        }

        private void txtKwota_TextChanged(object sender, TextChangedEventArgs e)
        {
            Przelicz();
        }

        private void IbxZWaluty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Przelicz();
        }

        private void IbxNaWalute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Przelicz();
        }

        private void Przelicz()
        {
            if (IbxZWaluty.SelectedItem == null || IbxNaWalute.SelectedItem == null)
                return;

            var walutaWyjsciowa = IbxZWaluty.SelectedItem as PozycjaTabeliA;

            var kursWyjsciowyText = walutaWyjsciowa.kurs_sredni;
            var przelicznikWyjsciowyText = walutaWyjsciowa.przelicznik;


            var walutaDocelowa = IbxNaWalute.SelectedItem as PozycjaTabeliA;

            var kursDocelowyText = walutaDocelowa.kurs_sredni;
            var przelicznikDocelowyText = walutaDocelowa.przelicznik;

            var kwotaText = txtKwota.Text;

            try
            {
                double kwota = double.Parse(kwotaText);
                double kursWyjsciowy = double.Parse(kursWyjsciowyText);
                double kursDocelowy = double.Parse(kursDocelowyText);
                double przelicznikWyjsciowy = double.Parse(przelicznikWyjsciowyText);
                double przelicznikDocelowy = double.Parse(przelicznikDocelowyText);

                double kwotaPLN = kwota * kursWyjsciowy / przelicznikWyjsciowy;
                double kwotaDocelowa = kwotaPLN * przelicznikDocelowy / kursDocelowy;

                tbPrzeliczona.Text = string.Format("{0:0.00}", kwotaDocelowa);
            }
            catch
            {
                tbPrzeliczona.Text = "0.00";
            }
        }

        public void ZapiszAktualnyStan()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (IbxZWaluty.SelectedIndex >= 0)
                localSettings.Values["ZWalutyIndex"] = IbxZWaluty.SelectedIndex;

            if (IbxNaWalute.SelectedIndex >= 0)
                localSettings.Values["NaWaluteIndex"] = IbxNaWalute.SelectedIndex;

            localSettings.Values["Kwota"] = txtKwota.Text;
        }

        private void OdczytajZapisanyStan()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("ZWalutyIndex"))
                IbxZWaluty.SelectedIndex = (int)localSettings.Values["ZWalutyIndex"];
            else
                IbxZWaluty.SelectedIndex = 0;

            if (localSettings.Values.ContainsKey("NaWaluteIndex"))
                IbxNaWalute.SelectedIndex = (int)localSettings.Values["NaWaluteIndex"];
            else
                IbxNaWalute.SelectedIndex = 0;

            if (localSettings.Values.ContainsKey("Kwota"))
                txtKwota.Text = localSettings.Values["Kwota"].ToString();
        }
    }

    // Klasa parametru do nawigacji
    public class NavigationParameter
    {
        public PozycjaTabeliA WalutaDocelowa { get; set; }
        public string DataPublikacji { get; set; }
    }
}