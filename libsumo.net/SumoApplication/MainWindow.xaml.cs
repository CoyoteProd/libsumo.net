using LibSumo.Net.Logger;
using LibSumo.Net;
using System;
using System.Windows;
using System.IO;
using LibSumo.Net.Events;
using OpenCvSharp.Extensions;
using System.Windows.Media;
using LibSumo.Net.Hook;
using LibSumo.Net.Helpers;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using LibSumo.Net.Protocol;
using System.Linq;

namespace SumoApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private SumoController controller;
        private SumoKeyboardPiloting piloting;
        private string AlerteStr, BatteryLevelStr, WifiRssiStr, WifiQualityStr;

        // Framerate calculation
        private double frameRate;
        private Stopwatch frameWatch;        


        public MainWindow()
        {

            InitializeComponent();
            LOGGER.GetInstance.MessageAvailable += GetInstance_MessageAvailable;
            //LOGGER.GetInstance.MessageLevel = log4net.Core.Level.All;
            frameWatch = new Stopwatch();
            controller = new SumoController(out piloting);            
            controller.ImageAvailable += Controller_ImageAvailable;
            controller.SumoEvents += Controller_SumoEvents ;            

            // If you want process video in OpenCV separate window Set EnableOpenCV to true
            // In this case controller.ImageAvailable is not fired
            //controller.EnableOpenCV = true;

            // Connect Piloting Events to control Sumo
            piloting.Disconnect += Piloting_Disconnect;
            piloting.Move += Piloting_Move;
            piloting.KeyboardKeysAvailable += Piloting_KeyboardKeysAvailable;
          
            cbxAudioTheme.ItemsSource = Enum.GetValues(typeof(SumoEnumGenerated.Theme_theme)).Cast<SumoEnumGenerated.Theme_theme>();
            cbxAudioTheme.SelectedIndex = 0;
            this.cbxAudioTheme.SelectionChanged += CbxAudioTheme_SelectionChanged;
        }

      

        #region Controller CallBack
        private void Controller_SumoEvents(object sender, SumoEventArgs e)
        {
            switch(e.TypeOfEvent)
            {
                case (SumoEnumCustom.TypeOfEvents.AlertEvent):
                    AlerteStr = e.Alert.ToString();                    
                    break;
                case (SumoEnumCustom.TypeOfEvents.BatteryLevelEvent):
                    BatteryLevelStr = e.BatteryLevel + "%";

                    lblBatteryLevel.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lblBatteryLevel.Content = e.BatteryLevel + "%";
                        
                    }));
                    break;
                case (SumoEnumCustom.TypeOfEvents.Connected):
                    // Enable Btn
                    cvtTop.IsEnabled = true;
                    cvtRight.IsEnabled = true;
                    image.IsEnabled = true;
                    break;
                case (SumoEnumCustom.TypeOfEvents.Disconnected):
                    break;
                case (SumoEnumCustom.TypeOfEvents.Discovered):
                    txtBox.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        txtBox.AppendText(String.Format("Sumo is available {0} ", Environment.NewLine));
                        txtBox.ScrollToEnd();
                    }));
                    break;
                case (SumoEnumCustom.TypeOfEvents.PilotingEvent):
                    break;
                case (SumoEnumCustom.TypeOfEvents.PostureEvent):
                    lblPostureState.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lblPostureState.Content = "Sumo in: " + e.Posture.ToString()+" position";
                    }));
                    break;
                case (SumoEnumCustom.TypeOfEvents.RSSI):
                    WifiRssiStr = e.Rssi.ToString() + " dbm";
                    lblRssi.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lblRssi.Content = "Wifi Signal : "+ e.Rssi.ToString() +" dbm";                        
                    }));
                    break;
                case (SumoEnumCustom.TypeOfEvents.LinkQuality):
                    WifiQualityStr = e.LinkQuality.ToString() + "/6";
                    lblQuality.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lblQuality.Content = "Link Quality: " +e.LinkQuality.ToString() +"/6";
                        
                    }));
                    break;

                case (SumoEnumCustom.TypeOfEvents.VolumeChange):
                    slVolume.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        slVolume.Value = e.Volume;
                    }));
                    break;
            }

            // Update Info Label
            lblInfo.Dispatcher.BeginInvoke((Action)(() =>
            {
                lblInfo.Content = String.Format("{0}, {1}, {2}, {3}",BatteryLevelStr, WifiQualityStr, WifiRssiStr, AlerteStr);
            }));
        }
 
        private void Controller_ImageAvailable(object sender, ImageEventArgs e)
        {
            image.Dispatcher.BeginInvoke((Action)(() =>
            {
                DisplayFPS();
                try
                {                    
                    image.Source = e.RawImage.ToWriteableBitmap(PixelFormats.Bgr24);
                }
                catch
                {
                }
            }));
        }

        //TODO : To Improved
        private void DisplayFPS()
        {
            if (frameWatch.IsRunning)
            {
                frameWatch.Stop();
                if (frameWatch.ElapsedMilliseconds > 0)
                {
                    double fps = (1000 / frameWatch.ElapsedMilliseconds);
                    if (fps < 20) frameRate = fps;
                    lblBatteryLevel.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lblFramerate.Content = Math.Round(frameRate, 1) + " fps";
                    }));
                }
            }
            else frameWatch = Stopwatch.StartNew();
        }
      
        #endregion

        #region Piloting CallBack
        private void Piloting_KeyboardKeysAvailable(object sender, KeyboardEventArgs e)
        {            
            HookUtils.VirtualKeyStates key = e.CurrentKey;
            bool IsPressed = e.IsPressed;

            switch((int)key)
            {
                // Postures
                case ((int)HookUtils.VirtualKeyStates.VK_F1): // Normal
                    controller.Postures(LibSumo.Net.Protocol.SumoEnumGenerated.Posture_type.jumper);
                    break;
                case ((int)HookUtils.VirtualKeyStates.VK_F2): // Upside-Down
                    controller.Postures(LibSumo.Net.Protocol.SumoEnumGenerated.Posture_type.kicker);
                    break;
                case (int)(HookUtils.VirtualKeyStates.VK_F3): // Auto-Balance
                    controller.Postures(LibSumo.Net.Protocol.SumoEnumGenerated.Posture_type.standing);
                    break;

                // Quick Turn
                case ((int)HookUtils.VirtualKeyStates.VK_SPACE): // Quick turn of 180°                  
                case (0x57): // Letter w
                    controller.QuickTurn(ToRadians(180)); //Quick turn right
                    break;
                case (0x53): // Letter w
                    controller.QuickTurn(ToRadians(-180));//Quick turn left
                    break;
                case (0x41): // Letter a 
                    controller.QuickTurn(ToRadians(-90)); //Quick half turn left
                    break;
                case (0x44): // Letter d 
                    controller.QuickTurn(ToRadians(90)); //Quick half turn right
                    break;
                                        
                // Animations
                case (0x31): // nuber 1
                    controller.Animation(LibSumo.Net.Protocol.SumoEnumGenerated.SimpleAnimation_id.tap);
                    break;
                case (0x32): // nuber 2
                    controller.Animation(LibSumo.Net.Protocol.SumoEnumGenerated.SimpleAnimation_id.ondulation);
                    break;
                case (0x33): // nuber 3
                    controller.Animation(LibSumo.Net.Protocol.SumoEnumGenerated.SimpleAnimation_id.slowshake);
                    break;

            }            
        }

        private void Piloting_Move(object sender, MoveEventArgs e)
        {
            // Move the Sumo
            controller.Move(e.Speed, e.Turn);

            // Display value on UI
            HSlider.Dispatcher.BeginInvoke((Action)(() => HSlider.Value = e.Turn));
            VSlider.Dispatcher.BeginInvoke((Action)(() => VSlider.Value = e.Speed));
            Vlbl.Dispatcher.BeginInvoke((Action)(() => Vlbl.Content = e.Speed));
            Hlbl.Dispatcher.BeginInvoke((Action)(() => Hlbl.Content = e.Turn));
        }

        private void Piloting_Disconnect(object sender, EventArgs e)
        {
            controller.Disconnect();
            cvtTop.IsEnabled = false;
            cvtRight.IsEnabled = false;
            image.IsEnabled = false;
        }     
        #endregion

        #region UI
        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            controller.Connect();                        
        }
        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            controller.Disconnect();
        }

        private void BtnSaveLog_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("sumo.log", txtBox.Text);
        }

        private void BtnLongJump_Click(object sender, RoutedEventArgs e)
        {            
            controller.Jump(LibSumo.Net.Protocol.SumoEnumGenerated.Jump_type._long);
        }
        private void BtnHighJump_Click(object sender, RoutedEventArgs e)
        {
            controller.Jump(LibSumo.Net.Protocol.SumoEnumGenerated.Jump_type.high);
        }
        private void BtnKick_Click(object sender, RoutedEventArgs e)
        {
            controller.JumpLoad();
        }
        private void BtnCancelJump_Click(object sender, RoutedEventArgs e)
        {
            controller.CancelJump();
        }
        private void BtnSTOP_Click(object sender, RoutedEventArgs e)
        {
            controller.STOP();
        }
        private void CbxAudioTheme_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            controller.SetAudioTheme((SumoEnumGenerated.Theme_theme)e.AddedItems[0]);
            FakeTxtBox.Focus();
        }
        private void SlVolume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            controller.Volume((byte)slVolume.Value);
        }
        private void GetInstance_MessageAvailable(object sender, MessageEventArgs e)
        {
            txtBox.Dispatcher.BeginInvoke((Action)(() =>
            {
                txtBox.AppendText(DateTime.Now.ToString() + " " + e.Level + " : " + e.Message + Environment.NewLine);
                txtBox.ScrollToEnd();
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {   
            // Inform controller to quit cleanly
            controller.Disconnect();
            controller.Dispose();
        }
        private void TxtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FakeTxtBox.Focus();
        }
        #endregion

        public float ToRadians(float degree)
        {
            return (float)((degree / 2 * Math.PI) / 180.0);
        }

      

        private void BtnHeadlightOff_Click(object sender, RoutedEventArgs e)
        {
            controller.Headlight_off();
        }

        private void BtnHeadlightOn_Click(object sender, RoutedEventArgs e)
        {
            controller.Headlight_on();
        }
    }     

}
