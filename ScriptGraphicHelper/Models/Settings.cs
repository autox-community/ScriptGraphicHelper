using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScriptGraphicHelper.Models
{
    public class Settings
    {
        public static Settings Instance { get; set; } = new();
        [JsonProperty("width")]
        public double Width { get; set; } = 1720;
        [JsonProperty("height")]
        public double Height { get; set; } = 900;
        [JsonProperty("simSelectedIndex")]
        public int SimSelectedIndex { get; set; } = 0;
        [JsonProperty("formatSelectedIndex")]
        public int FormatSelectedIndex { get; set; } = 0;
        [JsonProperty("addRange")]
        public bool AddRange { get; set; } = true;
        [JsonProperty("addInfo")]
        public bool AddInfo { get; set; } = false;
        [JsonProperty("rangeTolerance")]
        public int RangeTolerance { get; set; } = 50;
        [JsonProperty("diySim")]
        public int DiySim { get; set; } = 95;
        [JsonProperty("isOffset")]
        public bool IsOffset { get; set; } = false;
        [JsonProperty("dmRegcode")]
        public string DmRegcode { get; set; } = string.Empty;
        [JsonProperty("ldPath3")]
        public string LdPath3 { get; set; } = string.Empty;
        [JsonProperty("ldPath4")]
        public string LdPath4 { get; set; } = string.Empty;
        [JsonProperty("ldPath64")]
        public string LdPath64 { get; set; } = string.Empty;
        [JsonProperty("formats")]
        public List<FormatConfig>? Formats { get; set; }
        [JsonProperty("imgEditor")]
        public ImgEditorConfig ImgEditor { get; set; } = new ImgEditorConfig();
    }

    public class FormatConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; } = true;
        [JsonProperty("anchorIsEnabled")]
        public bool? AnchorIsEnabled { get; set; }
        [JsonProperty("isCompareMode")]
        public bool? IsCompareMode { get; set; }
        [JsonProperty("isDiyFormat")]
        public bool? IsDiyFormat { get; set; }
        [JsonProperty("diyFormatFileName")]
        public string? DiyFormatFileName { get; set; }

        public static FormatConfig? GetFormat(string name)
        {
            foreach (var format in Settings.Instance.Formats)
            {
                if (format.Name.Equals(name))
                {
                    return format;
                }
            }
            return null;
        }

        public static List<string> GetEnabledFormats()
        {
            var result = new List<string>();
            if (Settings.Instance.Formats is not null)
            {
                foreach (var format in Settings.Instance.Formats)
                {
                    if (format.IsEnabled)
                    {
                        result.Add(format.Name);
                    }
                }
            }
            return result;
        }

        public static List<FormatConfig> CreateFormats()
        {
            var formats = new List<FormatConfig>
                {
                    new FormatConfig
                    {
                        Name = "AT找色",
                        IsEnabled = true,
                    },
                    new FormatConfig
                    {
                        Name = "AT比色",
                        IsEnabled = true,
                        IsCompareMode = true,
                    },
                    new FormatConfig
                    {
                        Name = "AT锚点找色",
                        IsEnabled = true,
                        AnchorIsEnabled = true,
                    },
                    new FormatConfig
                    {
                        Name = "AT锚点比色",
                        IsEnabled = true,
                        AnchorIsEnabled = true,
                        IsCompareMode = true,
                    },
                    new FormatConfig
                    {
                        Name = "锚点找色",
                        IsEnabled = true,
                        AnchorIsEnabled = true,
                    },
                    new FormatConfig
                    {
                        Name = "锚点比色",
                        IsEnabled = true,
                        AnchorIsEnabled = true,
                        IsCompareMode = true,
                    },
                    new FormatConfig
                    {
                        Name = "AJ找色",
                        IsEnabled = true,
                    },
                    new FormatConfig
                    {
                        Name = "AJ比色",
                        IsEnabled = true,
                        IsCompareMode = true,
                    },
                    new FormatConfig
                    {
                        Name = "大漠找色",
                        IsEnabled = true,
                    },
                    new FormatConfig
                    {
                        Name = "按键找色",
                        IsEnabled = true,
                    },
                    new FormatConfig
                    {
                        Name = "按键比色",
                        IsEnabled = true,
                        IsCompareMode= true,
                    },
                    new FormatConfig
                    {
                        Name = "EC找色",
                        IsEnabled = true,
                    },
                    new FormatConfig
                    {
                        Name = "EC比色",
                        IsEnabled = true,
                        IsCompareMode= true,
                    },
                    new FormatConfig
                    {
                        Name = "触动找色",
                        IsEnabled = true,
                    },
                    new FormatConfig
                    {
                        Name = "触动比色",
                        IsEnabled = true,
                        IsCompareMode= true,
                    },
                    new FormatConfig
                    {
                        Name = "自定义找色",
                        IsEnabled = true,
                        IsDiyFormat = true,
                        DiyFormatFileName = "diyForMat.json",
                    },
                };
            return formats;
        }
    }

    public class ImgEditorConfig
    {
        [JsonProperty("modeSelectedIndex")]
        public int ModeSelectedIndex { get; set; } = 0;
        [JsonProperty("threshold")]
        public int Threshold { get; set; } = 12;
        [JsonProperty("size")]
        public int Size { get; set; } = -1;
    }
}
