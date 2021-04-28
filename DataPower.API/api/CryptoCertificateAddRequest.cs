using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CryptoCertificateAddRequest : Request
    {
        public CryptoCertificateAddRequest(string domain)
        {
            this.CryptoCert = new CryptoCertificate();
            this.Domain = domain;
            this.Method = "POST";
        }

        public new string GetResource()
        {
            return "/mgmt/config/" + this.Domain + "/CryptoCertificate";
        }

        [JsonProperty("CryptoCertificate")]
        public CryptoCertificate CryptoCert { get; set; }
    }
}