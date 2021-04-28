using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CertificateRequest
    {
        public CertificateRequest()
        {
            this.Name = "";
            this.Content = "";
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

    }
}