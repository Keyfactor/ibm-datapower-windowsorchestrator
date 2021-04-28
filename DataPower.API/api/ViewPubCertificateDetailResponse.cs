using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class ViewPubCertificateDetailResponse
    {

        [JsonProperty("file")]
        public string File { get; set; }
    }
}