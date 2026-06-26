using System;
using System.IO;

namespace ScriptGraphicHelper.Utils
{
    /// <summary>
    /// 全局日志：Debug/Info/Warn/Error 四级，线程安全，写入 logs/ScriptGraphicHelper.log
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new();
        private static string? _logPath;

        /// <summary>
        /// 初始化日志路径（默认 logs/ScriptGraphicHelper.log）
        /// </summary>
        public static void Init(string? logPath = null)
        {
            _logPath = logPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "ScriptGraphicHelper.log");
            var dir = Path.GetDirectoryName(_logPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
        }

        private static string LogPath =>
            _logPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "ScriptGraphicHelper.log");

        public static void Debug(string message) => Write("DEBUG", message);
        public static void Info(string message)  => Write("INFO", message);
        public static void Warn(string message)  => Write("WARN", message);
        public static void Error(string message) => Write("ERROR", message);

        private static void Write(string level, string message)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
            lock (_lock)
            {
                try { File.AppendAllText(LogPath, line); }
                catch { /* 日志写入失败不能影响主流程 */ }
            }
        }
    }
}
