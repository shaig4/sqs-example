using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Nayax.Infra.Clients.Cloud;

namespace SqsToFiles
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Starting!");
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var accessKey = config.GetValue<string>("accessKey");
            var secretKey = config.GetValue<string>("secretKey");
            var url = config.GetValue<string>("url");

            var client = new CloudQueueClient(accessKey, secretKey, url);
            var messages = await client.ReceiveMessageAsync(1);
            Console.WriteLine("messages count: " + messages.Count);

            foreach (var message in messages)
                File.WriteAllText("sqs_message_" + message.MessageId + ".txt", message.body);

            await client.DeleteMessageBatchAsync(messages, CancellationToken.None);
        }

    }
}
