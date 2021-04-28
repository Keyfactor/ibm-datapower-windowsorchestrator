namespace DataPower.API.api
{
    public class DeleteCryptoKeyRequest : Request
    {
        public DeleteCryptoKeyRequest(string domain, string filename)
        {
            this.Domain = domain;
            this.Filename = filename.Trim();
            this.Method = "DELETE";
        }

        public new string GetResource()
        {
            return "/mgmt/config/" + this.Domain + "/CryptoKey/" + this.Filename;
        }

    }
}