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
                //Get a count of the crypto certificates that have the name we are looking for should be equal to one if it exists
                var viewAllCryptoCertRequest = new ViewCryptoCertificatesRequest(ci.Domain);
                var viewAllCryptoCertResponse = apiClient.ViewCertificates(viewAllCryptoCertRequest);

                if (viewAllCryptoCertResponse.CryptoCertificates.Count(x => x.Name == cryptoCertObjectName) == 1)
                    bUpdateCryptoCertificateObject = true;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"There was an issue receiving the certificates: {cryptoCertObjectName} Error {ex.Message}");
            }

            return bUpdateCryptoCertificateObject;
        }

        public void DisableCryptoCertificateObject(string cryptoCertObjectName, ApiClient apiClient)
        {
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
                apiClient.UpdateCryptoCertificate(cryptoCertUpdateRequest);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"There was an issue disabling the certificate object: {cryptoCertObjectName} Error {ex.Message}");
            }
        }

        public bool DoesCryptoKeyObjectExist(CertStoreInfo ci, string cryptoKeyObjectName, ApiClient apiClient)
        {
            var bUpdateCryptoKeyObject = false;
            try
            {
                //Look for CryptoKey
                var viewCryptoKeyRequest = new ViewCryptoKeysRequest(ci.Domain);
                var viewCryptoKeyResponse = apiClient.ViewCryptoKeys(viewCryptoKeyRequest);
                if (viewCryptoKeyResponse.CryptoKeys.Count(x => x.Name == cryptoKeyObjectName) == 1)
                    bUpdateCryptoKeyObject = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Crypto Key Object does not exist: {cryptoKeyObjectName} : {ex.Message}");
            }

            return bUpdateCryptoKeyObject;
        }

        public void DisableCryptoKeyObject(string cryptoKeyObjectName, ApiClient apiClient)
        {
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
                apiClient.UpdateCryptoKey(cryptoKeyUpdateRequest);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"There was an issue disabling the certificate *key*: {cryptoKeyObjectName} Error {ex.Message}");
            }
        }

        public void UpdatePrivateKey(CertStoreInfo ci, string cryptoKeyObjectName,
            ApiClient apiClient, string keyFileName, string alias)
        {
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

                apiClient.UpdateCryptoKey(cryptoKeyRequest);
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an issue updating the private key: {cryptoKeyObjectName} Error {ex.Message}");
            }
        }

        public void AddCryptoKey(CertStoreInfo ci, string cryptoKeyObjectName, ApiClient apiClient, string keyFileName,
            string alias)
        {
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

                apiClient.AddCryptoKey(cryptoKeyRequest);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Error Adding CryptoKey Object for Private Key {alias}: {cryptoKeyObjectName} Error {ex.Message}");
            }
        }

        public AnyErrors RemovePrivateKeyFile(AnyJobConfigInfo addConfig, CertStoreInfo ci,
            string keyFileName)
        {
            Logger.Trace($"Removing Old Private Key File {keyFileName}");
            var removeFileResult = RemoveFile(addConfig, ci, keyFileName);
            Logger.Trace($"Private Key {keyFileName} is removed");

            return removeFileResult;
        }

        public CertificateAddRequest AddPrivateKey(CertStoreInfo ci, string alias, string keyFileName,
            ApiClient apiClient,
            string privateKeyString)
        {
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
                return certKeyRequest;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Error Adding Private Key {alias} to CERT store with Filename {keyFileName} Error {ex.Message}");
            }

            return null;
        }

        public void UpdateCryptoCert(CertStoreInfo ci, string cryptoCertObjectName,
            ApiClient apiClient, string certFileName, string alias)
        {
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

                apiClient.UpdateCryptoCertificate(cryptoCertRequest);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Updating Crypto Certificate Object: {cryptoCertObjectName} Error {ex.Message}");
            }
        }

        public void AddCryptoCert(CertStoreInfo ci, string cryptoCertObjectName, ApiClient apiClient,
            string certFileName,
            string alias)
        {
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

                apiClient.AddCryptoCertificate(cryptoCertRequest);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Error Adding Crypto Object for Certificate {alias} to CERT store with Filename {certFileName} Error {ex.Message}");
            }
        }

        public AnyErrors RemoveCertificate(AnyJobConfigInfo addConfig, CertStoreInfo ci, string certFileName)
        {
            Logger.Trace($"Removing Old Certificate File {certFileName}");
            var result = RemoveFile(addConfig, ci, certFileName);
            Logger.Trace($"Old Certificate File {certFileName} is removed");

            return result;
        }

        public CertificateAddRequest CertificateAddRequest(CertStoreInfo ci, string alias, string certFileName,
            ApiClient apiClient, string certPem)
        {
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
                return certRequest;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error Adding Certificate {alias} with Filename {certFileName} Error {ex.Message}");
            }

            return null;
        }

        public bool DoesKeyFileExist(CertStoreInfo ci, string keyFileName,
            ViewPublicCertificatesResponse viewCertificateCollection)
        {
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
                Logger.Error(
                    $"Error Matching Key File {keyFileName} was found in domain {ci.Domain} Error {ex.Message}");
            }

            return bRemoveKeyFile;
        }

        public bool DoesCertificateFileExist(CertStoreInfo ci, ApiClient apiClient,
            string certFileName, ViewPublicCertificatesResponse viewCertificateCollection)
        {
            var bRemoveCertificateFile = false;
            try
            {
                var publicCertificate =
                    viewCertificateCollection.PubFileStoreLocation?.PubFileStore?.PubFiles?.FirstOrDefault(x =>
                        x?.Name == certFileName);

                if (!(publicCertificate is null))
                {
                    Logger.Trace($"Matching Certificate File {certFileName} was found in domain {ci.Domain}");
                    bRemoveCertificateFile = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Error Matching Certificate File {certFileName} was found in domain {ci.Domain} Error {ex.Message}");
            }

            return bRemoveCertificateFile;
        }


        public string GetCertPem(AnyJobConfigInfo addConfig, string alias, ref string privateKeyString)
        {
            string certPem = null;
            try
            {
                if (!string.IsNullOrEmpty(addConfig.Job.PfxPassword))
                {
                    Logger.Trace($"Certificate and Key exist for {alias}");
                    var certData = Convert.FromBase64String(addConfig.Job.EntryContents);

                    Pkcs12Store store;

                    using (var ms = new MemoryStream(certData))
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

            return certPem;
        }

        public AnyErrors AddPubCert(AnyJobConfigInfo addPubConfig, CertStoreInfo ci, NamePrefix np)
        {
            var error = new AnyErrors();
            Logger.Trace("Entering AddPubCert");
            error.HasError = false;
            Logger.Trace($"Entering AddPubCert for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");

            var apiClient = new ApiClient(addPubConfig.Server.Username, addPubConfig.Server.Password,
                $"{_protocol}://" + addPubConfig.Store.ClientMachine.Trim(), ci.Domain);

            var certAlias = addPubConfig.Job.Alias;
            if (string.IsNullOrEmpty(certAlias))
                certAlias = Guid.NewGuid().ToString();

            try
            {
                Pkcs12Store store;
                string certPem;
                var certData = Convert.FromBase64String(addPubConfig.Job.EntryContents);

                //If you have a password then you will get a PFX in return instead of the base64 encoded string
                if (!string.IsNullOrEmpty(addPubConfig.Job?.PfxPassword))
                    using (var ms = new MemoryStream(certData))
                    {
                        store = new Pkcs12Store(ms, addPubConfig.Job.PfxPassword.ToCharArray());
                        var storeAlias = store.Aliases.Cast<string>().SingleOrDefault(a => store.IsKeyEntry(a));
                        certPem = Utility.Pemify(
                            Convert.ToBase64String(store.GetCertificate(storeAlias).Certificate.GetEncoded()));
                    }
                else
                    certPem = Utility.Pemify(addPubConfig.Job.EntryContents);

                var certFileName = certAlias.Replace(".", "_") + ".pem";

                Logger.Trace(
                    $"Adding Public Cert Certificate {certAlias} to PUBCERT store with Filename {certFileName} ");
                var certRequest =
                    new CertificateAddRequest(apiClient.Domain, certFileName, ci.CertificateStore.Trim())
                    {
                        Certificate = new CertificateRequest
                        {
                            Name = certFileName,
                            Content = certPem
                        }
                    };

                apiClient.AddCertificateFile(certRequest);
                apiClient.SaveConfig();
            }
            catch (Exception ex)
            {
                error.HasError = true;
                Logger.Trace($"Error on {certAlias}: {ex.Message}");
                apiClient.SaveConfig();
            }

            return error;
        }


        private AnyErrors RemoveCertFromDomain(AnyJobConfigInfo removeConfig, CertStoreInfo ci, NamePrefix np)
        {
            var error = new AnyErrors {HasError = false};
            Logger.Trace($"Entering RemoveCertStore for {removeConfig.Job.Alias} ");
            Logger.Trace(
                $"Entering RemoveCertStore for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            var apiClient = new ApiClient(removeConfig.Server.Username, removeConfig.Server.Password,
                $"{_protocol}://" + removeConfig.Store.ClientMachine.Trim(), ci.Domain);

            try
            {
                Logger.Trace($"Checking to find CryptoCertObject {removeConfig.Job.Alias} ");
                var viewCert = new ViewCryptoCertificatesRequest(apiClient.Domain, removeConfig.Job.Alias);
                var viewCertificateSingle = apiClient.ViewCryptoCertificate(viewCert);
                if (viewCertificateSingle != null &&
                    !string.IsNullOrEmpty(viewCertificateSingle.CryptoCertificate.Name))
                {
                    Logger.Trace($"Remove CryptoObject {viewCertificateSingle.CryptoCertificate.Name} ");
                    var request =
                        new DeleteCryptoCertificateRequest(apiClient.Domain, removeConfig.Job.Alias);
                    apiClient.DeleteCryptoCertificate(request);
                    Logger.Trace($"Remove Certificate File {viewCertificateSingle.CryptoCertificate.CertFile} ");
                    var request2 = new DeleteCertificateRequest(apiClient.Domain,
                        viewCertificateSingle.CryptoCertificate.CertFile.Replace(ci.CertificateStore + ":///", ""));
                    apiClient.DeleteCertificate(request2);
                }

                var cryptoKeyObjectName = Utility.ReplaceFirstOccurrence(removeConfig.Job.Alias,
                    np.CryptoCertObjectPrefix?.Trim() ?? string.Empty,
                    np.CryptoKeyObjectPrefix?.Trim() ?? string.Empty);
                Logger.Trace($"Checking to find CryptoKeyObject {cryptoKeyObjectName} ");
                var viewKey = new ViewCryptoKeysRequest(apiClient.Domain);
                var viewKeyResponse = apiClient.ViewCryptoKeys(viewKey);
                var cryptoKey = viewKeyResponse.CryptoKeys.FirstOrDefault(x => x.Name == cryptoKeyObjectName);
                if (viewKeyResponse.CryptoKeys != null && !string.IsNullOrEmpty(cryptoKey?.Name))
                {
                    Logger.Trace($"Remove CryptoKeyObject {cryptoKey.Name} ");
                    var request = new DeleteCryptoKeyRequest(apiClient.Domain, cryptoKeyObjectName);
                    apiClient.DeleteCryptoKey(request);
                    Logger.Trace($"Remove Key File {cryptoKey.CertFile} ");
                    var request2 = new DeleteCertificateRequest(apiClient.Domain,
                        cryptoKey.CertFile.Replace(ci.CertificateStore + ":///", ""));
                    apiClient.DeleteCertificate(request2);
                }
            }
            catch (Exception ex)
            {
                error.HasError = true;
                error.ErrorMessage = ex.Message;
                Logger.Trace($"Error on {removeConfig.Job.Alias}: {ex.Message}");
            }

            apiClient.SaveConfig();

            return error;
        }

        private AnyErrors RemoveFile(AnyJobConfigInfo removeConfig, CertStoreInfo ci, string filename)
        {
            var error = new AnyErrors {HasError = false};
            Logger.Trace($"Entering RemoveFile for {removeConfig.Job.Alias} ");
            Logger.Trace($"Entering RemoveFile for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            var apiClient = new ApiClient(removeConfig.Server.Username, removeConfig.Server.Password,
                $"{_protocol}://" + removeConfig.Store.ClientMachine.Trim(), ci.Domain);

            try
            {
                Logger.Trace($"Deleting Actual File {filename} ");
                var request2 = new DeleteCertificateRequest(apiClient.Domain,
                    filename.Replace(ci.CertificateStore + ":///", ""));
                apiClient.DeleteCertificate(request2);
            }
            catch (Exception ex)
            {
                error.HasError = true;
                error.ErrorMessage = ex.Message;
                Logger.Trace($"Error on {removeConfig.Job.Alias}: {ex.Message}");
            }

            apiClient.SaveConfig();

            return error;
        }

        public AnyErrors Remove(AnyJobConfigInfo removeConfig, CertStoreInfo ci, NamePrefix np)
        {
            var error = new AnyErrors();
            Logger.Trace("Entering Remove");
            error.HasError = false;

            var publicCertStoreName = _appConfig.AppSettings.Settings["PublicCertStoreName"].Value;
            var storePath = removeConfig.Store.StorePath;

            if (storePath.Contains(publicCertStoreName))
            {
                Logger.Trace("Cannot Remove Public Certificates");
                error.HasError = true;
            }
            else
            {
                error = RemoveCertFromDomain(removeConfig, ci, np);
            }

            return error;
        }

        public AnyErrors Add(AnyJobConfigInfo addConfig, CertStoreInfo ci, NamePrefix np)
        {
            var result = new AnyErrors();
            Logger.Trace("Entering Add");
            result.HasError = false;

            var publicCertStoreName = _appConfig.AppSettings.Settings["PublicCertStoreName"].Value;
            var storePath = addConfig.Store.StorePath;

            result = storePath.Contains(publicCertStoreName)
                ? AddPubCert(addConfig, ci, np)
                : AddCertStore(addConfig, ci, np);

            return result;
        }

        private AnyErrors AddCertStore(AnyJobConfigInfo addConfig, CertStoreInfo ci, NamePrefix np)
        {
            var error = new AnyErrors();
            Logger.Trace("Entering AddCertStore");
            var privateKeyString = "";

            Logger.Trace($"Entering AddCertStore for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            var apiClient = new ApiClient(addConfig.Server.Username, addConfig.Server.Password,
                $"{_protocol}://" + addConfig.Store.ClientMachine.Trim(),
                ci.Domain);

            var alias = addConfig.Job.Alias.ToLower();
            if (string.IsNullOrEmpty(alias))
                alias = Guid.NewGuid().ToString().ToLower();
            try
            {
                if (!string.IsNullOrEmpty(addConfig.Job.PfxPassword))
                {
                    var certPem = GetCertPem(addConfig, alias, ref privateKeyString);

                    var baseAlias = alias.ToLower();

                    var cryptoObjectPrefix = np.CryptoCertObjectPrefix?.Trim().ToLower() ?? string.Empty;
                    var keyFileNamePrefix = np.KeyFilePrefix?.Trim().ToLower() ?? string.Empty;
                    var certFileNamePrefix = np.CertFilePrefix?.Trim().ToLower() ?? string.Empty;
                    var cryptoKeyObjectPrefix = np.CryptoKeyObjectPrefix?.Trim().ToLower() ?? string.Empty;

                    if (alias.ToLower().StartsWith(cryptoObjectPrefix))
                        baseAlias = Utility.ReplaceAlias(alias.ToLower(), cryptoObjectPrefix,
                            "");

                    var certFileName = certFileNamePrefix + baseAlias + ".cer";
                    var keyFileName = keyFileNamePrefix + baseAlias + ".pem";
                    var cryptoCertObjectName = cryptoObjectPrefix + baseAlias;
                    var cryptoKeyObjectName = cryptoKeyObjectPrefix + baseAlias;

                    //Get the certificate collection to be used to check for cert files and private keys
                    var viewCert = new ViewPublicCertificatesRequest(ci.Domain, ci.CertificateStore);
                    var viewCertificateCollection = apiClient.ViewPublicCertificates(viewCert);

                    ReplaceCertificateFile(addConfig, ci, apiClient, certFileName, viewCertificateCollection, alias,
                        certPem);
                    ReplaceCryptoObject(ci, cryptoCertObjectName, apiClient, certFileName, alias);
                    ReplacePrivateKey(addConfig, ci, keyFileName, viewCertificateCollection, alias, apiClient,
                        privateKeyString);
                    ReplaceCryptoKeyObject(ci, cryptoKeyObjectName, apiClient, keyFileName, alias);
                }

                apiClient.SaveConfig();
            }
            catch (Exception ex)
            {
                error.HasError = true;
                Logger.Trace($"Error on {alias}: {ex.Message}");
                apiClient.SaveConfig();
            }

            return error;
        }

        private void ReplacePrivateKey(AnyJobConfigInfo addConfig, CertStoreInfo ci, string keyFileName,
            ViewPublicCertificatesResponse viewCertificateCollection, string alias, ApiClient apiClient,
            string privateKeyString)
        {
            //See if KeyFile Exists if so remove and add a new one, if not just add a new one
            var bRemoveKeyFile = DoesKeyFileExist(ci, keyFileName, viewCertificateCollection);
            if (bRemoveKeyFile)
                RemovePrivateKeyFile(addConfig, ci, keyFileName);

            var certKeyRequest =
                AddPrivateKey(ci, alias, keyFileName, apiClient, privateKeyString);
            Logger.Trace($"Adding Private File {keyFileName}");
            apiClient.AddCertificateFile(certKeyRequest);
        }

        private void ReplaceCertificateFile(AnyJobConfigInfo addConfig, CertStoreInfo ci, ApiClient apiClient,
            string certFileName, ViewPublicCertificatesResponse viewCertificateCollection, string alias, string certPem)
        {
            //See if Certificate File Exists, if so remove it and add a new one, if not just add it
            var certificateFileExists =
                DoesCertificateFileExist(ci, apiClient, certFileName, viewCertificateCollection);
            if (certificateFileExists)
                RemoveCertificate(addConfig, ci, certFileName);

            Logger.Trace($"Adding Certificate File {certFileName}");
            var certRequest = CertificateAddRequest(ci, alias, certFileName, apiClient, certPem);
            apiClient.AddCertificateFile(certRequest);
        }

        private void ReplaceCryptoKeyObject(CertStoreInfo ci, string cryptoKeyObjectName, ApiClient apiClient,
            string keyFileName,
            string alias)
        {
            //Search to See if the Crypto *Key* Object Already Exists (If so, it needs disabled and updated, If not add a new one)
            //Crypto Objects can not be removed since they may be already referenced by sites and such so they need disabled instead
            var cryptoKeyExists =
                DoesCryptoKeyObjectExist(ci, cryptoKeyObjectName, apiClient);
            if (cryptoKeyExists)
            {
                DisableCryptoKeyObject(cryptoKeyObjectName, apiClient);
                UpdatePrivateKey(ci, cryptoKeyObjectName, apiClient, keyFileName, alias);
            }
            else
            {
                AddCryptoKey(ci, cryptoKeyObjectName, apiClient, keyFileName, alias);
            }
        }

        private void ReplaceCryptoObject(CertStoreInfo ci, string cryptoCertObjectName, ApiClient apiClient,
            string certFileName, string alias)
        {
            //Search to See if the Crypto *Certificate* Object Already Exists (If so, it needs disabled and updated, If not add a new one)
            //Crypto Objects can not be removed since they may be already referenced by sites and such so they need disabled instead
            var cryptoObjectExists =
                DoesCryptoCertificateObjectExist(ci, cryptoCertObjectName, apiClient);
            if (cryptoObjectExists)
            {
                DisableCryptoCertificateObject(cryptoCertObjectName, apiClient);
                UpdateCryptoCert(ci, cryptoCertObjectName, apiClient,
                    certFileName, alias);
            }
            else
            {
                AddCryptoCert(ci, cryptoCertObjectName, apiClient, certFileName, alias);
            }
        }

        public InventoryResult GetPublicCerts(ApiClient apiClient)
        {
            var result = new InventoryResult();
            var error = new AnyErrors {HasError = false};

            Logger.Trace("GetPublicCerts");
            var viewCert = new ViewPublicCertificatesRequest();
            var viewCertificateCollection = apiClient.ViewPublicCertificates(viewCert);

            var intCount = 0;
            char[] s = {','};


            var intMax = Convert.ToInt32(_appConfig.AppSettings.Settings["MaxInventoryCapacity"].Value);
            var blackList = _appConfig.AppSettings.Settings["InventoryBlackList"].Value.Split(s);
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
                        var pem = Convert.FromBase64String(viewCertResponse.File);
                        var pemString = Encoding.UTF8.GetString(pem);
                        Logger.Trace($"Pem File: {pemString}");

                        if (pemString.Contains("BEGIN CERTIFICATE"))
                        {
                            Logger.Trace("Valid Pem File Adding to KF");
                            var cert = new X509Certificate2(pem);
                            var b64 = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
                            Logger.Trace($"Created X509Certificate2: {cert.SerialNumber} : {cert.Subject}");

                            if (intCount < intMax)
                            {
                                if (!blackList.Contains(pc.Name) && cert.Thumbprint != null)
                                    inventoryItems.Add(
                                        new AgentCertStoreInventoryItem
                                        {
                                            Certificates = new[] {b64},
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
                        else
                        {
                            Logger.Trace("Not a valid Pem File, Skipping the Add to Keyfactor...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error on {pc.Name}: {LogHandler.FlattenException(ex)}");
                        error.ErrorMessage = ex.Message;
                        error.HasError = true;
                    }
                }

            result.Errors = error;
            result.InventoryList = inventoryItems;

            return result;
        }

        public InventoryResult GetCerts(ApiClient apiClient)
        {
            var result = new InventoryResult();
            var error = new AnyErrors {HasError = false};

            Logger.Trace("GetCerts");
            var viewCert = new ViewCryptoCertificatesRequest(apiClient.Domain);
            var viewCertificateCollection = apiClient.ViewCertificates(viewCert);

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

                        var viewCertResponse = apiClient.ViewCryptoCertificate(viewCertDetail);

                        //check this is a valid cert, if not fall to the errors
                        var cert = new X509Certificate2(Encoding.UTF8.GetBytes(viewCertResponse.CryptoCertObject
                            .CertDetailsObject.EncodedCert.Value));

                        Logger.Trace($"Add to list: {cc.Name}");
                        if (cert.Thumbprint != null)
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
                    catch (Exception ex)
                    {
                        Logger.Error($"Certificate not retrievable: Error on {cc.Name}: {ex.Message}");
                        error.ErrorMessage = ex.Message;
                        error.HasError = true;
                    }
                }


            result.Errors = error;
            result.InventoryList = inventoryItems;

            return result;
        }
    }
}