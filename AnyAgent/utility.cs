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

using System;
using DataPower.API.api;
using Keyfactor.Platform.Extensions.Agents;
using Newtonsoft.Json;

namespace DataPower
{
    internal static class Utility
    {
        public static string ReplaceAlias(string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);
            return pos < 0 ? text : text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static Func<string, string> Pemify = ss =>
            ss.Length <= 64 ? ss : ss.Substring(0, 64) + "\n" + Pemify(ss.Substring(64));

        public static NamePrefix ParseStoreProperties(AnyJobConfigInfo config)
        {
            return JsonConvert.DeserializeObject<NamePrefix>(config.Store.Properties.ToString());
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static CertStoreInfo ParseCertificateConfig(AnyJobConfigInfo config)
        {
            var ci = new CertStoreInfo
            {
                Domain = "default",
                CertificateStore = config.Store.StorePath
            };

            if (config.Store.StorePath.Contains(@"\"))
            {
                ci.Domain = GetDomain(config.Store.StorePath, @"\");
                ci.CertificateStore = GetCertStore(config.Store.StorePath, @"\");
            }
            else if (config.Store.StorePath.Contains(@"/"))
            {
                ci.Domain = GetDomain(config.Store.StorePath, @"/");
                ci.CertificateStore = GetCertStore(config.Store.StorePath, @"/");
            }

            return ci;
        }

        public static string GetDomain(string strSource, string deliminiter)
        {
            var start = strSource.IndexOf(deliminiter, 0, StringComparison.Ordinal);
            return strSource.Substring(0, start).Trim();
        }

        public static string GetCertStore(string strSource, string deliminiter)
        {
            var start = strSource.IndexOf(deliminiter, 0, StringComparison.Ordinal) + 1;
            var end = strSource.Length;
            return strSource.Substring(start, end - start).Trim();
        }

        public static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            var place = source.IndexOf(find, StringComparison.Ordinal);
            var result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }


    }
}