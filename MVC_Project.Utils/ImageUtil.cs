using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace PlataformaSWC.Utils
{
    public class ImageUtil
    {
        public static byte[] DownloadRemoteImageFile(string uri)
        {
            byte[] content;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create(uri);

            using (var response = request.GetResponse())
            using (var reader = new BinaryReader(response.GetResponseStream()))
            {
                content = reader.ReadBytes(100000);
            }

            return content;
        }

        public static string ConvertImageToBase64(byte[] imgBytes)
        {
            Image original;
            byte[] newImgBytes;

            using (var ms = new MemoryStream(imgBytes))
            {
                original = Image.FromStream(ms);
            }

            Bitmap newPic = ScaleImage(original, 250, 250);
            Graphics gr = Graphics.FromImage(newPic);

            using(MemoryStream ms = new MemoryStream())
            {
                gr.DrawImage(original, 0, 0, newPic.Width, newPic.Height);
                newPic.Save(ms, ImageFormat.Jpeg);
                newImgBytes = ms.ToArray();

                return Convert.ToBase64String(newImgBytes);
            }
        }

        public static MemoryStream ConvertImageToMemoryStream(byte[] imgBytes)
        {
            Image original;
            MemoryStream result = new MemoryStream();

            using (var ms = new MemoryStream(imgBytes))
            {
                original = Image.FromStream(ms);
            }

            Bitmap newPic = ScaleImage(original, 250, 250);
            Graphics gr = Graphics.FromImage(newPic);

            gr.DrawImage(original, 0, 0, newPic.Width, newPic.Height);
            newPic.Save(result, ImageFormat.Jpeg);

            return result;
            
        }

        private static int ScaleWidth(int originalHeight, int newHeight, int originalWidth)
        {
            var scale = Convert.ToDouble(newHeight) / Convert.ToDouble(originalHeight);

            return Convert.ToInt32(originalWidth * scale);
        }

        public static Bitmap ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            Bitmap bmp = new Bitmap(newImage);

            return bmp;
        }
    }
}
