using System;
using System.Runtime.InteropServices;

namespace ScriptGraphicHelper.Models.UnmanagedMethods
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize;
        public IntPtr hwnd = IntPtr.Zero;
        public IntPtr hinst = IntPtr.Zero;
        public string? filter;
        public string? custFilter;
        public int custFilterMax;
        public int filterIndex;
        public string? file;
        public int maxFile;
        public string? fileTitle;
        public int maxFileTitle = 0;
        public string? initialDir;
        public string? title;
        public int flags;
        public short fileOffset;
        public short fileExtMax = 0;
        public string? defExt;
        public int custData;
        public IntPtr pHook = IntPtr.Zero;
        public string? template;
    }
}
