using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CertificateObjectRequest
    {
        [JsonProperty("CertificateObject")]
        public string ObjectName { get; set; }
    }
}