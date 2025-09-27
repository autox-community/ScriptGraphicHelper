using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using ScriptGraphicHelper.Models;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Bitmap = System.Drawing.Bitmap;
using Color = Avalonia.Media.Color;

namespace ScriptGraphicHelper.Helpers
{
    public static class ImgEditorHelper
    {
        private static int Width;
        private static int Height;
        private static int RowStride;
        private static List<byte> Data = new();

        public static WriteableBitmap Init(MyRange range, byte[] data)
        {
            Width = (int)(range.Right - range.Left + 1);
            Height = (int)(range.Bottom - range.Top + 1);
            Data = data.ToList();
            WriteableBitmap drawBitmap = new(new PixelSize(Width, Height), new Vector(96, 96), GraphicHelper.PxFormat, AlphaFormat.Premul);
            var drawBmpData = drawBitmap.Lock();
            unsafe
            {
                var ptr = (byte*)drawBmpData.Address;
                for (var j = 0; j < Height; j++)
                {
                    var k = j * drawBmpData.RowBytes;
                    for (var i = 0; i < Width; i++, k += 4)
                    {
                        ptr[k] = data[k];
                        ptr[k + 1] = data[k + 1];
                        ptr[k + 2] = data[k + 2];
                        ptr[k + 3] = data[k + 3];
                    }
                }
            }
            RowStride = drawBmpData.RowBytes;
            drawBmpData.Dispose();
            return drawBitmap;
        }

        public static async Task SetPixel(this WriteableBitmap drawBitmap, int x, int y, Color color)
        {
            await Task.Run(() =>
            {
                if (x >= 0 && y >= 0 && x < Width && y < Height)
                {
                    using var drawBmpData = drawBitmap.Lock();
                    unsafe
                    {
                        var ptr = (byte*)drawBmpData.Address;
                        var k = y * drawBmpData.RowBytes + x * 4;
                        ptr[k] = color.B;
                        ptr[k + 1] = color.G;
                        ptr[k + 2] = color.R;
                        ptr[k + 3] = color.A;
                    }
                }
            });
        }

        public static async Task<Color> GetPixel(this WriteableBitmap drawBitmap, int x, int y)
        {
            return await Task.Run(() =>
             {
                 if (x >= 0 && y >= 0 && x < Width && y < Height)
                 {
                     using var drawBmpData = drawBitmap.Lock();
                     unsafe
                     {
                         var ptr = (byte*)drawBmpData.Address;
                         var k = y * drawBmpData.RowBytes + x * 4;
                         return Color.FromRgb(ptr[k + 2], ptr[k + 1], ptr[k]);
                     }
                 }
                 return Colors.White;
             });
        }

        public static async void SetPixels(this WriteableBitmap drawBitmap, Color src, Color dest, int offset, bool reverse)
        {

            var srcR = src.R; var srcG = src.G; var srcB = src.B; var srcA = src.A;
            var destR = dest.R; var destG = dest.G; var destB = dest.B; var destA = dest.A;
            var similarity = (int)(255 - 255 * ((100 - offset) / 100.0));

            await Task.Run(() =>
            {
                if (reverse)
                {
                    var step = 0;
                    var drawBmpData = drawBitmap.Lock();
                    unsafe
                    {
                        var ptr = (byte*)drawBmpData.Address;
                        for (var i = 0; i < Height; i++)
                        {
                            for (var j = 0; j < Width; j++)
                            {
                                if (Math.Abs(ptr[step] - srcB) > similarity && Math.Abs(ptr[step + 1] - srcG) > similarity && Math.Abs(ptr[step + 2] - srcR) > similarity)
                                {
                                    ptr[step] = destB;
                                    ptr[step + 1] = destG;
                                    ptr[step + 2] = destR;
                                    ptr[step + 3] = destA;
                                }
                                step += 4;
                            }
                        }
                    }
                    drawBmpData.Dispose();
                }
                else
                {
                    var step = 0;
                    var drawBmpData = drawBitmap.Lock();
                    unsafe
                    {
                        var ptr = (byte*)drawBmpData.Address;
                        for (var i = 0; i < Height; i++)
                        {
                            for (var j = 0; j < Width; j++)
                            {
                                if (Math.Abs(ptr[step] - srcB) <= similarity && Math.Abs(ptr[step + 1] - srcG) <= similarity && Math.Abs(ptr[step + 2] - srcR) <= similarity)
                                {
                                    ptr[step] = destB;
                                    ptr[step + 1] = destG;
                                    ptr[step + 2] = destR;
                                    ptr[step + 3] = destA;
                                }
                                step += 4;
                            }
                        }
                    }
                    drawBmpData.Dispose();
                }
            });
        }


