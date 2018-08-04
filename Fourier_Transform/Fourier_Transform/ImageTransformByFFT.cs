using System;
using System.DrawingCore;
using System.DrawingCore.Imaging;

namespace Fourier_Transform
{
    public class ImageTransformByFFT
    {
        public ImageTransformByFFT()
        {
        }

        /// <summary>
        /// Bitmapをロードしbyte[,]配列で返す
        /// </summary>
        public static byte[,] LoadByteImage(string filename)
        {
            try
            {
                Bitmap bitmap = new Bitmap(filename);
                byte[,] data = new byte[bitmap.Width, bitmap.Height];

                // bitmapクラスの画像ピクセル値を配列に挿入
                for (int i = 0; i < bitmap.Height; i++)
                {
                    for (int j = 0; j < bitmap.Width; j++)
                    {
                        // ここではグレイスケールに変換して格納
                        data[j, i] =
                            (byte)(
                                (bitmap.GetPixel(j, i).R +
                                bitmap.GetPixel(j, i).B +
                                bitmap.GetPixel(j, i).G) / 3
                                );
                    }
                }
                return data;
            }
            catch
            {
                Console.WriteLine("ファイルが読めません。");
                return null;
            }
        }

        /// <summary>
        /// 画像配列をbmpに書き出す
        /// </summary>
        public static void SaveByteImage(byte[,] data, string filename)
        {
            try
            {
                // 縦横サイズを配列から読み取り
                int xsize = data.GetLength(0);
                int ysize = data.GetLength(1);
                Bitmap bitmap = new Bitmap(xsize, ysize);

                // bitmapクラスのSetPixelでbitmapオブジェクトに
                // ピクセル値をセット
                for (int i = 0; i < ysize; i++)
                {
                    for (int j = 0; j < xsize; j++)
                    {
                        bitmap.SetPixel(
                            j,
                            i,
                            Color.FromArgb(
                                data[j, i],
                                data[j, i],
                                data[j, i])
                            );
                    }
                }
                // 画像の保存
                bitmap.Save(filename, ImageFormat.Bmp);
            }
            catch
            {
                Console.WriteLine("ファイルが書き込めません。");
            }
        }

