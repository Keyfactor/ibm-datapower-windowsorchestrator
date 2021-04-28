namespace DataPower.API.api
{
    public class ViewPubCertificateDetailRequest : Request
    {
        public ViewPubCertificateDetailRequest(string filename)
        {
            this.Domain = "pubcert";
            this.Filename = filename.Trim();
            this.Method = "GET";
        }

        public new string GetResource()
        {
            return "/mgmt/filestore/default/pubcert/" + this.Filename;
        }

    }
}