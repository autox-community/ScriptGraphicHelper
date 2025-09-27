using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using CommunityToolkit.Mvvm.ComponentModel;

using Newtonsoft.Json.Linq;

using ScriptGraphicHelper.Helpers;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Utils.ViewModel;

using SkiaSharp;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Cursor _windowCursor = new(StandardCursorType.Arrow);

        [ObservableProperty]
        private double _windowWidth = 1720d;

        [ObservableProperty]
        private double _windowHeight = 900d;

        [ObservableProperty]
        private int _emulatorSelectedIndex = -1;

        partial void OnEmulatorSelectedIndexChanged(int value)
        {
            Emulator_Selected(value);
        }

        [ObservableProperty]
        private int _simSelectedIndex = -1;

        partial void OnSimSelectedIndexChanged(int value)
        {
            Settings.Instance.SimSelectedIndex = value;
        }

        [ObservableProperty]
        private string _testResult = string.Empty;

        [ObservableProperty]
        private string _rect = string.Empty;

        [ObservableProperty]
        private string _createStr = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _emulatorInfo;

        [ObservableProperty]
        private int _titleBarWidth;

        [ObservableProperty]
        private TabItems<TabItem> _tabItems = new();

        [ObservableProperty]
        private int _tabControlSelectedIndex;

        partial void OnTabControlSelectedIndexChanged(int value)
        {
            if (value != -1)
            {
                this.Img = this.TabItems[value].Img;
                var stream = new MemoryStream();
                this.Img.Save(stream);
                stream.Position = 0;
                var sKBitmap = SKBitmap.Decode(stream);
                GraphicHelper.KeepScreen(sKBitmap);
                sKBitmap.Dispose();
                stream.Dispose();
            }
            else
            {
                var sKBitmap = new SKBitmap(1, 1);
                GraphicHelper.KeepScreen(sKBitmap);
                this.Img = new Bitmap(GraphicHelper.PxFormat, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(1, 1), new Vector(96, 96), sKBitmap.RowBytes);
                sKBitmap.Dispose();
            }
        }

        [ObservableProperty]
        private Bitmap _img;

        partial void OnImgChanged(Bitmap value)
        {
            this.ImgWidth = value.Size.Width;
            this.ImgHeight = value.Size.Height;
        }

        [ObservableProperty]
        private Thickness _imgMargin = new(220, 50, 280, 20);

        [ObservableProperty]
        private double _imgWidth;

        partial void OnImgWidthChanged(double value)
        {
            this.ImgDrawWidth = Math.Floor(value * this.ScaleFactor);
        }

        [ObservableProperty]
        private double _imgHeight;

        partial void OnImgHeightChanged(double value)
        {
            this.ImgDrawHeight = Math.Floor(value * this.ScaleFactor);
        }

        [ObservableProperty]
        private double _imgDrawWidth;

        [ObservableProperty]
        private double _imgDrawHeight;

        /// <summary>
        /// 主图片缩放系数
        /// </summary>
        [ObservableProperty]
        private double _scaleFactor = 1.0;

        partial void OnScaleFactorChanged(double value)
        {
            this.ImgDrawWidth = Math.Floor(this.ImgWidth * value);
            this.ImgDrawHeight = Math.Floor(this.ImgHeight * value);
        }

        /// <summary>
        /// 放大镜显示的颜色信息
        /// </summary>
        [ObservableProperty]
        private WriteableBitmap _loupeWriteBmp;

        /// <summary>
        /// 放大镜刷新间隔 (毫秒)
        /// </summary>
        [ObservableProperty]
        private int _loupeRefreshInterval = 100;

        [ObservableProperty]
        private int _pointX;

        [ObservableProperty]
        private int _pointY;

        [ObservableProperty]
        private string _pointColor = "#000000";

        [ObservableProperty]
        private double _rectWidth;

        [ObservableProperty]
        private double _rectHeight;

        [ObservableProperty]
        private Thickness _rectMargin;

        [ObservableProperty]
        private bool _rect_IsVisible = false;

        [ObservableProperty]
        private Thickness _findedPoint_Margin;

        [ObservableProperty]
        private bool _findedPoint_IsVisible = false;

        [ObservableProperty]
        private ObservableCollection<ColorInfo> _colorInfos;

        [ObservableProperty]
        private int _dataGridSelectedIndex;

        [ObservableProperty]
        private int _dataGridHeight;

        [ObservableProperty]
        private bool _dataGrid_IsVisible = true;

        [ObservableProperty]
        private List<string> _formatItems;

        /// <summary>
        /// 选择的格式化方案索引
        /// </summary>
        [ObservableProperty]
        private int _formatSelectedIndex = -1;

        // NOTE:
        // 由于 Toolkit 生成的代码会先判断不相等才继续触发set函数,
        // int默认值是0, 所以这里初始值给一个不可能的数,确保首次稳定触发
        private FormatConfig CurrentFormat;

        partial void OnFormatSelectedIndexChanged(int value)
        {
            Settings.Instance.FormatSelectedIndex = value;
            this.CurrentFormat = FormatConfig.GetFormat(this.FormatItems[value])!;
            if (this.CurrentFormat.AnchorIsEnabled is true)
            {
                this.DataGrid_IsVisible = false;
                this.ImgMargin = new Thickness(220, 50, 340, 20);
            }
            else
            {
                this.DataGrid_IsVisible = true;
                this.ImgMargin = new Thickness(220, 50, 280, 20);
            }
        }

    }
}