        /// <summary>
        /// 周波数フィルタリング
        /// </summary>
        public static byte[,] FrequencyFiltering(byte[,] image)
        {
            try
            {
                // サイズ取得
                int xSize = image.GetLength(0);
                int ySize = image.GetLength(1);
                // double型配列にする
                double[,] data = ByteToDouble2D(image);
                double[,] re;
                double[,] im = new double[xSize, ySize];
                // 2次元フーリエ変換
                FFT2D(data, im, out re, out im, xSize, ySize);
                // 2次元逆フーリエ変換　周波数画像からもとの空間画像に戻す
                IFFT2D(re, im, out data, out im, xSize, ySize);
                // byte型配列に戻す
                image = DoubleToByte2D(re);
                return image;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// byte型2次元配列からdouble型2次元配列に変換
        /// </summary>
        public static double[,] ByteToDouble2D(byte[,] data)
        {
            try
            {
                // サイズ取得
                int xSize = data.GetLength(0);
                int ySize = data.GetLength(1);



                double[,] convdata = new double[xSize, ySize];
                for (int i = 0; i < ySize; i++)
                {
                    for (int j = 0; j < xSize; j++)
                    {
                        convdata[j, i] = (double)data[j, i];
                    }
                }
                return convdata;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// byte型2次元配列からdouble型2次元配列に変換
        /// </summary>
        public static byte[,] DoubleToByte2D(double[,] data)
        {
            try
            {
                // サイズ取得
                int xSize = data.GetLength(0);
                int ySize = data.GetLength(1);


                byte[,] convdata = new byte[xSize, ySize];
                for (int i = 0; i < ySize; i++)
                {
                    for (int j = 0; j < xSize; j++)
                    {
                        if (data[j, i] >= 255)
                        {
                            convdata[j, i] = 255;
                        }
                        else if (data[j, i] < 0)
                        {
                            convdata[j, i] = 0;
                        }
                        else
                        {
                            convdata[j, i] = (byte)data[j, i];
                        }
                    }
                }
                return convdata;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 1次元FFT
        /// </summary>
        public static void FFT(
            double[] inputRe,
            double[] inputIm,
            out double[] outputRe,
            out double[] outputIm,
            int bitSize)
        {
            int dataSize = 1 << bitSize;
            int[] reverseBitArray = BitScrollArray(dataSize);

            outputRe = new double[dataSize];
            outputIm = new double[dataSize];

            // バタフライ演算のための置き換え
            for (int i = 0; i < dataSize; i++)
            {
                outputRe[i] = inputRe[reverseBitArray[i]];
                outputIm[i] = inputIm[reverseBitArray[i]];
            }

            // バタフライ演算
            for (int stage = 1; stage <= bitSize; stage++)
            {
                int butterflyDistance = 1 << stage;
                int numType = butterflyDistance >> 1;
                int butterflySize = butterflyDistance >> 1;
                double wRe = 1.0;
                double wIm = 0.0;
                double uRe =
                    System.Math.Cos(System.Math.PI / butterflySize);
                double uIm =
                    -System.Math.Sin(System.Math.PI / butterflySize);

                for (int type = 0; type < numType; type++)
                {
                    for (int j = type; j < dataSize; j += butterflyDistance)
                    {
                        int jp = j + butterflySize;
                        double tempRe =
                            outputRe[jp] * wRe - outputIm[jp] * wIm;
                        double tempIm =
                            outputRe[jp] * wIm + outputIm[jp] * wRe;
                        outputRe[jp] = outputRe[j] - tempRe;
                        outputIm[jp] = outputIm[j] - tempIm;
                        outputRe[j] += tempRe;
                        outputIm[j] += tempIm;
                    }
                    double tempWRe = wRe * uRe - wIm * uIm;
                    double tempWIm = wRe * uIm + wIm * uRe;
                    wRe = tempWRe;
                    wIm = tempWIm;
                }
            }
        }

        /// <summary>
        /// 1次元IFFT
        /// </summary>
        public static void IFFT(
            double[] inputRe,
            double[] inputIm,
            out double[] outputRe,
            out double[] outputIm,
            int bitSize)
        {
            int dataSize = 1 << bitSize;
            outputRe = new double[dataSize];
            outputIm = new double[dataSize];
            for (int i = 0; i < dataSize; i++)
            {
                inputIm[i] = -inputIm[i];
            }
            FFT(inputRe, inputIm, out outputRe, out outputIm, bitSize);
            for (int i = 0; i < dataSize; i++)
            {
                outputRe[i] /= (double)dataSize;
                outputIm[i] /= (double)(-dataSize);
            }
        }

        // ビットを左右反転した配列を返す
        private static int[] BitScrollArray(int arraySize)
        {
            int[] reBitArray = new int[arraySize];
            int arraySizeHarf = arraySize >> 1;



            reBitArray[0] = 0;
            for (int i = 1; i < arraySize; i <<= 1)
            {
                for (int j = 0; j < i; j++)
                    reBitArray[j + i] = reBitArray[j] + arraySizeHarf;
                arraySizeHarf >>= 1;
            }
            return reBitArray;
        }

        /// <summary>
        /// 2次元FFT
        /// </summary>
        /// <param name="inDataR">実数入力部
        /// <param name="inDataI">虚数入力部
        /// <param name="outDataR">実数出力部
        /// <param name="outDataI">虚数出力部
        /// <param name="xSize">x方向サイズ
        /// <param name="ySize">y方向サイズ
        public static void FFT2D(double[,] inDataRe, double[,] inDataIm, out double[,] outDataRe, out double[,] outDataIm, int xSize, int ySize)
        {
            double[,] tempRe = new double[ySize, xSize];
            double[,] tempIm = new double[ySize, xSize];
            int xbit = GetBitNum(xSize);
            int ybit = GetBitNum(ySize);
            outDataRe = new double[xSize, ySize];
            outDataIm = new double[xSize, ySize];

            for (int j = 0; j < ySize; j++)
            {
                double[] re = new double[xSize];
                double[] im = new double[xSize];
                FFT(
                    GetArray(inDataRe, j),
                    GetArray(inDataIm, j),
                    out re, out im, xbit);



                for (int i = 0; i < xSize; i++)
                {
                    tempRe[j, i] = re[i];
                    tempIm[j, i] = im[i];
                }
            }

            for (int i = 0; i < xSize; i++)
            {
                double[] re = new double[ySize];
                double[] im = new double[ySize];
                FFT(
                    GetArray(tempRe, i),
                    GetArray(tempIm, i),
                    out re, out im, ybit);
                
                for (int j = 0; j < ySize; j++)
                {
                    outDataRe[i, j] = re[j];
                    outDataIm[i, j] = im[j];
                }
            }
        }

        // ビット数取得
        private static int GetBitNum(int num)
        {
            int bit = -1;
            while (num > 0)
            {
                num >>= 1;
                bit++;
            }
            return bit;
        }
        // 1次元配列取り出し
        private static double[] GetArray(double[,] data2D, int seg)
        {
            double[] reData = new double[data2D.GetLength(0)];
            for (int i = 0; i < data2D.GetLength(0); i++)
            {
                reData[i] = data2D[i, seg];
            }
            return reData;
        }

        /// <summary>
        /// 2次元IFFT
        /// </summary>
        /// <param name="inDataR">実数入力部
        /// <param name="inDataI">虚数入力部
        /// <param name="outDataR">実数出力部
        /// <param name="outDataI">虚数出力部
        /// <param name="xSize">x方向サイズ
        /// <param name="ySize">y方向サイズ
        public static void IFFT2D(double[,] inDataRe, double[,] inDataIm, out double[,] outDataRe,
            out double[,] outDataIm,
            int xSize,
            int ySize)
        {
            double[,] tempRe = new double[ySize, xSize];
            double[,] tempIm = new double[ySize, xSize];
            int xbit = GetBitNum(xSize);
            int ybit = GetBitNum(ySize);
            outDataRe = new double[xSize, ySize];
            outDataIm = new double[xSize, ySize];

            for (int j = 0; j < ySize; j++)
            {
                double[] re = new double[xSize];
                double[] im = new double[xSize];
                IFFT(
                    GetArray(inDataRe, j),
                    GetArray(inDataIm, j),
                    out re, out im, xbit);

                for (int i = 0; i < xSize; i++)
                {
                    tempRe[j, i] = re[i];
                    tempIm[j, i] = im[i];
                }
            }

            for (int i = 0; i < xSize; i++)
            {
                double[] re = new double[ySize];
                double[] im = new double[ySize];
                IFFT(
                    GetArray(tempRe, i),
                    GetArray(tempIm, i),
                    out re, out im, ybit);


                for (int j = 0; j < ySize; j++)
                {
                    outDataRe[i, j] = re[j];
                    outDataIm[i, j] = im[j];
                }
            }
        }
    }
}
