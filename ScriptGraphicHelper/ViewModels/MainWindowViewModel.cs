using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Newtonsoft.Json;
using ScriptGraphicHelper.Converters;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.ViewModels.Core;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Image = Avalonia.Controls.Image;
using Point = Avalonia.Point;
using Range = ScriptGraphicHelper.Models.Range;
using TabItem = ScriptGraphicHelper.Models.TabItem;
using Cursor = Avalonia.Input.Cursor;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"assets\settings.json"))
            {
                var settingsStr = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"assets\settings.json").Replace("\\\\", "\\").Replace("\\", "\\\\");
                var settings = JsonConvert.DeserializeObject<Settings>(settingsStr);
                if (settings is null)
                {
                    settings = new Settings
                    {
                        Formats = FormatConfig.CreateFormats()
                    };
                }
                Settings.Instance = settings;
            }
            else
            {
                Settings.Instance = new Settings
                {
                    Formats = FormatConfig.CreateFormats()
                };
            }

            this.FormatItems = FormatConfig.GetEnabledFormats();

            this.WindowWidth = Settings.Instance.Width;
            this.WindowHeight = Settings.Instance.Height;
            this.SimSelectedIndex = Settings.Instance.SimSelectedIndex;
            this.FormatSelectedIndex = Settings.Instance.FormatSelectedIndex;


            this.ColorInfos = new ObservableCollection<ColorInfo>();
            this.LoupeWriteBmp = LoupeWriteBitmap.Init(241, 241);
            this.DataGridHeight = 40;
            this.Loupe_IsVisible = false;
            this.Rect_IsVisible = false;
            this.EmulatorSelectedIndex = -1;
            this.EmulatorInfo = ScreenshotHelperBridge.Init();
        }

        private Point StartPoint;

        public ICommand Img_PointerPressed => new Command((param) =>
        {
            if (param != null)
            {
                var parameters = (CommandParameters)param;
                var eventArgs = (PointerPressedEventArgs)parameters.EventArgs;
                if (eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                {
                    this.Loupe_IsVisible = false;
                    this.StartPoint = eventArgs.GetPosition(null);
                    this.RectMargin = new Thickness(this.StartPoint.X, this.StartPoint.Y, 0, 0);
                    this.Rect_IsVisible = true;
                }
            }
        });

        public ICommand Img_PointerMoved => new Command((param) =>
        {
            if (param != null)
            {
                var parameters = (CommandParameters)param;
                var eventArgs = (PointerEventArgs)parameters.EventArgs;
                var point = eventArgs.GetPosition(null);
                if (this.Rect_IsVisible)
                {
                    var width = point.X - this.StartPoint.X - 1;
                    var height = point.Y - this.StartPoint.Y - 1;
                    if (width > 0 && height > 0)
                    {
                        this.RectWidth = width;
                        this.RectHeight = height;
                    }
                }
                else
                {
                    this.LoupeMargin = point.Y > 500 ? new Thickness(point.X + 20, point.Y - 261, 0, 0) : new Thickness(point.X + 20, point.Y + 20, 0, 0);

                    var position = eventArgs.GetPosition((Image)parameters.Sender);
                    var imgPoint = new Point(Math.Floor(position.X / this.ScaleFactor), Math.Floor(position.Y / this.ScaleFactor));
                    this.PointX = (int)imgPoint.X;
                    this.PointY = (int)imgPoint.Y;
                    var color = GraphicHelper.GetPixel(this.PointX, this.PointY);
                    this.PointColor = "#" + color[0].ToString("X2") + color[1].ToString("X2") + color[2].ToString("X2");
                    var sx = this.PointX - 7;
                    var sy = this.PointY - 7;

                    var colors = new List<byte[]>();
                    for (var j = 0; j < 15; j++)
                    {
                        for (var i = 0; i < 15; i++)
                        {
                            var x = sx + i;
                            var y = sy + j;

                            if (x >= 0 && y >= 0 && x < this.ImgWidth && y < this.ImgHeight)
                            {
                                colors.Add(GraphicHelper.GetPixel(x, y));
                            }
                            else
                            {
                                colors.Add(new byte[] { 0, 0, 0 });
                            }
                        }
                    }
                    this.LoupeWriteBmp.WriteColor(colors);
                }
            }
        });


        private DateTime AddColorInfoTime = DateTime.Now;
        public ICommand Img_PointerReleased => new Command((param) =>
        {
            if (param != null)
            {
                var parameters = (CommandParameters)param;
                if (this.Rect_IsVisible)
                {
                    var eventArgs = (PointerEventArgs)parameters.EventArgs;
                    var position = eventArgs.GetPosition((Image)parameters.Sender);
                    var point = new Point(Math.Floor(position.X / this.ScaleFactor), Math.Floor(position.Y / this.ScaleFactor));
                    var sx = (int)(point.X - Math.Floor(this.RectWidth / this.ScaleFactor));
                    var sy = (int)(point.Y - Math.Floor(this.rectHeight / this.ScaleFactor));
                    if (this.RectWidth > 10 && this.rectHeight > 10)
                    {
                        this.Rect = string.Format("[{0},{1},{2},{3}]", sx, sy, Math.Min(point.X, this.ImgWidth - 1), Math.Min(point.Y, this.ImgHeight - 1));
                    }
                    else
                    {
                        if ((DateTime.Now - this.AddColorInfoTime).TotalMilliseconds > 200)
                        {
                            this.AddColorInfoTime = DateTime.Now;

                            var color = GraphicHelper.GetPixel(sx, sy);

                            if (this.ColorInfos.Count == 0)
                            {
                                ColorInfo.Width = this.ImgWidth;
                                ColorInfo.Height = this.ImgHeight;
                            }

                            var anchor = AnchorMode.None;
                            var quarterWidth = this.ImgWidth / 4;
                            if (sx > quarterWidth * 3)
                            {
                                anchor = AnchorMode.Right;
                            }
                            else if (sx > quarterWidth)
                            {
                                anchor = AnchorMode.Center;
                            }
                            else
                            {
                                anchor = AnchorMode.Left;
                            }

                            this.ColorInfos.Add(new ColorInfo(this.ColorInfos.Count, anchor, sx, sy, color));

                            this.DataGridHeight = (this.ColorInfos.Count + 1) * 40;
                        }
                    }
                }
                this.Rect_IsVisible = false;
                this.Loupe_IsVisible = true;
                this.RectWidth = 0;
                this.RectHeight = 0;
                this.RectMargin = new Thickness(0, 0, 0, 0);
            }
        });

        public ICommand Img_PointerEnter => new Command((param) => this.Loupe_IsVisible = true);

        public ICommand Img_PointerLeave => new Command((param) => this.Loupe_IsVisible = false);

        public ICommand GetList => new Command(async (param) =>
        {
            if (ScreenshotHelperBridge.Select != -1)
            {
                var list = await ScreenshotHelperBridge.Helpers[ScreenshotHelperBridge.Select].GetList();
                var temp = new ObservableCollection<string>();
                foreach (var item in list)
                {
                    temp.Add(item.Value);
                }
                ScreenshotHelperBridge.Info = list;
                this.EmulatorInfo = temp;
            }
        });

        public async void Emulator_Selected(int value)
        {
            try
            {
                if (ScreenshotHelperBridge.State == LinkState.success)
                {
                    ScreenshotHelperBridge.Index = value;
                }
                else if (ScreenshotHelperBridge.State == LinkState.Waiting)
                {
                    this.WindowCursor = new Cursor(StandardCursorType.Wait);
                    ScreenshotHelperBridge.Changed(value);
                    this.EmulatorInfo = await ScreenshotHelperBridge.Initialize();
                    this.EmulatorSelectedIndex = -1;

                    ScreenshotHelperBridge.Helpers[ScreenshotHelperBridge.Select].OnSuccessed = new Action<Bitmap>((bitmap) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            this.Img = bitmap;
                            var item = new TabItem(this.Img);
                            item.Command = new Command((param) =>
                            {
                                this.TabItems.Remove(item);
                            });
                            this.TabItems.Add(item);
                            this.TabControlSelectedIndex = this.TabItems.Count - 1;
                            this.WindowCursor = new Cursor(StandardCursorType.Arrow);
                        });
                    });

                    ScreenshotHelperBridge.Helpers[ScreenshotHelperBridge.Select].OnFailed = new Action<string>((errorMessage) =>
                    {
                        MessageBox.ShowAsync(errorMessage);
                        this.WindowCursor = new Cursor(StandardCursorType.Arrow);
                    });

                }
                else if (ScreenshotHelperBridge.State == LinkState.Starting)
                {
                    ScreenshotHelperBridge.State = LinkState.success;
                }
            }
            catch (Exception e)
            {
                this.EmulatorSelectedIndex = -1;
                ScreenshotHelperBridge.Dispose();
                this.EmulatorInfo.Clear();
                this.EmulatorInfo = ScreenshotHelperBridge.Init();
                MessageBox.ShowAsync(e.ToString());
            }
            this.WindowCursor = new Cursor(StandardCursorType.Arrow);

        }

        public void ScreenShot_Click()
        {
            try
            {
                this.WindowCursor = new Cursor(StandardCursorType.Wait);
                if (ScreenshotHelperBridge.Select == -1
                    || ScreenshotHelperBridge.Index == -1 ||
                    ScreenshotHelperBridge.Info[ScreenshotHelperBridge.Index].Value == "null")
                {
                    MessageBox.ShowAsync("请先配置 -> (模拟器/tcp/句柄)");
                    this.WindowCursor = new Cursor(StandardCursorType.Arrow);
                    return;
                }
                ScreenshotHelperBridge.ScreenShot();
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync(ex.ToString());
            }
        }

        public void ResetEmulatorOptions_Click()
        {
            if (ScreenshotHelperBridge.State == LinkState.Starting || ScreenshotHelperBridge.State == LinkState.success)
            {
                this.EmulatorSelectedIndex = -1;
            }
            ScreenshotHelperBridge.Dispose();
            this.EmulatorInfo.Clear();
            this.EmulatorInfo = ScreenshotHelperBridge.Init();
        }

        public async void TurnRight_Click()
        {
            if (this.Img == null)
            {
                return;
            }
            this.Img = await GraphicHelper.TurnRight();
        }

        public void DropImage_Event(object? sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.Contains(DataFormats.FileNames))
                {
                    foreach (var name in e.Data.GetFileNames())
                    {
                        if (name != "" && name != string.Empty)
                        {
                            var stream = new FileStream(name, FileMode.Open, FileAccess.Read);
                            this.Img = new Bitmap(stream);
                            stream.Position = 0;
                            var sKBitmap = SKBitmap.Decode(stream);
                            GraphicHelper.KeepScreen(sKBitmap);
                            sKBitmap.Dispose();
                            stream.Dispose();

                            var item = new TabItem(this.Img);
                            item.Command = new Command((param) =>
                            {
                                this.TabItems.Remove(item);
                            });
                            this.TabItems.Add(item);
                            this.TabControlSelectedIndex = this.TabItems.Count - 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync(ex.ToString());
            }
        }

        public async void Load_Click()
        {
            try
            {
                var fileName = string.Empty;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    OpenFileName ofn = new();
                    ofn.hwnd = MainWindow.Instance.Handle;
                    ofn.structSize = Marshal.SizeOf(ofn);
                    ofn.filter = "位图文件 (*.png;*.bmp;*.jpg)\0*.png;*.bmp;*.jpg\0";
                    ofn.file = new string(new char[256]);
                    ofn.maxFile = ofn.file.Length;
                    ofn.fileTitle = new string(new char[64]);
                    ofn.maxFileTitle = ofn.fileTitle.Length;
                    ofn.title = "请选择文件";

                    if (NativeApi.GetOpenFileName(ofn))
                    {
                        fileName = ofn.file;
                    }
                }
                else
                {
                    var dlg = new OpenFileDialog
                    {
                        Title = "请选择文件",
                        AllowMultiple = false,
                        Filters = new List<FileDialogFilter>
                        {
                            new FileDialogFilter
                            {
                                Name = "位图文件",
                                Extensions = new List<string>()
                                {
                                "png",
                                "bmp",
                                "jpg"
                                }
                            }
                        }
                    };
                    var fileNames = await dlg.ShowAsync(MainWindow.Instance);
                    if (fileNames.Length != 0)
                    {
                        fileName = fileNames[0];
                    }
                }

                if (fileName != "" && fileName != string.Empty)
                {
                    var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    this.Img = new Bitmap(stream);
                    stream.Position = 0;
                    var sKBitmap = SKBitmap.Decode(stream);
                    GraphicHelper.KeepScreen(sKBitmap);
                    sKBitmap.Dispose();
                    stream.Dispose();

                    var item = new TabItem(this.Img);
                    item.Command = new Command((param) =>
                    {
                        this.TabItems.Remove(item);
                    });
                    this.TabItems.Add(item);
                    this.TabControlSelectedIndex = this.TabItems.Count - 1;
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.ToString());
            }
        }

        public async void Save_Click()
        {
            if (this.Img == null)
            {
                return;
            }

            try
            {
                var fileName = string.Empty;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    OpenFileName ofn = new();

                    ofn.hwnd = MainWindow.Instance.Handle;
                    ofn.structSize = Marshal.SizeOf(ofn);
                    ofn.filter = "位图文件 (*.png;*.bmp;*.jpg)\0*.png;*.bmp;*.jpg\0";
                    ofn.file = new string(new char[256]);
                    ofn.maxFile = ofn.file.Length;
                    ofn.fileTitle = new string(new char[64]);
                    ofn.maxFileTitle = ofn.fileTitle.Length;
                    ofn.title = "保存文件";
                    ofn.defExt = ".png";
                    if (NativeApi.GetSaveFileName(ofn))
                    {
                        fileName = ofn.file;
                    }
                }
                else
                {
                    var dlg = new SaveFileDialog
                    {
                        InitialFileName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"),
                        Title = "保存文件",
                        Filters = new List<FileDialogFilter>
                        {
                            new FileDialogFilter
                            {
                                Name = "位图文件",
                                Extensions = new List<string>()
                                {
                                    "png",
                                    "bmp",
                                    "jpg"
                                }
                            }
                        }
                    };
                    fileName = await dlg.ShowAsync(MainWindow.Instance);
                }


                if (fileName != null && fileName != "" && fileName != string.Empty)
                {
                    this.Img.Save(fileName);
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.ToString());
            }
        }

        public async void Test_Click()
        {
            if (this.Img != null && this.ColorInfos.Count > 0)
            {
                var sims = new int[] { 100, 95, 90, 85, 80, 0 };
                if (this.CurrentFormat.IsCompareMode is true)
                {
                    CompareResult result;
                    if (this.CurrentFormat.AnchorIsEnabled is true)
                    {
                        var width = ColorInfo.Width;
                        var height = ColorInfo.Height;
                        var testStr = CreateColorStrHelper.Create(FormatMode.AnchorsCmpStrTest, this.ColorInfos);
                        result = GraphicHelper.AnchorsCompareColor(width, height, testStr.Trim('"'), sims[this.SimSelectedIndex]);
                    }
                    else
                    {
                        var testStr = CreateColorStrHelper.Create(FormatMode.CmpStrTest, this.ColorInfos);
                        result = GraphicHelper.CompareColorEx(testStr.Trim('"'), sims[this.SimSelectedIndex]);
                    }

                    if (!result.Result)
                    {
                        MessageBox.ShowAsync(result.ErrorMessage);
                    }
                    this.TestResult = result.Result.ToString();
                }
                else
                {
                    if (colorInfos.Count < 2)
                    {
                        MessageBox.ShowAsync("错误", "多点找色至少需要勾选两个颜色才可进行测试!");
                        this.TestResult = "error";
                        return;
                    }
                    Point result;
                    if (this.CurrentFormat.AnchorIsEnabled is true)
                    {
                        var width = ColorInfo.Width;
                        var height = ColorInfo.Height;
                        var testStr = CreateColorStrHelper.Create(FormatMode.AnchorsFindStrTest, this.ColorInfos);
                        result = GraphicHelper.AnchorsFindColor(new Range(0, 0, width - 1, height - 1), width, height, testStr.Trim('"'), sims[this.SimSelectedIndex]);
                    }
                    else
                    {
                        var testStr = CreateColorStrHelper.Create(FormatMode.FindStrTest, this.ColorInfos);
                        var strArray = testStr.Split("\",\"");
                        var _str = strArray[0].Split(",\"");
                        result = GraphicHelper.FindMultiColor(0, 0, (int)(this.ImgWidth - 1), (int)(this.ImgHeight - 1), _str[^1].Trim('"'), strArray[1].Trim('"'), sims[this.SimSelectedIndex]);
                    }
                    this.TestResult = result.ToString();

                    if (result.X >= 0 && result.Y >= 0)
                    {
                        this.FindedPoint_Margin = new(result.X * this.ScaleFactor - 36, result.Y * this.ScaleFactor - 69, 0, 0);
                        this.FindedPoint_IsVisible = true;
                        await Task.Delay(2500);
                        this.FindedPoint_IsVisible = false;
                    }
                }
            }
        }

        public void Create_Click()
        {
            if (this.ColorInfos.Count > 0)
            {
                var rect = GetRange();

                if (this.Rect.IndexOf("[") != -1)
                {
                    this.Rect = string.Format("[{0}]", rect.ToString());
                }
                else if (FormatConfig.GetFormat(this.FormatItems[this.formatSelectedIndex])!.AnchorIsEnabled is true)
                {
                    this.Rect = rect.ToString(2);
                }
                else
                {
                    this.Rect = rect.ToString();
                }

                this.CreateStr = CreateColorStrHelper.Create(this.CurrentFormat.Name, this.ColorInfos, rect);
                CreateStr_Copy_Click();
            }
        }

        public async void CreateStr_Copy_Click()
        {
            try
            {
                await Application.Current.Clipboard.SetTextAsync(this.CreateStr);
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync("设置剪贴板失败 , 你的剪贴板可能被其他软件占用\r\n\r\n" + ex.Message, "error");
            }
        }

        public void Clear_Click()
        {
            if (this.CreateStr == string.Empty && this.Rect == string.Empty)
            {
                this.ColorInfos.Clear();
                this.DataGridHeight = 40;
            }
            else
            {
                this.CreateStr = string.Empty;
                this.Rect = string.Empty;
                this.TestResult = string.Empty;
            }
        }

        public ICommand Key_AddColorInfo => new Command((param) =>
        {
            if (!this.Loupe_IsVisible)
            {
                return;
            }
            var x = this.PointX;
            var y = this.PointY;
            var key = (string)param;

            var color = GraphicHelper.GetPixel(x, y);


            if (this.ColorInfos.Count == 0)
            {
                ColorInfo.Width = this.ImgWidth;
                ColorInfo.Height = this.ImgHeight;
            }

            var anchor = AnchorMode.None;

            if (key == "A")
                anchor = AnchorMode.Left;
            else if (key == "S")
                anchor = AnchorMode.Center;
            else if (key == "D")
                anchor = AnchorMode.Right;

            this.ColorInfos.Add(new ColorInfo(this.ColorInfos.Count, anchor, x, y, color));
            this.DataGridHeight = (this.ColorInfos.Count + 1) * 40;
        });

        public ICommand Key_ScaleFactorChanged => new Command((param) =>
        {
            var num = this.ScaleFactor switch
            {
                0.3 => 0,
                0.4 => 1,
                0.5 => 2,
                0.6 => 3,
                0.7 => 4,
                0.8 => 5,
                0.9 => 6,
                1.0 => 7,
                1.2 => 8,
                1.4 => 9,
                1.6 => 10,
                1.8 => 11,
                2.0 => 12,
                _ => 7
            };

            if (param.ToString() == "Add")
            {
                num++;
            }
            else if (param.ToString() == "Subtract")
            {
                num--;
            }
            else
            {
                if (num == 0)
                {
                    num = 12;
                }
                else
                {
                    num--;
                }
            }
            num = Math.Min(num, 12);
            num = Math.Max(num, 0);
            this.ScaleFactor = num switch
            {
                0 => 0.3,
                1 => 0.4,
                2 => 0.5,
                3 => 0.6,
                4 => 0.7,
                5 => 0.8,
                6 => 0.9,
                7 => 1.0,
                8 => 1.2,
                9 => 1.4,
                10 => 1.6,
                11 => 1.8,
                12 => 2.0,
                _ => 1.0
            };

        });

        public async void Key_GetClipboardData()
        {
            try
            {
                var formats = await Application.Current.Clipboard.GetFormatsAsync();
                var fileName = string.Empty;

                if (Array.IndexOf(formats, "FileNames") != -1)
                {
                    var fileNames = (List<string>)await Application.Current.Clipboard.GetDataAsync(DataFormats.FileNames);
                    if (fileNames.Count != 0)
                    {
                        fileName = fileNames[0];
                    }
                }

                if (fileName.IndexOf(".bmp") != -1 || fileName.IndexOf(".png") != -1 || fileName.IndexOf(".jpg") != -1)
                {
                    var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    this.Img = new Bitmap(stream);
                    stream.Position = 0;
                    var sKBitmap = SKBitmap.Decode(stream);
                    GraphicHelper.KeepScreen(sKBitmap);
                    sKBitmap.Dispose();
                    stream.Dispose();

                    var item = new TabItem(this.Img);
                    item.Command = new Command((param) =>
                    {
                        this.TabItems.Remove(item);
                    });
                    this.TabItems.Add(item);
                    this.TabControlSelectedIndex = this.TabItems.Count - 1;
                }
                else
                {
                    var text = await Application.Current.Clipboard.GetTextAsync();
                    if (text != string.Empty)
                    {
                        this.ColorInfos.Clear();
                        var sims = new int[] { 100, 95, 90, 85, 80, 0 };
                        var sim = sims[this.SimSelectedIndex];
                        if (sim == 0)
                        {
                            sim = Settings.Instance.DiySim;
                        }
                        var result = DataImportHelper.Import(text);

                        var similarity = (255 - 255 * (sim / 100.0)) / 2;
                        for (var i = 0; i < result.Count; i++)
                        {
                            if (GraphicHelper.CompareColor(new byte[] { result[i].Color.R, result[i].Color.G, result[i].Color.B }, similarity, (int)result[i].Point.X, (int)result[i].Point.Y, 0))
                            {
                                result[i].IsChecked = true;
                            }
                            this.ColorInfos.Add(result[i]);
                        }
                        this.DataGridHeight = (this.ColorInfos.Count + 1) * 40;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync(ex.ToString());
            }
        }

        public void Key_ColorInfo_Clear()
        {
            this.ColorInfos.Clear();
            this.DataGridHeight = 40;
        }

        public async void Key_SetConfig()
        {
            var config = new Config();
            var setting = Settings.Instance;
            var ldpath3 = setting.LdPath3;
            var ldpath4 = setting.LdPath4;
            var ldpath64 = setting.LdPath64;

            await config.ShowDialog(MainWindow.Instance);

            if (ldpath3 != setting.LdPath3 || ldpath4 != setting.LdPath4 || ldpath64 != setting.LdPath64)
            {
                ResetEmulatorOptions_Click();
            }
        }

        public async void Rect_Copy_Click()
        {
            try
            {
                await Application.Current.Clipboard.SetTextAsync(this.Rect);
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync("设置剪贴板失败 , 你的剪贴板可能被其他软件占用\r\n\r\n" + ex.Message, "error");
            }
        }

        public void Rect_Clear_Click()
        {
            this.Rect = string.Empty;
        }

        public async void Point_Copy_Click()
        {
            try
            {
                if (this.DataGridSelectedIndex == -1 || this.DataGridSelectedIndex > this.ColorInfos.Count)
                {
                    return;
                }
                var point = this.ColorInfos[this.DataGridSelectedIndex].Point;
                var pointStr = string.Format("{0},{1}", point.X, point.Y);
                await Application.Current.Clipboard.SetTextAsync(pointStr);
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync("设置剪贴板失败\r\n\r\n" + ex.Message, "错误");
            }
        }

        public async void Color_Copy_Click()
        {
            try
            {
                if (this.DataGridSelectedIndex == -1 || this.DataGridSelectedIndex > this.ColorInfos.Count)
                {
                    return;
                }
                var color = this.ColorInfos[this.DataGridSelectedIndex].Color;
                var hexColor = string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
                await Application.Current.Clipboard.SetTextAsync(hexColor);
            }
            catch (Exception ex)
            {
                MessageBox.ShowAsync("设置剪贴板失败\r\n\r\n" + ex.Message, "错误");
            }
        }

        public void ColorInfo_Reset_Click()
        {
            var temp = new ObservableCollection<ColorInfo>();

            foreach (var colorInfo in this.ColorInfos)
            {
                var x = (int)colorInfo.Point.X;
                var y = (int)colorInfo.Point.Y;
                var color = GraphicHelper.GetPixel(x, y);
                colorInfo.Color = Color.FromRgb(color[0], color[1], color[2]);
                if (x >= this.ImgWidth || y >= this.ImgHeight)
                {
                    colorInfo.IsChecked = false;
                }
                temp.Add(colorInfo);
            }

            this.ColorInfos = temp;

        }

        public void ColorInfo_SelectItemClear_Click()
        {
            if (this.DataGridSelectedIndex == -1 || this.DataGridSelectedIndex > this.ColorInfos.Count)
            {
                return;
            }
            this.ColorInfos.RemoveAt(this.DataGridSelectedIndex);
            this.DataGridHeight = (this.ColorInfos.Count + 1) * 40;
        }

        public async void CutImg_Click()
        {
            var range = GetRange();
            var imgEditor = new ImgEditor(range, GraphicHelper.GetRectData(range));
            await imgEditor.ShowDialog(MainWindow.Instance);
            if (ImgEditor.Result_ACK && ImgEditor.ResultColorInfos != null && ImgEditor.ResultColorInfos.Count != 0)
            {
                this.ColorInfos = new ObservableCollection<ColorInfo>(ImgEditor.ResultColorInfos);
                ImgEditor.ResultColorInfos.Clear();
                ImgEditor.Result_ACK = false;
                this.DataGridHeight = (this.ColorInfos.Count + 1) * 40;
            }
        }

        private Range GetRange()
        {
            //if (ColorInfos.Count == 0)
            //{
            //    return new Range(0, 0, ImgWidth - 1, ImgHeight - 1);
            //}
            if (this.Rect != string.Empty)
            {
                if (this.Rect.IndexOf("[") != -1)
                {
                    var range = this.Rect.TrimStart('[').TrimEnd(']').Split(',');

                    return new Range(int.Parse(range[0].Trim()), int.Parse(range[1].Trim()), int.Parse(range[2].Trim()), int.Parse(range[3].Trim()));
                }
            }
            var imgWidth = this.ImgWidth - 1;
            var imgHeight = this.ImgHeight - 1;

            if (this.CurrentFormat.AnchorIsEnabled is true)
            {
                imgWidth = ColorInfo.Width - 1;
                imgHeight = ColorInfo.Height - 1;
            }

            var left = imgWidth;
            var top = imgHeight;
            var right = 0d;
            var bottom = 0d;
            var mode_1 = -1;
            var mode_2 = -1;

            foreach (var colorInfo in this.ColorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.Point.X < left)
                    {
                        left = colorInfo.Point.X;
                        mode_1 = colorInfo.Anchor == AnchorMode.Left ? 0 : colorInfo.Anchor == AnchorMode.Center ? 1 : colorInfo.Anchor == AnchorMode.Right ? 2 : -1;
                    }
                    if (colorInfo.Point.X > right)
                    {
                        right = colorInfo.Point.X;
                        mode_2 = colorInfo.Anchor == AnchorMode.Left ? 0 : colorInfo.Anchor == AnchorMode.Center ? 1 : colorInfo.Anchor == AnchorMode.Right ? 2 : -1;
                    }
                    if (colorInfo.Point.Y < top)
                    {
                        top = colorInfo.Point.Y;
                    }
                    if (colorInfo.Point.Y > bottom)
                    {
                        bottom = colorInfo.Point.Y;
                    }
                }
            }
            var tolerance = Settings.Instance.RangeTolerance;
            return new Range(left >= tolerance ? left - tolerance : 0, top >= tolerance ? top - tolerance : 0, right + tolerance > imgWidth ? imgWidth : right + tolerance, bottom + tolerance > imgHeight ? imgHeight : bottom + tolerance, mode_1, mode_2);

        }
    }

}
