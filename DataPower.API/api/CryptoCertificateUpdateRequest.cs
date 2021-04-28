using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CryptoCertificateUpdateRequest : Request
    {
        public CryptoCertificateUpdateRequest(string domain, string name)
        {
            this.CryptoCert = new CryptoCertificate();
            this.Domain = domain;
            this.Method = "PUT";
            this.Name = name;

        }

        public new string GetResource()
        {
            return "/mgmt/config/" + this.Domain + "/CryptoCertificate/" + this.Name;
        }

        [JsonIgnore]
        private string Name { get; set; }

        [JsonProperty("CryptoCertificate")]
        public CryptoCertificate CryptoCert { get; set; }
    }
}