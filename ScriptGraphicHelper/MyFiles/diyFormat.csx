using System;
using System.Collections.Generic;
using ScriptGraphicHelper.Models;
using Avalonia;
using Range = ScriptGraphicHelper.Models.Range;

namespace CreateColorStrHelper
{
    public static class DiyFormat
    {
        //---------------------------------------------------ColorInfo类---------------------------------------------------------------------//
        // 宽
        // public static double Width;
        // 高
        // public static double Height;
        // 下标
        // public int Index;
        //锚点模式, 不需要的话忽略
        // public AnchorMode Anchor;
        // 坐标
        // public Point Point;
        // 颜色
        // public Color Color;
        // 是否勾选
        // public bool IsChecked;

        //-----------------------------------------------------Range类----------------------------------------------------------------------//
        //
        //左
        // public double Left { get; set; }
        //上
        // public double Top { get; set; }
        //右
        // public double Right { get; set; }
        //下
        // public double Bottom { get; set; }


        /// <summary>
        /// 格式化范围
        /// </summary>
        /// <param name="mode"></param>
        /// 0. 返回 sx, sy, ex, ey 格式
        /// 1. 返回 sx, sy, width, height 格式
        /// 2. 返回 锚点范围格式
        /// <returns/>
        //public string ToString(int mode = 0)

        //----------------------------------------------------------------------------------------------------------------------------------//

        //例子: 大漠找色格式
        public static string CreateFindColor(List<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            Point firstPoint = new();

            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "dm", firstPoint.X, firstPoint.Y);
                        }
                        if (Setting.Instance.AddRange)
                        {
                            result += rect.ToString() + ",";
                        }
                        result += "\"" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        //例子:　ａｊ比色格式
        public static string CreateCmpColor(List<ColorInfo> colorInfos)
        {
            var result = string.Empty;
            Point firstPoint = new();

            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}\",", "autojs");
                        }
                        result += firstPoint.X.ToString() + "," + firstPoint.Y.ToString() + ",\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",[";
                    }
                    else
                    {
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += "[" + offsetX.ToString() + "," + offsetY.ToString() + ",\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                            colorInfo.Color.B.ToString("x2") + "\"],";
                    }
                }
            }
            result = result.Trim(',');
            result += "]";
            return result;
        }
    }
}