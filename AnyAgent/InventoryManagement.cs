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

using System.Configuration;
using System.Reflection;
using CSS.Common.Logging;
using Keyfactor.Platform.Extensions.Agents;
using Keyfactor.Platform.Extensions.Agents.Delegates;
using Keyfactor.Platform.Extensions.Agents.Enums;
using Keyfactor.Platform.Extensions.Agents.Interfaces;

namespace DataPower
{
    public class InventoryManagement : LoggingClientBase, IAgentJobExtension
    {
        private readonly CertManager _certManager;
        private readonly Configuration _appConfig;

        public InventoryManagement()
        {
            _certManager = new CertManager();
            _appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
        }

        public string GetJobClass()
        {
            return "Management";
        }

        public string GetStoreType()
        {
            return _appConfig.AppSettings.Settings["StoreType"].Value;
        }

        public AnyJobCompleteInfo processJob(AnyJobConfigInfo initialConfig, SubmitInventoryUpdate submitInventory,
            SubmitEnrollmentRequest submitEnrollmentRequest, SubmitDiscoveryResults sdr)
        {
            var ci = Utility.ParseCertificateConfig(initialConfig);
            var np = Utility.ParseStoreProperties(initialConfig);

            AnyErrors result;

            Logger.Trace("Entering IBM DataPower: Inventory Management for DOMAIN: " + ci.Domain);
            switch (initialConfig.Job.OperationType)
            {
                case AnyJobOperationType.Add:
                    result = _certManager.Add(initialConfig, ci, np);
                    break;
                case AnyJobOperationType.Remove:
                    result = _certManager.Remove(initialConfig, ci, np);
                    break;
                default:
                    return new AnyJobCompleteInfo
                    {
                        Status = (int) JobStatuses.JobError,
                        Message = "Unsupported operation " + initialConfig.Job.OperationType
                    };
            }

            return result.HasError
                ? new AnyJobCompleteInfo
                {
                    Status = (int) JobStatuses.JobWarning,
                    Message = "Management has Issues creating certificate objects"
                }
                : new AnyJobCompleteInfo {Status = (int) JobStatuses.JobSuccess, Message = "Job complete"};
        }
    }
}