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