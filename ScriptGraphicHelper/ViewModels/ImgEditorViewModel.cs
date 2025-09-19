using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.ViewModels.Core;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScriptGraphicHelper.ViewModels
{
    public class ImgEditorViewModel : ViewModelBase
    {
        private int windowWidth;
        public int WindowWidth
        {
            get => this.windowWidth;
            set => this.RaiseAndSetIfChanged(ref this.windowWidth, value);
        }

        private int windowHeight;
        public int WindowHeight
        {
            get => this.windowHeight;
            set => this.RaiseAndSetIfChanged(ref this.windowHeight, value);
        }

        private WriteableBitmap drawBitmap;
        public WriteableBitmap DrawBitmap
        {
            get => this.drawBitmap;
            set => this.RaiseAndSetIfChanged(ref this.drawBitmap, value);
        }

        private int imgWidth;
        public int ImgWidth
        {
            get => this.imgWidth;
            set => this.RaiseAndSetIfChanged(ref this.imgWidth, value);
        }

        private int imgHeight;
        public int ImgHeight
        {
            get => this.imgHeight;
            set => this.RaiseAndSetIfChanged(ref this.imgHeight, value);
        }

        private Color srcColor = Colors.White;
        public Color SrcColor
        {
            get => this.srcColor;
            set => this.RaiseAndSetIfChanged(ref this.srcColor, value);
        }

        private Color destColor = Colors.Red;
        public Color DestColor
        {
            get => this.destColor;
            set => this.RaiseAndSetIfChanged(ref this.destColor, value);
        }

        private bool pen_IsChecked;
        public bool Pen_IsChecked
        {
            get => this.pen_IsChecked;
            set => this.RaiseAndSetIfChanged(ref this.pen_IsChecked, value);
        }

        private int tolerance = 5;
        public int Tolerance
        {
            get => this.tolerance;
            set
            {
                this.RaiseAndSetIfChanged(ref this.tolerance, value);
                this.DrawBitmap = ImgEditorHelper.ResetImg();
                this.DrawBitmap.SetPixels(this.SrcColor, this.destColor, this.Tolerance, this.reverse_IsChecked);
                this.ImgWidth -= 1;
                this.ImgWidth += 1;
            }
        }

        private bool reverse_IsChecked;
        public bool Reverse_IsChecked
        {
            get => this.reverse_IsChecked;
            set => this.RaiseAndSetIfChanged(ref this.reverse_IsChecked, value);
        }

        private bool getColorInfosBtnState;
        public bool GetColorInfosBtnState
        {
            get => this.getColorInfosBtnState;
            set => this.RaiseAndSetIfChanged(ref this.getColorInfosBtnState, value);
        }

        private int getColorInfosModeSelectedIndex;
        public int GetColorInfosModeSelectedIndex
        {
            get => this.getColorInfosModeSelectedIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref this.getColorInfosModeSelectedIndex, value);
                Settings.Instance.ImgEditor.ModeSelectedIndex = value;
            }
        }

        private int getColorInfosThreshold;
        public int GetColorInfosThreshold
        {
            get => this.getColorInfosThreshold;
            set
            {
                this.RaiseAndSetIfChanged(ref this.getColorInfosThreshold, value);
                Settings.Instance.ImgEditor.Threshold = value;
            }
        }

        private int getColorInfosSize;
        public int GetColorInfosSize
        {
            get => this.getColorInfosSize;
            set
            {
                this.RaiseAndSetIfChanged(ref this.getColorInfosSize, value);
                Settings.Instance.ImgEditor.Size = value;
            }
        }

        public ImgEditorViewModel(Models.Range range, byte[] data)
        {
            this.DrawBitmap = ImgEditorHelper.Init(range, data);
            this.ImgWidth = (int)this.DrawBitmap.Size.Width * 5;
            this.ImgHeight = (int)this.DrawBitmap.Size.Height * 5;
            this.WindowWidth = this.ImgWidth + 320;
            this.WindowHeight = this.ImgHeight + 40;

            this.GetColorInfosBtnState = true;
            this.GetColorInfosModeSelectedIndex = Settings.Instance.ImgEditor.ModeSelectedIndex;
            this.GetColorInfosSize = Settings.Instance.ImgEditor.Size;
            this.GetColorInfosThreshold = Settings.Instance.ImgEditor.Threshold;

            ImgEditorHelper.StartX = (int)range.Left;
            ImgEditorHelper.StartY = (int)range.Top;
        }

        public void CutImg_Click()
        {
            this.DrawBitmap = this.DrawBitmap.CutImg();
            this.ImgWidth = (int)this.DrawBitmap.Size.Width * 5;
            this.ImgHeight = (int)this.DrawBitmap.Size.Height * 5;

            this.WindowWidth = this.ImgWidth + 320;
            this.WindowHeight = this.ImgHeight + 40;
        }

        public void Reset_Click()
        {
            this.DrawBitmap = ImgEditorHelper.ResetImg();
        }

        public async void Save_Click()
        {
            if (this.DrawBitmap == null)
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
                    if (fileName.IndexOf("bmp") != -1 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var bitmap = this.DrawBitmap.GetBitmap();
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    else
                    {
                        this.DrawBitmap.Save(fileName);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.ToString());
            }
        }


        private bool IsDown = false;
        public ICommand Img_PointerPressed => new Command(async (param) =>
        {
            this.IsDown = true;
            if (this.pen_IsChecked)
            {
                if (param != null)
                {
                    var parameters = (CommandParameters)param;
                    var eventArgs = (PointerPressedEventArgs)parameters.EventArgs;
                    if (eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                    {
                        var point = eventArgs.GetPosition((Image)parameters.Sender);
                        var x = (int)point.X / 5;
                        var y = (int)point.Y / 5;

                        for (var i = -1; i < 2; i++)
                        {
                            for (var j = -1; j < 2; j++)
                            {
                                await this.DrawBitmap.SetPixel(x + i, y + j, this.DestColor);
                            }
                        }
                        var width = (int)this.DrawBitmap.Size.Width * 5;
                        var height = (int)this.DrawBitmap.Size.Height * 5;

                        this.ImgWidth -= 1;
                        this.ImgWidth += 1;
                        //Image控件不会自动刷新, 解决方案是改变一次宽高, 可能是bug https://github.com/AvaloniaUI/Avalonia/issues/1995
                    }
                }
            }

        });

        public ICommand Img_PointerMoved => new Command(async (param) =>
        {
            if (this.pen_IsChecked && this.IsDown)
            {
                if (param != null)
                {
                    var parameters = (CommandParameters)param;
                    var eventArgs = (PointerEventArgs)parameters.EventArgs;
                    var point = eventArgs.GetPosition((Image)parameters.Sender);
                    var x = (int)point.X / 5;
                    var y = (int)point.Y / 5;
                    for (var i = -1; i < 2; i++)
                    {
                        for (var j = -1; j < 2; j++)
                        {
                            await this.DrawBitmap.SetPixel(x + i, y + j, this.DestColor);
                        }
                    }
                    this.ImgWidth -= 1;
                    this.ImgWidth += 1;
                }
            }
        });

        public ICommand Img_PointerReleased => new Command(async (param) =>
        {

            if (this.IsDown && !this.Pen_IsChecked)
            {
                if (param != null)
                {
                    var parameters = (CommandParameters)param;
                    var eventArgs = (PointerEventArgs)parameters.EventArgs;
                    var point = eventArgs.GetPosition((Image)parameters.Sender);
                    var x = (int)point.X / 5;
                    var y = (int)point.Y / 5;
                    this.SrcColor = await this.DrawBitmap.GetPixel(x, y);
                    this.DrawBitmap.SetPixels(this.SrcColor, this.DestColor, this.Tolerance, this.reverse_IsChecked);
                    this.ImgWidth -= 1;
                    this.ImgWidth += 1;
                }
            }
            this.IsDown = false;
        });

        public ICommand GetColorInfos_Click => new Command(async (param) =>
        {
            this.GetColorInfosBtnState = false;

            CutImg_Click();
            if (this.getColorInfosModeSelectedIndex == 0)
            {
                ImgEditor.ResultColorInfos = await this.DrawBitmap.GetAllColorInfos(this.GetColorInfosSize);
            }
            else
            {
                ImgEditor.ResultColorInfos = await this.DrawBitmap.GetColorInfos(this.GetColorInfosSize, this.GetColorInfosThreshold);
            }
            this.ImgWidth -= 1;
            this.ImgWidth += 1;
            await Task.Delay(1000);
            Reset_Click();

            this.GetColorInfosBtnState = true;
        });
    }
}
