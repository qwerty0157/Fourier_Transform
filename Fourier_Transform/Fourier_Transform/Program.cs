using System;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.IO;

namespace Fourier_Transform
{
    class Program
    {
        static void Main(string[] args)
        {
            //var dir = Directory.GetCurrentDirectory();
            //Console.WriteLine(dir);
            //var image = (Bitmap)Image.FromFile("../../../../input/wtnbyou.png");

            // 画像読み込み
            //var loadImage = ImageTransformByFFT.LoadByteImage(image);
            //byte[,] loadImage = ImageTransformByFFT.LoadByteImage("wtnbyou.bmp");
            var loadImage = ImageTransformByFFT.LoadByteImage("../../../../input/you.jpg");

            // 2次元フーリエ変換を行う
            var filterdata_2D = ImageTransformByFFT.FrequencyFiltering(loadImage);

            // 画像出力
            ImageTransformByFFT.SaveByteImage(filterdata_2D, "../../../../output/output.bmp");
        }
    }
}
