using System;
using System.Numerics;

namespace Fourier_Transform
{
    class Program
    {
        static void Main(string[] args)
        {
            //ImageFFT();
            FastFourierTransformTest();

        }

        public static void ImageFFT()
        {
            // 画像読み込み
            //var loadImage = ImageTransformByFFT.LoadByteImage(image);
            //byte[,] loadImage = ImageTransformByFFT.LoadByteImage("wtnbyou.bmp");
            var loadImage = ImageTransformByFFT.LoadByteImage("../../../../input/you.jpg");

            // 2次元フーリエ変換を行う
            var filterdata_2D = ImageTransformByFFT.FrequencyFiltering(loadImage);

            // 画像出力
            ImageTransformByFFT.SaveByteImage(filterdata_2D, "../../../../output/output.bmp");   
        }

        public static void FastFourierTransformTest()
        {
            const int size = 256;

            double[] data = new double[size];
            double[] re;    // real part
            double[] im = new double[size];    // imaginary part

            // 初期化、適当な値でテスト
            for (int i = 0; i < size; i++)
            {
                var rnd = new Random();
                data[i] = rnd.Next(10);
            }

            // 高速フーリエ変換、データが256個なのでbitSizeは8を指定
            FFT.FastFourierTransform(data, im, out re, out im, (int)Math.Log(size, 2));

            // 出力
            for (int i = 0; i < size; i++)
            {
                Console.WriteLine("{0} {1} {2}", data[i], re[i], im[i]);
            }
        }
    }
}
