using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

using ScriptGraphicHelper.Helpers;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Tools;
using ScriptGraphicHelper.Utils.ViewModel;
using ScriptGraphicHelper.Views;

namespace ScriptGraphicHelper.ViewModels
{
    public partial class ImgEditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int _windowWidth;

        [ObservableProperty]
        private int _windowHeight;

        [ObservableProperty]
        private WriteableBitmap _drawBitmap;

        [ObservableProperty]
        private int _imgWidth;

        [ObservableProperty]
        private int _imgHeight;

        [ObservableProperty]
        private Color _srcColor = Colors.White;

        [ObservableProperty]
        private Color _destColor = Colors.Red;

        [ObservableProperty]
        private bool _pen_IsChecked;

        [ObservableProperty]
        private int _tolerance = 5;
        
        partial void OnToleranceChanged(int value)
        {
            this.DrawBitmap = ImgEditorHelper.ResetImg();
            this.DrawBitmap.SetPixels(this.SrcColor, this.DestColor, this.Tolerance, this.Reverse_IsChecked);
            this.ImgWidth -= 1;
            this.ImgWidth += 1;
        }

        [ObservableProperty]
        private bool _reverse_IsChecked;

        [ObservableProperty]
        private bool _getColorInfosBtnState;

        [ObservableProperty]
        private int _getColorInfosModeSelectedIndex;

        partial void OnGetColorInfosModeSelectedIndexChanged(int value)
        {
            Settings.Instance.ImgEditor.ModeSelectedIndex = value;
        }

        [ObservableProperty]
        private int _getColorInfosThreshold;

        partial void OnGetColorInfosThresholdChanged(int value)
        {
            Settings.Instance.ImgEditor.Threshold = value;
        }

        [ObservableProperty]
        private int _getColorInfosSize;

        partial void OnGetColorInfosSizeChanged(int value)
        {
            Settings.Instance.ImgEditor.Size = value;
        }

        public ImgEditorViewModel(Models.MyRange range, byte[] data)
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
                var tl = IocTools.GetTopLevel();
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
                        this.DrawBitmap.Save(filePath);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBoxWindow.ShowAsync(e.ToString());
            }
        }

        private bool IsDown = false;
        public ICommand Img_PointerPressed => new Command(async (param) =>
        {
            this.IsDown = true;
            if (this.Pen_IsChecked)
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
                        // TODO:
                        //Image控件不会自动刷新, 解决方案是改变一次宽高, 可能是bug https://github.com/AvaloniaUI/Avalonia/issues/1995
                    }
                }
            }

        });

        public ICommand Img_PointerMoved => new Command(async (param) =>
        {
            if (this.Pen_IsChecked && this.IsDown)
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
                    this.DrawBitmap.SetPixels(this.SrcColor, this.DestColor, this.Tolerance, this.Reverse_IsChecked);
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
            if (this.GetColorInfosModeSelectedIndex == 0)
            {
                ImgEditorWindow.ResultColorInfos = await this.DrawBitmap.GetAllColorInfos(this.GetColorInfosSize);
            }
            else
            {
                ImgEditorWindow.ResultColorInfos = await this.DrawBitmap.GetColorInfos(this.GetColorInfosSize, this.GetColorInfosThreshold);
            }
            this.ImgWidth -= 1;
            this.ImgWidth += 1;
            await Task.Delay(1000);
            Reset_Click();

            this.GetColorInfosBtnState = true;
        });
    }
}
