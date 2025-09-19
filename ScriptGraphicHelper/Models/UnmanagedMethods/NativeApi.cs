using System.Drawing;
using System.Runtime.InteropServices;

namespace ScriptGraphicHelper.Models.UnmanagedMethods
{

    public static class NativeApi
    {
        //[DllImport("./Assets/mouse")]
        //public static extern void Move2Left();
        //[DllImport("./Assets/mouse")]
        //public static extern void Move2Top();
        //[DllImport("./Assets/mouse")]
        //public static extern void Move2Right();
        //[DllImport("./Assets/mouse")]
        //public static extern void Move2Bottom();

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);

        /// <summary>
        /// 鼠标左移 1 像素
        /// </summary>
        public static void Move2Left()
        {
            Point currentPos = new Point();
            GetCursorPos(ref currentPos);

            int newX = currentPos.X - 1;
            int newY = currentPos.Y;

            SetCursorPos(newX, newY);
        }

        /// <summary>
        /// 鼠标上移 1 像素
        /// </summary>
        public static void Move2Top()
        {
            Point currentPos = new Point();
            GetCursorPos(ref currentPos);

            int newX = currentPos.X;
            int newY = currentPos.Y - 1;

            SetCursorPos(newX, newY);
        }

        /// <summary>
        /// 鼠标右移 1 像素
        /// </summary>
        public static void Move2Right()
        {
            Point currentPos = new Point();
            GetCursorPos(ref currentPos);

            int newX = currentPos.X + 1;
            int newY = currentPos.Y;

            SetCursorPos(newX, newY);
        }

        /// <summary>
        /// 鼠标下移 1 像素 
        /// </summary>
        public static void Move2Bottom()
        {
            Point currentPos = new Point();
            GetCursorPos(ref currentPos);

            int newX = currentPos.X;
            int newY = currentPos.Y + 1;

            SetCursorPos(newX, newY);
        }

        //由于avalonia的fileDialog在win上会偶发ui阻塞问题, 原因不明, 暂时用win32api替代
        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
    }
}
