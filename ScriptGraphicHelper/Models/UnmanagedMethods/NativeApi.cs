using System.Runtime.InteropServices;

namespace ScriptGraphicHelper.Models.UnmanagedMethods
{

    public static class NativeApi
    {

        [DllImport("./Assets/mouse")]
        public static extern void Move2Left();

        [DllImport("./Assets/mouse")]
        public static extern void Move2Top();

        [DllImport("./Assets/mouse")]
        public static extern void Move2Right();

        [DllImport("./Assets/mouse")]
        public static extern void Move2Bottom();



        //由于avalonia的fileDialog在win上会偶发ui阻塞问题, 原因不明, 暂时用win32api替代
        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
    }
}
