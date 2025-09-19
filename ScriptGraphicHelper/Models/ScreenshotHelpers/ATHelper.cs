using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using ScriptGraphicHelper.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    class ATHelper : BaseHelper
    {
        public override string Path { get; } = "AT连接";
        public override string Name { get; } = "AT连接";
        public override Action<Bitmap>? OnSuccessed { get; set; }
        public override Action<string>? OnFailed { get; set; }

        private IMqttClient? client;

        private string deviceName = "null";

        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            var config = new ATConfig();
            var remoteIP = await config.ShowDialog<string?>(MainWindow.Instance);

            if (!string.IsNullOrEmpty(remoteIP))
            {
                try
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(remoteIP)
                        .WithKeepAlivePeriod(TimeSpan.FromMinutes(6))
                        .Build();

                    var mqttFactory = new MqttFactory();
                    client = mqttFactory.CreateMqttClient();

                    client.UseApplicationMessageReceivedHandler(ApplicationMessageReceived);
                    client.UseConnectedHandler(async (e) =>
                    {
                        await client.SubscribeAsync(
                            new MqttTopicFilter
                            {
                                Topic = "server/init"
                            },
                            new MqttTopicFilter
                            {
                                Topic = "server/screen-shot"
                            });

                        await client.PublishAsync("client/init");
                    });
                    client.UseDisconnectedHandler(async (e) =>
                    {
                        await client.UnsubscribeAsync("server/init", "server/logging");
                        client.Dispose();
                    });

                    await client.ConnectAsync(mqttClientOptions, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    MessageBox.ShowAsync(ex.ToString());
                }
            }

            var list = new List<KeyValuePair<int, string>>
            {
                    new KeyValuePair<int, string>(key: 0, value: "null")
            };

            return list;
        }

        private void ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            var pack = PackData.Parse(e.ApplicationMessage.Payload);
            if (pack is null) return;

            switch (e.ApplicationMessage.Topic)
            {
                case "server/init":
                    this.deviceName = pack.Description;
                    break;
                case "server/screen-shot":
                    try
                    {
                        if (pack.Key == "successed")
                        {
                            var sKBitmap = SKBitmap.Decode(pack.Buffer);
                            var pxFormat = sKBitmap.ColorType == SKColorType.Rgba8888 ? PixelFormat.Rgba8888 : PixelFormat.Bgra8888;
                            var bitmap = new Bitmap(pxFormat, AlphaFormat.Opaque, sKBitmap.GetPixels(), new PixelSize(sKBitmap.Width, sKBitmap.Height), new Vector(96, 96), sKBitmap.RowBytes);

                            this.OnSuccessed?.Invoke(bitmap);
                        }
                        else
                        {
                            this.OnFailed?.Invoke(pack.Description);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.OnFailed?.Invoke(ex.ToString());
                    }
                    break;
            }
        }


        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            return await Task.Run(() =>
            {
                var result = new List<KeyValuePair<int, string>>
                 {
                     new KeyValuePair<int, string>(key: 0, value: this.deviceName)
                 };
                return result;
            });
        }


        public override void ScreenShot(int _)
        {
            if (this.client is null || !this.client.IsConnected)
            {
                throw new Exception("已断开连接!");
            }
            var pack = Stick.MakePackData("...");
            this.client.PublishAsync("client/screen-shot", pack);
        }

        public override bool IsStart(int _)
        {
            return this.client?.IsConnected ?? false;
        }


        public override void Close()
        {
            try
            {
                this.client?.Dispose();
            }
            catch { };
        }
    }
}
