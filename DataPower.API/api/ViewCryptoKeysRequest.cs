namespace DataPower.API.api
{
    public class ViewCryptoKeysRequest : Request
    {
        
        public ViewCryptoKeysRequest(string domain)
        {
            this.Domain = domain;
            this.Method = "GET";
        }

        public new string GetResource()
        {
            return "/mgmt/config/" + this.Domain + "/CryptoKey";
        }
    }
}