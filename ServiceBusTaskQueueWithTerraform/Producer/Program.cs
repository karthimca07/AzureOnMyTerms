using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;

namespace ServiceBusTaskQueueWithTerraform
{
    internal class Program
    {
        private static string SbConnectionString;

        static async Task Main(string[] args)
        {
            // Manually configure appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load appsettings.json
                .Build(); // Build the configuration

            Console.WriteLine("Task producer");

            //reading connection string from config file
            SbConnectionString = configuration.GetConnectionString("ServiceBusConnectionString");

            string queueName = configuration["queueName"];

            ServiceBusAdministrationClient adminClient = new ServiceBusAdministrationClient(SbConnectionString);

            //create queue if does not exists
            if (!await adminClient.QueueExistsAsync(queueName))
            {
                Console.WriteLine($"{queueName} does not exists. Creating...");

                var createQueueOptions = new CreateQueueOptions(queueName);
                createQueueOptions.MaxSizeInMegabytes = 1024;
                createQueueOptions.RequiresSession = true;
                createQueueOptions.LockDuration = TimeSpan.FromSeconds(60);
                createQueueOptions.DefaultMessageTimeToLive = TimeSpan.FromMinutes(5);
                
                await adminClient.CreateQueueAsync(createQueueOptions);

                Console.WriteLine("Queue created successfully");
             }

            //send messages to queue and break when user interrupts with key X

            ServiceBusClient client = new ServiceBusClient(SbConnectionString);

            ServiceBusSender sender = client.CreateSender(queueName);
            while (true)
            {
                Console.WriteLine("Enter the number of messages to send. Enter X to quit");

                string input = Console.ReadLine();
                if (input == "X") { break; }

                if (int.TryParse(input, out int msgCount))
                {
                    for (int i = 0; i < msgCount; i++)
                    {
                        string sessionId = $"session-{i % 3}";

                        var msgBody = new
                        {
                            MessageId = Guid.NewGuid().ToString(),
                            content = $"Message {i}",
                            Timestamp = DateTime.UtcNow,
                            sessionId = sessionId
                        };

                        string jsonmsg = JsonSerializer.Serialize(msgBody);

                        ServiceBusMessage msg = new ServiceBusMessage(jsonmsg)
                        {
                            MessageId = msgBody.MessageId,
                            SessionId = sessionId,
                            ContentType = "application/json"
                        };

                        await sender.SendMessageAsync(msg);

                        Console.WriteLine($"sent message {i}/{msgCount} with sessionId: {sessionId}");
                    }
                }
                else 
                { 
                    Console.WriteLine("Invalid input. Please enter valid number or X");
                }
            }
            Console.WriteLine("end");
            //Creating a Queue
        }
    }
}