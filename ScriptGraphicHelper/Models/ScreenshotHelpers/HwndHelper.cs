using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using ScriptGraphicHelper.Views;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    /// <summary>
    /// 大漠句柄
    /// </summary>
    public class HwndHelper : BaseHelper
    {
        public override Action<Bitmap>? OnSuccessed { get; set; }
        public override Action<string>? OnFailed { get; set; }
        public override string Path { get; } = "大漠句柄";
        public override string Name { get; } = "大漠句柄";

        private bool Inited = false;

        Dmsoft Dm;

        public override void Close()
        {
            try
            {
                this.Dm?.UnBindWindow();
            }
            catch { }
        }

        public override bool IsStart(int Index)
        {
            return this.Inited;
        }

        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            this.Dm = Dmsoft.Instance;
            this.Dm.Hwnd = -1;
            HwndConfig config = new();
            await config.ShowDialog(MainWindow.Instance);
            return await GetList();
        }

        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            return await Task.Run(() =>
            {
                List<KeyValuePair<int, string>> result = new();
                if (this.Dm.Hwnd == -1)
                {
                    result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                    return result;
                }
                if (this.Dm.BindWindowEx() == 1)
                {
                    this.Inited = true;
                    result.Add(new KeyValuePair<int, string>(key: 0, value: this.Dm.Hwnd.ToString() + "-" + this.Dm.Display));
                    return result;
                }
                result.Add(new KeyValuePair<int, string>(key: 0, value: "null"));
                return result;

            });
        }

        public override async void ScreenShot(int Index)
        {
            if (Index == -1 || !this.Inited)
            {
                throw new Exception("请先选择窗口句柄!");
            }
            await Task.Run(() =>
             {
                 var point = this.Dm.GetClientSize();
                 var width = (int)point.X;
                 var height = (int)point.Y;
                 var data = this.Dm.GetScreenData(width, height);

                 SKBitmap sKBitmap = new(new SKImageInfo(width, height));

                 var pxFormat = data.Length / height / width;

                 var dataStep = 0;

                 unsafe
                 {
                     var intPtr = (byte*)sKBitmap.GetPixels();
                     for (var y = 0; y < height; y++)
                     {
                         var intPtrStep = y * width * 4;

                         for (var x = 0; x < width; x++)
                         {
                             intPtr[intPtrStep] = data[dataStep];
                             intPtr[intPtrStep + 1] = data[dataStep + 1];
                             intPtr[intPtrStep + 2] = data[dataStep + 2];

                             if (pxFormat == 3)
                             {
                                 intPtr[intPtrStep + 3] = 255;
                                 dataStep += 3;
                             }
                             else
                             {
                                 intPtr[intPtrStep + 3] = data[dataStep + 3];
                                 dataStep += 4;
                             }

                             intPtrStep += 4;
                         }
                     }
                 }

                 Marshal.Copy(data, 0, sKBitmap.GetPixels(), data.Length);
                 GraphicHelper.KeepScreen(sKBitmap);
                 var bitmap = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(width, height), new Vector(96, 96), sKBitmap.RowBytes);
                 sKBitmap.Dispose();
                 this.OnSuccessed?.Invoke(bitmap);
             }).ContinueWith((t) =>
             {
                 if (t.Exception != null)
                     this.OnFailed?.Invoke(t.Exception.ToString());
             });
        }
    }
}
