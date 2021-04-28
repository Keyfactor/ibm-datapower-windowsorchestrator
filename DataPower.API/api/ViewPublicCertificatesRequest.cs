using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class ViewPublicCertificatesRequest : Request
    {

        public ViewPublicCertificatesRequest()
        {
            this.Method = "GET";
            this.Domain = "default";
            this.Folder = "pubcert";
        }
        public ViewPublicCertificatesRequest(string domain, string folder)
        {
            this.Method = "GET";
            this.Domain = domain;
            this.Folder = folder.Trim();
        }

        [JsonIgnore]
        public string Folder { get; set; }

        public new string GetResource()
        {
            return "/mgmt/filestore/" + this.Domain + "/" + this.Folder;
        }
    }
}