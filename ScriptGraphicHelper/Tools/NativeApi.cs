using System.Drawing;
using System.Runtime.InteropServices;

namespace ScriptGraphicHelper.Tools
{
    public static class NativeApi
    {
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

        /// <summary>
        /// 打开控制台窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        /// <summary>
        /// 关闭控制台窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
    }
}
