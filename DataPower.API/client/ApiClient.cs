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
using System.IO;
using System.Net;
using System.Text;
using CSS.Common.Logging;
using DataPower.API.api;
using Newtonsoft.Json;
using static System.Net.ServicePointManager;
using DataPower;

namespace DataPower.API.client
{
    public class ApiClient : LoggingClientBase
    {
        //public string AuthenticationSignature { get; set; }
        public string BaseUrl { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        private static readonly Encoding Encoding = Encoding.UTF8;

        #region Constructors

        public ApiClient(string user, string pass, string baseUrl, string domain)
        {
            BaseUrl = baseUrl;
            Username = user;
            Password = pass;
            Domain = domain.Trim();
        }

        #endregion

        #region Class Methods

        public bool SaveConfig()
        {
            try
            {
                var saveConfig = new SaveConfigRequest(Domain);
                var strRequest = JsonConvert.SerializeObject(saveConfig);
                var strResponse = ApiRequestString("SaveConfig", saveConfig.GetResource(), saveConfig.Method,
                    strRequest, false, true);
                JsonConvert.DeserializeObject<SaveConfigResponse>(strResponse);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Saving the Config: {ex.Message}");
                return false;
            }
        }

        public bool AddCertificateFile(CertificateAddRequest certAddRequest)
        {
            var strRequest = JsonConvert.SerializeObject(certAddRequest);
            var strResponse = ApiRequestString("CertFileAddRequest", certAddRequest.GetResource(),
                certAddRequest.Method, strRequest, false, true);
            JsonConvert.DeserializeObject<CertificateAddResponse>(strResponse);
            return true;
        }

        public bool AddCryptoCertificate(CryptoCertificateAddRequest cryptoCertAddRequest)
        {
            var strRequest = JsonConvert.SerializeObject(cryptoCertAddRequest);
            ApiRequestString("CryptoCertAddRequest", cryptoCertAddRequest.GetResource(), cryptoCertAddRequest.Method,
                strRequest, false, true);
            return true;
        }

        public bool UpdateCryptoCertificate(CryptoCertificateUpdateRequest cryptoCertUpdateRequest)
        {
            var strRequest = JsonConvert.SerializeObject(cryptoCertUpdateRequest);
            ApiRequestString("CryptoCertUpdateRequest", cryptoCertUpdateRequest.GetResource(),
                cryptoCertUpdateRequest.Method, strRequest, false, true);
            return true;
        }

        public bool AddCryptoKey(CryptoKeyAddRequest cryptoKeyAddRequest)
        {
            var strRequest = JsonConvert.SerializeObject(cryptoKeyAddRequest);
            ApiRequestString("CryptoKeyAddRequest", cryptoKeyAddRequest.GetResource(), cryptoKeyAddRequest.Method,
                strRequest, false, true);
            return true;
        }

        public bool UpdateCryptoKey(CryptoKeyUpdateRequest cryptoKeyUpdateRequest)
        {
            var strRequest = JsonConvert.SerializeObject(cryptoKeyUpdateRequest);
            ApiRequestString("CryptoKeyAddRequest", cryptoKeyUpdateRequest.GetResource(), cryptoKeyUpdateRequest.Method,
                strRequest, false, true);
            return true;
        }

        public ViewCryptoCertificatesResponse ViewCertificates(ViewCryptoCertificatesRequest viewCertificates)
        {
            var strRequest = JsonConvert.SerializeObject(viewCertificates);
            var strResponse = ApiRequestString("ViewCertificates", viewCertificates.GetResource(),
                viewCertificates.Method, strRequest, false, true);

            var viewCertificatesResponse = new ViewCryptoCertificatesResponse();

            if (strResponse.Contains("No configuration retrieved")) return viewCertificatesResponse;
            var responseCounter = strResponse;
            var strCheck = "PasswordAlias";
            var respCount = (responseCounter.Length - responseCounter.Replace(strCheck, "").Length) /
                            strCheck.Length;

            if (respCount == 1)
            {
                var viewSingleCertificateResponse =
                    JsonConvert.DeserializeObject<ViewCryptoCertificateSingleResponse>(strResponse);
                viewCertificatesResponse.CryptoCertificates = new CryptoCertificate[1];
                viewCertificatesResponse.CryptoCertificates[0] = viewSingleCertificateResponse.CryptoCertificate;
            }
            else
            {
                viewCertificatesResponse =
                    JsonConvert.DeserializeObject<ViewCryptoCertificatesResponse>(strResponse);
            }

            return viewCertificatesResponse;
        }

        public ViewCertificateDetailResponse ViewCryptoCertificate(ViewCertificateDetailRequest viewCertificate)
        {
            var strRequest = JsonConvert.SerializeObject(viewCertificate);
            var strResponse = ApiRequestString("ViewCertificateDetail", viewCertificate.GetResource(),
                viewCertificate.Method, strRequest, false, true);
            var viewCertificateDetailResponse =
                JsonConvert.DeserializeObject<ViewCertificateDetailResponse>(strResponse);
            return viewCertificateDetailResponse;
        }

        public ViewPublicCertificatesResponse ViewPublicCertificates(ViewPublicCertificatesRequest viewPubCertificates)
        {
            var strRequest = JsonConvert.SerializeObject(viewPubCertificates);
            var strResponse = ApiRequestString("ViewPublicCertificates", viewPubCertificates.GetResource(),
                viewPubCertificates.Method, strRequest, false, true);

            var containerName = "file";

            //Datapower API does not return single item arrays correctly (missing brackets) need to add them in to deseralize properly
            if (strResponse.Contains($"\"{containerName}\" :") && !strResponse.Contains($"\"{containerName}\" : ["))
                strResponse = FixDataPowerBadJson(strResponse, containerName);

            var viewPubCertificatesResponse =
                JsonConvert.DeserializeObject<ViewPublicCertificatesResponse>(strResponse);
            return viewPubCertificatesResponse;
        }

        public ViewPubCertificateDetailResponse ViewPublicCertificate(
            ViewPubCertificateDetailRequest viewPubCertificate)
        {
            var strRequest = JsonConvert.SerializeObject(viewPubCertificate);
            var strResponse = ApiRequestString("ViewPublicCertificateDetail", viewPubCertificate.GetResource(),
                viewPubCertificate.Method, strRequest, false, true);
            var viewPubCertificateDetailResponse =
                JsonConvert.DeserializeObject<ViewPubCertificateDetailResponse>(strResponse);
            return viewPubCertificateDetailResponse;
        }

        //DeleteCryptoKeyRequest
        public void DeleteCryptoKey(DeleteCryptoKeyRequest request)
        {
            var strRequest = JsonConvert.SerializeObject(request);
            ApiRequestString("DeleteCryptoKey", request.GetResource(), request.Method, strRequest, false, true);
        }

        //DeleteCryptoCertificateRequest
        public void DeleteCryptoCertificate(DeleteCryptoCertificateRequest request)
        {
            var strRequest = JsonConvert.SerializeObject(request);
            ApiRequestString("DeleteCryptoCertificate", request.GetResource(), request.Method, strRequest, false, true);
        }

        //DeleteCertificateRequest
        public void DeleteCertificate(DeleteCertificateRequest request)
        {
            var strRequest = JsonConvert.SerializeObject(request);
            ApiRequestString("DeleteCertificate", request.GetResource(), request.Method, strRequest, false, true);
        }

        //ViewCryptoKeyRequest
        public ViewCryptoKeysResponse ViewCryptoKeys(ViewCryptoKeysRequest request)
        {
            var strRequest = JsonConvert.SerializeObject(request);
            var strResponse = ApiRequestString("ViewCryptoKey", request.GetResource(), request.Method, strRequest,
                false, true);
            var response = new ViewCryptoKeysResponse();

            var containerName = "CryptoKey";

            //Datapower API does not return single item arrays correctly (missing brackets) need to add them in to deseralize properly
            if (strResponse.Contains($"\"{containerName}\" :") && !strResponse.Contains($"\"{containerName}\" : ["))
                strResponse = FixDataPowerBadJson(strResponse,containerName);

            if (!strResponse.Contains("error"))
                response = JsonConvert.DeserializeObject<ViewCryptoKeysResponse>(strResponse);

            return response;
        }


        private string FixDataPowerBadJson(string json,string containerName)
        {
            json = json.Replace($"\"{containerName}\" :", $"\"{containerName}\" : [");

            if (containerName == "CryptoKey")
            {
                int lastIndex = json.LastIndexOf("}", StringComparison.Ordinal);
                int secondLastIndex =
                    lastIndex > 0 ? json.LastIndexOf("}", lastIndex - 1, StringComparison.Ordinal) : -1;
                if (secondLastIndex >= 0)
                {
                    json = json.Remove(secondLastIndex).Insert(secondLastIndex, "}]}");
                }
            }
            else
            {
                int lastIndex = json.LastIndexOf(",", StringComparison.Ordinal);
                json = json.Remove(lastIndex,1).Insert(lastIndex, "],");
            }

            return json;
        }

        //ViewCryptoCertificatesRequest
        public ViewCryptoCertificateSingleResponse ViewCryptoCertificate(ViewCryptoCertificatesRequest request)
        {
            var strRequest = JsonConvert.SerializeObject(request);
            var strResponse = ApiRequestString("ViewCryptoCertificate", request.GetResource(), request.Method,
                strRequest, false, true);
            var response = new ViewCryptoCertificateSingleResponse();
            if (!strResponse.Contains("error"))
                response = JsonConvert.DeserializeObject<ViewCryptoCertificateSingleResponse>(strResponse);

            return response;
        }

        public string ApiRequestString(string strCall, string strPostUrl, string strMethod, string strQueryString,
            bool bWrite, bool bUseToken)
        {
            Logger.Trace($"BEGIN API Request: {strCall}");
            Logger.Trace($"BaseUrl: {BaseUrl}");
            Logger.Trace($"url: {strPostUrl}");
            Logger.Trace($"strMethod: {strMethod}");
            Logger.Trace($"strQueryString: {strQueryString}");

            try
            {
                ServerCertificateValidationCallback = delegate { return true; };

                var objRequest = (HttpWebRequest) WebRequest.Create(BaseUrl + strPostUrl);
                objRequest.Method = strMethod;
                objRequest.ContentType = "application/json";
                var encoded =
                    Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(Username + ":" + Password));
                objRequest.Headers.Add("Authorization", "Basic " + encoded);

                if (!string.IsNullOrEmpty(strQueryString) &&
                    (objRequest.Method == "POST" || objRequest.Method == "PUT"))
                {
                    var postBytes = Encoding.UTF8.GetBytes(strQueryString);
                    objRequest.ContentLength = postBytes.Length;

                    var requestStream = objRequest.GetRequestStream();
                    requestStream.Write(postBytes, 0, postBytes.Length);
                    requestStream.Close();
                }

                var objResponse = (HttpWebResponse) objRequest.GetResponse();
                var strResponse =
                    new StreamReader(objResponse.GetResponseStream() ?? throw new InvalidOperationException())
                        .ReadToEnd();
                Logger.Trace($"strResponse: {strResponse}");
                Logger.Trace("END APIRequestString");

                return strResponse;
            }
            catch (Exception ex)
            {
                Logger.Trace($"END APIRequestString error: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}