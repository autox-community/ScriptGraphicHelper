using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using ScriptGraphicHelper.Models;

using SkiaSharp;

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Point = Avalonia.Point;

namespace ScriptGraphicHelper.Helpers
{
    public struct CompareResult
    {
        public bool Result { get; set; }
        public string ErrorMessage { get; set; }

        public CompareResult(bool result)
        {
            Result = result;
            ErrorMessage = string.Empty;
        }

        public CompareResult(bool result, string message)
        {
            Result = result;
            ErrorMessage = message;
        }
    }

    public static class GraphicHelper
    {
        public static int Width { get; set; } = 0;
        public static int Height { get; set; } = 0;
        public static int PixelStride { get; set; }
        public static PixelFormat PxFormat { get; set; }
        public static int RowStride { get; set; }
        public static byte[] ScreenData { get; set; }

        public static void KeepScreen(SKBitmap bitmap)
        {
            Width = bitmap.Width;
            Height = bitmap.Height;
            ScreenData = new byte[bitmap.RowBytes * Height];
            RowStride = bitmap.RowBytes;
            PixelStride = bitmap.RowBytes / Width;
            PxFormat = bitmap.ColorType == SKColorType.Rgba8888 ? PixelFormat.Rgba8888 : PixelFormat.Bgra8888;
            Marshal.Copy(bitmap.GetPixels(), ScreenData, 0, ScreenData.Length);
        }

        public static byte[] GetRectData(MyRange range)
        {
            var sx = (int)range.Left;
            var sy = (int)range.Top;
            var ex = (int)range.Right;
            var ey = (int)range.Bottom;
            var width = ex - sx + 1;
            var height = ey - sy + 1;
            var data = new byte[width * height * 4];
            var site = 0;
            for (var i = sy; i <= ey; i++)
            {
                var location = sx * 4 + Width * 4 * i;
                for (var j = sx; j <= ex; j++)
                {
                    data[site] = ScreenData[location];
                    data[site + 1] = ScreenData[location + 1];
                    data[site + 2] = ScreenData[location + 2];
                    data[site + 3] = ScreenData[location + 3];
                    location += 4;
                    site += 4;
                }
            }
            return data;
        }

        public static async Task<Bitmap> TurnRight()
        {
            var task = Task.Run(() =>
            {
                var data = new byte[RowStride * Height];
                var step = 0;
                for (var j = 0; j < Width; j++)
                {
                    for (var i = Height - 1; i >= 0; i--)
                    {
                        var location = j * PixelStride + i * RowStride;
                        data[step] = ScreenData[location];
                        data[step + 1] = ScreenData[location + 1];
                        data[step + 2] = ScreenData[location + 2];
                        data[step + 3] = 255;
                        step += 4;
                    }
                }
                SKBitmap sKBitmap = new(new SKImageInfo(Height, Width));
                Marshal.Copy(data, 0, sKBitmap.GetPixels(), data.Length);
                KeepScreen(sKBitmap);
                var bitmap = new Bitmap(PxFormat, AlphaFormat.Unpremul, sKBitmap.GetPixels(), new PixelSize(Width, Height), new Vector(96, 96), sKBitmap.RowBytes);
                sKBitmap.Dispose();
                return bitmap;
            });
            return await task;
        }

        public static byte[] GetPixel(int x, int y)
        {
            var retRGB = new byte[] { 0, 0, 0 };
            try
            {
                if (x < Width && y < Height)
                {
                    var location = x * PixelStride + y * RowStride;
                    if (PxFormat == PixelFormat.Bgra8888)
                    {
                        retRGB[0] = ScreenData[location + 2];
                        retRGB[1] = ScreenData[location + 1];
                        retRGB[2] = ScreenData[location];
                    }
                    else if (PxFormat == PixelFormat.Rgba8888)
                    {
                        retRGB[0] = ScreenData[location];
                        retRGB[1] = ScreenData[location + 1];
                        retRGB[2] = ScreenData[location + 2];
                    }
                }
            }
            catch
            {
                retRGB = new byte[] { 0, 0, 0 };
            }
            return retRGB;
        }

