using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;
using ReactiveUI;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels.Core;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ScriptGraphicHelper.ViewModels
{

    public class Win32Api
    {
        internal const uint SPI_SETCURSORS = 87;
        internal const uint SPIF_SENDWININICHANGE = 2;

        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadCursorFromFile(string fileName);

        [DllImport("User32.DLL")]
        internal static extern bool SetSystemCursor(IntPtr hcur, uint id);
        internal const uint OCR_NORMAL = 32512;

        [DllImport("User32.DLL")]
        internal static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
    }

    public class HwndConfigViewModel : ViewModelBase
    {
        private HwndConfig? configWindow;
        public HwndConfig? ConfigWindow
        {
            get => this.configWindow;
            set => this.RaiseAndSetIfChanged(ref this.configWindow, value);
        }


        private ObservableCollection<MoveCategory> hwndInfos = new();
        public ObservableCollection<MoveCategory> HwndInfos
        {
            get => this.hwndInfos;
            set => this.RaiseAndSetIfChanged(ref this.hwndInfos, value);
        }

        private MoveCategory? selectedItem;
        public MoveCategory? SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedItem, value);
                this.BindHwnd = value.Hwnd;
            }
        }


        private int bindHwnd = -1;
        public int BindHwnd
        {
            get => this.bindHwnd;
            set => this.RaiseAndSetIfChanged(ref this.bindHwnd, value);
        }

        private int bindGraphicMode = 0;
        public int BindGraphicMode
        {
            get => this.bindGraphicMode;
            set => this.RaiseAndSetIfChanged(ref this.bindGraphicMode, value);
        }

        private int bindAttribute = 0;
        public int BindAttribute
        {
            get => this.bindAttribute;
            set => this.RaiseAndSetIfChanged(ref this.bindAttribute, value);
        }

        private int bindMode = 0;
        public int BindMode
        {
            get => this.bindMode;
            set => this.RaiseAndSetIfChanged(ref this.bindMode, value);
        }

        private Dmsoft Dm = Dmsoft.Instance;

        public HwndConfigViewModel(HwndConfig hwndConfig)
        {
            this.hwndInfos = new ObservableCollection<MoveCategory>();
            this.ConfigWindow = hwndConfig;
        }

        public void Ok_Tapped()
        {
            if (this.BindHwnd == -1)
            {
                MessageBox.ShowAsync("请选择句柄!");
                return;
            }
            var graphicModes = new string[] { "normal", "gdi", "gdi2", "dx2", "dx3", "dx.graphic.2d", "dx.graphic.2d.2", "dx.graphic.3d", "dx.graphic.3d.8", "dx.graphic.opengl", "dx.graphic.opengl.esv2", "dx.graphic.3d.10plus" };
            var attributes = new string[] { "", "dx.public.active.api", "dx.public.active.message", "dx.public.hide.dll", "dx.public.graphic.protect", "dx.public.anti.api", "dx.public.prevent.block", "dx.public.inject.super" };
            var modes = new int[] { 0, 2, 101, 103, 11, 13 };
            this.Dm.Hwnd = this.BindHwnd;
            this.Dm.Display = graphicModes[this.BindGraphicMode];
            this.Dm.Public_desc = attributes[this.BindAttribute];
            this.Dm.Mode = modes[this.BindMode];
            this.ConfigWindow.Close();
        }

        public ICommand GetHwnd_PointerPressed => new Command((param) =>
        {
            if (param != null)
            {
                var parameters = (CommandParameters)param;
                var eventArgs = (PointerPressedEventArgs)parameters.EventArgs;
                if (eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                {
                    var cur = Win32Api.LoadCursorFromFile(AppDomain.CurrentDomain.BaseDirectory + @"assets/aiming.cur");
                    Win32Api.SetSystemCursor(cur, Win32Api.OCR_NORMAL);
                }

            }
        });

        public ICommand GetHwnd_PointerReleased => new Command((param) =>
        {
            this.HwndInfos.Clear();
            Win32Api.SystemParametersInfo(Win32Api.SPI_SETCURSORS, 0, IntPtr.Zero, Win32Api.SPIF_SENDWININICHANGE);
            var hwnd = this.Dm.GetMousePointWindow() ?? -1;
            var parentHwnd = this.Dm.GetWindow(hwnd, 7) ?? -1;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            this.HwndInfos.Add(new MoveCategory(parentHwnd, this.Dm.GetWindowTitle(parentHwnd) ?? string.Empty, this.Dm.GetWindowClass(parentHwnd) ?? string.Empty));
            EnumWindows(parentHwnd, this.HwndInfos[0]);
        });

        private void EnumWindows(int parentHwd, MoveCategory movieCategory)
        {
            var hwnds = this.Dm.EnumWindow(parentHwd, "", "", 4).Split(',');
            for (var i = 0; i < hwnds.Length; i++)
            {
                if (hwnds[i].Trim() != "")
                {
                    var hwnd = int.Parse(hwnds[i].Trim());
                    movieCategory.Moves.Add(new MoveCategory(hwnd, this.Dm.GetWindowTitle(hwnd) ?? string.Empty, this.Dm.GetWindowClass(hwnd) ?? string.Empty));
                    EnumWindows(hwnd, movieCategory.Moves[i]);
                }
            }
        }
    }
}
