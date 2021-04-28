using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class PublicFileStore
    {
        [JsonProperty("file")]
        public PublicFile[] PubFiles { get; set; }
    }
}