using Stripe;
using System;

namespace vp.orchestrations
{
    public class TransactionBase
    {
        public TransactionBase() { }

        public string transactionId { get; set; } = $"{Guid.NewGuid()}";
    }
}
