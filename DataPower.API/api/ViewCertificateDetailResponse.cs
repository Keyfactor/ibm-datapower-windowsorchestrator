using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class ViewCertificateDetailResponse
    {
        [JsonProperty("value")]
        public string Result { get; set; }

        [JsonProperty("CryptoCertificate")]
        public CryptoCert CryptoCertObject { get; set; }
    }
}