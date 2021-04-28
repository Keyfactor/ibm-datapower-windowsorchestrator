using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class SaveConfigRequest : Request
    {
        public SaveConfigRequest(string domain)
        {
            this.SaveConfig = "";
            this.Domain = domain;
            this.Method = "POST";
        }

        public new string GetResource()
        {
            return "/mgmt/actionqueue/" + this.Domain;
        }

        [JsonProperty("SaveConfig")]
        public string SaveConfig { get; set; }

    }
}