        public static CompareResult AnchorsCompareColor(double width, double height, string colorString, int sim = 95)
        {
            var compareColorArr = colorString.Trim('"').Split(',');

            var multiple = Height / height;
            var result = string.Empty;
            for (var i = 0; i < compareColorArr.Length; i++)
            {
                var compareColor = compareColorArr[i].Split('|');
                double findX = int.Parse(compareColor[1]);
                double findY = int.Parse(compareColor[2]);
                if (compareColor[0] == "Left" || compareColor[0] == "None")
                {
                    findX = Math.Floor(findX * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                else if (compareColor[0] == "Center")
                {
                    findX = Math.Floor(Width / 2 - 1 - (width / 2 - findX - 1) * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                else if (compareColor[0] == "Right")
                {
                    findX = Math.Floor(Width - 1 - (width - findX - 1) * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                result += findX.ToString() + "|" + findY.ToString() + "|" + compareColor[3] + ",";
            }
            result = result.Trim(',');
            return CompareColorEx(result, sim);
        }

        public static Point AnchorsFindColor(MyRange rect, double width, double height, string colorString, int sim = 95)
        {
            var compareColorStr = colorString.Trim('"');
            var compareColorArr = compareColorStr.Split(',');
            if (compareColorArr.Length < 2)
            {
                return new Point(-1, -1);
            }
            var multiple = Height / height;
            var startColorArr = compareColorArr[0].Split('|');
            double x = int.Parse(startColorArr[1]);
            double y = int.Parse(startColorArr[2]);
            double startX = -1;
            double startY = -1;
            if (startColorArr[0] == "Left" || startColorArr[0] == "None")
            {
                startX = Math.Floor(x * multiple);
                startY = Math.Floor(y * multiple);
            }
            else if (startColorArr[0] == "Center")
            {
                startX = Math.Floor(Width / 2 - 1 - (width / 2 - x - 1) * multiple);
                startY = Math.Floor(y * multiple);
            }
            else if (startColorArr[0] == "Right")
            {
                startX = Math.Floor(Width - 1 - (width - x - 1) * multiple);
                startY = Math.Floor(y * multiple);
            }

            var result = string.Empty;
            for (var i = 1; i < compareColorArr.Length; i++)
            {
                var compareColor = compareColorArr[i].Split('|');
                double findX = int.Parse(compareColor[1]);
                double findY = int.Parse(compareColor[2]);
                if (compareColor[0] == "Left" || compareColor[0] == "None")
                {
                    findX = Math.Floor(findX * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                else if (compareColor[0] == "Center")
                {
                    findX = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - findX) * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                else if (compareColor[0] == "Right")
                {
                    findX = Math.Floor(Width - 1 - (width - findX - 1) * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                result += findX.ToString() + "|" + findY.ToString() + "|" + compareColor[3] + ",";
            }
            result = result.Trim(',');

            if (rect.Mode_1 == 0 || rect.Mode_1 == -1)
            {
                rect.Left = Math.Floor(rect.Left * multiple);
            }
            else if (rect.Mode_1 == 1)
            {
                rect.Left = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - rect.Left) * multiple);
            }
            else if (rect.Mode_1 == 2)
            {
                rect.Left = Math.Floor(Width - 1 - (width - rect.Left - 1) * multiple);
            }
            if (rect.Mode_2 == 0 || rect.Mode_2 == -1)
            {
                rect.Right = Math.Floor(rect.Right * multiple);
            }
            else if (rect.Mode_2 == 1)
            {
                rect.Right = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - rect.Right) * multiple);
            }
            else if (rect.Mode_2 == 2)
            {
                rect.Right = Math.Floor(Width - 1 - (width - rect.Right - 1) * multiple);
            }
            rect.Top = Math.Floor(rect.Top * multiple);
            rect.Bottom = Math.Floor(rect.Bottom * multiple);
            return FindMultiColor((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom, startColorArr[3], result, sim);
        }

        public static bool CompareColor(byte[] rgb, double similarity, int x, int y, int offset)
        {
            var offsetSize = offset == 0 ? 1 : 9;
            var offsetPoint = new Point[]{
                new Point(x, y),
                new Point(x - 1, y - 1),
                new Point(x - 1, y),
                new Point(x - 1, y + 1),
                new Point(x, y - 1),
                new Point(x, y + 1),
                new Point(x + 1, y - 1),
                new Point(x + 1, y),
                new Point(x + 1, y + 1),
            };

            for (var j = 0; j < offsetSize; j++)
            {
                var _x = (int)offsetPoint[j].X;
                var _y = (int)offsetPoint[j].Y;
                if (_x >= 0 && _x < Width && _y >= 0 && _y < Height)
                {
                    var GetRGB = GetPixel(_x, _y);
                    if (Math.Abs(GetRGB[0] - rgb[0]) <= similarity && Math.Abs(GetRGB[1] - rgb[1]) <= similarity && Math.Abs(GetRGB[2] - rgb[2]) <= similarity)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static CompareResult CompareColorEx(string colorString, int sim = 95, int x = 0, int y = 0)
        {
            if (string.IsNullOrWhiteSpace(colorString))
            {
                return new CompareResult(false, "colorString 没有数据");
            }

            int findX;
            int findY;

            var offset = Settings.Instance.IsOffset ? 1 : 0;
            if (sim == 0)
            {
                sim = Settings.Instance.DiySim;
            }

            var similarity = 255 - 255 * (sim / 100.0);
            colorString = colorString.Trim("\"".ToCharArray());
            var findColors = colorString.Split(',');
            if (findColors.Length != 0)
            {
                for (var i = 0; i < findColors.Length; i++)
                {
                    var findColor = findColors[i].Split('|');
                    byte[] findRGB = { 0, 0, 0 };
                    findRGB[0] = Convert.ToByte(findColor[2].Substring(0, 2), 16);
                    findRGB[1] = Convert.ToByte(findColor[2].Substring(2, 2), 16);
                    findRGB[2] = Convert.ToByte(findColor[2].Substring(4, 2), 16);

                    findX = x + int.Parse(findColor[0]);
                    findY = y + int.Parse(findColor[1]);
                    if (findX < 0 || findY < 0 || findX > Width || findY > Height)
                    {
                        return new CompareResult(false, string.Format("坐标越界:  index = {0}, x = {1}, y = {2}, color = 0x{3}", i, findX, findY, findColor[2]));
                    }

                    if (!CompareColor(findRGB, similarity, findX, findY, offset))
                    {
                        return new CompareResult(false, string.Format("return false:  index = {0}, x = {1}, y = {2}, color = 0x{3}", i, findX, findY, findColor[2]));
                    }
                }
            }
            return new CompareResult(true);
        }

        public static Point FindMultiColor(int startX, int startY, int endX, int endY, string findcolorString, string compareColorString, int sim = 95)
        {
            startX = Math.Max(startX, 0);
            startY = Math.Max(startY, 0);
            endX = Math.Min(endX, Width - 1);
            endY = Math.Min(endY, Height - 1);

            if (sim == 0)
            {
                sim = Settings.Instance.DiySim;
            }

            var similarity = 255 - 255 * (sim / 100.0);
            var findColor = findcolorString.Split('-');
            var findR = Convert.ToByte(findColor[0].Substring(0, 2), 16);
            var findG = Convert.ToByte(findColor[0].Substring(2, 2), 16);
            var findB = Convert.ToByte(findColor[0].Substring(4, 2), 16);

            for (var i = startY; i <= endY; i++)
            {
                for (var j = startX; j <= endX; j++)
                {
                    var GetRGB = GetPixel(j, i);
                    if (Math.Abs(GetRGB[0] - findR) <= similarity)
                    {
                        if (Math.Abs(GetRGB[1] - findG) <= similarity)
                        {
                            if (Math.Abs(GetRGB[2] - findB) <= similarity)
                            {
                                var compareResult = CompareColorEx(compareColorString, sim, j, i);
                                if (compareResult.Result)
                                {
                                    return new Point(j, i);
                                }
                            }
                        }
                    }
                }
            }
            return new Point(-1, -1);
        }
    }
}
