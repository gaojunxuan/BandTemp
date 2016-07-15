using Microsoft.Band;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BandTempApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();            
            SetStatusBar(Color.FromArgb(255, 41, 111, 204), Colors.White);            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GetBandsAndDisplay();
        }
        private static async void SetStatusBar(Color bc, Color fc)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = bc;
                statusBar.ForegroundColor = fc;
                statusBar.BackgroundOpacity = 1;
                await statusBar.ShowAsync();
            }
        }
        private async void GetBandsAndDisplay()
        {
            try
            {
                band_lst.Items.Clear();
                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands != null)
                {
                    if (pairedBands.Count() == 0)
                    {
                        emptymsg.Visibility = Visibility.Visible;
                        return;
                    }
                    else
                    {
                        emptymsg.Visibility = Visibility.Collapsed;
                    }
                    foreach (var b in pairedBands)
                    {
                        band_lst.Items.Add(b);                        
                    }
                }               
            }
            catch
            {
                errmsg.Visibility = Visibility.Visible;
            }
        }

        private void band_lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(band_lst.SelectedIndex!=-1)
            {
                Frame.Navigate(typeof(TempPage), band_lst.SelectedIndex);
            }         
        }

        private async void help_btn_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("With this app, you can get your current skin temperature using the sensor from Microsoft Band. You can switch the unit (Fahrenheit, Celcius or Kelvin) of the temperature value by tapping it.\n\nDevelop by Kevin Gao from Skylark Workshop\n\nSource code available on Github", "About").ShowAsync();
        }

        private void sync_btn_Click(object sender, RoutedEventArgs e)
        {
            GetBandsAndDisplay();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //GetBandsAndDisplay();
        }
    }
}
