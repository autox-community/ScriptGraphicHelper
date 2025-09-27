using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.DependencyInjection;

using ExCSS;

using Newtonsoft.Json;

using ScriptGraphicHelper.Helpers;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Tools;
using ScriptGraphicHelper.Tools.Converters;
using ScriptGraphicHelper.Utils.ViewModel;
using ScriptGraphicHelper.Views;

using SkiaSharp;

using Cursor = Avalonia.Input.Cursor;
using Image = Avalonia.Controls.Image;
using Point = Avalonia.Point;
using TabItem = ScriptGraphicHelper.Helpers.TabItem;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            // 读取 json 配置文件
            Settings.Instance = SettingsTools.InitSettings();

            if (Settings.Instance.ShowConsole)
            {
                NativeApi.AllocConsole();
            }

#if DEBUG
            NativeApi.AllocConsole();
#endif

            // 获取 开启 的生成格式
            this.FormatItems = FormatConfig.GetEnabledFormats();

            // 窗口宽高
            this.WindowWidth = Settings.Instance.Width;
            this.WindowHeight = Settings.Instance.Height;

            // 相似度
            this.SimSelectedIndex = Settings.Instance.SimSelectedIndex;

            // 代码生成格式选择
            this.FormatSelectedIndex = Settings.Instance.FormatSelectedIndex;

            this.ColorInfos = new ObservableCollection<ColorInfo>();

            // 放大镜
            this.LoupeWriteBmp = LoupeHelper.Init(241, 241);

            // 放大镜刷新间隔
            this.LoupeRefreshInterval = Settings.Instance.LoupeRefreshInterval;

            this.Rect_IsVisible = false;

            this.DataGridHeight = 40;

            // 模拟器 (夜神,逍遥,雷电)
            this.EmulatorSelectedIndex = -1;

            // 获取可选择的模式
            this.EmulatorInfo = ScreenshotHelperBridge.Init();
        }

        private Point _startPoint;

        /// <summary>
        /// 图片 点 鼠标按下
        /// </summary>
        public ICommand Img_PointerPressed => new Command((param) =>
        {
            if (param == null)
            {
                return;
            }

            var parameters = (CommandParameters)param;
            var eventArgs = (PointerPressedEventArgs)parameters.EventArgs;

            // 左键按下
            if (eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                // 获取点击的 xy
                this._startPoint = eventArgs.GetPosition(null);
                // 设置矩形 距离左边的距离
                this.RectMargin = new Thickness(this._startPoint.X, this._startPoint.Y, 0, 0);
                // 矩形显示 (开始框选图片)
                this.Rect_IsVisible = true;
            }
        });

        private DateTime _lastPixelTime = DateTime.MinValue;

        /// <summary>
        /// 图片 点 鼠标移动
        /// </summary>
        public ICommand Img_PointerMoved => new Command((param) =>
        {
            if (param == null)
            {
                return;
            }
            var parameters = (CommandParameters)param;
            var eventArgs = (PointerEventArgs)parameters.EventArgs;

            var point = eventArgs.GetPosition(null);

            // 矩形已经显示 (正在框选图片)
            if (this.Rect_IsVisible)
            {
                var width = point.X - this._startPoint.X - 1;
                var height = point.Y - this._startPoint.Y - 1;
                if (width > 0 && height > 0)
                {
                    // 设置矩形的宽高
                    this.RectWidth = width;
                    this.RectHeight = height;
                }
            }

            // 操作放大镜

            // 降低放大镜的刷新频率, 提高软件性能 (刷新过快也没啥用)
            var now = DateTime.UtcNow;
            if ((now - _lastPixelTime).TotalMilliseconds < this.LoupeRefreshInterval) // 间隔
                return;

            _lastPixelTime = now;

            var position = eventArgs.GetPosition((Image)parameters.Sender);

            var imgPoint = new Point(
                Math.Floor(position.X / this.ScaleFactor),
                Math.Floor(position.Y / this.ScaleFactor));

            this.PointX = (int)imgPoint.X;
            this.PointY = (int)imgPoint.Y;

            // 获取当前点的颜色
            var color = GraphicHelper.GetPixel(this.PointX, this.PointY);
            this.PointColor = "#" + color[0].ToString("X2") + color[1].ToString("X2") + color[2].ToString("X2");

            var sx = this.PointX - 7;
            var sy = this.PointY - 7;

            // 获取 15x15 大小的矩阵颜色
            var colors = new List<byte[]>();
            for (var j = 0; j < 15; j++)
            {
                for (var i = 0; i < 15; i++)
                {
                    var x = sx + i;
                    var y = sy + j;

                    if (x >= 0 && y >= 0 && x < this.ImgWidth && y < this.ImgHeight)
                    {
                        // 获取颜色
                        colors.Add(GraphicHelper.GetPixel(x, y));
                    }
                    else
                    {
                        // 出界则为黑色
                        colors.Add(new byte[] { 255, 250, 250 });
                    }
                }
            }
            // 放大镜 显示颜色矩阵
            this.LoupeWriteBmp.WriteColor(colors);
            // 刷新图片
            MainWindow.MyLoupeImg?.InvalidateVisual();
        });

        private DateTime _addColorInfoTime = DateTime.Now;

        /// <summary>
        /// 图片 点 鼠标释放
        /// </summary>
        public ICommand Img_PointerReleased => new Command((param) =>
        {
            if (param == null)
            {
                return;
            }

            var parameters = (CommandParameters)param;

            // 矩形已经显示 (正在框选图片)
            if (this.Rect_IsVisible)
            {
                var eventArgs = (PointerEventArgs)parameters.EventArgs;
                var position = eventArgs.GetPosition((Image)parameters.Sender);

                // 当前鼠标的结束点
                var endPoint = new Point(Math.Floor(position.X / this.ScaleFactor), Math.Floor(position.Y / this.ScaleFactor));

                // 用 结束点 减去宽高, 得到起始点
                var startX = (int)(endPoint.X - Math.Floor(this.RectWidth / this.ScaleFactor));
                var startY = (int)(endPoint.Y - Math.Floor(this.RectHeight / this.ScaleFactor));

                // 移动距离大,是在框选
                if (this.RectWidth > 10 && this.RectHeight > 10)
                {
                    // 框选的范围 (用于填写到文本框)
                    this.Rect = string.Format("[{0},{1},{2},{3}]", startX, startY, Math.Min(endPoint.X, this.ImgWidth - 1), Math.Min(endPoint.Y, this.ImgHeight - 1));
                }
                else
                {
                    // 移动距离较小,是在添加颜色

                    // 两次添加颜色信息的间隔大于 200 毫秒,则本次正常添加
                    if ((DateTime.Now - this._addColorInfoTime).TotalMilliseconds > 200)
                    {
                        // 记录本次添加颜色的时间
                        this._addColorInfoTime = DateTime.Now;

                        // 获取颜色
                        var color = GraphicHelper.GetPixel(startX, startY);

                        if (this.ColorInfos.Count == 0)
                        {
                            ColorInfo.Width = this.ImgWidth;
                            ColorInfo.Height = this.ImgHeight;
                        }

                        var anchor = AnchorMode.None;

                        var quarterWidth = this.ImgWidth / 4;

                        if (startX > quarterWidth * 3)
                        {
                            anchor = AnchorMode.Right;
                        }
                        else if (startX > quarterWidth)
                        {
                            anchor = AnchorMode.Center;
                        }
                        else
                        {
                            anchor = AnchorMode.Left;
                        }

                        // 添加 颜色信息
                        this.ColorInfos.Add(new ColorInfo(this.ColorInfos.Count, anchor, startX, startY, color));

                        // 增加 表格的高度
                        this.DataGridHeight = (this.ColorInfos.Count + 1) * 40;
                    }
                }
            }
            // 隐藏矩形 (结束框选图片)
            this.Rect_IsVisible = false;
            this.RectWidth = 0;
            this.RectHeight = 0;
            this.RectMargin = new Thickness(0, 0, 0, 0);
        });

        /// <summary>
        /// 模式配置中的数据
        /// </summary>
        public ICommand GetList => new Command(async (param) =>
        {
            // 已经选择了模式
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

        /// <summary>
        /// 模拟器 选择
        /// </summary>
        /// <param name="value"></param>
        public async void Emulator_Selected(int value)
        {
            try
            {
                if (ScreenshotHelperBridge.State == LinkState.success)
                {
                    // 已经连接过,则直接保存
                    ScreenshotHelperBridge.Index = value;
                }
                else if (ScreenshotHelperBridge.State == LinkState.Starting)
                {
                    // 已经保存过,则改变为 成功状态
                    ScreenshotHelperBridge.State = LinkState.success;
                }
                else if (ScreenshotHelperBridge.State == LinkState.Waiting) // 已经初始化
                {
                    // 设置鼠标样式为 转圈 繁忙
                    this.WindowCursor = new Cursor(StandardCursorType.Wait);

                    // 保存选中的项
                    ScreenshotHelperBridge.Changed(value);

                    // 设备列表
                    this.EmulatorInfo = await ScreenshotHelperBridge.Initialize();

                    this.EmulatorSelectedIndex = -1;

                    // 目标模式 截屏成功回调
                    ScreenshotHelperBridge.Helpers[ScreenshotHelperBridge.Select].OnSuccessed = new Action<Bitmap>((bitmap) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            // 设置当前显示的图片
                            this.Img = bitmap;

                            // 创建一个 tab, 用于切换多张图片
                            var item = new TabItem(this.Img);

                            // 关闭 tab 的回调函数
                            item.Command = new Command((param) =>
                            {
                                this.TabItems.Remove(item);
                            });

                            // 添加一个 tab 按钮
                            this.TabItems.Add(item);

                            // 当前选中的 tab 为 新图片
                            this.TabControlSelectedIndex = this.TabItems.Count - 1;

                            // 设置鼠标样式为 普通箭头
                            this.WindowCursor = new Cursor(StandardCursorType.Arrow);
                        });
                    });

                    // 目标模式 截屏失败回调
                    ScreenshotHelperBridge.Helpers[ScreenshotHelperBridge.Select].OnFailed = new Action<string>((errorMessage) =>
                    {
                        MessageBoxWindow.ShowAsync(errorMessage);

                        // 设置鼠标样式为 普通箭头
                        this.WindowCursor = new Cursor(StandardCursorType.Arrow);
                    });

                    // 没有获取到设备列表,则重置
                    if (this.EmulatorInfo.Count == 0)
                    {
                        ResetEmulatorOptions_Click();
                    }
                }

            }
            catch (Exception e)
            {
                this.EmulatorSelectedIndex = -1;
                ScreenshotHelperBridge.Dispose();
                this.EmulatorInfo?.Clear();
                this.EmulatorInfo = ScreenshotHelperBridge.Init();
                MessageBoxWindow.ShowAsync(e.ToString());
            }
            // 设置鼠标样式为 普通箭头
            this.WindowCursor = new Cursor(StandardCursorType.Arrow);
        }

        /// <summary>
        /// 截屏_点击事件
        /// </summary>
        public void ScreenShot_Click()
        {
            try
            {
                this.WindowCursor = new Cursor(StandardCursorType.Wait);
                if (ScreenshotHelperBridge.Select == -1
                    || ScreenshotHelperBridge.Index == -1 ||
                    ScreenshotHelperBridge.Info[ScreenshotHelperBridge.Index].Value == "null")
                {
                    MessageBoxWindow.ShowAsync("请先配置 -> (模拟器/tcp/句柄)");
                    this.WindowCursor = new Cursor(StandardCursorType.Arrow);
                    return;
                }

                // 调用截屏, 最后会调用 成功 / 失败 的回调函数
                ScreenshotHelperBridge.ScreenShot();

            }
            catch (Exception ex)
            {
                MessageBoxWindow.ShowAsync(ex.ToString());
            }
        }

        /// <summary>
        /// 右键菜单_重置模式选择_点击事件
        /// </summary>
        public void ResetEmulatorOptions_Click()
        {
            if (ScreenshotHelperBridge.State == LinkState.Starting || ScreenshotHelperBridge.State == LinkState.success)
            {
                this.EmulatorSelectedIndex = -1;
            }
            ScreenshotHelperBridge.Dispose();
            this.EmulatorInfo.Clear();

            // 获取模式列表
            this.EmulatorInfo = ScreenshotHelperBridge.Init();
        }

        /// <summary>
        /// 图片右转_点击事件
        /// </summary>
        public async void TurnRight_Click()
        {
            if (this.Img == null)
            {
                return;
            }
            this.Img = await GraphicHelper.TurnRight();
        }

        /// <summary>
        /// 拖放图片_事件 (拖放一个图片到窗口,可以快速打开)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DropImage_Event(object? sender, DragEventArgs e)
        {
            try
            {
                foreach (var item in e.Data.GetFiles())
                {
                    var filePath = item.TryGetLocalPath();

                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        // 显示图片到界面
                        this.Img = new Bitmap(stream);

                        stream.Position = 0; // 重置到 0 位置,再去解析, 否则读不到. ( 估计上面的 new Bitmap() 会操作这个 Position)
                        using var sKBitmap = SKBitmap.Decode(stream);

                        // 保存图片到内存中
                        GraphicHelper.KeepScreen(sKBitmap);

                        // 添加一个 tab
                        var tabItem = new TabItem(this.Img);
                        tabItem.Command = new Command((param) =>
                        {
                            this.TabItems.Remove(tabItem);
                        });
                        this.TabItems.Add(tabItem);
                        this.TabControlSelectedIndex = this.TabItems.Count - 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxWindow.ShowAsync(ex.ToString());
            }
        }

        /// <summary>
        /// 加载_点击事件
        /// </summary>
        public async void Load_Click()
        {
            try
            {
                var tl = Ioc.Default.GetService<TopLevel>();
                var fileList = await tl.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions()
                {
                    Title = "请选择文件",
                    AllowMultiple = false,
                    FileTypeFilter = new List<FilePickerFileType>()
                    {
                        new FilePickerFileType("位图文件")
                        {
                            Patterns = new List<string>()
                            {
                                "*.png","*.bmp","*.jpg"
                            }
                        }
                    }
                });

                var fileName = string.Empty;
                if (fileList.Count > 0)
                {
                    // 文件的完整地址
                    fileName = fileList[0].Path.LocalPath;
                }

                if (!string.IsNullOrWhiteSpace(fileName))
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
                MessageBoxWindow.ShowAsync(e.ToString());
            }
        }

        /// <summary>
        /// 保存_点击事件
        /// </summary>
        public async void Save_Click()
        {
            if (this.Img == null)
            {
                return;
            }

            try
            {
                var tl = Ioc.Default.GetService<TopLevel>();
                var storageFile = await tl.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
                {
                    SuggestedFileName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"),
                    Title = "保存文件",
                    FileTypeChoices = new List<FilePickerFileType>()
                    {
                        new FilePickerFileType("位图文件")
                        {
                            Patterns = new List<string>(){"*.png", "*.bmp", "*.jpg"}
                        }
                    }
                });
                if (storageFile != null)
                {
                    var filePath = storageFile.TryGetLocalPath();
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        this.Img.Save(filePath);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBoxWindow.ShowAsync(e.ToString());
            }
        }

        /// <summary>
        /// 测试_点击事件
        /// </summary>
        public async void Test_Click()
        {
            if (this.Img != null && this.ColorInfos.Count > 0)
            {
                var sims = new int[] { 100, 95, 90, 85, 80, 0 };
                // 比色
                if (this.CurrentFormat.IsCompareMode is true)
                {
                    CompareResult result;

                    // 锚点比色
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
                        MessageBoxWindow.ShowAsync(result.ErrorMessage);
                    }
                    this.TestResult = result.Result.ToString();
                }
                else
                {
                    // 找色

                    if (this.ColorInfos.Count < 2)
                    {
                        MessageBoxWindow.ShowAsync("错误", "多点找色至少需要勾选两个颜色才可进行测试!");
                        this.TestResult = "error";
                        return;
                    }

                    Point result;

                    // 锚点找色
                    if (this.CurrentFormat.AnchorIsEnabled is true)
                    {
                        var width = ColorInfo.Width;
                        var height = ColorInfo.Height;
                        var testStr = CreateColorStrHelper.Create(FormatMode.AnchorsFindStrTest, this.ColorInfos);
                        result = GraphicHelper.AnchorsFindColor(new MyRange(0, 0, width - 1, height - 1), width, height, testStr.Trim('"'), sims[this.SimSelectedIndex]);
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

        /// <summary>
        /// 生成_点击事件
        /// </summary>
        public void Create_Click()
        {
            if (this.ColorInfos.Count > 0)
            {
                // 获取范围
                var rect = GetRange();

                if (this.Rect.IndexOf("[") != -1)
                {
                    this.Rect = string.Format("[{0}]", rect.ToString());
                }
                else if (FormatConfig.GetFormat(this.FormatItems[this.FormatSelectedIndex])!.AnchorIsEnabled is true)
                {
                    this.Rect = rect.ToString(2);
                }
                else
                {
                    this.Rect = rect.ToString();
                }

                this.CreateStr = CreateColorStrHelper.Create(this.CurrentFormat?.Name, this.ColorInfos, rect);

                // 复制
                CreateStr_Copy_Click();
            }
        }

        /// <summary>
        /// 复制_点击事件
        /// </summary>
        public async void CreateStr_Copy_Click()
        {
            try
            {
                var tl = Ioc.Default.GetService<TopLevel>();
                await tl.Clipboard.SetTextAsync(this.CreateStr);
            }
            catch (Exception ex)
            {
                MessageBoxWindow.ShowAsync("设置剪贴板失败 , 你的剪贴板可能被其他软件占用\r\n\r\n" + ex.Message, "error");
            }
        }

        /// <summary>
        /// 清空_点击事件
        /// </summary>
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

        /// <summary>
        /// 快捷键_添加颜色
        /// </summary>
        public ICommand Key_AddColorInfo => new Command((param) =>
        {
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

        /// <summary>
        /// 快捷键_设置图片缩放
        /// </summary>
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

        /// <summary>
        /// 快捷键_导入剪贴板数据 (文件/文字)
        /// </summary>
        public async void Key_GetClipboardData()
        {
            try
            {
                var tl = Ioc.Default.GetService<TopLevel>();
                var formats = await tl.Clipboard.GetFormatsAsync();

                var fileName = string.Empty;

                if (Array.IndexOf(formats, "FileNames") != -1)
                {
                    var fileNames = (List<string>)await tl.Clipboard.GetDataAsync(DataFormats.FileNames);
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
                    var text = await tl.Clipboard.GetTextAsync();
                    if (!string.IsNullOrWhiteSpace(text))
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
                MessageBoxWindow.ShowAsync(ex.ToString());
            }
        }

        /// <summary>
        /// 快捷键_清空颜色列表
        /// </summary>
        public void Key_ColorInfo_Clear()
        {
            this.ColorInfos.Clear();
            this.DataGridHeight = 40;
        }

        /// <summary>
        /// 快捷键_打开设置界面
        /// </summary>
        public async void Key_SetConfig()
        {
            var config = new ConfigWindow();
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

        /// <summary>
        /// 右键菜单_复制范围_点击事件
        /// </summary>
        public async void Rect_Copy_Click()
        {
            try
            {
                var tl = Ioc.Default.GetService<TopLevel>();
                await tl.Clipboard.SetTextAsync(this.Rect);
            }
            catch (Exception ex)
            {
                MessageBoxWindow.ShowAsync("设置剪贴板失败 , 你的剪贴板可能被其他软件占用\r\n\r\n" + ex.Message, "error");
            }
        }

        /// <summary>
        /// 右键菜单_清空范围_点击事件
        /// </summary>
        public void Rect_Clear_Click()
        {
            this.Rect = string.Empty;
        }

        /// <summary>
        /// 右键菜单_复制坐标点_点击事件
        /// </summary>
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

                var tl = Ioc.Default.GetService<TopLevel>();
                await tl.Clipboard.SetTextAsync(pointStr);
            }
            catch (Exception ex)
            {
                MessageBoxWindow.ShowAsync("设置剪贴板失败\r\n\r\n" + ex.Message, "错误");
            }
        }

        /// <summary>
        /// 右键菜单_复制颜色_点击事件
        /// </summary>
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

                var tl = Ioc.Default.GetService<TopLevel>();
                await tl.Clipboard.SetTextAsync(hexColor);
            }
            catch (Exception ex)
            {
                MessageBoxWindow.ShowAsync("设置剪贴板失败\r\n\r\n" + ex.Message, "错误");
            }
        }

        /// <summary>
        /// 右键菜单_重置颜色_点击事件
        /// </summary>
        public void ColorInfo_Reset_Click()
        {
            var temp = new ObservableCollection<ColorInfo>();

            foreach (var colorInfo in this.ColorInfos)
            {
                var x = (int)colorInfo.Point.X;
                var y = (int)colorInfo.Point.Y;
                var color = GraphicHelper.GetPixel(x, y);
                colorInfo.Color = Avalonia.Media.Color.FromRgb(color[0], color[1], color[2]);
                if (x >= this.ImgWidth || y >= this.ImgHeight)
                {
                    colorInfo.IsChecked = false;
                }
                temp.Add(colorInfo);
            }

            this.ColorInfos = temp;

        }

        /// <summary>
        /// 右键菜单_删除选中的颜色_点击事件
        /// </summary>
        public void ColorInfo_SelectItemClear_Click()
        {
            if (this.DataGridSelectedIndex == -1 || this.DataGridSelectedIndex > this.ColorInfos.Count)
            {
                return;
            }
            this.ColorInfos.RemoveAt(this.DataGridSelectedIndex);
            this.DataGridHeight = (this.ColorInfos.Count + 1) * 40;
        }

        /// <summary>
        /// 截图按钮_点击事件 (编辑图片)
        /// </summary>
        public async void CutImg_Click()
        {
            var range = GetRange();
            var imgEditor = new ImgEditorWindow(range, GraphicHelper.GetRectData(range));
            await imgEditor.ShowDialog(MainWindow.Instance);
            if (ImgEditorWindow.Result_ACK && ImgEditorWindow.ResultColorInfos != null && ImgEditorWindow.ResultColorInfos.Count != 0)
            {
                this.ColorInfos = new ObservableCollection<ColorInfo>(ImgEditorWindow.ResultColorInfos);
                ImgEditorWindow.ResultColorInfos.Clear();
                ImgEditorWindow.Result_ACK = false;
                this.DataGridHeight = (this.ColorInfos.Count + 1) * 40;
            }
        }

        /// <summary>
        /// 获取范围
        /// </summary>
        /// <returns></returns>
        private MyRange GetRange()
        {
            //if (ColorInfos.Count == 0)
            //{
            //    return new MyRange(0, 0, ImgWidth - 1, ImgHeight - 1);
            //}
            if (this.Rect != string.Empty)
            {
                if (this.Rect.IndexOf("[") != -1)
                {
                    var range = this.Rect.TrimStart('[').TrimEnd(']').Split(',');

                    return new MyRange(int.Parse(range[0].Trim()), int.Parse(range[1].Trim()), int.Parse(range[2].Trim()), int.Parse(range[3].Trim()));
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
            return new MyRange(left >= tolerance ? left - tolerance : 0, top >= tolerance ? top - tolerance : 0, right + tolerance > imgWidth ? imgWidth : right + tolerance, bottom + tolerance > imgHeight ? imgHeight : bottom + tolerance, mode_1, mode_2);
        }
    }

}
