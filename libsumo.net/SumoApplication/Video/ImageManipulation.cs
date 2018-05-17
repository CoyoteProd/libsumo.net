using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LibSumo.Net.Events;
using LibSumo.Net.Protocol;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace SumoApplication.Video
{
    class ImageManipulation
    {
        public static Mat Decorate(Mat RawImage, SumoInformations sumoInformations)
        {
            if (sumoInformations == null) return RawImage;

            Assembly _assembly = Assembly.GetExecutingAssembly();

            var dst = new Mat();
            var logo = new Mat();

            // Double resolution to avoid bad logo
            dst = RawImage.Resize(new OpenCvSharp.Size(RawImage.Width * 2, RawImage.Height * 2), 0, 0, InterpolationFlags.Cubic);
            Scalar color;
            if (sumoInformations.IsBatteryUnderLevelAlert) color = Scalar.Red;
            else color = Scalar.Black;

            // Write Battery Level            
            Cv2.PutText(dst, sumoInformations.BatteryLevel.ToString() +"%", new OpenCvSharp.Point((dst.Width / 2) - 140, 60), HersheyFonts.HersheyComplexSmall, 2.0, color, 2, LineTypes.AntiAlias, false);

            // Write wifi logo                                      
            if (sumoInformations.Rssi < 0)
            {
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-5.png")));
                if (sumoInformations.Rssi < -50)
                    logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-4.png")));
                if (sumoInformations.Rssi < -60)
                    logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-3.png")));
                if (sumoInformations.Rssi < -70)
                    logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-2.png")));
                if (sumoInformations.Rssi < -80)
                    logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-1.png")));
                if (sumoInformations.Rssi < -90)
                    logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-0.png")));
                CopyTransparentImage(dst, logo, dst.Width - logo.Width - 10, 10);
            }

            // Link Quality           
            logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-4.png")));
            if (sumoInformations.LinkQuality < 5)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-3.png")));
            if (sumoInformations.LinkQuality < 4)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-2.png")));
            if (sumoInformations.LinkQuality < 3)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-1.png")));
            if (sumoInformations.LinkQuality < 1)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-0.png")));    
                
            CopyTransparentImage(dst, logo, 10, 10);

            // battery
            logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-4.png")));
            if (sumoInformations.BatteryLevel < 70)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-3.png")));
            if (sumoInformations.BatteryLevel < 50)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-2.png")));
            if (sumoInformations.BatteryLevel < 20)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-1.png")));
            if (sumoInformations.BatteryLevel < 10)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-0.png")));

            CopyTransparentImage(dst, logo, (dst.Width / 2) - (logo.Width / 2), 0);

            // Posture
            if(sumoInformations.Posture == LibSumo.Net.Protocol.SumoEnumGenerated.PostureChanged_state.jumper)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.product_0902_posture_icn_jumper.png")));
            else if (sumoInformations.Posture == LibSumo.Net.Protocol.SumoEnumGenerated.PostureChanged_state.kicker)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.product_0902_posture_icn_kicker.png")));
            else if (sumoInformations.Posture == LibSumo.Net.Protocol.SumoEnumGenerated.PostureChanged_state.standing)
                logo = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.product_0902_posture_icn_standing.png")));

            CopyTransparentImage(dst, logo, 0, dst.Height - logo.Height);

            if(sumoInformations.Alert != SumoEnumGenerated.AlertStateChanged_state.none)
                Cv2.PutText(dst, sumoInformations.Alert.ToString(), new OpenCvSharp.Point(100, dst.Height / 2), HersheyFonts.HersheyComplexSmall, 6.0, Scalar.Red, 4, LineTypes.AntiAlias, false);
            
            if(sumoInformations.Speed!=0)
                Cv2.PutText(dst, sumoInformations.Speed.ToString() + "cm/s", new OpenCvSharp.Point((dst.Width / 2) - 470, 60), HersheyFonts.HersheyComplexSmall, 2.0, color, 2, LineTypes.AntiAlias, false);
            // TODO : Box icon (open/close)

            return dst;
            
        }


        private static void CopyTransparentImage(Mat dst, Mat logo, int x, int y, int newWidth, int newHeight)
        {
            var ResizedLogo = new Mat();
            ResizedLogo = logo.Resize(new OpenCvSharp.Size(newWidth, newHeight), 0, 0, InterpolationFlags.Nearest);
            CopyTransparentImage(dst, ResizedLogo, x, y);
        }
        private static void CopyTransparentImage(Mat dst, Mat logo, int x, int y)
        {
            Mat mask;
            int a = logo.Height + x;
            int b = logo.Width + y;

            if (logo.Channels() == 4)
            {
                Cv2.Split(logo, out Mat[] rgbLayer);         // seperate channels
                Mat[] cs = { rgbLayer[0], rgbLayer[1], rgbLayer[2] };
                Cv2.Merge(cs, logo);        // glue together again
                mask = rgbLayer[3];       // png's alpha channel used as mask
                logo.CopyTo(dst[y, b, x, a], mask);
            }
            else
                logo.CopyTo(dst[y, b, x, a]);
        }

    }
}
