using Avalonia.Media.Imaging;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    class AdbHelper : BaseHelper
    {
        public override Action<Bitmap>? OnSuccessed { get; set; }
        public override Action<string>? OnFailed { get; set; }
        public override string Path { get; } = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/") + "assets/adb/";
        public override string Name { get; } = "Adb连接";

        private List<KeyValuePair<int, string>> DeviceInfos = new();

        public AdbHelper()
        {
            if (!Directory.Exists(this.Path + "/screenshot"))
            {
                Directory.CreateDirectory(this.Path + "/screenshot");
            }
        }

        public override bool IsStart(int index)
        {
            return true;
        }
        public override void Close() { }
        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            this.DeviceInfos.Clear();
            AdbConfig config = new();
            var result = await config.ShowDialog<(string, int)?>(MainWindow.Instance);

            if (result != null)
            {
                PipeCmd("connect " + result.Value.Item1 + ":" + result.Value.Item2);
            }
            return await GetList();
        }

        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            this.DeviceInfos.Clear();
            return await Task.Run(() =>
             {
                 var output = PipeCmd("devices");
                 var array = output.Split("\r\n");
                 for (var i = 0; i < array.Length; i++)
                 {
                     var deviceInfo = array[i].Split("\t");
                     if (deviceInfo.Length == 2)
                     {
                         if (deviceInfo[1].Trim() == "device")
                         {
                             this.DeviceInfos.Add(new KeyValuePair<int, string>(this.DeviceInfos.Count, deviceInfo[0].Trim()));
                         }
                     }
                 }

                 if (this.DeviceInfos.Count == 0)
                 {
                     this.DeviceInfos.Add(new KeyValuePair<int, string>(0, "null"));
                 }

                 return this.DeviceInfos;
             });
        }

        public override async void ScreenShot(int index)
        {
            await Task.Run(() =>
            {
                var name = "screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".png";
                var fullName = this.Path + "screenshot/" + name;
                PipeCmd($"-s { this.DeviceInfos[index].Value }  exec-out screencap -p > { fullName }");
                for (var i = 0; i < 50; i++)
                {
                    Thread.Sleep(100);
                    if (File.Exists(fullName))
                    {
                        break;
                    }
                }
                FileStream stream = new(fullName, FileMode.Open, FileAccess.Read);
                var bitmap = new Bitmap(stream);
                stream.Position = 0;
                var sKBitmap = SKBitmap.Decode(stream);
                GraphicHelper.KeepScreen(sKBitmap);
                sKBitmap.Dispose();
                stream.Dispose();
                this.OnSuccessed?.Invoke(bitmap);
            }).ContinueWith((t) =>
            {
                if (t.Exception != null)
                    this.OnFailed?.Invoke(t.Exception.ToString());
            });
        }

        public string PipeCmd(string theCommand)
        {
            var command = $"/C {this.Path}adb.exe {theCommand}";
            ProcessStartInfo start = new("cmd.exe")
            {
                Arguments = command,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
            };

            var pipe = Process.Start(start);
            var readStream = pipe.StandardOutput;
            var OutputStr = readStream.ReadToEnd();
            pipe.WaitForExit(10000);
            pipe.Close();
            readStream.Close();
            return OutputStr;
        }
    }
}
