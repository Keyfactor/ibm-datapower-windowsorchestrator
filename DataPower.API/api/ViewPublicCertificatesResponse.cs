using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class ViewPublicCertificatesResponse
    {

        [JsonProperty("filestore")]
        public PublicFileStoreLocation PubFileStoreLocation  { get; set; }
    }
}