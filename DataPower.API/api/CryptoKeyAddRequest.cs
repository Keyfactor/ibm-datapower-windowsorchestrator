using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CryptoKeyAddRequest : Request
    {
        public CryptoKeyAddRequest(string domain)
        {
            this.CryptoKey = new CryptoKey();
            this.Domain = domain;
            this.Method = "POST";
        }

        public new string GetResource()
        {
            return "/mgmt/config/" + this.Domain + "/CryptoKey";
        }

        [JsonProperty("CryptoKey")]
        public CryptoKey CryptoKey { get; set; }
    }
}