// Copyright 2021 Keyfactor
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;

namespace DataPower.API.api
{
    public class CryptoCertificateUpdateRequest : Request
    {
        public CryptoCertificateUpdateRequest(string domain, string name)
        {
            this.CryptoCert = new CryptoCertificate();
            this.Domain = domain;
            this.Method = "PUT";
            this.Name = name;

        }

        public new string GetResource()
        {
            return "/mgmt/config/" + this.Domain + "/CryptoCertificate/" + this.Name;
        }

        [JsonIgnore]
        private string Name { get; set; }

        [JsonProperty("CryptoCertificate")]
        public CryptoCertificate CryptoCert { get; set; }
    }
}