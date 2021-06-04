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