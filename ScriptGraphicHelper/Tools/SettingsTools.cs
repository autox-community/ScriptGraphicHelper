using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using ScriptGraphicHelper.Models;

namespace ScriptGraphicHelper.Tools
{
    public static class SettingsTools
    {
        public static readonly string FilePath = Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
            "MyFiles",
            "settings.json"
            );

        /// <summary>
        /// 保存软件配置
        /// </summary>
        public static void SaveSettings()
        {
            var settingStr = JsonConvert.SerializeObject(
                Settings.Instance,
                Formatting.Indented,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }
                );

            File.WriteAllText(FilePath, settingStr);
        }

        public static Settings GetDefaultSettings()
        {
            return new Settings
            {
                Formats = FormatConfig.CreateFormats()
            };
        }

        public static Settings? GetSettingsFromFile()
        {
            Settings settings = null;

            if (File.Exists(SettingsTools.FilePath))
            {
                var settingsStr = File.ReadAllText(SettingsTools.FilePath)
                .Replace("\\\\", "\\")
                .Replace("\\", "\\\\");

                settings = JsonConvert.DeserializeObject<Settings>(settingsStr);
            }

            return settings;
        }

        public static Settings InitSettings()
        {
            return GetSettingsFromFile() ?? GetDefaultSettings();
        }
    }
}
