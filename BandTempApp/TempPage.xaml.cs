using Microsoft.Band;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BandTempApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TempPage : Page
    {
        DispatcherTimer timer;
        public TempPage()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }
        }
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            
            ct.Cancel();
            if (bandClient != null)
            {
                bandClient.Dispose();
                bandClient = null;
            }
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
            }
            
        }
        private void Timer_Tick(object sender, object e)
        {
            value.Text = ConvertTemperature(va,TemperatureUnit.Celcius,currentTempUnit).ToString();
        }
        private double ConvertTemperature(double value,TemperatureUnit from,TemperatureUnit to)
        {
            if (from == TemperatureUnit.Celcius)
            {
                if (to == TemperatureUnit.Fahrenheit)
                    return Math.Round(32 + (value * 1.8), 2);
                else if (to == TemperatureUnit.Kelvin)
                    return Math.Round(273.15 + value, 2);
                else
                    return Math.Round(value, 2);
            }
            else if (from == TemperatureUnit.Fahrenheit)
            {
                if (to == TemperatureUnit.Celcius)
                    return Math.Round((value - 32) / 1.8, 2);
                else if (to == TemperatureUnit.Kelvin)
                    return Math.Round(((value - 32) * 5 / 9 + 273.15), 2);
                else
                    return Math.Round(value, 2);
            }
            else
            {
                if (to == TemperatureUnit.Celcius)
                    return Math.Round(value - 273.15, 2);
                else if (to == TemperatureUnit.Fahrenheit)
                    return Math.Round((value - 273.15) * 9 / 5 + 32, 2);
                else
                    return Math.Round(value, 2);
            }
           
        }
        TemperatureUnit currentTempUnit = TemperatureUnit.Celcius;
        double va;
        IBandClient bandClient;
        CancellationTokenSource ct=new CancellationTokenSource();
        private async void EstablishConnection(int index)
        {
            ct = new CancellationTokenSource();
            await Task.Run(async() => 
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async() => 
                {
                    try
                    {
                        var pairedBands = await BandClientManager.Instance.GetBandsAsync();
                        if(pairedBands==null||pairedBands.Count()==0)
                        {
                            await new MessageDialog("Cannot connect to your band. Make sure your bluetooth then and try again.", "Error").ShowAsync();
                            ct.Cancel();
                            if (bandClient != null)
                            {
                                bandClient.Dispose();
                            }
                            if (Frame.CanGoBack)
                            {
                                Frame.GoBack();
                            }
                            return;
                        }
                        bandClient = null;
                        bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[index]);
                        if (bandClient.SensorManager.SkinTemperature.GetCurrentUserConsent() != UserConsent.Granted)
                        {
                            // user hasn’t consented, request consent
                            await bandClient.SensorManager.SkinTemperature.RequestUserConsentAsync();
                        }
                        bandClient.SensorManager.SkinTemperature.ReadingChanged += (sender, args) =>
                        {
                            va = args.SensorReading.Temperature;
                        };
                        try
                        {
                            await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();
                        }
                        catch (BandException)
                        {
                            await new MessageDialog("Cannot connect to your band. Make sure your bluetooth then and try again.", "Error").ShowAsync();
                            ct.Cancel();
                            if (bandClient != null)
                            {
                                bandClient.Dispose();
                            }
                            if (Frame.CanGoBack)
                            {                             
                                Frame.GoBack();
                            }
                        }
                        catch (Exception ex)
                        {
                            // handle a Band connection exception
                            Debug.WriteLine(ex.ToString());
                        }
                        timer.Start();
                    }
                    catch(BandException)
                    {
                        await new MessageDialog("Cannot connect to your band. Make sure your bluetooth then and try again.", "Error").ShowAsync();
                        ct.Cancel();
                        if (bandClient != null)
                        {
                            bandClient.Dispose();
                        }
                        if (Frame.CanGoBack)
                        {
                            Frame.GoBack();
                        }
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(ex.ToString(), "Unknown Error").ShowAsync();
                        ct.Cancel();
                        if (bandClient != null)
                        {
                            bandClient.Dispose();
                        }
                        if (Frame.CanGoBack)
                        {
                            Frame.GoBack();
                        }
                    }
                });               
            },ct.Token);           
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if(e.Parameter!=null)
            {
                EstablishConnection(Int32.Parse(e.Parameter.ToString()));
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {            
            base.OnNavigatingFrom(e);
        }
        public enum TemperatureUnit
        {
            Fahrenheit,
            Celcius,
            Kelvin
        }

        private void value_panel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(value.Text)&&value.Text!="N/A")
            {
                if (currentTempUnit == TemperatureUnit.Celcius)
                {
                    currentTempUnit = TemperatureUnit.Fahrenheit;
                    value.Text = ConvertTemperature(Double.Parse(value.Text), TemperatureUnit.Celcius, TemperatureUnit.Fahrenheit).ToString("0.00");
                    unit.Text = "°F";
                }
                else if (currentTempUnit == TemperatureUnit.Fahrenheit)
                {
                    currentTempUnit = TemperatureUnit.Kelvin;
                    value.Text = ConvertTemperature(Double.Parse(value.Text), TemperatureUnit.Fahrenheit, TemperatureUnit.Kelvin).ToString("0.00");
                    unit.Text = "K";
                }
                else if (currentTempUnit == TemperatureUnit.Kelvin)
                {
                    currentTempUnit = TemperatureUnit.Celcius;
                    value.Text = ConvertTemperature(Double.Parse(value.Text), TemperatureUnit.Kelvin, TemperatureUnit.Celcius).ToString("0.00");
                    unit.Text = "°C";
                }
            }         
        }
    }
}
