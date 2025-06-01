using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using BingMapsRESTToolkit;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace mapy
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            mojaMapa.MapServiceToken = DaneGeograficzne.BingKey;
        }

        private void powMape(object sender, RoutedEventArgs e)
        {
            mojaMapa.ZoomLevel++;

            if (mojaMapa.ZoomLevel > 20)
            {
                mojaMapa.ZoomLevel = 20;
            }
        }

        private void zmnMape(object sender, RoutedEventArgs e)
        {
            mojaMapa.ZoomLevel--;

            if (mojaMapa.ZoomLevel < 1)
            {
                mojaMapa.ZoomLevel = 1;
            }
        }

        private void trybMapy(object sender, RoutedEventArgs e)
        {

            var bt = sender as AppBarButton;

            var label = bt.Label;

            if(mojaMapa.Style == Windows.UI.Xaml.Controls.Maps.MapStyle.AerialWithRoads)
            {
                mojaMapa.Style = Windows.UI.Xaml.Controls.Maps.MapStyle.Road;
                label = "Satelita";
                (bt.Icon as FontIcon).Glyph = "S";
            }
            else
            {
                mojaMapa.Style = Windows.UI.Xaml.Controls.Maps.MapStyle.AerialWithRoads;
                label = "Mapa";
                (bt.Icon as FontIcon).Glyph = "M";
            }
        }

        private void nowaStrona(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Koordynaty));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DaneGeograficzne.pktStartowy.Latitude != 0 && DaneGeograficzne.pktStartowy.Longitude != 0)
            {
                var znacznikStart = new MapIcon
                {
                    Location = new Geopoint(DaneGeograficzne.pktStartowy),
                    Title = "Tu jestem"
                };

                mojaMapa.MapElements.Add(znacznikStart);


                if (DaneGeograficzne.pktDocelowy.Latitude != 0 && DaneGeograficzne.pktDocelowy.Longitude != 0)
                {
                    var znacznikCel = new MapIcon
                    {
                        Location = new Geopoint(DaneGeograficzne.pktDocelowy),
                        Title = DaneGeograficzne.opisCelu ?? "Cel podróży",
                    };
                    mojaMapa.MapElements.Add(znacznikCel);

                    var trasaLotem = new MapPolyline
                    {
                        StrokeColor = Windows.UI.Colors.Black,
                        StrokeThickness = 3,
                        StrokeDashed = true,
                        Path = new Geopath(new List<BasicGeoposition>
                        {
                            DaneGeograficzne.pktStartowy,
                            DaneGeograficzne.pktDocelowy
                        })
                    };
                    mojaMapa.MapElements.Add(trasaLotem);
                }

                await mojaMapa.TrySetViewAsync(new Geopoint(DaneGeograficzne.pktStartowy), 8);

            }

            base.OnNavigatedTo(e);
        }

        private async void Trasa()
        {
            var routeReq = new RouteRequest()
            {
                BingMapsKey = DaneGeograficzne.BingKey,
                Culture = "pl",
                Waypoints = new List<SimpleWaypoint>
                {
                    new SimpleWaypoint(DaneGeograficzne.pktStartowy.Latitude, DaneGeograficzne.pktStartowy.Longitude),
                    new SimpleWaypoint(DaneGeograficzne.pktDocelowy.Latitude, DaneGeograficzne.pktDocelowy.Longitude),
                },
                RouteOptions = new RouteOptions()
                {
                    RouteAttributes = new List<RouteAttributeType> { RouteAttributeType.RoutePath }
                }
            };
            var response = await ServiceManager.GetResponseAsync(routeReq);

            if (response != null && response.ResourceSets != null && response.ResourceSets.Length > 0)
            {
                var resource = response.ResourceSets[0].Resources[0];
                var route = resource as Route;

                double[][] path = null;
                if (route != null && route.RoutePath != null && route.RoutePath.Line != null && route.RoutePath.Line.Coordinates != null)
                {
                    path = route.RoutePath.Line.Coordinates;
                }

                
            }
        }
    }
}
