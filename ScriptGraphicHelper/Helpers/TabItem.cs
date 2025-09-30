using Avalonia.Media.Imaging;

using ScriptGraphicHelper.Views;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ScriptGraphicHelper.Tools;

namespace ScriptGraphicHelper.Helpers
{
    public class TabItems<Item> : ObservableCollection<TabItem>
    {
        public new void Add(TabItem item)
        {
            if (Count >= 8)
            {
                RemoveAt(0);
            }
            base.Add(item);
            
            var mainWindow = IocTools.GetMainWindow();
            
            var width = (int)((mainWindow.Width - 450) / (Count < 8 ? Count : 8));
            for (var i = 0; i < Count; i++)
            {
                this[i].Width = width < 160 ? width : 160;
            }
        }
    }

    /// <summary>
    /// 图片 tab
    /// </summary>
    public class TabItem : INotifyPropertyChanged
    {
        private int width;
        public int Width
        {
            get => width;
            set
            {
                width = value;
                NotifyPropertyChanged(nameof(Width));
            }
        }

        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// 图片
        /// </summary>
        public Bitmap Img { get; set; }

        /// <summary>
        /// 点击关闭后的回调
        /// </summary>
        public ICommand? Command { get; set; }

        /// <summary>
        /// 初始化一个 tab
        /// </summary>
        /// <param name="img">图片数据</param>
        public TabItem(Bitmap img)
        {
            Header = DateTime.Now.ToString("HH-mm-ss");
            Img = img;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
