namespace DataPower.API.api
{
    public class DeleteCertificateRequest : Request
    {
        public DeleteCertificateRequest(string domain, string filename)
        {
            this.Domain = domain;
            this.Filename = filename.Trim();
            this.Method = "DELETE";
        }

        public new string GetResource()
        {
            return "/mgmt/filestore/" + this.Domain + "/cert/" + this.Filename;
        }

    }
}