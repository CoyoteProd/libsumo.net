using LibSumoUni;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gui.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DroneController dr = new DroneController("192.168.2.1", new HandshakeRequest("PC", "PC"));
            dr.evtImageReady +=dr_evtImageReady;
        }

        void dr_evtImageReady(object sender, EventArgs e)
        {
            // display new image
            ImageSourceConverter c = new ImageSourceConverter();
            imageDrone.Source = (ImageSource)c.ConvertFrom(((LibSumoUni.DroneController.ImageReadyEventArgs)e).img);
        }
        
    }
}
