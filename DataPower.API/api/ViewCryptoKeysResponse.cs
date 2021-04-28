using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class ViewCryptoKeysResponse
    {
        public ViewCryptoKeysResponse()
        {
            CryptoKeys = new CryptoKey[0];
        }


        [JsonProperty("CryptoKey")]
        public CryptoKey[] CryptoKeys { get; set; }
    }
}