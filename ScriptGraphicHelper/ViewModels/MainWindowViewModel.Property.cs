using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Cursor windowCursor = new(StandardCursorType.Arrow);
        public Cursor WindowCursor
        {
            get => this.windowCursor;
            set => this.RaiseAndSetIfChanged(ref this.windowCursor, value);
        }

        private double windowWidth = 1720d;
        public double WindowWidth
        {
            get => this.windowWidth;
            set => this.RaiseAndSetIfChanged(ref this.windowWidth, value);
        }

        private double windowHeight = 900d;
        public double WindowHeight
        {
            get => this.windowHeight;
            set => this.RaiseAndSetIfChanged(ref this.windowHeight, value);
        }

        private int emulatorSelectedIndex = 0;
        public int EmulatorSelectedIndex
        {
            get => this.emulatorSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref this.emulatorSelectedIndex, value);
                Emulator_Selected(value);
            }
        }

        private int simSelectedIndex = 0;
        public int SimSelectedIndex
        {
            get => this.simSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref this.simSelectedIndex, value);
                Settings.Instance.SimSelectedIndex = value;
            }
        }

        private string testResult = string.Empty;
        public string TestResult
        {
            get => this.testResult;
            set => this.RaiseAndSetIfChanged(ref this.testResult, value);
        }

        private string rect = string.Empty;
        public string Rect
        {
            get => this.rect;
            set => this.RaiseAndSetIfChanged(ref this.rect, value);
        }

        private string createStr = string.Empty;
        public string CreateStr
        {
            get => this.createStr;
            set => this.RaiseAndSetIfChanged(ref this.createStr, value);
        }

        private ObservableCollection<string> emulatorInfo;
        public ObservableCollection<string> EmulatorInfo
        {
            get => this.emulatorInfo;
            set => this.RaiseAndSetIfChanged(ref this.emulatorInfo, value);
        }

        private int titleBarWidth;
        public int TitleBarWidth
        {
            get => this.titleBarWidth;
            set => this.RaiseAndSetIfChanged(ref this.titleBarWidth, value);
        }

        private TabItems<TabItem> tabItems = new();
        public TabItems<TabItem> TabItems
        {
            get => this.tabItems;
            set => this.RaiseAndSetIfChanged(ref this.tabItems, value);
        }

        private int tabControlSelectedIndex;
        public int TabControlSelectedIndex
        {
            get => this.tabControlSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref this.tabControlSelectedIndex, value);
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
        }

        private Bitmap img;
        public Bitmap Img
        {
            get => this.img;
            set
            {
                this.RaiseAndSetIfChanged(ref this.img, value);
                this.ImgWidth = value.Size.Width;
                this.ImgHeight = value.Size.Height;
            }
        }

        private Thickness imgMargin = new(220, 50, 280, 20);
        public Thickness ImgMargin
        {
            get => this.imgMargin;
            set => this.RaiseAndSetIfChanged(ref this.imgMargin, value);
        }

        private double imgWidth = 0;
        private double ImgWidth
        {
            get => this.imgWidth;
            set
            {
                this.imgWidth = value;
                this.ImgDrawWidth = Math.Floor(value * this.ScaleFactor);
            }
        }

        private double imgHeight = 0;
        private double ImgHeight
        {
            get => this.imgHeight;
            set
            {
                this.imgHeight = value;
                this.ImgDrawHeight = Math.Floor(value * this.ScaleFactor);
            }
        }

        private double imgDrawWidth = 0;
        public double ImgDrawWidth
        {
            get => this.imgDrawWidth;
            set => this.RaiseAndSetIfChanged(ref this.imgDrawWidth, value);
        }

        private double imgDrawHeight = 0;
        public double ImgDrawHeight
        {
            get => this.imgDrawHeight;
            set => this.RaiseAndSetIfChanged(ref this.imgDrawHeight, value);
        }

        private double scaleFactor = 1.0;
        /// <summary>
        /// 缩放比例
        /// </summary>
        public double ScaleFactor
        {
            get => this.scaleFactor;
            set
            {
                this.ImgDrawWidth = Math.Floor(this.ImgWidth * value);
                this.ImgDrawHeight = Math.Floor(this.ImgHeight * value);
                this.RaiseAndSetIfChanged(ref this.scaleFactor, value);
            }
        }

        private WriteableBitmap loupeWriteBmp;
        public WriteableBitmap LoupeWriteBmp
        {
            get => this.loupeWriteBmp;
            set => this.RaiseAndSetIfChanged(ref this.loupeWriteBmp, value);
        }

        private bool loupe_IsVisible = false;
        public bool Loupe_IsVisible
        {
            get => this.loupe_IsVisible;
            set => this.RaiseAndSetIfChanged(ref this.loupe_IsVisible, value);
        }

        private Thickness loupeMargin;
        public Thickness LoupeMargin
        {
            get => this.loupeMargin;
            set => this.RaiseAndSetIfChanged(ref this.loupeMargin, value);
        }

        private int pointX = 0;
        public int PointX
        {
            get => this.pointX;
            set => this.RaiseAndSetIfChanged(ref this.pointX, value);
        }

        private int pointY = 0;
        public int PointY
        {
            get => this.pointY;
            set => this.RaiseAndSetIfChanged(ref this.pointY, value);
        }

        private string pointColor;

        public string PointColor
        {
            get => this.pointColor;
            set => this.RaiseAndSetIfChanged(ref this.pointColor, value);
        }


        private double rectWidth = 0;
        public double RectWidth
        {
            get => this.rectWidth;
            set => this.RaiseAndSetIfChanged(ref this.rectWidth, value);
        }

        private double rectHeight = 0;
        public double RectHeight
        {
            get => this.rectHeight;
            set => this.RaiseAndSetIfChanged(ref this.rectHeight, value);
        }

        private Thickness rectMargin;
        public Thickness RectMargin
        {
            get => this.rectMargin;
            set => this.RaiseAndSetIfChanged(ref this.rectMargin, value);
        }

        private bool rect_IsVisible = false;
        public bool Rect_IsVisible
        {
            get => this.rect_IsVisible;
            set => this.RaiseAndSetIfChanged(ref this.rect_IsVisible, value);
        }

        private Thickness findedPoint_Margin;
        public Thickness FindedPoint_Margin
        {
            get => this.findedPoint_Margin;
            set => this.RaiseAndSetIfChanged(ref this.findedPoint_Margin, value);
        }

        private bool findedPoint_IsVisible = false;
        public bool FindedPoint_IsVisible
        {
            get => this.findedPoint_IsVisible;
            set => this.RaiseAndSetIfChanged(ref this.findedPoint_IsVisible, value);
        }

        private ObservableCollection<ColorInfo> colorInfos;
        public ObservableCollection<ColorInfo> ColorInfos
        {
            get => this.colorInfos;
            set => this.RaiseAndSetIfChanged(ref this.colorInfos, value);
        }

        private int dataGridSelectedIndex;
        public int DataGridSelectedIndex
        {
            get => this.dataGridSelectedIndex;
            set => this.RaiseAndSetIfChanged(ref this.dataGridSelectedIndex, value);
        }

        private int dataGridHeight;
        public int DataGridHeight
        {
            get => this.dataGridHeight;
            set
            {
                if (value > 1000)
                {
                    value = 1000;
                }
                this.RaiseAndSetIfChanged(ref this.dataGridHeight, value);
            }
        }

        private bool dataGrid_IsVisible = true;
        public bool DataGrid_IsVisible
        {
            get => this.dataGrid_IsVisible;
            set => this.RaiseAndSetIfChanged(ref this.dataGrid_IsVisible, value);
        }


        private List<string> formatItems;
        public List<string> FormatItems
        {
            get => this.formatItems;
            set => this.formatItems = value;
        }

        private FormatConfig CurrentFormat;

        private int formatSelectedIndex = 0;
        public int FormatSelectedIndex
        {
            get => this.formatSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref this.formatSelectedIndex, value);
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

}
