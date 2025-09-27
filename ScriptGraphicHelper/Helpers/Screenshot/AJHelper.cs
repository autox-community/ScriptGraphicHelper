using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ScriptGraphicHelper.Utils;
using ScriptGraphicHelper.Views;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Helpers.Screenshot
{
    internal class RunData
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string command { get; set; } = "run";
        public string script { get; set; } = string.Empty;
    }

    internal class RunCommand
    {
        public int id { get; set; }
        public string type { get; set; } = "command";
        public RunData? data { get; set; } = null;
    }

    class AJHelper : BaseHelper
    {
        public override string Path { get; } = "AJ连接";
        public override string Name { get; } = "AJ连接";
        public override Action<Bitmap>? OnSuccessed { get; set; }
        public override Action<string>? OnFailed { get; set; }

        public string LocalIP { get; set; } = string.Empty;

        public string RemoteIP { get; set; } = string.Empty;

        private TcpListener? server;

        private TcpClient? client;

        private int runStep;

        private string sendCode = string.Empty;

        private string deviceName { get; set; } = string.Empty;

        public AJHelper()
        {
            var sendCodePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyFiles", "js", "send_script.js");
            if (File.Exists(sendCodePath))
            {
                sendCode = File.ReadAllText(sendCodePath);
                runStep = 0;
            }
            else
            {
                throw new FileNotFoundException(sendCodePath);
            }
        }

        private byte[] GetRunCommandBytes(string runCode, string id)
        {
            RunCommand command = new()
            {
                id = runStep,
                data = new RunData()
                {
                    id = id,
                    name = id,
                    script = runCode
                }
            };
            runStep++;
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
            var recv = new byte[data.Length + 8];
            var len = data.Length.ToBytes();
            len.CopyTo(recv, 0);
            recv[7] = 1;
            data.CopyTo(recv, 8);
            return recv;
        }

        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            var config = new AJConfigWindow(Util.GetLocalAddress());

            var result = await config.ShowDialog<(string, string)?>(MainWindow.Instance);

            if (result != null)
            {
                LocalIP = result.Value.Item1;
                RemoteIP = result.Value.Item2;

                await Task.Run(async () =>
                {
                    try
                    {
                        client = new TcpClient(RemoteIP, 9317);
                        var networkStream = client.GetStream();
                        for (var i = 0; i < 50; i++)
                        {
                            Thread.Sleep(100);
                            if (networkStream.DataAvailable)
                            {

                                var buf = new byte[256];
                                var len = networkStream.Read(buf, 0, 256);
                                var info = Encoding.UTF8.GetString(buf, 8, len - 8);

                                var obj = (JObject?)JsonConvert.DeserializeObject(info);
                                if (obj != null)
                                {
                                    var data = (JObject?)obj.GetValue("data");
                                    if (data != null)
                                    {
                                        var name = (string?)data.GetValue("device_name");
                                        if (name != null)
                                        {
                                            deviceName = name;
                                        }
                                    }
                                }

                                var send = new byte[59]
                                {
                                    0x00,0x00,0x00,0x33,0x00,0x00,0x00,0x01,
                                    0x7B,0x22,0x69,0x64,0x22,0x3A,0x31,0x2C,
                                    0x22,0x74,0x79,0x70,0x65,0x22,0x3A,0x22,
                                    0x68,0x65,0x6C,0x6C,0x6F,0x22,0x2C,0x22,
                                    0x64,0x61,0x74,0x61,0x22,0x3A,0x7B,0x22,
                                    0x63,0x6C,0x69,0x65,0x6E,0x74,0x5F,0x76,
                                    0x65,0x72,0x73,0x69,0x6F,0x6E,0x22,0x3A,
                                    0x32,0x7D,0x7D
                                };

                                await networkStream.WriteAsync(send);

                                var initCodePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyFiles", "js", "init_script.js");
                                if (!File.Exists(initCodePath))
                                {
                                    throw new FileNotFoundException(initCodePath);
                                }
                                var initCode = File.ReadAllText(initCodePath);
                                await networkStream.WriteAsync(GetRunCommandBytes(initCode, "init_script"));
                                await Task.Delay(500);

                                var capCodePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyFiles", "js", "cap_script.js");
                                if (!File.Exists(capCodePath))
                                {
                                    throw new FileNotFoundException(capCodePath);
                                }

                                var capCode = File.ReadAllText(capCodePath);
                                await networkStream.WriteAsync(GetRunCommandBytes(capCode, "cap_script"));
                                server = new TcpListener(IPAddress.Parse(LocalIP), 5678);
                                server.Start();
                                server.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), server);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBoxWindow.ShowAsync(ex.ToString());
                    }
                });
            }
            return await GetList();
        }

        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            return await Task.Run(() =>
             {
                 var result = new List<KeyValuePair<int, string>>
                 {
                     new KeyValuePair<int, string>(key: 0, value: deviceName)
                 };
                 return result;
             });
        }

        private async void ConnectCallback(IAsyncResult ar)
        {
            if (ar.AsyncState != null)
            {
                try
                {
                    var listener = (TcpListener)ar.AsyncState;
                    if (listener.Server == null || !listener.Server.IsBound)
                    {
                        return;
                    }

                    var client = listener.EndAcceptTcpClient(ar);
                    var stream = client.GetStream();

                    var data = await Stick.ReadPackAsync(stream);

                    if (data.Key == "screenShot_successed")
                    {
                        var sKBitmap = SKBitmap.Decode(data.Buffer);
                        var pxFormat = sKBitmap.ColorType == SKColorType.Rgba8888 ? PixelFormat.Rgba8888 : PixelFormat.Bgra8888;
                        var bitmap = new Bitmap(pxFormat, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(sKBitmap.Width, sKBitmap.Height), new Vector(96, 96), sKBitmap.RowBytes);
                        sKBitmap.Dispose();
                        OnSuccessed?.Invoke(bitmap);
                    }
                    else if (data.Key == "screenShot_failed")
                    {
                        OnFailed?.Invoke(data.Description ?? "未知错误");
                    }

                    stream.Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    OnFailed?.Invoke(ex.ToString());
                }
                finally
                {
                    if (server.Server.Connected)
                    {
                        server?.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), server);
                    }

                }
            }
        }

        public override void ScreenShot(int Index)
        {
            if (client == null)
            {
                throw new Exception("tcp连接失效");
            }
            var sendCode = this.sendCode.Replace("let remoteIP;", $"let remoteIP = '{LocalIP}'");
            client.GetStream().Write(GetRunCommandBytes(sendCode, "send_script"));
        }

        public override bool IsStart(int Index)
        {
            return true;
        }


        public override void Close()
        {
            try
            {
                server?.Server.Close();
                server.Stop();
                client?.Close();
                client?.Dispose();
            }
            catch { }
            ;
        }
    }
}
