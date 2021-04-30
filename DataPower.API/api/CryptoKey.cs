﻿// Copyright 2021 Keyfactor
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
    public class CryptoKey
    {
        public CryptoKey()
        {
            this.Name = "";
            this.MAdminState = "enabled";
            this.CertFile = "";
            this.PasswordAlias = "off";
            this.IgnoreExpiration = "off";
        }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("mAdminState", NullValueHandling = NullValueHandling.Ignore)]
        public string MAdminState { get; set; }

        [JsonProperty("Filename", NullValueHandling = NullValueHandling.Ignore)]
        public string CertFile { get; set; }

        [JsonProperty("PasswordAlias", NullValueHandling = NullValueHandling.Ignore)]
        public string PasswordAlias { get; set; }

        [JsonProperty("IgnoreExpiration", NullValueHandling = NullValueHandling.Ignore)]
        public string IgnoreExpiration { get; set; }
    }
}