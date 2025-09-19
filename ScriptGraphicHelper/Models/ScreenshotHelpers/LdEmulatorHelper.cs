using Avalonia.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    class LdEmulatorHelper : BaseHelper
    {
        public override Action<Bitmap>? OnSuccessed { get; set; }

        public override Action<string>? OnFailed { get; set; }

        public override string Path { get; } = string.Empty;

        public override string Name { get; } = string.Empty;

        public string BmpPath { get; set; } = string.Empty;

        /// <summary>
        /// 初始化 , 获取雷电模拟器路径
        /// </summary>
        /// <param name="version"></param>
        public LdEmulatorHelper(int version)
        {
            try
            {
                if (version == 0)
                {
                    this.Name = "雷电模拟器3.0";
                    var Hkml = Registry.CurrentUser;
                    var Aimdir = Hkml.OpenSubKey("Software\\ChangZhi2\\dnplayer", true);
                    if (Aimdir == null)
                    {
                        if (Settings.Instance.LdPath3 != null && Settings.Instance.LdPath3 != string.Empty)
                        {
                            this.Path = Settings.Instance.LdPath3;
                        }
                        return;
                    }
                    this.Path = Aimdir.GetValue("InstallDir").ToString() ?? string.Empty;
                }
                else if (version == 1)
                {
                    this.Name = "雷电模拟器4.0";
                    var Hkml = Registry.CurrentUser;
                    var Aimdir = Hkml.OpenSubKey("Software\\leidian\\ldplayer", true);
                    if (Aimdir == null)
                    {
                        if (Settings.Instance.LdPath4 != null && Settings.Instance.LdPath4 != string.Empty)
                        {
                            this.Path = Settings.Instance.LdPath4;
                        }
                        return;
                    }
                    this.Path = Aimdir.GetValue("InstallDir").ToString() ?? string.Empty;
                }
                else if (version == 2)
                {
                    this.Name = "雷电模拟器64";
                    var Hkml = Registry.CurrentUser;
                    var Aimdir = Hkml.OpenSubKey("Software\\leidian\\ldplayer64", true);
                    if (Aimdir == null)
                    {
                        if (Settings.Instance.LdPath64 != null && Settings.Instance.LdPath64 != string.Empty)
                        {
                            this.Path = Settings.Instance.LdPath64;
                        }
                        return;
                    }
                    this.Path = Aimdir.GetValue("InstallDir").ToString() ?? string.Empty;
                }
                else if (version == 3)
                {
                    this.Name = "雷神模拟器";
                    var Hkml = Registry.CurrentUser;
                    var Aimdir = Hkml.OpenSubKey("Software\\baizhi\\lsplayer", true);
                    if (Aimdir == null)
                    {
                        if (Settings.Instance.LdPath64 != null && Settings.Instance.LdPath64 != string.Empty)
                        {
                            this.Path = Settings.Instance.LdPath64;
                        }
                        return;
                    }
                    this.Path = Aimdir.GetValue("InstallDir").ToString() ?? string.Empty;
                }
            }
            catch
            {
                this.Path = string.Empty;
            }
        }
        public override void Close() { }

        public string PipeCmd(string theCommand, bool select = false)
        {
            var ThePath = this.Path + "ldconsole.exe";

            if (select)
            {
                ThePath = this.Path + "ld.exe";
            }

            if (this.Name == "雷神模拟器")
            {
                if (!select)
                {
                    ThePath = this.Path + "lsconsole.exe";
                }
                else
                {
                    ThePath = this.Path + "ls.exe";
                }
            }

            ProcessStartInfo start = new(ThePath)
            {
                Arguments = theCommand,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };

            var pipe = Process.Start(start);
            var readStream = pipe.StandardOutput;
            var OutputStr = readStream.ReadToEnd();
            pipe.WaitForExit(10000);
            pipe.Close();
            readStream.Close();
            return OutputStr;
        }
        /// <summary>
        /// 获取模拟器信息
        /// </summary>
        /// <param name="ldName"></param>
        /// <returns>返回数组, 顺序为: 序号，标题，顶层窗口句柄，绑定窗口句柄，是否进入android，进程PID，VBox进程PID</returns>
        public string[] List(string ldName)
        {
            var resultArray = PipeCmd("list2").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
            for (var i = 0; i < resultArray.Length; i++)
            {
                var LineArray = resultArray[i].Split(',');
                if (LineArray.Length > 1)
                {
                    if (LineArray[1] == ldName)
                    {
                        return LineArray;
                    }
                }
            }
            return Array.Empty<string>();
        }

        public string[] List(int ldIndex)
        {
            var resultArray = PipeCmd("list2").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
            for (var i = 0; i < resultArray.Length; i++)
            {
                var LineArray = resultArray[i].Split(',');
                if (LineArray.Length > 1)
                {
                    if (LineArray[0] == ldIndex.ToString())
                    {
                        return LineArray;
                    }
                }
            }
            return Array.Empty<string>();
        }

        public override bool IsStart(int ldIndex)
        {
            var resultArray = PipeCmd("list2").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
            for (var i = 0; i < resultArray.Length; i++)
            {
                var LineArray = resultArray[i].Split(',');
                if (LineArray.Length > 1)
                {
                    if (LineArray[0] == ldIndex.ToString())
                    {
                        return LineArray[4] == "1";
                    }
                }
            }
            return false;
        }

        public override async Task<List<KeyValuePair<int, string>>> Initialize()
        {
            return await GetList();
        }

        public override async Task<List<KeyValuePair<int, string>>> GetList()
        {
            return await Task.Run(() =>
            {
                var resultArray = PipeCmd("list2").Trim("\n".ToCharArray()).Split("\n".ToCharArray());
                List<KeyValuePair<int, string>> result = new();
                for (var i = 0; i < resultArray.Length; i++)
                {
                    var LineArray = resultArray[i].Split(',');
                    result.Add(new KeyValuePair<int, string>(key: int.Parse(LineArray[0].Trim()), value: LineArray[1]));
                }
                return result;
            });
        }

        public override async void ScreenShot(int index)
        {
            await Task.Run(() =>
            {
                if (!IsStart(index))
                {
                    throw new Exception("模拟器未启动 ! ");
                }
                if (this.BmpPath == string.Empty)
                {
                    this.BmpPath = BmpPathGet(index);
                }
                var BmpName = "Screen_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".png";
                Screencap(index, "/mnt/sdcard/Pictures", BmpName);
                for (var i = 0; i < 10; i++)
                {
                    Task.Delay(200).Wait();
                    if (File.Exists(this.BmpPath + "\\" + BmpName))
                    {
                        break;
                    }
                }
                FileStream stream = new(this.BmpPath + "\\" + BmpName, FileMode.Open, FileAccess.Read);
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
        public void Screencap(int ldIndex, string savePath, string saveName)//截图
        {
            PipeCmd("-s " + ldIndex.ToString() + " /system/bin/screencap -p " + savePath.TrimEnd('/') + "/" + saveName, true);
        }
        public string BmpPathGet(int index)
        {
            try
            {
                StreamReader streamReader = new(string.Format(@"{0}\vms\config\leidian{1}.config", this.Path, index), false);
                var ret = streamReader.ReadToEnd();
                streamReader.Close();
                var jsonObj = JObject.Parse(ret);
                return jsonObj["statusSettings.sharedPictures"].ToString();
            }
            catch
            {
                return "";
            }

        }
        /// <summary>
        /// 启动模拟器
        /// </summary>
        /// <param name="ldName"></param>
        public void Launch(string ldName)
        {
            PipeCmd("launch --name " + ldName);
        }

        public void Launch(int ldIndex)
        {
            PipeCmd("launch --index " + ldIndex.ToString());
        }

        /// <summary>
        /// 关闭模拟器
        /// </summary>
        public void Quit()
        {
            PipeCmd("quitall");
        }

        public void Quit(string ldName)
        {
            PipeCmd("quit --name " + ldName);
        }

        public void Quit(int ldIndex)
        {
            PipeCmd("quit --index " + ldIndex.ToString());
        }

        /// <summary>
        /// 重启模拟器
        /// </summary>
        /// <param name="ldName"></param>
        public void Reboot(string ldName)
        {
            PipeCmd("reboot --name " + ldName);
        }

        public void Reboot(int ldIndex)
        {
            PipeCmd("reboot --index " + ldIndex.ToString());
        }

        /// <summary>
        /// 重启模拟器并打开指定应用
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="appid"></param>
        public void RebootToApp(string ldName, string appid = "null")
        {
            PipeCmd("action --name " + ldName + " --key call.reboot--value " + appid);

        }
        public void RebootToApp(int ldIndex, string appid = "null")
        {
            PipeCmd("action --index " + ldIndex.ToString() + " --key call.reboot--value " + appid);
        }

        /// <summary>
        /// 新建模拟器
        /// </summary>
        /// <param name="ldName"></param>
        public void Add(string ldName)
        {
            PipeCmd("add --name " + ldName);
        }

        /// <summary>
        /// 复制模拟器 
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="ldIndex"></param>
        public void Copy(string ldName, int ldIndex)
        {
            PipeCmd("copy --name " + ldName + " --from " + ldIndex.ToString());
        }

        /// <summary>
        /// 删除模拟器
        /// </summary>
        /// <param name="ldName"></param>
        public void Remove(string ldName)
        {
            PipeCmd("remove  --name " + ldName);
        }

        /// <summary>
        /// 删除模拟器
        /// </summary>
        /// <param name="ldIndex"></param>
        public void Remove(int ldIndex)
        {
            PipeCmd("remove  --index " + ldIndex.ToString());
        }

        /// <summary>
        /// 启动app
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="appid"></param>
        public void RunApp(string ldName, string appid)
        {
            PipeCmd("runapp --name " + ldName + " --packagename " + appid);
        }
        public void RunApp(int ldIndex, string appid)
        {
            PipeCmd("runapp --index " + ldIndex.ToString() + " --packagename " + appid);
        }

        /// <summary>
        /// 关闭app
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="appid"></param>
        public void Killapp(string ldName, string appid)
        {
            PipeCmd("killapp --name " + ldName + " --packagename " + appid);
        }
        public void Killapp(int ldIndex, string appid)
        {
            PipeCmd("killapp --index " + ldIndex.ToString() + " --packagename " + appid);
        }

        /// <summary>
        /// 安装app
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="filePath"></param>
        public void Installapp(string ldName, string filePath)
        {
            PipeCmd("installapp --name " + ldName + " --filename " + filePath);
        }

        public void Installapp(int ldIndex, string filePath)
        {
            PipeCmd("installapp --index " + ldIndex.ToString() + " --filename " + filePath);
        }

        /// <summary>
        /// 卸载app
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="appid"></param>
        public void Uninstallapp(string ldName, string appid)
        {
            PipeCmd("uninstallapp --name " + ldName + " --packagename " + appid);
        }

        public void Uninstallapp(int ldIndex, string appid)
        {
            PipeCmd("uninstallapp --index " + ldIndex.ToString() + " --packagename " + appid);
        }

        /// <summary>
        /// 执行安卓按键(back/home/menu/volumeup/volumedown)
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="keyValue"></param>
        public void Keyboard(string ldName, string keyValue)
        {
            PipeCmd("action --name " + ldName + " --key call.keyboard --value " + keyValue);
        }

        public void Keyboard(int ldIndex, string keyValue)
        {
            PipeCmd("action --index " + ldIndex.ToString() + " --key call.keyboard --value " + keyValue);
        }

        /// <summary>
        /// 修改经纬度
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="lngLat"></param>
        public void Locate(string ldName, string lngLat)
        {
            PipeCmd("action --name " + ldName + " --key call.locate --value " + lngLat);
        }
        public void Locate(int ldIndex, string lngLat)
        {
            PipeCmd("action --index " + ldIndex.ToString() + " --key call.locate --value " + lngLat);
        }

        /// <summary>
        /// 摇一摇
        /// </summary>
        /// <param name="ldName"></param>
        public void Shake(string ldName)
        {
            PipeCmd("action --name " + ldName + " --key call.shake --value null");
        }

        public void Shake(int ldIndex)
        {
            PipeCmd("action --index " + ldIndex.ToString() + " --key call.shake --value null");
        }

        /// <summary>
        /// 文字输入
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="inputStr"></param>
        public void Input(string ldName, string inputStr)
        {
            PipeCmd("action --name " + ldName + " --key call.input --value " + inputStr);
        }

        public void Input(int ldIndex, string inputStr)
        {
            PipeCmd("action --index " + ldIndex.ToString() + " --key call.input --value " + inputStr);
        }

        /// <summary>
        /// 模拟器属性设置
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="dpi"></param>
        /// <param name="cpu"></param>
        /// <param name="memory"></param>
        /// <param name="manufacturer"></param>
        /// <param name="model"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="imei"></param>
        /// <param name="imsi"></param>
        /// <param name="simserial"></param>
        /// <param name="androidid"></param>
        /// <param name="mac"></param>
        public void Modify(string ldName, short width, short height, short dpi, short cpu, short memory, string manufacturer, string model, long phoneNumber, string imei = "auto", string imsi = "auto", string simserial = "auto", string androidid = "auto", string mac = "auto")
        {
            PipeCmd("modify --name " + ldName + " --resolution " + width.ToString() + "," + height.ToString() + "," + dpi.ToString() + " --cpu " +
               cpu.ToString() + " --memory " + memory.ToString() + " --manufacturer " + manufacturer + " --model " + model + " --pnumber " +
                phoneNumber.ToString() + " --imei " + imei + " --imsi " + imsi + " --simserial " + simserial + " --androidid " + androidid + " --mac " + mac);
        }

        public void Modify(int ldIndex, short width, short height, short dpi, short cpu, short memory, string manufacturer, string model, long phoneNumber, string imei = "auto", string imsi = "auto", string simserial = "auto", string androidid = "auto", string mac = "auto")
        {
            PipeCmd("modify --index " + ldIndex.ToString() + " --resolution " + width.ToString() + "," + height.ToString() + "," + dpi.ToString() + " --cpu " +
               cpu.ToString() + " --memory " + memory.ToString() + " --manufacturer " + manufacturer + " --model " + model + " --pnumber " +
                phoneNumber.ToString() + " --imei " + imei + " --imsi " + imsi + " --simserial " + simserial + " --androidid " + androidid + " --mac " + mac);
        }

        /// <summary>
        /// 扫描二维码,需要app先启动扫描,再调用这个命令
        /// </summary>
        /// <param name="ldName"></param>
        /// <param name="filePath"></param>
        public void Scan(string ldName, string filePath)//
        {
            PipeCmd("qrpicture --name " + ldName + " --file " + filePath);
        }

        public void Scan(int ldIndex, string filePath)
        {
            PipeCmd("qrpicture --index " + ldIndex.ToString() + " --file " + filePath);
        }

        /// <summary>
        /// 一键排序 , 需先在多开器配置排序规则
        /// </summary>
        public void SortWnd()//
        {
            PipeCmd("sortWnd");
        }

        /// <summary>
        /// 清除应用数据
        /// </summary>
        /// <param name="ldIndex"></param>
        /// <param name="appid"></param>
        public void ClearApp(int ldIndex, string appid)
        {
            PipeCmd("-s " + ldIndex.ToString() + " pm clear " + appid, true);
        }

        /// <summary>
        /// 模拟按键 , 具体键值请百度
        /// </summary>
        /// <param name="ldIndex"></param>
        /// <param name="keyCode"></param>
        public void InputKey(int ldIndex, short keyCode)
        {
            PipeCmd("-s " + ldIndex.ToString() + " input keyevent " + keyCode, true);
        }

        /// <summary>
        /// 文本输入 , 不支持中文
        /// </summary>
        /// <param name="ldIndex"></param>
        /// <param name="text"></param>
        public void InputText(int ldIndex, string text)
        {
            PipeCmd("-s " + ldIndex.ToString() + " input text " + text, true);
        }

        /// <summary>
        /// 点击
        /// </summary>
        /// <param name="ldIndex"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public void Click(int ldIndex, short X, short Y)
        {
            PipeCmd("-s " + ldIndex.ToString() + " input tap " + X.ToString() + " " + Y.ToString(), true);
        }

        /// <summary>
        /// 滑动
        /// </summary>
        /// <param name="ldIndex"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="time"></param>
        public void Swipe(int ldIndex, short startX, short startY, short endX, short endY, short time = 1000)
        {
            PipeCmd("-s " + ldIndex.ToString() + " input swipe " + startX.ToString() + " " + startY.ToString() + " " + endX.ToString() + " " + endY.ToString() + " " + time.ToString(), true);
        }
    }
}
