using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CryptoCert
    {
        [JsonProperty("CertificateDetails")]
        public CertificateDetailsObject CertDetailsObject { get; set; }
    }
}