using Avalonia.Media.Imaging;

using ScriptGraphicHelper.Views;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Helpers.Screenshot
{
    /// <summary>
    /// adb 连接
    /// </summary>
    public class AdbHelper : BaseHelper
    {
        public override Action<Bitmap>? OnSuccessed { get; set; }

        public override Action<string>? OnFailed { get; set; }

        /// <summary>
        /// adb 文件的目录
        /// </summary>
        public override string Path { get; } = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/") + "MyFiles/adb/";

        public override string Name { get; } = "Adb连接";

        /// <summary>
        /// 设备列表
        /// </summary>
        private List<KeyValuePair<int, string>> DeviceInfos = new();

        public AdbHelper()
        {
            if (!Directory.Exists(Path + "/screenshot"))
            {
                Directory.CreateDirectory(Path + "/screenshot");
            }
        }

        public override bool IsStart(int index)
        {
            return true;
        }

        public override void Close() { }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>设备列表</returns>
        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            var ret = new List<KeyValuePair<int, string>>();

            DeviceInfos.Clear();

            // 弹窗, 让用户填写 ip 地址 端口
            var res = await new AdbConfigWindow().ShowDialog<string?>(MainWindow.Instance);

            // 填写了 ip 与 端口, "跳过" 则不进行这一步
            if (res != null)
            {
                var resArr = res.Split(",");

                var state = resArr[0];
                if (bool.Parse(state))
                {
                    var ip = resArr[1];
                    var port = resArr[2];
                    // 执行 adb 命令
                    PipeCmd("connect " + ip + ":" + port);
                }
                // 获取 adb 中的设备列表
                ret = await GetList();

                MessageBoxWindow.ShowAsync("adb 获取到的设备数量:" + ret.Count);
            }
            return ret;
        }

        /// <summary>
        /// 获取 adb 中的设备列表 (执行 adb devices)
        /// </summary>
        /// <returns></returns>
        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            DeviceInfos.Clear();

            return await Task.Run(() =>
             {
                 // 执行 adb 命令
                 var output = PipeCmd("devices");

                 var array = output.Split("\r\n");

                 for (var i = 0; i < array.Length; i++)
                 {

                     var deviceInfo = array[i].Split("\t");

                     if (deviceInfo.Length == 2)
                     {
                         if (deviceInfo[1].Trim() == "device")
                         {
                             DeviceInfos.Add(new KeyValuePair<int, string>(DeviceInfos.Count, deviceInfo[0].Trim()));
                         }
                     }
                 }

                 return DeviceInfos;
             });
        }

        /// <summary>
        /// 截屏
        /// </summary>
        /// <param name="index"></param>
        public override async void ScreenShot(int index)
        {
            await Task.Run(() =>
            {
                var name = "screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".png";

                var fullName = Path + "screenshot/" + name;

                // 执行命令, 截图保存到电脑
                PipeCmd($"-s {DeviceInfos[index].Value}  exec-out screencap -p > {fullName}");

                // 等待图片
                for (var i = 0; i < 50; i++)
                {
                    Thread.Sleep(100);

                    // 检查图片是否存在
                    if (File.Exists(fullName))
                    {
                        break;
                    }
                }

                FileStream stream = new(fullName, FileMode.Open, FileAccess.Read);

                var bitmap = new Bitmap(stream);

                stream.Position = 0;

                var sKBitmap = SKBitmap.Decode(stream);

                // 保存到静态变量中
                GraphicHelper.KeepScreen(sKBitmap);

                sKBitmap.Dispose();

                stream.Dispose();

                OnSuccessed?.Invoke(bitmap);

            }).ContinueWith((t) =>
            {
                if (t.Exception != null)
                    OnFailed?.Invoke(t.Exception.ToString());
            });
        }

        /// <summary>
        /// 使用 cmd 执行 adb 命令
        /// </summary>
        /// <param name="theCommand"></param>
        /// <returns></returns>
        public string PipeCmd(string theCommand)
        {
            var command = $"/C {Path}adb.exe {theCommand}";
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
