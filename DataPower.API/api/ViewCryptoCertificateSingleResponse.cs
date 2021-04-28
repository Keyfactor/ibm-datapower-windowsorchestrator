using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class ViewCryptoCertificateSingleResponse
    {

        [JsonProperty("CryptoCertificate")]
        public CryptoCertificate CryptoCertificate { get; set; }
    }
}