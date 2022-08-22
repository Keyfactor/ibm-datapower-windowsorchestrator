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

// Release 5.1.0.0 - Runtime v4.0.30319
using System.Configuration;
using System.Reflection;
using CSS.Common.Logging;
using DataPower.API.client;
using Keyfactor.Platform.Extensions.Agents;
using Keyfactor.Platform.Extensions.Agents.Delegates;
using Keyfactor.Platform.Extensions.Agents.Interfaces;
using Newtonsoft.Json;

namespace DataPower
{
    public class Inventory : LoggingClientBase, IAgentJobExtension
    {
        private readonly Configuration _appConfig;
        private readonly CertManager _certManager;
        private readonly string _protocol;

        public Inventory()
        {
            _appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            _certManager = new CertManager();
            _protocol = _appConfig.AppSettings.Settings["Protocol"].Value;
        }

        public string GetJobClass()
        {
            return "Inventory";
        }

        public string GetStoreType()
        {
            return _appConfig.AppSettings.Settings["StoreType"].Value;
        }


        public AnyJobCompleteInfo processJob(AnyJobConfigInfo config, SubmitInventoryUpdate submitInventory,
            SubmitEnrollmentRequest submitEnrollmentRequest, SubmitDiscoveryResults sdr)
        {
            Logger.Trace("Parse: Certificate Inventory: " + config.Store.StorePath);
            var ci = Utility.ParseCertificateConfig(config);
            Logger.Trace($"Certificate Config Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            Logger.Trace($"Any Job Config {JsonConvert.SerializeObject(config)}");
            Logger.Trace($"submitEnrollmentRequest {JsonConvert.SerializeObject(submitEnrollmentRequest)}");
            Logger.Trace("Entering IBM DataPower: Certificate Inventory");
            Logger.Trace($"Entering processJob for Domain: {ci.Domain} and Certificate Store: {ci.CertificateStore}");
            var apiClient = new ApiClient(config.Server.Username, config.Server.Password,
                $"{_protocol}://" + config.Store.ClientMachine.Trim(), ci.Domain);

            var publicCertStoreName = _appConfig.AppSettings.Settings["PublicCertStoreName"].Value;
            Logger.Trace($"$Public Store name is {publicCertStoreName}");

            var storePath = config.Store.StorePath; var inventoryResult = storePath.Contains(_appConfig.AppSettings.Settings["PublicCertStoreName"].Value)
                ? _certManager.GetPublicCerts(apiClient)
                : _certManager.GetCerts(apiClient);

            var returnVal = submitInventory.Invoke(inventoryResult.InventoryList);

            if (returnVal == false)
            {
                Logger.Error("There were issues submitting the inventory.");
                return new AnyJobCompleteInfo { Status = (int)JobStatuses.JobError, Message = "Error submitting the inventory to Keyfactor" };
            }

            if (inventoryResult.Errors.HasError)
            {
                Logger.Error("Inventory had issues retrieving some certificates");
                return new AnyJobCompleteInfo { Status = (int)JobStatuses.JobWarning, Message = inventoryResult.Errors.ErrorMessage };
            }

            //we want to inventory what we can an log the rest as errors
            return new AnyJobCompleteInfo { Status = (int)JobStatuses.JobSuccess, Message = "Inventory Complete" };
        }
    }
}