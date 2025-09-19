using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models.ScreenshotHelpers
{
    public abstract class BaseHelper
    {
        /// <summary>
        /// 截屏成功回调
        /// </summary>
        public abstract Action<Bitmap>? OnSuccessed { get; set; }

        /// <summary>
        /// 截屏失败回调
        /// </summary>
        public abstract Action<string>? OnFailed { get; set; }

        public abstract string Path { get; }

        public abstract string Name { get; }

        public abstract bool IsStart(int Index);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public abstract Task<List<KeyValuePair<int, string>>> Initialize();

        public abstract Task<List<KeyValuePair<int, string>>> GetList();

        /// <summary>
        /// 截屏, 最后会调用 成功 / 失败 的回调函数
        /// </summary>
        /// <param name="Index"></param>
        public abstract void ScreenShot(int Index);

        public abstract void Close();
    }
}
