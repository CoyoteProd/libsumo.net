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
            
            // Double resolution to avoid bad logo
            dst = RawImage.Resize(new OpenCvSharp.Size(RawImage.Width * 2, RawImage.Height * 2), 0, 0, InterpolationFlags.Cubic);

            // Alert Color
            Scalar Textcolor;
            Scalar Imagecolor;
            if (sumoInformations.IsBatteryUnderLevelAlert)
            {
                Textcolor = Scalar.Red;
                Imagecolor = Scalar.Red;
            }
            else
            {
                Textcolor = Scalar.Black;
                Imagecolor = Scalar.White;
            }

            // Write Battery Level            
            Cv2.PutText(dst, sumoInformations.BatteryLevel.ToString() +"%", new OpenCvSharp.Point((dst.Width / 2) - 140, 60), HersheyFonts.HersheyComplexSmall, 2.0, Textcolor, 2, LineTypes.AntiAlias, false);

            // Write wifi logo                                                  
            Mat logoWifi = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-5.png")));
            if (sumoInformations.Rssi < -50)
                logoWifi = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-4.png")));
            if (sumoInformations.Rssi < -60)
                logoWifi = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-3.png")));
            if (sumoInformations.Rssi < -70)
                logoWifi = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-2.png")));
            if (sumoInformations.Rssi < -80)
                logoWifi = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-1.png")));
            if (sumoInformations.Rssi < -90)
                logoWifi = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.wifi-signal-0.png")));                        
            CopyTransparentImage(dst, logoWifi, dst.Width - logoWifi.Width - 10, 10);
            

            // Link Quality           
            Mat logoQuality = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-4.png")));
            if (sumoInformations.LinkQuality < 5)
                logoQuality = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-3.png")));
            if (sumoInformations.LinkQuality < 4)
                logoQuality = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-2.png")));
            if (sumoInformations.LinkQuality < 3)
                logoQuality = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-1.png")));
            if (sumoInformations.LinkQuality < 1)
                logoQuality = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-signal-filled-0.png")));                    
            CopyTransparentImage(dst, logoQuality, 10, 10);

            // battery
            Mat logoBat = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-4.png")));
            if (sumoInformations.BatteryLevel < 70)
                logoBat = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-3.png")));
            if (sumoInformations.BatteryLevel < 50)
                logoBat = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-2.png")));
            if (sumoInformations.BatteryLevel < 20)
                logoBat = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-1.png")));
            if (sumoInformations.BatteryLevel < 10)
                logoBat = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.icons8-low-battery-0.png")));            
                        
            Mat logo1 = ChangeWhiteColor(logoBat, Imagecolor);             
            CopyTransparentImage(dst, logo1, (dst.Width / 2) - (logoBat.Width / 2), 0);

            // Posture
            Mat logoPosture = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.product_0902_posture_icn_jumper.png")));
            if (sumoInformations.Posture == LibSumo.Net.Protocol.SumoEnumGenerated.PostureChanged_state.jumper)
                logoPosture = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.product_0902_posture_icn_jumper.png")));
            else if (sumoInformations.Posture == LibSumo.Net.Protocol.SumoEnumGenerated.PostureChanged_state.kicker)
                logoPosture = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.product_0902_posture_icn_kicker.png")));
            else if (sumoInformations.Posture == LibSumo.Net.Protocol.SumoEnumGenerated.PostureChanged_state.standing)
                logoPosture = BitmapConverter.ToMat(new Bitmap(_assembly.GetManifestResourceStream("SumoApplication.Images.product_0902_posture_icn_standing.png")));            

            CopyTransparentImage(dst, logoPosture, 0, dst.Height - logoPosture.Height);

            if(sumoInformations.Alert != SumoEnumGenerated.AlertStateChanged_state.none)
                Cv2.PutText(dst, sumoInformations.Alert.ToString(), new OpenCvSharp.Point(100, dst.Height / 2), HersheyFonts.HersheyComplexSmall, 6.0, Scalar.Red, 4, LineTypes.AntiAlias, false);
            
            if(sumoInformations.Speed!=0)
                Cv2.PutText(dst, sumoInformations.Speed.ToString() + "cm/s", new OpenCvSharp.Point((dst.Width / 2) - 470, 60), HersheyFonts.HersheyComplexSmall, 2.0, Textcolor, 2, LineTypes.AntiAlias, false);
            // TODO : Box icon (open/close)

            return dst;
            
        }
        private static Mat ChangeWhiteColor(Mat img, Scalar newColor)
        {

            Cv2.Split(img, out Mat[] rgbLayer);         // seperate channels
            Mat alpha = new Mat();
            rgbLayer[3].CopyTo(alpha); // Preserve Alpha Channel
            Mat[] cs = { rgbLayer[0], rgbLayer[1], rgbLayer[2] };
            Mat img1 = new Mat();
            Cv2.Merge(cs, img1);        // glue together again
                       
            Mat tmp = new Mat();            
            tmp = img1.CvtColor(ColorConversionCodes.BGRA2GRAY);

            Mat mask = new Mat();
            mask = tmp.InRange(new Scalar(200,0,0), new Scalar(255, 255, 255));

            img1.SetTo(newColor, mask);            
            Mat dst = new Mat(img.Size(), MatType.CV_8UC4);
            Cv2.Split(img1, out Mat[] rgbLayer1);         // seperate channels
            Mat[] cs1 = { rgbLayer1[0], rgbLayer1[1], rgbLayer1[2], rgbLayer[3] };
            Cv2.Merge(cs1, dst);            

            //Cv2.ImShow("mask", mask);
            //Cv2.ImShow("tmp", tmp);
            //Cv2.ImShow("img", img);            

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
