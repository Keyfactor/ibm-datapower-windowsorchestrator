using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CertificateAddResponse
    {
        public CertificateAddResponse()
        {
            this.Result = "";
        }

        [JsonProperty("result")]
        public string Result { get; set; }

    }
}