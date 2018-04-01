using System;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Diagnostics;
using System.Linq;

using LibSumo.Net;
using LibSumo.Net.Events;
using LibSumo.Net.Logger;
using LibSumo.Net.Hook;
using LibSumo.Net.Protocol;

using OpenCvSharp;
using OpenCvSharp.Extensions;

using System.Reflection;
using System.Drawing;
using SumoApplication.Video;

namespace SumoApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window, IDisposable
    {        
        private SumoController controller;
        private SumoKeyboardPiloting piloting;
        private SumoInformations sumoInformations;

        //private string AlerteStr, BatteryLevelStr, WifiRssiStr, WifiQualityStr;

        // Framerate calculation
        private double frameRate;
        private Stopwatch frameWatch;

        Mat SplashImage;

        public MainWindow()
        {

            InitializeComponent();           

            LOGGER.GetInstance.MessageAvailable += GetInstance_MessageAvailable;
            //LOGGER.GetInstance.MessageLevel = log4net.Core.Level.Debug;
            frameWatch = new Stopwatch();
            //sumoInformations = new SumoInformations();

            // Init Audio Theme Box
            cbxAudioTheme.ItemsSource = Enum.GetValues(typeof(SumoEnumGenerated.Theme_theme)).Cast<SumoEnumGenerated.Theme_theme>();
            cbxAudioTheme.SelectedIndex = 0;
            this.cbxAudioTheme.SelectionChanged += CbxAudioTheme_SelectionChanged;

            // Init Default UI
            cvtTop.IsEnabled = false;
            cvtRight.IsEnabled = false;
            image.IsEnabled = false;

            // Set SplashImage
            Assembly _assembly = Assembly.GetExecutingAssembly();                              
            SplashImage = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.SplashScreen.jpg")));
            // add HUD logo on Splash Image for testing
            Mat FinalImage = ImageManipulation.Decorate(SplashImage, new SumoInformations());
            image.Source = FinalImage.ToWriteableBitmap();

            InitDrone();
        }


        private void InitDrone()
        {
            // Create Controller
            controller = new SumoController(out piloting);
            controller.ImageAvailable += Controller_ImageAvailable;
            controller.SumoEvents += Controller_SumoEvents;

            // Connect Piloting Events to control Sumo
            piloting.Disconnect += Piloting_Disconnect;
            piloting.Move += Piloting_Move;
            piloting.KeyboardKeysAvailable += Piloting_KeyboardKeysAvailable;

            // If you want process video in OpenCV separate window Set EnableOpenCV to true
            // In this case controller.ImageAvailable is not fired
            //controller.EnableOpenCV = true;

           
        }


        private void InitUI()
        {
            cvtTop.IsEnabled = true;
            cvtRight.IsEnabled = true;
            image.IsEnabled = true;
        }


        private void InitSoundUI()
        {                        
            if (controller.DroneIsCapableOfAudio)
            {
                // TODO Init GroupBox

                // Init Sound Box
                string[] soundsFiles = Directory.EnumerateFiles(@".\Sound", "*.wav", SearchOption.TopDirectoryOnly).Select(x => Path.GetFileName(x)).ToArray();
                cbxSounds.ItemsSource = soundsFiles;

                // Change StreamDirection
                controller.SetAudioDroneRX(true);
            }
        }



        #region Controller CallBack
        private void Controller_SumoEvents(object sender, SumoEventArgs e)
        {
            sumoInformations = e.SumoInformations;
            switch(e.TypeOfEvent)
            {
                case (SumoEnumCustom.TypeOfEvents.AlertEvent):
                    break;
                case (SumoEnumCustom.TypeOfEvents.BatteryLevelEvent):                    
                    lblBatteryLevel.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lblBatteryLevel.Content = sumoInformations.BatteryLevel + "%";
                        
                    }));
                    break;
                case (SumoEnumCustom.TypeOfEvents.Connected):
                    // Enable Btn
                    InitUI();
                    InitSoundUI();
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
                        lblPostureState.Content = "Sumo in: " + sumoInformations.Posture.ToString()+" position";
                    }));
                    break;
                case (SumoEnumCustom.TypeOfEvents.RSSI):
                    lblRssi.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lblRssi.Content = "Wifi Signal : "+ sumoInformations.Rssi.ToString() +" dbm";                        
                    }));
                    break;
                case (SumoEnumCustom.TypeOfEvents.LinkQuality):                    
                    lblQuality.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lblQuality.Content = "Link Quality: " +sumoInformations.LinkQuality.ToString() +"/6";
                        
                    }));
                    break;

                case (SumoEnumCustom.TypeOfEvents.VolumeChange):
                    slVolume.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        slVolume.Value = sumoInformations.Volume;
                    }));
                    break;
            }

            /*
            // Update Info Label
            lblInfo.Dispatcher.BeginInvoke((Action)(() =>
            {
                lblInfo.Content = String.Format("{0}\t{1}\t{2}\t {3}",BatteryLevelStr, WifiQualityStr, WifiRssiStr, AlerteStr);
            }));
            */
        }

       

        private void Controller_ImageAvailable(object sender, ImageEventArgs e)
        {
            image.Dispatcher.BeginInvoke((Action)(() =>
            {
                DisplayFPS();
                try
                {
                    var FinalImage = ImageManipulation.Decorate(e.RawImage, sumoInformations);
                    image.Source = FinalImage.ToWriteableBitmap(PixelFormats.Bgr24);                    
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
                case (0x53): // Letter s
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

                case (0x51): // letter q
                    controller.Headlight_off();
                    break;
                case (0x45): // letter e
                    controller.Headlight_on();
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


       

        private void TxtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FakeTxtBox.Focus();
        }

        private void BtnAudioRecord_Click(object sender, RoutedEventArgs e)
        {
            // Send selected Wave to Drone
            // Not working for the moment
            MessageBox.Show("Not Working for the moment"); return;
            string item = cbxSounds.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(item))
            {
                controller.SetAudioDroneRX(true);
                controller.StreamAudioToDrone(item);
            }
        }

        private void SlLight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Send Light level to Drone
            controller.Headlight_Value(e.NewValue);
        }

        private void BtnAudioStreamOn_Click(object sender, RoutedEventArgs e)
        {
            controller.SetAudioDroneTX(true);
        }

        private void BtnAudioStreamOff_Click(object sender, RoutedEventArgs e)
        {
            controller.SetAudioDroneTX(false);
        }


        #endregion

        public float ToRadians(float degree)
        {
            return (float)((degree / 2 * Math.PI) / 180.0);
        }

        public void Dispose()
        {
            // Inform controller to quit cleanly
            if (controller != null)
            {
                controller.Disconnect();
                controller.Dispose();
            }
        }
    }     

}
