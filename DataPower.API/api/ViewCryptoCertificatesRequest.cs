namespace DataPower.API.api
{
    public class ViewCryptoCertificatesRequest : Request
    {
        public ViewCryptoCertificatesRequest(string domain)
        {
            this.Domain = domain;
            this.Method = "GET";
            this.Filename = "";
        }

        public ViewCryptoCertificatesRequest(string domain, string alias)
        {
            this.Domain = domain;
            this.Method = "GET";
            this.Filename = alias;
        }

        public new string GetResource()
        {
            if (string.IsNullOrEmpty(this.Filename))
                return "/mgmt/config/" + this.Domain + "/CryptoCertificate";
            else
                return "/mgmt/config/" + this.Domain + "/CryptoCertificate/" + this.Filename;
        }
    }
}