using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class PublicFileStoreLocation
    {

        [JsonProperty("location")]
        public PublicFileStore PubFileStore { get; set; }
    }
}