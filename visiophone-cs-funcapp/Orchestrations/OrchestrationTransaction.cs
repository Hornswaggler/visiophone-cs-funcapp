using System;

namespace vp.orchestrations
{
    public class OrchestrationTransaction<T>
    {
        public string transactionId { get; set; } = $"{Guid.NewGuid()}";
        public T payload { get; set; }
    }
}
