using LibSumo.Net.Events;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SumoApplication.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SumoApplication
{
    /// <summary>
    /// Interaction logic for Simulate.xaml
    /// </summary>
    public partial class Simulate : System.Windows.Window
    {
        private SumoInformations sumoInfo;
        Image image;
        Mat splashImage;

        public Simulate(ref Image _image, Mat _splashImage, ref SumoInformations _sumoInfo)
        {
            sumoInfo = _sumoInfo;
            image = _image;
            splashImage = _splashImage;
            InitializeComponent();                        
        }

        private void sldBattery_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sumoInfo.BatteryLevel = (int)sldBattery.Value;
            UpdateImage();
        }

        private void sldRssi_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sumoInfo.Rssi = (int)sldRssi.Value;
            UpdateImage();
        }

        private void sldQuality_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sumoInfo.LinkQuality = (int)sldQuality.Value;
            UpdateImage();
        }

        private void sldPosture_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sumoInfo.Posture = (LibSumo.Net.Protocol.SumoEnumGenerated.PostureChanged_state) sldPosture.Value;
            UpdateImage();
        }

        private void sldSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sumoInfo.Speed = (sbyte)sldSpeed.Value;
            UpdateImage();
        }

        void UpdateImage()
        {
            Mat tmpImage = ImageManipulation.Decorate(splashImage, sumoInfo);
            image.BeginInit();
            image.Source = tmpImage.ToWriteableBitmap();
            image.EndInit();
        }
    }
}
