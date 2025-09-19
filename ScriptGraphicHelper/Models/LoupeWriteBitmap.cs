using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;

namespace ScriptGraphicHelper.Models
{
    public static class LoupeWriteBitmap
    {
        private static WriteableBitmap Bitmap { get; set; }
        public static WriteableBitmap Init(int width, int height)
        {
            Bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);

            using (var bmpData = Bitmap.Lock())
            {
                unsafe
                {
                    var ptr = (byte*)bmpData.Address;
                    for (var j = 0; j < 16; j++)
                    {
                        for (var i = 0; i < 241; i++)
                        {
                            var k = j * 16 * bmpData.RowBytes + i * 4;
                            ptr[k] = 75;
                            ptr[k + 1] = 75;
                            ptr[k + 2] = 75;
                            ptr[k + 3] = 255;
                        }
                    }
                    for (var j = 0; j < 16; j++)
                    {
                        for (var i = 0; i < 241; i++)
                        {
                            var k = i * bmpData.RowBytes + j * 16 * 4;
                            ptr[k] = 75;
                            ptr[k + 1] = 75;
                            ptr[k + 2] = 75;
                            ptr[k + 3] = 255;
                        }
                    }
                }

            }

            return Bitmap;
        }

        private static void ChangeHighLight(WriteableBitmap bmp, byte[] highLightColor)
        {
            using (var bmpData = Bitmap.Lock())
            {
                unsafe
                {
                    var ptr = (byte*)bmpData.Address;
                    for (var j = 0; j < 16; j++)
                    {
                        for (var i = 0; i < 241; i++)
                        {
                            var k = j * 16 * bmpData.RowBytes + i * 4;
                            if (i >= 110 && i <= 130 && j <= 8 && j >= 7)
                            {
                                ptr[k] = highLightColor[0];
                                ptr[k + 1] = highLightColor[1];
                                ptr[k + 2] = highLightColor[2];
                                ptr[k + 3] = 255;

                            }
                        }
                    }
                    for (var j = 0; j < 16; j++)
                    {
                        for (var i = 0; i < 241; i++)
                        {
                            var k = i * bmpData.RowBytes + j * 16 * 4;
                            if (i >= 110 && i <= 130 && j <= 8 && j >= 7)
                            {
                                ptr[k] = highLightColor[0];
                                ptr[k + 1] = highLightColor[1];
                                ptr[k + 2] = highLightColor[2];
                                ptr[k + 3] = 255;
                            }
                        }
                    }
                }
            }
        }

        private static int IsOffsetSame(List<byte[]> colors)
        {
            var result = 24;
            double similarity = 12;
            var color = colors[7 * 15 + 7];
            var offsetIndex = new List<byte[]>
            {
                new byte[]{ 6, 7},
                new byte[]{ 6, 8},
                new byte[]{ 7, 8},
                new byte[]{ 8, 8},
                new byte[]{ 8, 7},
                new byte[]{ 8, 6},
                new byte[]{ 7, 6},
                new byte[]{ 6, 6},
                new byte[]{ 5, 6},
                new byte[]{ 5, 7},
                new byte[]{ 5, 8},
                new byte[]{ 5, 9},
                new byte[]{ 6, 9},
                new byte[]{ 7, 9},
                new byte[]{ 8, 9},
                new byte[]{ 9, 9},
                new byte[]{ 9, 8},
                new byte[]{ 9, 7},
                new byte[]{ 9, 6},
                new byte[]{ 9, 5},
                new byte[]{ 8, 5},
                new byte[]{ 7, 5},
                new byte[]{ 6, 5},
                new byte[]{ 5, 5},
            };
            for (var i = 0; i < 24; i++)
            {
                var offsetColor = colors[offsetIndex[i][0] * 15 + offsetIndex[i][1]];
                if (Math.Abs(color[0] - offsetColor[0]) > similarity || Math.Abs(color[1] - offsetColor[1]) > similarity || Math.Abs(color[2] - offsetColor[2]) > similarity)
                {
                    result = i;
                    break;
                }
            }
            if (result == 24)
                return 2;
            else if (result >= 8)
                return 1;
            else
                return 0;
        }
        public static bool WriteColor(this WriteableBitmap bmp, List<byte[]> colors)
        {
            var highLightColor = new List<byte[]>
            {
            new byte[]{ 60, 20, 220 },
            new byte[]{ 0x1A, 0xB1, 0xF9 },
            new byte[]{ 0x14, 0xB8, 0x6E }
            };
            var offsetRange = IsOffsetSame(colors);
            ChangeHighLight(bmp, highLightColor[offsetRange]);
            using (var bmpData = Bitmap.Lock())
            {
                unsafe
                {
                    var ptr = (byte*)bmpData.Address;
                    for (var y = 0; y < 15; y++)
                    {
                        for (var x = 0; x < 15; x++)
                        {
                            var color = colors[y * 15 + x];
                            for (var i = y * 16 + 1; i < y * 16 + 16; i++)
                            {
                                for (var j = x * 16 + 1; j < x * 16 + 16; j++)
                                {
                                    var k = i * bmpData.RowBytes + j * 4;
                                    if (i >= 110 && i <= 130 && ((j >= 110 && j <= 112) || (j >= 128 && j <= 130)))
                                    {
                                        ptr[k] = highLightColor[offsetRange][0];
                                        ptr[k + 1] = highLightColor[offsetRange][1];
                                        ptr[k + 2] = highLightColor[offsetRange][2];
                                        ptr[k + 3] = 255;
                                    }
                                    else if (j >= 110 && j <= 130 && ((i >= 110 && i <= 112) || (i >= 128 && i <= 130)))
                                    {
                                        ptr[k] = highLightColor[offsetRange][0];
                                        ptr[k + 1] = highLightColor[offsetRange][1];
                                        ptr[k + 2] = highLightColor[offsetRange][2];
                                        ptr[k + 3] = 255;
                                    }
                                    else
                                    {
                                        ptr[k] = color[2];
                                        ptr[k + 1] = color[1];
                                        ptr[k + 2] = color[0];
                                        ptr[k + 3] = 255;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
