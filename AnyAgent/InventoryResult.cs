using System.Collections.Generic;
using Keyfactor.Platform.Extensions.Agents;

namespace DataPower
{
    internal class InventoryResult
    {
        public AnyErrors Errors { get; set; }

        public List<AgentCertStoreInventoryItem> InventoryList { get; set; }
    }
}
