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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CSS.Common.Logging;
using DataPower.API.api;
using DataPower.API.client;
using Keyfactor.Platform.Extensions.Agents;
using Keyfactor.Platform.Extensions.Agents.Enums;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using CertificateRequest = DataPower.API.api.CertificateRequest;

namespace DataPower
{
    internal class CertManager : LoggingClientBase
    {
        private readonly Configuration _appConfig;
        private readonly string _protocol;

        public CertManager()
        {
            _appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            _protocol = _appConfig.AppSettings.Settings["Protocol"].Value;
        }

        public bool DoesCryptoCertificateObjectExist(CertStoreInfo ci, string cryptoCertObjectName, ApiClient apiClient)
        {
            var bUpdateCryptoCertificateObject = false;
            try
            {
                Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
                //Get a count of the crypto certificates that have the name we are looking for should be equal to one if it exists
                var viewAllCryptoCertRequest = new ViewCryptoCertificatesRequest(ci.Domain);
                Logger.Trace($"viewAllCryptoCertRequest JSON {JsonConvert.SerializeObject(viewAllCryptoCertRequest)}");
                var viewAllCryptoCertResponse = apiClient.ViewCertificates(viewAllCryptoCertRequest);
                Logger.Trace($"viewAllCryptoCertResponse JSON {JsonConvert.SerializeObject(viewAllCryptoCertResponse)}");

                if (viewAllCryptoCertResponse.CryptoCertificates.Count(x => x.Name == cryptoCertObjectName) == 1)
                {
                    Logger.Trace("Only One Found, we are good!");
                    bUpdateCryptoCertificateObject = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an issue receiving the certificates: {cryptoCertObjectName} Error {ex.Message}");
            }
            Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);

            return bUpdateCryptoCertificateObject;
        }

        public void DisableCryptoCertificateObject(string cryptoCertObjectName, ApiClient apiClient)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace($"Disable State for Crypto Certificate Object: {cryptoCertObjectName}");
            try
            {
                var cryptoCertUpdateRequest =
                    new CryptoCertificateUpdateRequest(apiClient.Domain, cryptoCertObjectName)
                    {
                        CryptoCert = new CryptoCertificate
                        {
                            MAdminState = "disabled",
                            Name = cryptoCertObjectName,
                            CertFile = null,
                            IgnoreExpiration = null,
                            PasswordAlias = null
                        }
                    };
                Logger.Trace($"cryptoCertUpdateRequest JSON {JsonConvert.SerializeObject(cryptoCertUpdateRequest)}");
                apiClient.UpdateCryptoCertificate(cryptoCertUpdateRequest);
                Logger.Trace("Crypto Certificate Updated");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an issue disabling the certificate object: {cryptoCertObjectName} Error {ex.Message}");
            }
        }

        public bool DoesCryptoKeyObjectExist(CertStoreInfo ci, string cryptoKeyObjectName, ApiClient apiClient)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            var bUpdateCryptoKeyObject = false;
            try
            {
                //Look for CryptoKey
                var viewCryptoKeyRequest = new ViewCryptoKeysRequest(ci.Domain);
                Logger.Trace($"viewCryptoKeyRequest JSON {JsonConvert.SerializeObject(viewCryptoKeyRequest)}");
                var viewCryptoKeyResponse = apiClient.ViewCryptoKeys(viewCryptoKeyRequest);
                Logger.Trace($"viewCryptoKeyResponse JSON {JsonConvert.SerializeObject(viewCryptoKeyResponse)}");
                if (viewCryptoKeyResponse.CryptoKeys.Count(x => x.Name == cryptoKeyObjectName) == 1)
                {
                    Logger.Trace("Only One Found, we are good!");
                    bUpdateCryptoKeyObject = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Crypto Key Object does not exist: {cryptoKeyObjectName} : {ex.Message}");
            }
            Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            return bUpdateCryptoKeyObject;
        }

