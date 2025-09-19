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

        public Bitmap Img { get; set; }

        public ICommand? Command { get; set; }

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
