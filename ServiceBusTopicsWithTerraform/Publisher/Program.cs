using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace Publisher
{
    internal class Program
    {
        private static readonly Random random = new Random();
        static async Task Main(string[] args)
        {
            //Check the main.tf terraform file infrastructure provisioning

            IConfigurationBuilder builder = new ConfigurationBuilder();
            IConfigurationRoot configuration = JsonConfigurationExtensions.AddJsonFile(builder, System.Environment.CurrentDirectory + "//appsettings.json").Build();

            Console.WriteLine("Publisher");

            string SbConnectionString = configuration.GetConnectionString("ServiceBusConnectionString");
            string TopicName = configuration["TopicName"];

            ServiceBusClient client = new ServiceBusClient(SbConnectionString);
            ServiceBusSender sender = client.CreateSender(TopicName);


            while (true)
            {
                Console.WriteLine("How many messages would you like to puxblish. Enter 0 to exit");
                int count = int.Parse(Console.ReadLine());

                if (count == 0)
                {
                    Console.WriteLine("Exiting..");
                    break;
                }
                else if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        ServiceBusMessage message = new ServiceBusMessage($"Message {i}");

                        switch (random.Next(3,3))
                        {
                            case 0:
                                //message.ApplicationProperties.Add("Priority", "Low");
                                message.ApplicationProperties.Add("sub1", "sub1");
                                break;
                            case 1:
                                message.ApplicationProperties.Add("sub2", "sub2");
                                message.ApplicationProperties.Add("sub3", "sub3");
                                break;
                            case 2:
                                message.Subject = "Demo";
                                message.ApplicationProperties.Add("customprop1", "customvalue1");
                                break;
                            case 3:
                                message.ApplicationProperties.Add("sub4", "sub4");
                                break;
                            case 4:
                                message.ApplicationProperties.Add("Type", "CompositeFilterDemo");
                                message.ApplicationProperties.Add("CustomProp", "CompositeFilterDemo");
                                break;
                        }
                        await sender.SendMessageAsync(message);
                        Console.WriteLine($"Sent message {message.Body} with properties {string.Join(", ",message.ApplicationProperties)}");
                    }
                }
                else
                {
                    Console.WriteLine("Enter a valid number!");
                }
            }
            await sender.DisposeAsync();
        }
    }
}