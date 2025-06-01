using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace paint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool czyRysuje;
        Point pktStartu;
        Line popKreska;
        SolidColorBrush pisak;
        double gruboscPisaka;
        string exitText;

        Stack<Shape> listaUndo;

        public MainPage()
        {
            this.InitializeComponent();
            czyRysuje = false;
            popKreska = null;
            listaUndo= new Stack<Shape>();
            pisak = new SolidColorBrush(Windows.UI.Colors.Red);
            poleRysowania.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, poleRysowania.Width, poleRysowania.Height)
            };
            exitText = "Are you sure you want to exit?";
        }

        private void poleRysowania_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            czyRysuje = true;
            pktStartu = e.GetCurrentPoint(poleRysowania).RawPosition;
        }

        private void poleRysowania_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (czyRysuje)
            {
                Point pktAktualny = e.GetCurrentPoint(poleRysowania).RawPosition;
                Line kreska = new Line()
                {
                    Stroke = pisak,
                    StrokeThickness = gruboscPisaka,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,

                    X1 = pktStartu.X,
                    Y1 = pktStartu.Y,
                    X2 = pktAktualny.X,
                    Y2 = pktAktualny.Y
                };

                poleRysowania.Children.Add(kreska);
                
                if (rdbDowolna.IsChecked == true)
                {
                    pktStartu = pktAktualny;
                    listaUndo.Push(kreska);
                }

                if (rdbProsta.IsChecked == true)
                {
                    pktStartu = pktStartu;

                    if (popKreska != null)
                        poleRysowania.Children.Remove(popKreska);
                    popKreska = kreska;
                }
            }
        }

        private void poleRysowania_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            czyRysuje = false;
            if(rdbProsta.IsChecked == true)
            {
                listaUndo.Push(popKreska);
            }
            popKreska = null;
        }

        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var kwadracik = e.OriginalSource as Rectangle;
            pisak = kwadracik.Fill as SolidColorBrush;
        }

        private void SliderGruboscKreski_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            gruboscPisaka = e.NewValue;
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            pisak = new SolidColorBrush(args.NewColor);
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (listaUndo.Count > 0)
            {
                Shape undo = listaUndo.Pop();
                poleRysowania.Children.Remove(undo);
            }
        }

        private async void ShowExitDialog()
        {
            ContentDialog exitDialog = new ContentDialog
            {
                Title = "Exit",
                Content = exitText,
                PrimaryButtonText = "Exit",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await exitDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Application.Current.Exit();
            }
        }

        private async void SpeakText(string text)
        {
            var syntezator = new SpeechSynthesizer();
            MediaElement mediaElement = new MediaElement();

            SpeechSynthesisStream speechSynthesisStream = await syntezator.SynthesizeTextToStreamAsync(text);

            mediaElement.SetSource(speechSynthesisStream, speechSynthesisStream.ContentType);
            mediaElement.Play();
           
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            SpeakText(exitText);
            ShowExitDialog();
           
        }

    }
}
