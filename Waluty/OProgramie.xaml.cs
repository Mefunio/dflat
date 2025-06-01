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

namespace Waluty
{
    public sealed partial class OProgramie : Page
    {
        private PozycjaTabeliA aktualnaWalutaDocelowa;
        private string dataPublikacji;

        public OProgramie()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is NavigationParameter param)
            {
                // pobieranie parametrow
                aktualnaWalutaDocelowa = param.WalutaDocelowa;
                dataPublikacji = param.DataPublikacji;

                AktualizujInformacje();
            }
        }

        private void AktualizujInformacje()
        {
            txtImieNazwisko.Text = "Mateusz Mroczkowski";

            txtDataPublikacji.Text = dataPublikacji;

            // zrodlowa z pola statycznego
            var walutaZrodlowa = MainPage.AktualnaWalutaZrodlowa;
            if (walutaZrodlowa != null)
            {
                txtWalutaZrodlowa.Text = walutaZrodlowa.nazwa_waluty;
            }

            // docelowa z parametru
            if (aktualnaWalutaDocelowa != null)
            {
                txtWalutaDocelowa.Text = aktualnaWalutaDocelowa.nazwa_waluty;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}