        public static int StartX { get; set; } = 0;
        public static int StartY { get; set; } = 0;

        public static WriteableBitmap CutImg(this WriteableBitmap drawBitmap)
        {
            var drawBmpData = drawBitmap.Lock();
            unsafe
            {
                var ptr = (byte*)drawBmpData.Address;
                var site = 0;
                var ltColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                site = (Width - 1) * 4;
                var rtColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                site = (Height - 1) * RowStride;
                var lbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                site = (Height - 1) * RowStride + (Width - 1) * 4;
                var rbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                MyRange range = new(0, 0, Width - 1, Height - 1);
                if (ltColor.SequenceEqual(rtColor) && ltColor.SequenceEqual(lbColor) && ltColor.SequenceEqual(rbColor))
                {
                    for (var i = 0; i < Height; i++)
                    {
                        var num = 0;
                        var location = i * RowStride;
                        for (var j = 0; j < Width; j++, location += 4)
                        {
                            if (ptr[location] == ltColor[0] && ptr[location + 1] == ltColor[1] && ptr[location + 2] == ltColor[2])
                            {
                                num++;
                            }
                        }
                        if (num != Width)
                        {
                            range.Top = i > 0 ? i - 1 : 0;
                            break;
                        }
                    }

                    for (var i = Height - 1; i >= 0; i--)
                    {
                        var num = 0;
                        var location = i * RowStride;
                        for (var j = 0; j < Width; j++, location += 4)
                        {
                            if (ptr[location] == ltColor[0] && ptr[location + 1] == ltColor[1] && ptr[location + 2] == ltColor[2])
                            {
                                num++;
                            }
                        }
                        if (num != Width)
                        {
                            range.Bottom = i < Height - 1 ? i + 1 : Height - 1;
                            break;
                        }
                    }

                    for (var i = 0; i < Width; i++)
                    {
                        var num = 0;
                        for (var j = 0; j < Height; j++)
                        {
                            var location = j * RowStride + i * 4;
                            if (ptr[location] == ltColor[0] && ptr[location + 1] == ltColor[1] && ptr[location + 2] == ltColor[2])
                            {
                                num++;
                            }
                            location += 4;
                        }
                        if (num != Height)
                        {
                            range.Left = i > 0 ? i - 1 : 0;
                            break;
                        }
                    }

                    for (var i = Width - 1; i >= 0; i--)
                    {
                        var num = 0;
                        for (var j = 0; j < Height; j++)
                        {
                            var location = j * RowStride + i * 4;
                            if (ptr[location] == ltColor[0] && ptr[location + 1] == ltColor[1] && ptr[location + 2] == ltColor[2])
                            {
                                num++;
                            }
                            location += 4;
                        }
                        if (num != Height)
                        {
                            range.Right = i < Width - 1 ? i + 1 : Width - 1;
                            break;
                        }
                    }
                }
                drawBmpData.Dispose();
                return drawBitmap.CutImg(range);
            }
        }

        public static WriteableBitmap CutImg(this WriteableBitmap drawBitmap, MyRange range)
        {
            var left = (int)range.Left;
            var top = (int)range.Top;
            var right = (int)range.Right;
            var bottom = (int)range.Bottom;

            StartX += left;
            StartY += top;

            Width = right - left + 1;
            Height = bottom - top + 1;
            var bitmap = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul);
            var bmpData = bitmap.Lock();
            var drawBmpData = drawBitmap.Lock();
            unsafe
            {
                var bmpPtr = (byte*)bmpData.Address;
                var rawBmpPtr = (byte*)drawBmpData.Address;

                var step = 0;
                for (var j = top; j <= bottom; j++)
                {
                    var k = j * drawBmpData.RowBytes + left * 4;
                    for (var i = left; i <= right; i++, k += 4, step += 4)
                    {
                        bmpPtr[step] = rawBmpPtr[k];
                        bmpPtr[step + 1] = rawBmpPtr[k + 1];
                        bmpPtr[step + 2] = rawBmpPtr[k + 2];
                        bmpPtr[step + 3] = rawBmpPtr[k + 3];
                    }
                }
            }

            Data.Clear();
            var data = new byte[Width * Height * 4];
            Marshal.Copy(bmpData.Address, data, 0, data.Length);
            Data = data.ToList();

            RowStride = bmpData.RowBytes;
            drawBmpData.Dispose();
            bmpData.Dispose();
            return bitmap;
        }

