using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class ViewCryptoCertificatesResponse
    {
        public ViewCryptoCertificatesResponse()
        {
            CryptoCertificates = new CryptoCertificate[0];
        }

        [JsonProperty("CryptoCertificate")]
        public CryptoCertificate[] CryptoCertificates { get; set; }
    }
}