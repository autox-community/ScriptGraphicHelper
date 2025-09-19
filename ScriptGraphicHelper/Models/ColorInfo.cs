
using Avalonia;
using Avalonia.Media;
using ScriptGraphicHelper.Converters;
using System.Collections.Generic;

namespace ScriptGraphicHelper.Models
{
    public class ColorInfo
    {
        public static double Width { get; set; } = 0;

        public static double Height { get; set; } = 0;

        public static List<string> AnchorItems => new() { "N", "L", "C", "R" };

        public int Index { get; set; }

        public AnchorMode Anchor { get; set; } = AnchorMode.None;

        public Point Point { get; set; }

        private Color _color;
        public Color Color
        {
            get => this._color;
            set
            {
                this._color = value;
                if (value.R < 0x40 && value.G < 0x40 && value.B < 0x40)
                {
                    this.MarkBrush = Color.FromRgb(0xe8, 0xe8, 0xe8);
                }
            }
        }

        public Color MarkBrush { get; set; } = Colors.Black;

        public bool IsChecked { get; set; } = false;

        public ColorInfo() { }

        public ColorInfo(int index, int x, int y, byte[] color)
        {
            this.Index = index;
            this.Point = new Point(x, y);
            this.Color = Color.FromRgb(color[0], color[1], color[2]);
            this.IsChecked = true;
            this.Anchor = AnchorMode.None;
        }

        public ColorInfo(int index, int x, int y, Color color)
        {
            this.Index = index;
            this.Point = new Point(x, y);
            this.Color = color;
            this.IsChecked = true;
            this.Anchor = AnchorMode.None;
        }

        public ColorInfo(int index, AnchorMode anchor, int x, int y, byte[] color)
        {
            this.Index = index;
            this.Anchor = anchor;
            this.Point = new Point(x, y);
            this.Color = Color.FromRgb(color[0], color[1], color[2]);
            this.IsChecked = true;
        }

    }
}
