using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class ViewCertificateDetailRequest : Request
    {
        public ViewCertificateDetailRequest(string domain)
        {
            this.Domain = domain;
            this.Method = "POST";
        }

        public new string GetResource()
        {
            return "/mgmt/actionqueue/" + this.Domain;
        }

        [JsonProperty("ViewCertificateDetails")]
        public CertificateObjectRequest CertObjectRequest { get; set; }
    }
}