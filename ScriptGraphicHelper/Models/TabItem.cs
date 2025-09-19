using Avalonia.Media.Imaging;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ScriptGraphicHelper.Models
{
    public class TabItems<item> : ObservableCollection<TabItem>
    {
        public new void Add(TabItem item)
        {
            if (base.Count >= 8)
            {
                base.RemoveAt(0);
            }
            base.Add(item);

            var width = (int)((MainWindow.Instance.Width - 450) / (this.Count < 8 ? this.Count : 8));
            for (var i = 0; i < this.Count; i++)
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
            get => this.width;
            set
            {
                this.width = value;
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
            this.Header = DateTime.Now.ToString("HH-mm-ss");
            this.Img = img;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
