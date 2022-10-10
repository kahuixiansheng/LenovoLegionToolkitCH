﻿using System;
using System.Threading.Tasks;
using LenovoLegionToolkit.Lib.System;
using Newtonsoft.Json;

namespace LenovoLegionToolkit.Lib.Automation.Pipeline.Triggers
{
    public class LowWattageACAdapterConnectedAutomationPipelineTrigger : IAutomationPipelineTrigger, IPowerStateAutomationPipelineTrigger, IDisallowDuplicatesAutomationPipelineTrigger
    {
        [JsonIgnore]
        public string DisplayName => "当接入低功率适配器时";

        public async Task<bool> IsSatisfiedAsync(IAutomationEvent automationEvent)
        {
            if (automationEvent is not (PowerStateAutomationEvent or StartupAutomationEvent))
                return false;

            return await Power.IsPowerAdapterConnectedAsync().ConfigureAwait(false) == PowerAdapterStatus.ConnectedLowWattage;
        }

        public IAutomationPipelineTrigger DeepCopy() => new LowWattageACAdapterConnectedAutomationPipelineTrigger();

        public override bool Equals(object? obj) => obj is LowWattageACAdapterConnectedAutomationPipelineTrigger;

        public override int GetHashCode() => HashCode.Combine(DisplayName);
    }
}