        public void DisableCryptoKeyObject(string cryptoKeyObjectName, ApiClient apiClient)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace($"Disable State for Crypto Certificate Object: {cryptoKeyObjectName}");
            try
            {
                var cryptoKeyUpdateRequest = new CryptoKeyUpdateRequest(apiClient.Domain, cryptoKeyObjectName)
                {
                    CryptoKey = new CryptoKey
                    {
                        MAdminState = "disabled",
                        Name = cryptoKeyObjectName,
                        CertFile = null,
                        IgnoreExpiration = null,
                        PasswordAlias = null
                    }
                };
                Logger.Trace($"cryptoKeyUpdateRequest JSON {JsonConvert.SerializeObject(cryptoKeyUpdateRequest)}");
                apiClient.UpdateCryptoKey(cryptoKeyUpdateRequest);
                Logger.Trace("Crypto Key Updated!");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an issue disabling the certificate *key*: {cryptoKeyObjectName} Error {ex.Message}");
            }
        }

        public void UpdatePrivateKey(CertStoreInfo ci, string cryptoKeyObjectName,
            ApiClient apiClient, string keyFileName, string alias)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace($"Updating Crypto Key Object: {cryptoKeyObjectName}");
            try
            {
                var cryptoKeyRequest = new CryptoKeyUpdateRequest(apiClient.Domain, cryptoKeyObjectName)
                {
                    CryptoKey = new CryptoKey
                    {
                        CertFile = ci.CertificateStore.Trim() + ":///" + keyFileName,
                        Name = cryptoKeyObjectName
                    }
                };
                Logger.Trace($"cryptoKeyRequest JSON {JsonConvert.SerializeObject(cryptoKeyRequest)}");
                apiClient.UpdateCryptoKey(cryptoKeyRequest);
                Logger.Trace("Private Key Updated!");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an issue updating the private key: {cryptoKeyObjectName} Error {ex.Message}");
            }
        }

        public void AddCryptoKey(CertStoreInfo ci, string cryptoKeyObjectName, ApiClient apiClient, string keyFileName,
            string alias)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace(
                $"Adding CryptoKey Object for Private Key {alias} to CERT store with Filename {keyFileName} ");
            try
            {
                var cryptoKeyRequest = new CryptoKeyAddRequest(apiClient.Domain)
                {
                    CryptoKey = new CryptoKey
                    {
                        CertFile = ci.CertificateStore.Trim() + ":///" + keyFileName,
                        Name = cryptoKeyObjectName
                    }
                };
                Logger.Trace($"cryptoKeyRequest JSON {JsonConvert.SerializeObject(cryptoKeyRequest)}");
                apiClient.AddCryptoKey(cryptoKeyRequest);
                Logger.Trace("Private Key Added!");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Adding CryptoKey Object for Private Key {alias}: {cryptoKeyObjectName} Error {ex.Message}");
            }
        }

        public AnyErrors RemovePrivateKeyFile(AnyJobConfigInfo addConfig, CertStoreInfo ci,
            string keyFileName)
        {
            try
            {
                Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
                Logger.Trace($"Removing Old Private Key File {keyFileName}");
                var removeFileResult = RemoveFile(addConfig, ci, keyFileName);
                Logger.Trace($"Private Key {keyFileName} is removed");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
                return removeFileResult;
            }
            catch (Exception e)
            {
                Logger.Error($"Error In CertManager.RemovePrivateKeyFile: {e.Message}");
                throw;
            }
        }

        public CertificateAddRequest AddPrivateKey(CertStoreInfo ci, string alias, string keyFileName,
            ApiClient apiClient,
            string privateKeyString)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace($"Adding Private Key {alias} to CERT store with Filename {keyFileName} ");
            try
            {
                var certKeyRequest = new CertificateAddRequest(apiClient.Domain, keyFileName, ci.CertificateStore)
                {
                    Certificate = new CertificateRequest
                    {
                        Name = keyFileName,
                        Content = privateKeyString
                    }
                };
                Logger.Trace($"certKeyRequest JSON {JsonConvert.SerializeObject(certKeyRequest)}");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
                return certKeyRequest;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Adding Private Key {alias} to CERT store with Filename {keyFileName} Error {ex.Message}");
            }

            return null;
        }

        public void UpdateCryptoCert(CertStoreInfo ci, string cryptoCertObjectName,
            ApiClient apiClient, string certFileName, string alias)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace($"Updating Crypto Certificate Object: {cryptoCertObjectName}");
            try
            {
                var cryptoCertRequest = new CryptoCertificateUpdateRequest(apiClient.Domain, cryptoCertObjectName)
                {
                    CryptoCert = new CryptoCertificate
                    {
                        CertFile = ci.CertificateStore.Trim() + ":///" + certFileName,
                        Name = cryptoCertObjectName
                    }
                };

                Logger.Trace($"certKeyRequest JSON {JsonConvert.SerializeObject(cryptoCertRequest)}");
                apiClient.UpdateCryptoCertificate(cryptoCertRequest);
                Logger.Trace("UpdateCryptoCert Updated !");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Updating Crypto Certificate Object: {cryptoCertObjectName} Error {ex.Message}");
            }
        }

        public void AddCryptoCert(CertStoreInfo ci, string cryptoCertObjectName, ApiClient apiClient, string certFileName,
            string alias)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace(
                $"Adding Crypto Object for Certificate {alias} to CERT store with Filename {certFileName} ");
            try
            {
                var cryptoCertRequest = new CryptoCertificateAddRequest(apiClient.Domain)
                {
                    CryptoCert = new CryptoCertificate
                    {
                        CertFile = ci.CertificateStore.Trim() + ":///" + certFileName,
                        Name = cryptoCertObjectName
                    }
                };
                Logger.Trace($"cryptoCertRequest JSON {JsonConvert.SerializeObject(cryptoCertRequest)}");
                apiClient.AddCryptoCertificate(cryptoCertRequest);
                Logger.Trace("AddCryptoCert Added!");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Adding Crypto Object for Certificate {alias} to CERT store with Filename {certFileName} Error {ex.Message}");
            }
        }

        public AnyErrors RemoveCertificate(AnyJobConfigInfo addConfig, CertStoreInfo ci, string certFileName)
        {
            try
            {
                Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
                Logger.Trace($"Removing Old Certificate File {certFileName}");
                var result = RemoveFile(addConfig, ci, certFileName);
                Logger.Trace($"Old Certificate File {certFileName} is removed");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"Error In CertManager.RemovePrivateKeyFile: {e.Message}");
                throw;
            }
        }

        public CertificateAddRequest CertificateAddRequest(CertStoreInfo ci, string alias, string certFileName,
            ApiClient apiClient, string certPem)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace($"Adding Certificate {alias} with Filename {certFileName} ");
            try
            {
                var certRequest = new CertificateAddRequest(apiClient.Domain, certFileName, ci.CertificateStore)
                {
                    Certificate = new CertificateRequest
                    {
                        Name = certFileName,
                        Content = certPem
                    }
                };
                Logger.Trace($"certRequest JSON {JsonConvert.SerializeObject(certRequest)}");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
                return certRequest;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Adding Certificate {alias} with Filename {certFileName} Error {ex.Message}");
            }

            return null;
        }

        public bool DoesKeyFileExist(CertStoreInfo ci, string keyFileName, ViewPublicCertificatesResponse viewCertificateCollection)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            var bRemoveKeyFile = false;
            try
            {
                var keyFile =
                    viewCertificateCollection.PubFileStoreLocation?.PubFileStore?.PubFiles?.FirstOrDefault(x =>
                        x?.Name == keyFileName);

                if (!(keyFile is null))
                {
                    Logger.Trace($"Matching Key File {keyFileName} was found in domain {ci.Domain}");
                    bRemoveKeyFile = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Matching Key File {keyFileName} was found in domain {ci.Domain} Error {ex.Message}");
            }
            Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            return bRemoveKeyFile;
        }

        public bool DoesCertificateFileExist(CertStoreInfo ci, ApiClient apiClient,
            string certFileName, ViewPublicCertificatesResponse viewCertificateCollection)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            var bRemoveCertificateFile = false;
            try
            {
                var publicCertificate =
                    viewCertificateCollection.PubFileStoreLocation?.PubFileStore?.PubFiles?.FirstOrDefault(x => x?.Name == certFileName);

                if (!(publicCertificate is null))
                {
                    Logger.Trace($"Matching Certificate File {certFileName} was found in domain {ci.Domain}");
                    bRemoveCertificateFile = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Matching Certificate File {certFileName} was found in domain {ci.Domain} Error {ex.Message}");
            }
            Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            return bRemoveCertificateFile;
        }


        public string GetCertPem(AnyJobConfigInfo addConfig, string alias, ref string privateKeyString)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            Logger.Trace($"alias {alias} privateKeyString {privateKeyString}");
            string certPem = null;
            try
            {
                if (!string.IsNullOrEmpty(addConfig.Job.PfxPassword))
                {
                    Logger.Trace($"Certificate and Key exist for {alias}");
                    var certData = Convert.FromBase64String(addConfig.Job.EntryContents);

                    Pkcs12Store store;

                    using (MemoryStream ms = new MemoryStream(certData))
                    {
                        store = new Pkcs12Store(ms,
                            addConfig.Job.PfxPassword.ToCharArray());

                        string storeAlias;
                        TextWriter streamWriter;
                        using (var memoryStream = new MemoryStream())
                        {
                            streamWriter = new StreamWriter(memoryStream);
                            var pemWriter = new PemWriter(streamWriter);

                            storeAlias = store.Aliases.Cast<string>().SingleOrDefault(a => store.IsKeyEntry(a));
                            var publicKey = store.GetCertificate(storeAlias).Certificate.GetPublicKey();
                            var privateKey = store.GetKey(storeAlias).Key;
                            var keyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);

                            var pkStart = "-----BEGIN RSA PRIVATE KEY-----\n";
                            var pkEnd = "\n-----END RSA PRIVATE KEY-----";


                            pemWriter.WriteObject(keyPair.Private);
                            streamWriter.Flush();
                            privateKeyString = Encoding.ASCII.GetString(memoryStream.GetBuffer()).Trim()
                                .Replace("\r", "")
                                .Replace("\0", "");
                            privateKeyString = privateKeyString.Replace(pkStart, "").Replace(pkEnd, "");

                            memoryStream.Close();
                        }

                        streamWriter.Close();

                        // Extract server certificate
                        certPem = Utility.Pemify(
                            Convert.ToBase64String(store.GetCertificate(storeAlias).Certificate.GetEncoded()));
                    }
                }
                else
                {
                    Logger.Trace($"Certificate ONLY for {alias}");
                    certPem = Utility.Pemify(addConfig.Job.EntryContents);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Generating PEM: Error {ex.Message}");
            }
            Logger.Trace($"PEM {certPem}");
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            return certPem;
        }

        public AnyErrors AddPubCert(AnyJobConfigInfo addPubConfig, CertStoreInfo ci, NamePrefix np)
        {

            var error = new AnyErrors();
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            error.HasError = false;
            Logger.Trace($"Entering AddPubCert for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            Logger.Trace($"Creating API Client Created with user: {addPubConfig.Server.Username} password: {addPubConfig.Server.Password} protocol: {_protocol} ClientMachine: {addPubConfig.Store.ClientMachine.Trim()} Domain: {ci.Domain}");
            var apiClient = new ApiClient(addPubConfig.Server.Username, addPubConfig.Server.Password,
                $"{_protocol}://" + addPubConfig.Store.ClientMachine.Trim(), ci.Domain);
            Logger.Trace("API Client Created");

            var certAlias = addPubConfig.Job.Alias;

            if (string.IsNullOrEmpty(certAlias))
                certAlias = Guid.NewGuid().ToString();

            Logger.Trace($"certAlias {certAlias}");

            try
            {

                Pkcs12Store store;
                string certPem;
                var certData = Convert.FromBase64String(addPubConfig.Job.EntryContents);

                //If you have a password then you will get a PFX in return instead of the base64 encoded string
                if (!String.IsNullOrEmpty(addPubConfig.Job?.PfxPassword))
                {
                    Logger.Trace($"Has PFX Password {addPubConfig.Job?.PfxPassword}");
                    using (MemoryStream ms = new MemoryStream(certData))
                    {

                        store = new Pkcs12Store(ms, addPubConfig.Job.PfxPassword.ToCharArray());
                        var storeAlias = store.Aliases.Cast<string>().SingleOrDefault(a => store.IsKeyEntry(a));
                        certPem = Utility.Pemify(
                            Convert.ToBase64String(store.GetCertificate(storeAlias).Certificate.GetEncoded()));
                    }
                }
                else
                {
                    certPem = Utility.Pemify(addPubConfig.Job.EntryContents);
                }

                Logger.Trace($"certPem {certPem}");

                var certFileName = certAlias.Replace(".", "_") + ".pem";

                Logger.Trace(
                    $"Adding Public Cert Certificate {certAlias} to PubCert store with Filename {certFileName} ");
                var certRequest =
                    new CertificateAddRequest(apiClient.Domain, certFileName, ci.CertificateStore.Trim())
                    {
                        Certificate = new CertificateRequest
                        {
                            Name = certFileName,
                            Content = certPem
                        }
                    };
                Logger.Trace($"certRequest JSON {JsonConvert.SerializeObject(certRequest)}");
                apiClient.AddCertificateFile(certRequest);
                Logger.Trace("Certificate Added!");
                apiClient.SaveConfig();
                Logger.Trace("Configuration Saved!");

            }
            catch (Exception ex)
            {
                error.HasError = true;
                Logger.Trace($"Error on {certAlias}: {ex.Message}");
                apiClient.SaveConfig();
            }
            Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            return error;
        }


        private AnyErrors RemoveCertFromDomain(AnyJobConfigInfo removeConfig, CertStoreInfo ci, NamePrefix np)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            var error = new AnyErrors { HasError = false };
            Logger.Trace($"Entering RemoveCertStore for {removeConfig.Job.Alias} ");
            Logger.Trace(
                $"Entering RemoveCertStore for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            Logger.Trace($"Creating API Client Created with user: {removeConfig.Server.Username} password: {removeConfig.Server.Password} protocol: {_protocol} ClientMachine: {removeConfig.Store.ClientMachine.Trim()} Domain: {ci.Domain}");
            var apiClient = new ApiClient(removeConfig.Server.Username, removeConfig.Server.Password,
                $"{_protocol}://" + removeConfig.Store.ClientMachine.Trim(), ci.Domain);
            Logger.Trace("API Client Created!");
            try
            {
                Logger.Trace($"Checking to find CryptoCertObject {removeConfig.Job.Alias} ");
                var viewCert = new ViewCryptoCertificatesRequest(apiClient.Domain, removeConfig.Job.Alias);
                Logger.Trace($"viewCert JSON {JsonConvert.SerializeObject(viewCert)}");

                var viewCertificateSingle = apiClient.ViewCryptoCertificate(viewCert);
                Logger.Trace($"viewCert JSON {JsonConvert.SerializeObject(viewCertificateSingle)}");

                if (viewCertificateSingle != null && !string.IsNullOrEmpty(viewCertificateSingle.CryptoCertificate.Name))
                {
                    Logger.Trace($"Remove CryptoObject {viewCertificateSingle.CryptoCertificate.Name} ");
                    var request =
                        new DeleteCryptoCertificateRequest(apiClient.Domain, removeConfig.Job.Alias);
                    Logger.Trace($"request JSON {JsonConvert.SerializeObject(request)}");
                    apiClient.DeleteCryptoCertificate(request);
                    Logger.Trace($"Remove Certificate File {viewCertificateSingle.CryptoCertificate.CertFile} ");
                    var request2 = new DeleteCertificateRequest(apiClient.Domain,
                        viewCertificateSingle.CryptoCertificate.CertFile.Replace(ci.CertificateStore + ":///", ""));
                    Logger.Trace($"request2 JSON {JsonConvert.SerializeObject(request2)}");
                    apiClient.DeleteCertificate(request2);
                    Logger.Trace("Certificate Deleted!");
                }

                var cryptoKeyObjectName = Utility.ReplaceFirstOccurrence(removeConfig.Job.Alias,
                    np.CryptoCertObjectPrefix?.Trim() ?? String.Empty, np.CryptoKeyObjectPrefix?.Trim() ?? String.Empty);
                Logger.Trace($"Checking to find CryptoKeyObject {cryptoKeyObjectName} ");
                var viewKey = new ViewCryptoKeysRequest(apiClient.Domain);
                Logger.Trace($"viewKey JSON {JsonConvert.SerializeObject(viewKey)}");
                var viewKeyResponse = apiClient.ViewCryptoKeys(viewKey);
                Logger.Trace($"viewKeyResponse JSON {JsonConvert.SerializeObject(viewKeyResponse)}");
                var cryptoKey = viewKeyResponse.CryptoKeys.FirstOrDefault(x => x.Name == cryptoKeyObjectName);
                Logger.Trace($"cryptoKey JSON {JsonConvert.SerializeObject(cryptoKey)}");
                if (viewKeyResponse.CryptoKeys != null && !string.IsNullOrEmpty(cryptoKey?.Name))
                {
                    Logger.Trace($"Remove CryptoKeyObject {cryptoKey.Name} ");
                    var request = new DeleteCryptoKeyRequest(apiClient.Domain, cryptoKeyObjectName);
                    Logger.Trace($"request JSON {JsonConvert.SerializeObject(request)}");
                    apiClient.DeleteCryptoKey(request);
                    Logger.Trace($"Remove Key File {cryptoKey.CertFile} ");
                    var request2 = new DeleteCertificateRequest(apiClient.Domain,
                        cryptoKey.CertFile.Replace(ci.CertificateStore + ":///", ""));
                    Logger.Trace($"request2 JSON {JsonConvert.SerializeObject(request2)}");
                    apiClient.DeleteCertificate(request2);
                    Logger.Trace("Certificate Deleted!");
                }
            }
            catch (Exception ex)
            {
                error.HasError = true;
                error.ErrorMessage = ex.Message;
                Logger.Trace($"Error on {removeConfig.Job.Alias}: {ex.Message}");
            }
            Logger.Trace("Saving Config!");
            apiClient.SaveConfig();
            Logger.Trace("Config Saved!");
            Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            return error;
        }

        private AnyErrors RemoveFile(AnyJobConfigInfo removeConfig, CertStoreInfo ci, string filename)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            var error = new AnyErrors { HasError = false };
            Logger.Trace($"Entering RemoveFile for {removeConfig.Job.Alias} filename {filename}");
            Logger.Trace($"Entering RemoveFile for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            Logger.Trace($"Creating API Client Created with user: {removeConfig.Server.Username} password: {removeConfig.Server.Password} protocol: {_protocol} ClientMachine: {removeConfig.Store.ClientMachine.Trim()} Domain: {ci.Domain}");
            var apiClient = new ApiClient(removeConfig.Server.Username, removeConfig.Server.Password,
                $"{_protocol}://" + removeConfig.Store.ClientMachine.Trim(), ci.Domain);
            Logger.Trace("Api Client Created!");
            try
            {
                Logger.Trace($"Deleting Actual File {filename} ");
                var request2 = new DeleteCertificateRequest(apiClient.Domain,
                    filename.Replace(ci.CertificateStore + ":///", ""));
                Logger.Trace($"request2 JSON {JsonConvert.SerializeObject(request2)}");
                apiClient.DeleteCertificate(request2);
                Logger.Trace("Certificate Deleted!");
            }
            catch (Exception ex)
            {
                error.HasError = true;
                error.ErrorMessage = ex.Message;
                Logger.Trace($"Error on {removeConfig.Job.Alias}: {ex.Message}");
            }
            Logger.Trace("Saving Config!");
            apiClient.SaveConfig();
            Logger.Trace("Config Saved!");

            Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            return error;
        }

        public AnyErrors Remove(AnyJobConfigInfo removeConfig, CertStoreInfo ci, NamePrefix np)
        {
            try
            {
                Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
                var error = new AnyErrors { HasError = false };

                var publicCertStoreName = _appConfig.AppSettings.Settings["PublicCertStoreName"].Value;
                var storePath = removeConfig.Store.StorePath;
                Logger.Trace($"publicCertStoreName: {publicCertStoreName} storePath: {storePath}");

                if (storePath.Contains(publicCertStoreName))
                {
                    Logger.Trace("Cannot Remove Public Certificates");
                    error.HasError = true;
                }
                else
                {
                    error = RemoveCertFromDomain(removeConfig, ci, np);
                }
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
                Logger.Trace($"AnyErrors Return {JsonConvert.SerializeObject(error)}");
                return error;
            }
            catch (Exception e)
            {
                Logger.Error($"Error In CertManager.Remove {e.Message}!");
                throw;
            }
        }

        public AnyErrors Add(AnyJobConfigInfo addConfig, CertStoreInfo ci, NamePrefix np)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            try
            {
                var result = new AnyErrors();
                Logger.Trace("Entering Add");
                result.HasError = false;

                var publicCertStoreName = _appConfig.AppSettings.Settings["PublicCertStoreName"].Value;
                var storePath = addConfig.Store.StorePath;
                Logger.Trace($"publicCertStoreName: {publicCertStoreName} storePath: {storePath}");

                result = storePath.Contains(publicCertStoreName) ? AddPubCert(addConfig, ci, np) : AddCertStore(addConfig, ci, np);
                Logger.Trace($"result Return {JsonConvert.SerializeObject(result)}");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"Error In CertManager.Add {e.Message}!");
                throw;
            }
        }

        private AnyErrors AddCertStore(AnyJobConfigInfo addConfig, CertStoreInfo ci, NamePrefix np)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            var error = new AnyErrors();
            var privateKeyString = "";

            Logger.Trace($"Entering AddCertStore for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            Logger.Trace($"Creating API Client Created with user: {addConfig.Server.Username} password: {addConfig.Server.Password} protocol: {_protocol} ClientMachine: {addConfig.Store.ClientMachine.Trim()} Domain: {ci.Domain}");
            var apiClient = new ApiClient(addConfig.Server.Username, addConfig.Server.Password,
                $"{_protocol}://" + addConfig.Store.ClientMachine.Trim(),
                ci.Domain);
            Logger.Trace("apiClient created!");

            var alias = addConfig.Job.Alias.ToLower();
            if (string.IsNullOrEmpty(alias))
                alias = Guid.NewGuid().ToString().ToLower();

            Logger.Trace($"alias: {alias}");

            try
            {
                if (!string.IsNullOrEmpty(addConfig.Job.PfxPassword))
                {
                    Logger.Trace($"Has Password: {addConfig.Job.PfxPassword}");
                    var certPem = GetCertPem(addConfig, alias, ref privateKeyString);
                    Logger.Trace($"certPem: {certPem}");
                    var baseAlias = alias.ToLower();
                    Logger.Trace($"baseAlias: {baseAlias}");

                    var cryptoObjectPrefix = np.CryptoCertObjectPrefix?.Trim().ToLower() ?? string.Empty;
                    var keyFileNamePrefix = np.KeyFilePrefix?.Trim().ToLower() ?? string.Empty;
                    var certFileNamePrefix = np.CertFilePrefix?.Trim().ToLower() ?? string.Empty;
                    var cryptoKeyObjectPrefix = np.CryptoKeyObjectPrefix?.Trim().ToLower() ?? string.Empty;

                    Logger.Trace($"cryptoObjectPrefix: {cryptoObjectPrefix}");
                    Logger.Trace($"keyFileNamePrefix: {keyFileNamePrefix}");
                    Logger.Trace($"certFileNamePrefix: {certFileNamePrefix}");
                    Logger.Trace($"cryptoKeyObjectPrefix: {cryptoKeyObjectPrefix}");

                    if (alias.ToLower().StartsWith(cryptoObjectPrefix))
                        baseAlias = Utility.ReplaceAlias(alias.ToLower(), cryptoObjectPrefix,
                            "");

                    Logger.Trace($"baseAlias: {baseAlias}");

                    var certFileName = certFileNamePrefix + baseAlias + ".cer";
                    var keyFileName = keyFileNamePrefix + baseAlias + ".pem";
                    var cryptoCertObjectName = cryptoObjectPrefix + baseAlias;
                    var cryptoKeyObjectName = cryptoKeyObjectPrefix + baseAlias;

                    Logger.Trace($"certFileName: {certFileName}");
                    Logger.Trace($"keyFileName: {keyFileName}");
                    Logger.Trace($"cryptoCertObjectName: {cryptoCertObjectName}");
                    Logger.Trace($"cryptoKeyObjectName: {cryptoKeyObjectName}");

                    //Get the certificate collection to be used to check for cert files and private keys
                    var viewCert = new ViewPublicCertificatesRequest(ci.Domain, ci.CertificateStore);
                    Logger.Trace($"viewCert JSON {JsonConvert.SerializeObject(viewCert)}");
                    var viewCertificateCollection = apiClient.ViewPublicCertificates(viewCert);
                    Logger.Trace($"viewCertificateCollection JSON {JsonConvert.SerializeObject(viewCertificateCollection)}");

                    Logger.Trace("Starting ReplaceCertificateFile!");
                    ReplaceCertificateFile(addConfig, ci, apiClient, certFileName, viewCertificateCollection, alias, certPem);
                    Logger.Trace("Finished ReplaceCertificateFile!");
                    Logger.Trace("Starting ReplaceCryptoObject!");
                    ReplaceCryptoObject(ci, cryptoCertObjectName, apiClient, certFileName, alias);
                    Logger.Trace("Finished ReplaceCryptoObject!");
                    Logger.Trace("Starting ReplacePrivateKey!");
                    ReplacePrivateKey(addConfig, ci, keyFileName, viewCertificateCollection, alias, apiClient, privateKeyString);
                    Logger.Trace("Finished ReplacePrivateKey!");
                    Logger.Trace("Starting ReplaceCryptoKeyObject!");
                    ReplaceCryptoKeyObject(ci, cryptoKeyObjectName, apiClient, keyFileName, alias);
                    Logger.Trace("Finished ReplaceCryptoKeyObject!");
                }
                Logger.Trace("Saving Config!");
                apiClient.SaveConfig();
                Logger.Trace("Config Saved!");
            }
            catch (Exception ex)
            {
                error.HasError = true;
                Logger.Trace($"Error on {alias}: {ex.Message}");
                apiClient.SaveConfig();
            }
            Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            return error;
        }

        private void ReplacePrivateKey(AnyJobConfigInfo addConfig, CertStoreInfo ci, string keyFileName,
            ViewPublicCertificatesResponse viewCertificateCollection, string alias, ApiClient apiClient,
            string privateKeyString)
        {
            Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
            try
            {
                //See if KeyFile Exists if so remove and add a new one, if not just add a new one
                var bRemoveKeyFile = DoesKeyFileExist(ci, keyFileName, viewCertificateCollection);
                Logger.Trace($"bRemoveKeyFile {bRemoveKeyFile}");
                if (bRemoveKeyFile)
                {
                    Logger.Trace("Removing Private Key!");
                    RemovePrivateKeyFile(addConfig, ci, keyFileName);
                    Logger.Trace("Private Key Removed!");
                }

                var certKeyRequest =
                    AddPrivateKey(ci, alias, keyFileName, apiClient, privateKeyString);
                Logger.Trace($"certKeyRequest {JsonConvert.SerializeObject(certKeyRequest)}");
                Logger.Trace($"Adding Private File {keyFileName}");
                apiClient.AddCertificateFile(certKeyRequest);
                Logger.Trace("Certificate File Added!");
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception e)
            {
                Logger.Error($"Error in CertManager.ReplacePrivateKey {e.Message}");
                throw;
            }
        }

        private void ReplaceCertificateFile(AnyJobConfigInfo addConfig, CertStoreInfo ci, ApiClient apiClient,
            string certFileName, ViewPublicCertificatesResponse viewCertificateCollection, string alias, string certPem)
        {
            try
            {
                Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
                Logger.Trace($"Cert Store Info {JsonConvert.SerializeObject(ci)}");
                Logger.Trace($"Cert Pem {certPem}");
                Logger.Trace($"certFileName {certFileName}");
                Logger.Trace($"alias {alias}");

                //See if Certificate File Exists, if so remove it and add a new one, if not just add it
                var certificateFileExists =
                    DoesCertificateFileExist(ci, apiClient, certFileName, viewCertificateCollection);
                if (certificateFileExists)
                    RemoveCertificate(addConfig, ci, certFileName);

                Logger.Trace($"Adding Certificate File {certFileName}");
                var certRequest = CertificateAddRequest(ci, alias, certFileName, apiClient, certPem);
                apiClient.AddCertificateFile(certRequest);
            }
            catch (Exception e)
            {
                Logger.Error($"Error in CertManager.ReplaceCertificateFile {e.Message}");
                throw;
            }
        }

        private void ReplaceCryptoKeyObject(CertStoreInfo ci, string cryptoKeyObjectName, ApiClient apiClient, string keyFileName,
            string alias)
        {
            try
            {
                Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);

                Logger.Trace($"Cert Store Info {JsonConvert.SerializeObject(ci)}");
                Logger.Trace($"Crypto Key Object Name {cryptoKeyObjectName}");
                Logger.Trace($"keyFileName {keyFileName}");
                Logger.Trace($"alias {alias}");

                //Search to See if the Crypto *Key* Object Already Exists (If so, it needs disabled and updated, If not add a new one)
                //Crypto Objects can not be removed since they may be already referenced by sites and such so they need disabled instead
                var cryptoKeyExists =
                    DoesCryptoKeyObjectExist(ci, cryptoKeyObjectName, apiClient);
                Logger.Trace($"Crypto Object Exists equals {cryptoKeyExists}");

                if (cryptoKeyExists)
                {
                    Logger.Trace("Disabling Crypto Key Object...");
                    DisableCryptoKeyObject(cryptoKeyObjectName, apiClient);
                    Logger.Trace("Updating Crypto Key Object...");
                    UpdatePrivateKey(ci, cryptoKeyObjectName, apiClient, keyFileName, alias);
                    Logger.Trace("Crypto Key Object Updated...");
                }
                else
                {
                    AddCryptoKey(ci, cryptoKeyObjectName, apiClient, keyFileName, alias);
                }

                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception e)
            {
                Logger.Error($"Error in CertManager.ReplaceCryptoKeyObject {e.Message}");
                throw;
            }
        }

        private void ReplaceCryptoObject(CertStoreInfo ci, string cryptoCertObjectName, ApiClient apiClient,
            string certFileName, string alias)
        {
            try
            {
                Logger.MethodEntry(ILogExtensions.MethodLogLevel.Debug);
                //Search to See if the Crypto *Certificate* Object Already Exists (If so, it needs disabled and updated, If not add a new one)
                //Crypto Objects can not be removed since they may be already referenced by sites and such so they need disabled instead

                Logger.Trace($"Cert Store Info {JsonConvert.SerializeObject(ci)}");
                Logger.Trace($"Crypto Object Name {cryptoCertObjectName}");
                Logger.Trace($"certFileName {certFileName}");
                Logger.Trace($"alias {alias}");

                var cryptoObjectExists =
                    DoesCryptoCertificateObjectExist(ci, cryptoCertObjectName, apiClient);

                Logger.Trace($"Crypto Object Exists equals {cryptoObjectExists}");

                if (cryptoObjectExists)
                {
                    Logger.Trace("Disabling Crypto Certificate Object...");
                    DisableCryptoCertificateObject(cryptoCertObjectName, apiClient);
                    Logger.Trace("Updating Crypto Certificate Object...");
                    UpdateCryptoCert(ci, cryptoCertObjectName, apiClient,
                        certFileName, alias);
                    Logger.Trace("Disable and Update Complete..");
                }
                else
                {
                    Logger.Trace("Adding Crypto Certificate Object...");
                    AddCryptoCert(ci, cryptoCertObjectName, apiClient, certFileName, alias);
                }
                Logger.MethodExit(ILogExtensions.MethodLogLevel.Debug);
            }
            catch (Exception e)
            {
                Logger.Error($"Error in CertManager.ReplaceCryptoObject {e.Message}");
                throw;
            }
        }

        public InventoryResult GetPublicCerts(ApiClient apiClient)
        {
            try
            {
                var result = new InventoryResult();
                var error = new AnyErrors { HasError = false };

                Logger.Trace("GetPublicCerts");
                var viewCert = new ViewPublicCertificatesRequest();
                Logger.Trace($"Public Cert List Request {JsonConvert.SerializeObject(viewCert)}");
                var viewCertificateCollection = apiClient.ViewPublicCertificates(viewCert);
                Logger.Trace($"Public Cert List Response {JsonConvert.SerializeObject(viewCertificateCollection)}");

                var intCount = 0;
                char[] s = { ',' };


                var intMax = Convert.ToInt32(_appConfig.AppSettings.Settings["MaxInventoryCapacity"].Value);
                var blackList = _appConfig.AppSettings.Settings["InventoryBlackList"].Value.Split(s);

                Logger.Trace($"Max Inventory: {intMax} Inventory Black List: {blackList}");

                Logger.Trace("Got App Config Settings from File");

                var inventoryItems = new List<AgentCertStoreInventoryItem>();
                if (viewCertificateCollection.PubFileStoreLocation.PubFileStore?.PubFiles != null)
                    foreach (var pc in viewCertificateCollection.PubFileStoreLocation.PubFileStore.PubFiles)
                    {
                        Logger.Trace($"Looping through public files: {pc.Name}");
                        var viewCertDetail = new ViewPubCertificateDetailRequest(pc.Name);
                        Logger.Trace($"Cert Detail Request: {JsonConvert.SerializeObject(viewCertDetail)}");
                        try
                        {
                            var viewCertResponse = apiClient.ViewPublicCertificate(viewCertDetail);
                            Logger.Trace($"Cert Detail Response: {JsonConvert.SerializeObject(viewCertResponse)}");

                            Logger.Trace($"Add to List: {pc.Name}");

                            var cert = new X509Certificate2(Encoding.UTF8.GetBytes(viewCertResponse.File));

                            Logger.Trace($"Created X509Certificate2: {cert.SerialNumber} : {cert.Subject}");

                            if (intCount < intMax)
                            {
                                if (!blackList.Contains(pc.Name) && cert.Thumbprint != null)
                                    inventoryItems.Add(
                                        new AgentCertStoreInventoryItem
                                        {
                                            Certificates = new[] { viewCertResponse.File },
                                            Alias = pc.Name,
                                            PrivateKeyEntry = false,
                                            ItemStatus = AgentInventoryItemStatus.Unknown,
                                            UseChainLevel = true
                                        });

                                intCount++;

                                Logger.Trace($"Inv-Certs: {pc.Name}");
                                Logger.Trace($"Certificates: {viewCertResponse.File}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Error on {pc.Name}: {ex.Message}");
                            error.ErrorMessage = ex.Message;
                            error.HasError = true;
                        }
                    }

                result.Errors = error;
                result.InventoryList = inventoryItems;
                Logger.Trace($"Serialized Result: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"Error in CertManager.GetPublicCerts {e.Message}");
                throw;
            }
        }

        public InventoryResult GetCerts(ApiClient apiClient)
        {
            try
            {
                var result = new InventoryResult();
                var error = new AnyErrors { HasError = false };

                Logger.Trace("GetCerts");
                var viewCert = new ViewCryptoCertificatesRequest(apiClient.Domain);
                Logger.Trace($"Get Certs Request: {JsonConvert.SerializeObject(viewCert)}");
                var viewCertificateCollection = apiClient.ViewCertificates(viewCert);
                Logger.Trace($"Get Certs Response: {JsonConvert.SerializeObject(viewCertificateCollection)}");
                var inventoryItems = new List<AgentCertStoreInventoryItem>();

                Logger.Trace("Start loop");

                foreach (var cc in viewCertificateCollection.CryptoCertificates)
                    if (!string.IsNullOrEmpty(cc.Name))
                    {
                        Logger.Trace($"Looping through Certificate Store files: {cc.Name}");

                        try
                        {
                            var viewCertDetail = new ViewCertificateDetailRequest(apiClient.Domain)
                            {
                                CertObjectRequest = new CertificateObjectRequest
                                {
                                    ObjectName = cc.Name
                                }
                            };
                            Logger.Trace($"Get Cert Request: {JsonConvert.SerializeObject(viewCertDetail)}");
                            var viewCertResponse = apiClient.ViewCryptoCertificate(viewCertDetail);
                            Logger.Trace($"Get Cert Response: {JsonConvert.SerializeObject(viewCertResponse)}");

                            //check this is a valid cert, if not fall to the errors
                            var cert = new X509Certificate2(Encoding.UTF8.GetBytes(viewCertResponse.CryptoCertObject.CertDetailsObject.EncodedCert.Value));

                            Logger.Trace($"Created X509Certificate2: {cert.SerialNumber} : {cert.Subject}");

                            Logger.Trace($"Add to list: {cc.Name}");
                            if (cert.Thumbprint != null)
                            {
                                inventoryItems.Add(
                                    new AgentCertStoreInventoryItem
                                    {
                                        Certificates = new[]
                                            {viewCertResponse.CryptoCertObject.CertDetailsObject.EncodedCert.Value},
                                        Alias = cc.Name,
                                        PrivateKeyEntry = true,
                                        ItemStatus = AgentInventoryItemStatus.Unknown,
                                        UseChainLevel = true
                                    });
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Certificate not retrievable: Error on {cc.Name}: {ex.Message}");
                            error.ErrorMessage = ex.Message;
                            error.HasError = true;
                        }
                    }


                result.Errors = error;
                result.InventoryList = inventoryItems;
                Logger.Trace($"Serialized Result: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"Error in CertManager.GetCerts {e.Message}");
                throw;
            }
        }
    }
}
