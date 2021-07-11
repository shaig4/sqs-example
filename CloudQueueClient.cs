using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nayax.Infra.Clients.Cloud
{
    public class CloudQueueClient
    {
        private AmazonSQSClient _awsSQSClient;

        public CloudQueueClient(string accessKey, string secretKey, string url)
        {
            if (string.IsNullOrEmpty(accessKey))
            {
                throw new ArgumentNullException("accessKey");
            }

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException("secretKey");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            AccessKey = accessKey;
            SecretKey = secretKey;
            Url = url;

                RestartClient();
        }

        public string AccessKey { get; private set; }
        public string SecretKey { get; private set; }
        public string Url { get; private set; }

        public string QueueName
        {
            get
            {
                var arr = Url.Split('/');

                return arr[arr.Length - 1];
            }
        }
        private void RestartClient()
        {
            if (_awsSQSClient != null)
            {
                try
                {
                    _awsSQSClient.Dispose();
                }
                catch { }

                _awsSQSClient = null;
            }

            var serviceUrl = new Uri(Url).GetLeftPart(UriPartial.Authority);
            var sqsConfig = new AmazonSQSConfig { ServiceURL = serviceUrl };
            _awsSQSClient = new AmazonSQSClient(AccessKey, SecretKey, sqsConfig);
        }

        private ReceiveMessageRequest BuildReceiveMessageRequest(int maxNumberOfMessages)
        {
            return new ReceiveMessageRequest()
            {
                QueueUrl = this.Url,
                MaxNumberOfMessages = maxNumberOfMessages
            };
        }

        public async Task<List<CloudQueueMessage>> ReceiveMessageAsync(int maxNumberOfMessages = 1)
        {
                var receiveMessageRequest = BuildReceiveMessageRequest(maxNumberOfMessages);
                var response = await _awsSQSClient.ReceiveMessageAsync(receiveMessageRequest);
                return HandleReceiveResponse(response);
      

        }

        private List<CloudQueueMessage> HandleReceiveResponse(ReceiveMessageResponse response)
        {
            return response.Messages.Select(itemMessage =>
                   new CloudQueueMessage()
                   {
                       body = itemMessage.Body,
                       receiptHandle = itemMessage.ReceiptHandle,
                       MessageId = itemMessage.MessageId
                   }
                ).ToList();
        }

        public async Task DeleteMessageBatchAsync(List<CloudQueueMessage> messages, System.Threading.CancellationToken cancellationToken)
        {
            var list = messages.Select(a => new DeleteMessageBatchRequestEntry(a.MessageId, a.receiptHandle)).ToList();
            await _awsSQSClient.DeleteMessageBatchAsync(Url, list, cancellationToken);
        }

    }
}