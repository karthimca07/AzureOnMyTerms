using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Receiver
{
    internal class Program
    {
        private static readonly Random Random = new Random();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Receiver");

            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(System.Environment.CurrentDirectory + "//appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            string SBConnectionString = configuration.GetConnectionString("ServiceBusConnectionString");
            string topicName = configuration["TopicName"];

            Dictionary<int,string> subscription = new Dictionary<int,string>();
            subscription.Add(0, "sb-topic_subscription-demo-1");
            subscription.Add(1, "sb-topic_subscription-demo-2");
            subscription.Add(2, "sb-topic_subscription-demo-3");
            subscription.Add(3, "sb-topic_subscription-demo-4");
            subscription.Add(4, "sb-topic_subscription-demo-5");

            ServiceBusClient client = new ServiceBusClient(SBConnectionString);
            ServiceBusReceiver receiver=null;
            ServiceBusReceivedMessage receivedMessage = null;

            while (true)
            {
                Console.WriteLine("Enter the number of message you would like to process. Enter 0 to exit.");
                int count = int.Parse(Console.ReadLine());

                if (count == 0)
                {
                    Console.WriteLine("Exiting...");
                    break;
                }
                else if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        switch (Random.Next(3, 3))
                        {
                            case 0:
                                receiver = client.CreateReceiver(topicName, subscription[0]);
                                receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
                                break;
                            case 1:
                                receiver = client.CreateReceiver(topicName, subscription[1]);
                                receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
                                break;
                            case 2:
                                receiver = client.CreateReceiver(topicName, subscription[2]);
                                receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
                                break;
                            case 3:
                                receiver = client.CreateReceiver(topicName, subscription[3]);
                                receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
                                break;
                            case 4:
                                receiver = client.CreateReceiver(topicName, subscription[4]);
                                receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
                                break;
                        }
                        if (receivedMessage != null)
                        {
                            Console.WriteLine($"Received message: {receivedMessage.Body} with Application Properties {string.Join(",", receivedMessage.ApplicationProperties)}");

                            // Complete the message (removes from subscription)
                            await receiver.CompleteMessageAsync(receivedMessage);
                        }
                    }
                }
                else
                {
                    Console.Write("Invalid input");
                }
            }
        }
    }
}