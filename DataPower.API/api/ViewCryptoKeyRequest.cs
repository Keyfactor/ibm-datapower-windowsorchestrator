namespace DataPower.API.api
{
    public class ViewCryptoKeyRequest : Request
    {


        public ViewCryptoKeyRequest(string domain, string alias)
        {
            this.Domain = domain;
            this.Method = "GET";
            this.Filename = alias;
        }

        public new string GetResource()
        {
                return "/mgmt/config/" + this.Domain + "/CryptoKey/" + this.Filename;
        }
    }
}