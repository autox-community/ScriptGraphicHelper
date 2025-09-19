using ScriptGraphicHelper.Models.ScreenshotHelpers;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models
{
    public enum LinkState
    {
        None = -1,
        Waiting = 0,
        Starting = 1,
        success = 2
    }

    public static class ScreenshotHelperBridge
    {
        /// <summary>
        /// 状态
        /// </summary>
        public static LinkState State { get; set; } = LinkState.None;

        /// <summary>
        /// 用于界面中的数据
        /// </summary>
        public static ObservableCollection<string> Result { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// 设备列表
        /// </summary>
        public static List<KeyValuePair<int, string>> Info { get; set; } = new List<KeyValuePair<int, string>>();
        
        /// <summary>
        /// 选中
        /// </summary>
        public static int Select { get; set; } = -1;

        private static int _index = -1;

        public static int Index
        {
            get => _index;
            set
            {
                if (value != -1)
                {
                    _index = Info[value].Key;
                }
            }
        }

        /// <summary>
        /// 模式选择中的数据 (eg: at, aj, adb, 雷电)
        /// </summary>
        public static List<BaseHelper> Helpers = new();

        /// <summary>
        /// 初始化, 获取模式列表
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<string> Init()
        {
            Helpers = new List<BaseHelper>();

            // windows 系统特有的功能
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 模拟器
                Helpers.Add(new LdEmulatorHelper(0));
                Helpers.Add(new LdEmulatorHelper(1));
                Helpers.Add(new LdEmulatorHelper(2));
                Helpers.Add(new LdEmulatorHelper(3));

                try
                {
                    // 大漠
                    Helpers.Add(new HwndHelper());
                }
                catch
                {

                }

                // adb
                Helpers.Add(new AdbHelper());
            }

            // auto.js
            Helpers.Add(new AJHelper());

            // at
            Helpers.Add(new ATHelper());


            Result = new ObservableCollection<string>();

            // 去掉无效的选项, 并返回到界面
            foreach (var emulator in Helpers)
            {
                if (emulator.Path != string.Empty && emulator.Path != "")
                {
                    Result.Add(emulator.Name);
                }
            }

            // 设置为 等待状态
            State = LinkState.Waiting;

            return Result;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public static void Dispose()
        {
            try
            {
                foreach (var emulator in Helpers)
                {
                    emulator.Close();
                }

                Result.Clear();

                Info.Clear();

                Helpers.Clear();

                Select = -1;

                State = LinkState.None;
            }
            catch { }
        }

        /// <summary>
        /// 保存选中项
        /// </summary>
        /// <param name="index"></param>
        public static void Changed(int index)
        {
            if (index >= 0)
            {
                for (var i = 0; i < Helpers.Count; i++)
                {
                    // 找到页面上选中的选项
                    if (Helpers[i].Name == Result[index])
                    {
                        Select = i;
                        State = LinkState.Starting;
                    }
                }
            }
            else
            {
                Select = -1;
                State = LinkState.Starting;
            }
        }

        /// <summary>
        /// 初始化, 拿到设备列表
        /// </summary>
        /// <returns></returns>
        public static async Task<ObservableCollection<string>> Initialize()
        {
            ObservableCollection<string> result = new();
            
            // 调用选中 模式的初始化, 拿到设备列表
            Info = await Helpers[Select].Initialize();

            foreach (var item in Info)
            {
                result.Add(item.Value);
            }
            return result;
        }

        /// <summary>
        /// 截屏, 最后会调用 成功 / 失败 的回调函数
        /// </summary>
        public static void ScreenShot()
        {
            Helpers[Select].ScreenShot(Index);
        }
    }
}
