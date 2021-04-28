using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CertDetailValue
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}