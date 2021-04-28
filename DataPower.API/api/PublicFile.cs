using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class PublicFile
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}