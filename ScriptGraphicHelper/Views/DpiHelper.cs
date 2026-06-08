using System;

using Avalonia;
using Avalonia.Controls;

namespace ScriptGraphicHelper.Views
{
    /// <summary>
    /// 高 DPI 屏幕辅助：Screens.WorkingArea 是物理像素，窗口属性是 DIP，需统一量纲
    /// </summary>
    public static class DpiHelper
    {
        /// <summary>
        /// 将窗口居中到主屏幕（DPI 感知）
        /// </summary>
        public static void CenterWindow(Window window)
        {
            CenterWindow(window, window.Width, window.Height);
        }

        /// <summary>
        /// 将窗口居中到主屏幕，显式指定 DIP 尺寸（用于 ClientSize 刚设置后 this.Width 尚未就绪的场景）
        /// </summary>
        public static void CenterWindow(Window window, double widthDip, double heightDip)
        {
            var primary = window.Screens.Primary;
            if (primary == null) return;
            var scaling = primary.Scaling;
            var workingArea = primary.WorkingArea;

            var physicalW = widthDip * scaling;
            var physicalH = heightDip * scaling;

            var targetX = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - physicalW) / 2);
            var targetY = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - physicalH) / 2);

            window.Position = new PixelPoint((int)targetX, (int)targetY);
        }
    }
}
