using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CryptoKeyUpdateRequest : Request
    {
        public CryptoKeyUpdateRequest(string domain, string name)
        {
            this.CryptoKey = new CryptoKey();
            this.Domain = domain;
            this.Method = "PUT";
            this.Name = name;
        }

        public new string GetResource()
        {
            return "/mgmt/config/" + this.Domain + "/CryptoKey/" + Name;
        }

        [JsonIgnore]
        private string Name { get; }

        [JsonProperty("CryptoKey")]
        public CryptoKey CryptoKey { get; set; }
    }
}