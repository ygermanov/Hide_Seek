using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hide_Seek.Librarys
{
    public static class PictureManipulations
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }
        public static void CopyPixels(
            this BitmapSource source,
            PixelColor[,] pixels,
            int stride,
            int offset)
        {
            //ensuring we are using the right image format for working
            if (source.Format != PixelFormats.Bgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }
            int height = source.PixelHeight;
            int width = source.PixelWidth;
            byte[] pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            //converting pixels from a single array to matrix array for easy use
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x + x0, y + y0] = new PixelColor
                    {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
                }
            }
        }
        //Writing pixels back to Image is using vector array so we need 
        //to convert the matrix array back to vector array
        public static byte[] RevertToSingleArray(PixelColor[,] pixel, int offset)
        {
            int width = pixel.GetLength(0);
            int height = pixel.GetLength(1);
            byte[] pixelBytes = new byte[width * height * 4];
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixelBytes[(y * width + x) * 4 + 0] = pixel[x + x0, y + y0].Blue;
                    pixelBytes[(y * width + x) * 4 + 1] = pixel[x + x0, y + y0].Green;
                    pixelBytes[(y * width + x) * 4 + 2] = pixel[x + x0, y + y0].Red;
                    pixelBytes[(y * width + x) * 4 + 3] = pixel[x + x0, y + y0].Alpha;

                }
            }
            return pixelBytes;
        }
        public static void PutPixels(
            WriteableBitmap bitmap,
            byte[] pixels,
            int width,
            int height)
        {
            bitmap.WritePixels(
                new Int32Rect(0, 0, width, height),
                pixels,
                width * 4,
                0);
        }
        //LSB method of adding string to a pixels in Image
        public static PixelColor[,] ChangePixels(string text, PixelColor[,] pixels)
        {
            int charValue = 0;
            int charIndex = 0;
            State state = State.Hiding; //this is needed to mark the end of the string we need to hide
            int checkSum = 0; //counting how many 0 we add at the end of the message
            long elementsCount = 0; //bits count used for every character
            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                for (int j = 0; j < pixels.GetLength(1); j++)
                {
                    int r = pixels[i, j].Red - pixels[i, j].Red % 2;
                    int g = pixels[i, j].Green - pixels[i, j].Green % 2;
                    int b = pixels[i, j].Blue - pixels[i, j].Blue % 2;

                    for (int n = 0; n < 3; n++)
                    {
                        if (elementsCount % 8 == 0)
                        {
                            //end point when we are finished with hiding characters
                            if (state == State.Filling_With_Zeros == true && checkSum == 8)
                            {
                                if ((elementsCount - 1) % 3 < 2)
                                {
                                    pixels[i, j].Red = (byte)r;
                                    pixels[i, j].Green = (byte)g;
                                    pixels[i, j].Blue = (byte)b;
                                }
                                return pixels;
                            }
                            //changing the state when we are done with the text
                            if (charIndex >= text.Length)
                            {
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        if (state == State.Hiding)
                        {
                            //adding the text bits into the pixels
                            switch (elementsCount % 3)
                            {
                                case 0:
                                    r += charValue % 2;
                                    charValue /= 2;
                                    break;
                                case 1:
                                    g += charValue % 2;
                                    charValue /= 2;
                                    break;
                                case 2:
                                    b += charValue % 2;
                                    charValue /= 2;
                                    pixels[i, j].Red = (byte)r;
                                    pixels[i, j].Green = (byte)g;
                                    pixels[i, j].Blue = (byte)b;
                                    break;
                            }
                        }
                        else
                        {
                            checkSum++;
                            //ensuring the pixel updates with the new values when we 
                            //are to switch to next pixel
                            if (elementsCount % 3 == 2)
                            {
                                pixels[i, j].Red = (byte)r;
                                pixels[i, j].Green = (byte)g;
                                pixels[i, j].Blue = (byte)b;
                            }
                        }
                        elementsCount++;
                    }
                }
            }

            return pixels;
        }
        //extracting the text out of the Image
        public static string ExtractText(PixelColor[,] pixel)
        {
            string tempString = String.Empty;
            int charValue = 0; //check sum for finding the end of the message
            int elemetsCount = 0; //bits count used for every character
            int mod = 0;
            for (int i = 0; i < pixel.GetLength(0); i++)
            {

                for (int j = 0; j < pixel.GetLength(1); j++)
                {
                    for (int n = 0; n < 3; n++)
                    {
                        //extracting text bits out of the pixels
                        switch (elemetsCount % 3)
                        {
                            case 0:
                                charValue += (int)Math.Pow(2, mod) * (pixel[i, j].Red % 2);
                                break;
                            case 1:
                                charValue += (int)Math.Pow(2, mod) * (pixel[i, j].Green % 2);
                                break;
                            case 2:
                                charValue += (int)Math.Pow(2, mod) * (pixel[i, j].Blue % 2);
                                break;
                        }
                        elemetsCount++;
                        mod++;
                        if (mod == 8) mod = 0;
                        if (elemetsCount % 8 == 0)
                        {
                            //if we reach end of the message before 
                            //we run though all of the pixels
                            if (charValue == 0)
                            {
                                return tempString;
                            }

                            char t = (char)charValue;
                            tempString += t.ToString();
                            charValue = 0;
                        }
                    }
                }
            }

            return tempString;
        }
    }
}
