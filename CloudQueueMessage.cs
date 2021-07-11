using System.Collections.Generic;

namespace Nayax.Infra.Clients.Cloud
{
    public class CloudQueueMessage 
    {
        public string body { get; internal set; }
        public string receiptHandle { get; internal set; }
        public string MessageId { get; internal set; }
        public Dictionary<string, string> MessageAttributes { get; internal set; }

    }
}
