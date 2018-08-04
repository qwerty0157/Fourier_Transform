using System;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using AForge.Imaging;

namespace Fourier_Transform
{
    public class ImageFFT
    {
        public ImageFFT()
        {
        }

        public static void FourierTranstormForImage(Bitmap image)
        {
            // 入力画像
            var img = image;
            // 画像の解像度が2のべき乗になるように調整
            var bitWidth = Convert.ToString(img.Width - 1, 2).Length;
            var bitHeight = Convert.ToString(img.Height - 1, 2).Length;
            var width = (int)Math.Pow(2, bitWidth);
            var height = (int)Math.Pow(2, bitHeight);
            // 隙間の部分はゼロ埋め
            var imgPadded = new Bitmap(width, height, img.PixelFormat);
            var graphics = Graphics.FromImage(imgPadded);
            graphics.DrawImage(img, 0, 0);
            graphics.Dispose();
            // グレースケール化
            var gray = GrayScale.CreateGrayscaleImage(imgPadded);
            // 高速フーリエ変換

            /*
            var complex = ComplexImage.FromBitmap(gray);
            complex.ForwardFourierTransform();
            // 保存
            Bitmap img2 = complex.ToBitmap();
            img2.Save(@"output.jpg");
            img2.Dispose();
            */
        }
    }
}
