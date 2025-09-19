using Avalonia;
using Newtonsoft.Json;
using ScriptGraphicHelper.Converters;
using ScriptGraphicHelper.Engine;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ScriptGraphicHelper.Models
{
    public class FormatMode
    {
        public const string ATFindStr = "AT找色";
        public const string ATCmpStr = "AT比色";
        public const string ATAnchorFindStr = "AT锚点找色";
        public const string ATAnchorCmpStr = "AT锚点比色";
        public const string AnchorFindStr = "锚点找色";
        public const string AnchorCmpStr = "锚点比色";
        public const string AutojsFindStr = "AJ找色";
        public const string AutojsCmpStr = "AJ比色";
        public const string DMFindStr = "大漠找色";
        public const string AnjianFindStr = "按键找色";
        public const string AnjianCmpStr = "按键比色";
        public const string DiyFindStr = "自定义找色";
        public const string DiyCmpStr = "自定义比色";
        public const string CDFindStr = "触动找色";
        public const string CDCmpStr = "触动比色";
        public const string ECFindStr = "EC找色";
        public const string ECCmpStr = "EC比色";
        public const string FindStrTest = "多点找色测试";
        public const string CmpStrTest = "多点比色测试";
        public const string AnchorsFindStrTest = "锚点找色测试";
        public const string AnchorsCmpStrTest = "锚点比色测试";

    };
    public static class CreateColorStrHelper
    {
        public static string Create(string mode, ObservableCollection<ColorInfo> colorInfos, Range? rect = null)
        {
            if (rect is null) rect = new(0, 0, 0, 0);
            var list = colorInfos.ToList();
            try
            {
                return mode switch
                {
                    FormatMode.ATCmpStr => AstatorCompareStr(list),
                    FormatMode.ATFindStr => AstatorFindStr(list, rect),
                    FormatMode.ATAnchorCmpStr => AstatorAnchorCompareStr(list),
                    FormatMode.ATAnchorFindStr => AstatorAnchorFindStr(list, rect),
                    FormatMode.AnchorCmpStr=>AnchorCompareStr(list),
                    FormatMode.AnchorFindStr => AnchorFindStr(list, rect),
                    FormatMode.AutojsFindStr => AutojsFindStr(list, rect),
                    FormatMode.AutojsCmpStr => AutojsCompareStr(list),
                    FormatMode.DMFindStr => DmFindStr(list, rect),
                    FormatMode.AnjianFindStr => AnjianFindStr(list, rect),
                    FormatMode.AnjianCmpStr => AnjianCompareStr(list),
                    FormatMode.CDFindStr => CdFindStr(list, rect),
                    FormatMode.CDCmpStr => CdCompareStr(list),
                    FormatMode.ECFindStr => EcFindStr(list, rect),
                    FormatMode.ECCmpStr => EcCompareStr(list),
                    FormatMode.FindStrTest => FindStrTest(list),
                    FormatMode.CmpStrTest => CompareStr(list),
                    FormatMode.AnchorsFindStrTest => AnchorCompareStrTest(list),
                    FormatMode.AnchorsCmpStrTest => AnchorCompareStrTest(list),
                    _ => DiyFormatStr(mode, list, rect),
                };
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.ToString());
                return string.Empty;
            }
        }

        private static string DiyFormatStr(string mode, List<ColorInfo> colorInfos, Range rect)
        {
            var format = FormatConfig.GetFormat(mode);
            if (format.IsDiyFormat is true && format.DiyFormatFileName is not null)
            {
                var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", format.DiyFormatFileName);
                if (File.Exists(fileName))
                {
                    if (format.DiyFormatFileName.EndsWith(".csx"))
                    {
                        if (format.IsCompareMode is true)
                        {
                            return DiyCompareStr_Script(fileName, colorInfos);
                        }
                        else
                        {
                            return DiyFindStr_Script(fileName, colorInfos, rect);
                        }
                    }
                    else
                    {
                        if (format.IsCompareMode is true)
                        {
                            return DiyCompareStr_Json(fileName, colorInfos);
                        }
                        else
                        {
                            return DiyFindStr_Json(fileName, colorInfos, rect);
                        }
                    }
                }
            }
            return string.Empty;
        }

        private static string DiyCompareStr_Script(string fileName, List<ColorInfo> colorInfos)
        {
            var engine = new ScriptEngine();
            engine.LoadScript(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", fileName));

            var compile = engine.Compile();
            if (!compile.Success)
            {
                var errorMessage = new List<string>();
                foreach (var msg in compile.Diagnostics)
                {
                    errorMessage.Add(msg.ToString());
                }
                MessageBox.ShowAsync("编译失败:\r\n" + string.Join("\r\n", errorMessage.ToArray()));
                return string.Empty;
            }

            var result = engine.Execute("CreateColorStrHelper.DiyFormat", "CreateCmpColor", new object[] { colorInfos.ToList() });
            engine.UnExecute();
            return result ?? string.Empty;
        }

        private static string DiyFindStr_Script(string fileName, List<ColorInfo> colorInfos, Range rect)
        {
            var engine = new ScriptEngine();
            engine.LoadScript(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", fileName));

            var compile = engine.Compile();
            if (!compile.Success)
            {
                var errorMessage = new List<string>();
                foreach (var msg in compile.Diagnostics)
                {
                    errorMessage.Add(msg.ToString());
                }
                MessageBox.ShowAsync("编译失败:\r\n" + string.Join("\r\n", errorMessage.ToArray()));
                return string.Empty;
            }

            var result = engine.Execute("CreateColorStrHelper.DiyFormat", "CreateFindColor", new object[] { colorInfos.ToList(), rect });
            engine.UnExecute();
            return result ?? string.Empty;

        }


        private static DiyFormat GetDiyFormatJson(string fileName)
        {
            var sr = File.OpenText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", fileName));
            var result = sr.ReadToEnd();
            sr.Close();

            DiyFormat format;

            format = JsonConvert.DeserializeObject<DiyFormat>(result);

            if (format == null)
            {
                MessageBox.ShowAsync("自定义格式错误!");
            }

            return format ?? new DiyFormat();
        }

        private static string DiyCompareStr_Json(string fileName, List<ColorInfo> colorInfos)
        {
            var diyFormat = GetDiyFormatJson(fileName);
            var colorStr = string.Empty;
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    var res = diyFormat.FollowColorFormat;

                    var color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                    if (diyFormat.IsBGR)
                    {
                        color = colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2");
                    }

                    if (res.IndexOf("{x}") != -1)
                    {
                        res = res.Replace("{x}", colorInfo.Point.X.ToString());
                    }
                    if (res.IndexOf("{y}") != -1)
                    {
                        res = res.Replace("{y}", colorInfo.Point.Y.ToString());
                    }
                    if (res.IndexOf("{color}") != -1)
                    {
                        res = res.Replace("{color}", color);
                    }
                    colorStr += res + ",";
                }
            }
            colorStr = colorStr.Trim(',');
            var result = diyFormat.CompareStrFormat;
            if (result.IndexOf("{ImportInfo}") != -1)
            {
                var info = diyFormat.ImportInfo;
                if (info != string.Empty && info != "")
                {
                    info = string.Format("\"{0}\"", info);
                }
                result = result.Replace("{ImportInfo}", info);
            }
            if (result.IndexOf("{colorStr}") != -1)
            {
                result = result.Replace("{colorStr}", colorStr);
            }
            return result;
        }

        private static string DiyFindStr_Json(string fileName, List<ColorInfo> colorInfos, Range rect)
        {
            var diyFormat = GetDiyFormatJson(fileName);
            var isInit = false;
            Point startPoint = new();
            var colorStr = new string[2];
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!isInit)
                    {
                        isInit = true;
                        startPoint = colorInfo.Point;
                        var res = diyFormat.FirstColorFormat;

                        var color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                        if (diyFormat.IsBGR)
                        {
                            color = colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2");
                        }

                        if (res.IndexOf("{color}") != -1)
                        {
                            res = res.Replace("{color}", color);
                        }
                        colorStr[0] = res;
                    }
                    else
                    {
                        var offsetX = colorInfo.Point.X - startPoint.X;
                        var offsetY = colorInfo.Point.Y - startPoint.Y;

                        var res = diyFormat.FollowColorFormat;

                        var color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                        if (diyFormat.IsBGR)
                        {
                            color = colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2");
                        }

                        if (res.IndexOf("{x}") != -1)
                        {
                            res = res.Replace("{x}", offsetX.ToString());
                        }
                        if (res.IndexOf("{y}") != -1)
                        {
                            res = res.Replace("{y}", offsetY.ToString());
                        }
                        if (res.IndexOf("{color}") != -1)
                        {
                            res = res.Replace("{color}", color);
                        }
                        colorStr[1] += res + ",";
                    }
                }
            }
            colorStr[1] = colorStr[1].Trim(',') ?? string.Empty;
            var result = diyFormat.FindStrFormat;
            if (result.IndexOf("{ImportInfo}") != -1)
            {
                var info = diyFormat.ImportInfo;
                if (info != string.Empty && info != "")
                {
                    info = string.Format("\"{0}/{1}/{2}\"", info, startPoint.X, startPoint.Y);
                }
                result = result.Replace("{ImportInfo}", info);
            }
            if (result.IndexOf("{range}") != -1)
            {
                var range = diyFormat.RangeFormat;
                if (range.IndexOf("{startX}") != -1)
                {
                    range = range.Replace("{startX}", rect.Left.ToString());
                }
                if (range.IndexOf("{startY}") != -1)
                {
                    range = range.Replace("{startY}", rect.Top.ToString());
                }
                if (range.IndexOf("{endX}") != -1)
                {
                    range = range.Replace("{endX}", rect.Right.ToString());
                }
                if (range.IndexOf("{endY}") != -1)
                {
                    range = range.Replace("{endY}", rect.Bottom.ToString());
                }
                if (range.IndexOf("{width}") != -1)
                {
                    range = range.Replace("{width}", (rect.Right - rect.Left).ToString());
                }
                if (range.IndexOf("{height}") != -1)
                {
                    range = range.Replace("{height}", (rect.Bottom - rect.Top).ToString());
                }
                result = result.Replace("{range}", range);
            }
            if (result.IndexOf("{firstColorStr}") != -1)
            {
                result = result.Replace("{firstColorStr}", colorStr[0]);
            }
            if (result.IndexOf("{followColorStr}") != -1)
            {
                result = result.Replace("{followColorStr}", colorStr[1]);
            }
            return result;
        }

        private static string DmFindStr(List<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;

            var inited = false;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!inited)
                    {
                        inited = true;
                        firstPoint = colorInfo.Point;
                        if (Settings.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "dm", firstPoint.X, firstPoint.Y);
                        }
                        if (Settings.Instance.AddRange)
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

        public static string CdFindStr(List<ColorInfo> colorInfos, Range rect)
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
                        if (Settings.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "cd", firstPoint.X, firstPoint.Y);
                        }
                        result += "0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",\"";
                    }
                    else
                    {
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            if (Settings.Instance.AddRange)
            {
                result += "\",90," + rect.ToString();
            }
            else
            {
                result += "\"";
            }
            return result;
        }

        private static string AnjianFindStr(List<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            var inited = false;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!inited)
                    {
                        inited = true;
                        firstPoint = colorInfo.Point;
                        if (Settings.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "anjian", firstPoint.X, firstPoint.Y);
                        }
                        if (Settings.Instance.AddRange)
                        {
                            result += rect.ToString() + ",";
                        }
                        result += "\"" + colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|" + colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.R.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AutojsFindStr(List<ColorInfo> colorInfos, Range rect)
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
                        if (Settings.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "autojs", firstPoint.X, firstPoint.Y);
                        }
                        result += "\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",[";
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
            if (Settings.Instance.AddRange)
            {
                result += "],{region:[" + rect.ToString(1) + "],threshold:[26]}";
            }
            else
            {
                result += "]";
            }
            return result;
        }

        private static string EcFindStr(List<ColorInfo> colorInfos, Range rect)
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
                        if (Settings.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "ec", firstPoint.X, firstPoint.Y);
                        }
                        result += "\"0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            if (Settings.Instance.AddRange)
            {
                result += "\",0.9," + rect.ToString();
            }
            else
            {
                result += "\"";
            }

            return result;
        }

        private static string CompareStr(List<ColorInfo> colorInfos)
        {
            var result = "\"";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Point.X.ToString() + "|" + colorInfo.Point.Y.ToString() + "|" + colorInfo.Color.R.ToString("x2") +
                    colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AnjianCompareStr(List<ColorInfo> colorInfos)
        {
            var result = "\"";
            if (Settings.Instance.AddInfo)
            {
                result += string.Format("{0}\",", "anjian");
            }
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Point.X.ToString() + "|" + colorInfo.Point.Y.ToString() + "|" + colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string CdCompareStr(List<ColorInfo> colorInfos)
        {
            var result = "{";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += "{" + colorInfo.Point.X.ToString() + "," + colorInfo.Point.Y.ToString() + ",0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "},";
                }
            }
            result = result.Trim(',');
            result += "}";
            return result;
        }

        private static string AutojsCompareStr(List<ColorInfo> colorInfos)
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
                        if (Settings.Instance.AddInfo)
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

        private static string EcCompareStr(List<ColorInfo> colorInfos)
        {
            var result = "\"";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Point.X.ToString() + "|" + colorInfo.Point.Y.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") +
                    colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AnchorCompareStr(List<ColorInfo> colorInfos)
        {
            var result = "[" + ColorInfo.Width.ToString() + "," + ColorInfo.Height.ToString() + ",\r\n[";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.Anchor == AnchorMode.Left)
                        result += "[left,";
                    else if (colorInfo.Anchor == AnchorMode.Center)
                        result += "[center,";
                    else if (colorInfo.Anchor == AnchorMode.Right)
                        result += "[right,";
                    else
                        result += "[none,";

                    result += colorInfo.Point.X.ToString() + "," + colorInfo.Point.Y.ToString() + ",0x" + colorInfo.Color.R.ToString("x2") +
                    colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "],\r\n";
                }
            }
            result = result.Trim(",\r\n".ToCharArray());
            result += "]\r\n]";
            return result;
        }

        private static string AnchorFindStr(List<ColorInfo> colorInfos, Range rect)
        {
            var result = "[" + ColorInfo.Width.ToString() + "," + ColorInfo.Height.ToString();
            if (Settings.Instance.AddRange)
            {
                result += string.Format(",\r\n[{0}],\r\n[\r\n", rect.ToString(2));
            }
            else
            {
                result += ",\r\n[\r\n";
            }
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.Anchor == AnchorMode.Left)
                        result += "[left,";
                    else if (colorInfo.Anchor == AnchorMode.Center)
                        result += "[center,";
                    else if (colorInfo.Anchor == AnchorMode.Right)
                        result += "[right,";
                    else
                        result += "[none,";

                    result += colorInfo.Point.X.ToString() + "," + colorInfo.Point.Y.ToString() + ",0x" + colorInfo.Color.R.ToString("x2") +
                    colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "],\r\n";
                }
            }
            result = result.Trim(",\r\n".ToCharArray());
            result += "\r\n]\r\n]";
            return result;
        }

        private static string AstatorCompareStr(List<ColorInfo> colorInfos)
        {
            var result = "\"";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += $"{colorInfo.Point.X}|{colorInfo.Point.Y}|{colorInfo.Color.ToHexString()},";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AstatorAnchorCompareStr(List<ColorInfo> colorInfos)
        {
            var result = $"\"{ColorInfo.Width},{ColorInfo.Height},";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += $"{colorInfo.Anchor.ToString().ToLower()}|{colorInfo.Point.X}|{colorInfo.Point.Y}|{colorInfo.Color.ToHexString()},";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AstatorFindStr(List<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            if (Settings.Instance.AddRange)
            {
                result += $"\"{rect.ToString()}\",";
            }
            result += "\"";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += $"{colorInfo.Point.X}|{colorInfo.Point.Y}|{colorInfo.Color.ToHexString()},";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AstatorAnchorFindStr(List<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            if (Settings.Instance.AddRange)
            {
                result += $"\"{rect.ToString(2)}\",";
            }

            result += $"\"{ColorInfo.Width},{ColorInfo.Height},";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += $"{colorInfo.Anchor.ToString().ToLower()}|{colorInfo.Point.X}|{colorInfo.Point.Y}|{colorInfo.Color.ToHexString()},";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AnchorCompareStrTest(List<ColorInfo> colorInfos)
        {
            var result = "\"";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Anchor.ToString() + "|" + colorInfo.Point.X.ToString() + "|" + colorInfo.Point.Y.ToString() + "|" + colorInfo.Color.R.ToString("x2") +
                        colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string FindStrTest(List<ColorInfo> colorInfos)
        {
            var result = string.Empty;

            var inited = false;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!inited)
                    {
                        inited = true;
                        firstPoint = colorInfo.Point;
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
    }
}
