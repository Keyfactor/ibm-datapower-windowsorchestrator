using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class SaveConfigResponse
    {
        [JsonProperty("SaveConfig")]
        public string SaveConfig { get; set; }
    }
}