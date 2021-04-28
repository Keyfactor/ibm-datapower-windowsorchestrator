using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CertificateAddRequest : Request
    {
        public CertificateAddRequest(string domain, string filename, string folder)
        {
            this.Certificate = new CertificateRequest();
            this.Domain = domain;
            this.Filename = filename.Trim();
            this.Folder = folder.Trim();
            this.Method = "PUT";
        }

        public new string GetResource()
        {
            return "/mgmt/filestore/" + this.Domain + "/" + this.Folder + "/" + this.Filename;
        }

        [JsonIgnore]
        public string Folder { get; set; }

        [JsonProperty("file")]
        public CertificateRequest Certificate { get; set; }
    }
}