        public static WriteableBitmap ResetImg()
        {
            var bitmap = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul);
            var drawBmpData = bitmap.Lock();
            unsafe
            {
                var ptr = (byte*)drawBmpData.Address;
                for (var j = 0; j < Height; j++)
                {
                    var k = j * drawBmpData.RowBytes;
                    for (var i = 0; i < Width; i++, k += 4)
                    {
                        ptr[k] = Data[k];
                        ptr[k + 1] = Data[k + 1];
                        ptr[k + 2] = Data[k + 2];
                        ptr[k + 3] = Data[k + 3];
                    }
                }
            }
            RowStride = drawBmpData.RowBytes;
            drawBmpData.Dispose();
            return bitmap;
        }

        public static Bitmap GetBitmap(this WriteableBitmap drawBitmap)
        {
            drawBitmap.CutImg(new MyRange(0, 0, Width - 1, Height - 1));
            var bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var data = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            unsafe
            {
                var ptr = (byte*)data.Scan0;
                for (var i = 0; i < bitmap.Height; i++)
                {
                    var step_1 = i * data.Stride;
                    var step_2 = i * RowStride;
                    for (var j = 0; j < bitmap.Width; j++)
                    {
                        ptr[step_1] = Data[step_2];
                        ptr[step_1 + 1] = Data[step_2 + 1];
                        ptr[step_1 + 2] = Data[step_2 + 2];
                        step_1 += 3;
                        step_2 += 4;
                    }
                }
            }

            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static async Task<List<ColorInfo>> GetColorInfos(this WriteableBitmap drawBitmap, int size, int threshold)
        {
            return await Task.Run(() =>
            {
                var result = new List<ColorInfo>();
                var temps = new List<ColorInfo>();
                var drawBmpData = drawBitmap.Lock();
                unsafe
                {
                    var isIgnore = false;
                    var ignoreColor = new byte[] { 255, 0, 0 };
                    var ptr = (byte*)drawBmpData.Address;
                    var site = 0;
                    var ltColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Width - 1) * 4;
                    var rtColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Height - 1) * RowStride;
                    var lbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Height - 1) * RowStride + (Width - 1) * 4;
                    var rbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };

                    if (ltColor.SequenceEqual(rtColor) && ltColor.SequenceEqual(lbColor) && ltColor.SequenceEqual(rbColor))
                    {
                        isIgnore = true;
                        ignoreColor = ltColor;
                    }

                    for (var y = 1; y < Height - 1; y++)
                    {
                        for (var x = 1; x < Width - 1; x++)
                        {
                            var location = new int[]
                            {
                                y * RowStride + x * 4,
                                (y - 1) * RowStride + (x - 1) * 4,
                                (y - 1) * RowStride + x * 4,
                                (y - 1) * RowStride + (x+ 1) * 4,
                                y * RowStride + (x - 1) * 4,
                                y * RowStride + (x + 1) * 4,
                                (y + 1) * RowStride + (x - 1) * 4,
                                (y + 1) * RowStride + x * 4,
                                (y + 1) * RowStride + (x + 1) * 4
                            };

                            var r = ptr[location[0] + 2];
                            var g = ptr[location[0] + 1];
                            var b = ptr[location[0]];

                            if (isIgnore && b == ignoreColor[0] && g == ignoreColor[1] && r == ignoreColor[2])
                            {
                                continue;
                            }
                            var isBreak = false;
                            for (var i = 1; i < 9; i++)
                            {
                                if (Math.Abs(b - ptr[location[i]]) > threshold || Math.Abs(g - ptr[location[i] + 1]) > threshold || Math.Abs(r - ptr[location[i] + 2]) > threshold)
                                {
                                    isBreak = true;
                                }
                            }
                            if (!isBreak)
                            {
                                temps.Add(new ColorInfo(temps.Count, x, y, new byte[] { r, g, b }));
                            }
                        }
                    }

                    if (temps.Count == 0)
                    {
                        drawBmpData.Dispose();
                        return result;
                    }

                    if (size == -1 || size > temps.Count)
                    {
                        size = temps.Count;
                    }

                    result.Add(new ColorInfo(result.Count, StartX + (int)temps[0].Point.X, StartY + (int)temps[0].Point.Y, temps[0].Color));

                    var randoms = GetRandoms(temps.Count, size - 1);

                    foreach (var rd in randoms)
                    {
                        var temp = temps[rd];
                        var x = (int)temp.Point.X;
                        var y = (int)temp.Point.Y;
                        result.Add(new ColorInfo(result.Count, StartX + x, StartY + y, temp.Color));

                        var location = y * RowStride + x * 4;
                        ptr[location] = 0x1A;
                        ptr[location + 1] = 0xB1;
                        ptr[location + 2] = 0xF9;

                    }

                    drawBmpData.Dispose();
                    return result;
                }
            });
        }


        public static async Task<List<ColorInfo>> GetAllColorInfos(this WriteableBitmap drawBitmap, int size)
        {
            return await Task.Run(async () =>
            {
                var s = await drawBitmap.GetFindStrData();
                var result = new List<ColorInfo>();
                var temps = new List<ColorInfo>();

                var drawBmpData = drawBitmap.Lock();
                unsafe
                {
                    var isIgnore = false;
                    var ignoreColor = new byte[] { 255, 0, 0 };
                    var ptr = (byte*)drawBmpData.Address;
                    var site = 0;
                    var ltColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Width - 1) * 4;
                    var rtColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Height - 1) * RowStride;
                    var lbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Height - 1) * RowStride + (Width - 1) * 4;
                    var rbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };

                    if (ltColor.SequenceEqual(rtColor) && ltColor.SequenceEqual(lbColor) && ltColor.SequenceEqual(rbColor))
                    {
                        isIgnore = true;
                        ignoreColor = ltColor;
                    }

                    for (var y = 0; y < Height; y++)
                    {
                        var location = y * RowStride;
                        for (var x = 0; x < Width; x++, location += 4)
                        {
                            var r = ptr[location + 2];
                            var g = ptr[location + 1];
                            var b = ptr[location];

                            if (isIgnore && b == ignoreColor[0] && g == ignoreColor[1] && r == ignoreColor[2])
                            {
                                continue;
                            }
                            temps.Add(new ColorInfo(temps.Count, x, y, new byte[] { r, g, b }));
                        }
                    }

                    if (temps.Count == 0)
                    {
                        drawBmpData.Dispose();
                        return result;
                    }

                    if (size == -1 || size > temps.Count)
                    {
                        size = temps.Count;
                    }

                    result.Add(new ColorInfo(result.Count, StartX + (int)temps[0].Point.X, StartY + (int)temps[0].Point.Y, temps[0].Color));

                    var randoms = GetRandoms(temps.Count, size - 1);

                    foreach (var rd in randoms)
                    {
                        var temp = temps[rd];
                        var x = (int)temp.Point.X;
                        var y = (int)temp.Point.Y;
                        result.Add(new ColorInfo(result.Count, StartX + x, StartY + y, temp.Color));

                        var location = y * RowStride + x * 4;
                        ptr[location] = 0x1A;
                        ptr[location + 1] = 0xB1;
                        ptr[location + 2] = 0xF9;
                    }

                    drawBmpData.Dispose();
                    return result;
                }
            });
        }

        public static int[] GetRandoms(int maxValue, int len)
        {

            var result = new int[len];
            var values = new int[maxValue];

            for (var i = 0; i < values.Length; i++)
            {
                values[i] = i;
            }

            if (len >= maxValue)
            {
                return values;
            }

            for (var i = 0; i < len; i++)
            {
                var rd = new Random().Next(1, maxValue);
                result[i] = values[rd];
                values[rd] = values[maxValue - 1];
                values[maxValue - 1] = result[i];
                maxValue--;
            }

            return result;
        }



        public static async Task<string> GetFindStrData(this WriteableBitmap drawBitmap)
        {
            return await Task.Run(() =>
            {
                var drawBmpData = drawBitmap.Lock();
                unsafe
                {
                    var isIgnore = false;
                    var ignoreColor = new byte[] { 255, 0, 0 };
                    var ptr = (byte*)drawBmpData.Address;
                    var site = 0;
                    var ltColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Width - 1) * 4;
                    var rtColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Height - 1) * RowStride;
                    var lbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                    site = (Height - 1) * RowStride + (Width - 1) * 4;
                    var rbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };

                    if (ltColor.SequenceEqual(rtColor) && ltColor.SequenceEqual(lbColor) && ltColor.SequenceEqual(rbColor))
                    {
                        isIgnore = true;
                        ignoreColor = ltColor;
                    }


                    var width = Width - 2;
                    var stride = width + 4 - width % 4;

                    var binaryStr = string.Empty;

                    for (var y = 1; y < Height - 1; y++)
                    {
                        var location = y * RowStride + 4;
                        var str = string.Empty;
                        for (var x = 1; x < Width - 1; x++, location += 4)
                        {
                            var r = ptr[location + 2];
                            var g = ptr[location + 1];
                            var b = ptr[location];

                            if (isIgnore && b == ignoreColor[0] && g == ignoreColor[1] && r == ignoreColor[2])
                            {
                                str += "0";
                            }
                            else
                            {
                                str += "1";
                            }
                        }
                        binaryStr += str.PadRight(stride, '0');
                    }

                    var result = stride.ToString() + "$";

                    for (var i = 0; i < binaryStr.Length; i += 4)
                    {
                        result += Convert.ToString(Convert.ToInt32(binaryStr[i..(i + 4)], 2), 16);
                    }


                    drawBmpData.Dispose();
                    return result;
                }
            });
        }
    }
}
