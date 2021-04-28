using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CryptoKey
    {
        public CryptoKey()
        {
            this.Name = "";
            this.MAdminState = "enabled";
            this.CertFile = "";
            this.PasswordAlias = "off";
            this.IgnoreExpiration = "off";
        }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("mAdminState", NullValueHandling = NullValueHandling.Ignore)]
        public string MAdminState { get; set; }

        [JsonProperty("Filename", NullValueHandling = NullValueHandling.Ignore)]
        public string CertFile { get; set; }

        [JsonProperty("PasswordAlias", NullValueHandling = NullValueHandling.Ignore)]
        public string PasswordAlias { get; set; }

        [JsonProperty("IgnoreExpiration", NullValueHandling = NullValueHandling.Ignore)]
        public string IgnoreExpiration { get; set; }
    }
}