using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace SumoApplication.Helpers
{
    class ImageManipulation
    {
        public static Mat Decorate(Mat RawImage, SumoInformations sumoInformations)
        {
            var dst = new Mat();
            
            RawImage.CopyTo(dst);

            // Write Battery Level
            if (!string.IsNullOrWhiteSpace(sumoInformations.BatteryLevelStr))
                Cv2.PutText(dst, sumoInformations.BatteryLevelStr, new Point(5, 30), HersheyFonts.HersheyTriplex, 1.0, Scalar.Red);

            // Write RSSI
            if (!string.IsNullOrWhiteSpace(sumoInformations.WifiRssiStr))
                Cv2.PutText(dst, sumoInformations.WifiRssiStr, new Point(100, 30), HersheyFonts.HersheyTriplex, 1.0, Scalar.Red);

            // Write Link Quality
            if (!string.IsNullOrWhiteSpace(sumoInformations.WifiQualityStr))
                Cv2.PutText(dst, sumoInformations.WifiQualityStr, new Point(300, 30), HersheyFonts.HersheyTriplex, 1.0, Scalar.Red);

            // Write Alert
            if (!string.IsNullOrWhiteSpace(sumoInformations.AlerteStr))
                Cv2.PutText(dst, sumoInformations.AlerteStr, new Point(450, 30), HersheyFonts.HersheyTriplex, 1.0, Scalar.Red);

            // Write Low Battery Level Alerte
            if (sumoInformations.IsBatteryUnderLevelAlert)
                Cv2.PutText(dst, "Low Battery Level", new Point(100, 300), HersheyFonts.HersheyTriplex, 4.0, Scalar.Red);

            // Write posture logo
            //Mat logo = Cv2.ImRead("", ImreadModes.AnyColor);                                                
            //var roi = new Mat(dst, new Rect(100, 10, logo.Width, logo.Height));
            //logo.CopyTo(roi);

            return dst;
            
        }
    }
}
