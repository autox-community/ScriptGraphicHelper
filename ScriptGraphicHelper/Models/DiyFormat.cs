using Newtonsoft.Json;

namespace ScriptGraphicHelper.Models
{
    public class DiyFormat
    {
        [JsonProperty("importInfo")]
        public string ImportInfo { get; set; } = string.Empty;
        [JsonProperty("rangeFormat")]
        public string RangeFormat { get; set; } = string.Empty;
        [JsonProperty("firstColorFormat")]
        public string FirstColorFormat { get; set; } = string.Empty;
        [JsonProperty("followColorFormat")]
        public string FollowColorFormat { get; set; } = string.Empty;
        [JsonProperty("findStrFormat")]
        public string FindStrFormat { get; set; } = string.Empty;
        [JsonProperty("compareStrFormat")]
        public string CompareStrFormat { get; set; } = string.Empty;
        [JsonProperty("isBGR")]
        public bool IsBGR { get; set; } = false;
    }
}