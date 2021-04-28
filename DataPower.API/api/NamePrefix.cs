using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class NamePrefix
    {
        [JsonProperty("CryptoCertObjectPrefix", NullValueHandling = NullValueHandling.Ignore)]
        public string CryptoCertObjectPrefix { get; set; }

        [JsonProperty("CryptoKeyObjectPrefix", NullValueHandling = NullValueHandling.Ignore)]
        public string CryptoKeyObjectPrefix { get; set; }

        [JsonProperty("CertFilePrefix", NullValueHandling = NullValueHandling.Ignore)]
        public string CertFilePrefix { get; set; }

        [JsonProperty("KeyFilePrefix", NullValueHandling = NullValueHandling.Ignore)]
        public string KeyFilePrefix { get; set; }

    }
}