using LibSumo.Net;
using LibSumo.Net.lib.network;
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
        public static int DEFAULT_TURN_DEGREE = 25;
        public static int DEFAULT_SPEED = 50;
        private static readonly log4net.ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DroneController droneController;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DroneConnection droneConnection = new WirelessLanDroneConnection("192.168.2.1", 44444, "pc");
            droneController = new DroneController(droneConnection);
            
            droneController.addBatteryListener(b=>LOGGER.Info("BatteryState: " + b));
            droneController.addCriticalBatteryListener(b=>LOGGER.Info("Critical-BatteryState: " + b));
            droneController.addPCMDListener(b=>LOGGER.Info("PCMD: " + b));
            droneController.addOutdoorSpeedListener(b=>LOGGER.Info("Speed: " + b));
        }

       
        
    }
}
