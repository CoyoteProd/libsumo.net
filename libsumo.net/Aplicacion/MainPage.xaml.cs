using LibSumo.Net;
using LibSumo.Net.lib.network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace Aplicacion
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public readonly List<string> animaciones = new List<string> {"High jump", "Long Jump", "Spin", "Tap", "Slow shake", "Metronome", "Ondulation",
            "Spin Jump", "Spin To Posture", "Spiral", "Slalom"};
        public static int DEFAULT_TURN_DEGREE = 25;
        public static int DEFAULT_SPEED = 50;
        private static readonly log4net.ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DroneController droneController;

        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += AcceleratorKeyActivated;
        }

        private void AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                var left = Window.Current.CoreWindow.GetKeyState(VirtualKey.Left).HasFlag(CoreVirtualKeyStates.Down);
                var right = Window.Current.CoreWindow.GetKeyState(VirtualKey.Right).HasFlag(CoreVirtualKeyStates.Down);
                var up = Window.Current.CoreWindow.GetKeyState(VirtualKey.Up).HasFlag(CoreVirtualKeyStates.Down);
                var down = Window.Current.CoreWindow.GetKeyState(VirtualKey.Down).HasFlag(CoreVirtualKeyStates.Down);

                if (up && left) droneController.forwardLeft();
                else if (up && right) droneController.forwardRight();
                else if (down && left) droneController.backwardLeft();
                else if (down && right) droneController.backwardRight();
                else if (up) droneController.forward();
                else if(down) droneController.backward();
                else if(left) droneController.left();
                else if(right) droneController.right();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (options.SelectedIndex)
            {
                case 0:
                    droneController.jumpHigh();
                    break;
                case 1:
                    droneController.jumpLong();
                    break;
                case 2:
                    droneController.spin();
                    break;
                case 3:
                    droneController.tap();
                    break;
                case 4:
                    droneController.slowShake();
                    break;
                case 5:
                    droneController.metronome();
                    break;
                case 6:
                    droneController.ondulation();
                    break;
                case 7:
                    droneController.spinJump();
                    break;
                case 8:
                    droneController.spinToPosture();
                    break;
                case 9:
                    droneController.spiral();
                    break;
                case 100:
                    droneController.slalom();
                    break;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            options.ItemsSource = animaciones;
            options.SelectedIndex = 0;

            await Task.Run( () =>
            {
                WirelessLanDroneConnection droneConnection = new WirelessLanDroneConnection("192.168.2.1", 44444, "com.example.arsdkap");
                droneController = new DroneController(droneConnection);

                droneController.addBatteryListener(b => LOGGER.Info("BatteryState: " + b));
                droneController.addPCMDListener(b => LOGGER.Info("PCMD: " + b));
            });

            progressRing.Visibility = Visibility.Collapsed;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var audio = droneController.audio();
            audio.theme(LibSumo.Net.lib.command.multimedia.AudioTheme.Theme.Monster);
        }
    }
}
