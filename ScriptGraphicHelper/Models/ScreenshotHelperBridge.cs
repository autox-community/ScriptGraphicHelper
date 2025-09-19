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
        public static LinkState State { get; set; } = LinkState.None;
        public static ObservableCollection<string> Result { get; set; } = new ObservableCollection<string>();
        public static List<KeyValuePair<int, string>> Info { get; set; } = new List<KeyValuePair<int, string>>();
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

        public static List<BaseHelper> Helpers = new();
        public static ObservableCollection<string> Init()
        {
            Helpers = new List<BaseHelper>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Helpers.Add(new LdEmulatorHelper(0));
                Helpers.Add(new LdEmulatorHelper(1));
                Helpers.Add(new LdEmulatorHelper(2));
                Helpers.Add(new LdEmulatorHelper(3));
                try
                {
                    Helpers.Add(new HwndHelper());
                }
                catch { }
                Helpers.Add(new AdbHelper());
            }

            Helpers.Add(new AJHelper());
            Helpers.Add(new ATHelper());

            Result = new ObservableCollection<string>();

            foreach (var emulator in Helpers)
            {
                if (emulator.Path != string.Empty && emulator.Path != "")
                {
                    Result.Add(emulator.Name);
                }
            }
            State = 0;
            return Result;

        }
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
        public static void Changed(int index)
        {
            if (index >= 0)
            {
                for (var i = 0; i < Helpers.Count; i++)
                {
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
        public static async Task<ObservableCollection<string>> Initialize()
        {
            ObservableCollection<string> result = new();
            Info = await Helpers[Select].Initialize();
            foreach (var item in Info)
            {
                result.Add(item.Value);
            }
            return result;
        }
        public static void ScreenShot()
        {
            Helpers[Select].ScreenShot(Index);
        }
    }
}
