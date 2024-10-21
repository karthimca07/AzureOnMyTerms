using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;

namespace Consumer
{
    internal class Program
    {
         static async Task Main(string[] args)
        {
            Console.WriteLine("-------- Consumer --------");

            // Manually configure appsettings.json
            IConfigurationBuilder builder = new ConfigurationBuilder();
            JsonConfigurationExtensions.AddJsonFile(builder, Directory.GetCurrentDirectory() + "\\appsettings.json");
            IConfigurationRoot configuration  = builder.Build();

            //reading connection string from config file
            string SbConnectionString = configuration.GetConnectionString("ServiceBusConnectionString");

            string queueName = configuration["queueName"];

            ServiceBusClient client = new ServiceBusClient(SbConnectionString);

            ServiceBusReceiver receiver = client.CreateReceiver(queueName,new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });
            // Optionally use session receiver if the queue is session-enabled
            ServiceBusSessionReceiver sessionReceiver = null;

            while (true)
            {
                Console.WriteLine($"Reading from queue {queueName}. {System.Environment.NewLine} Options {System.Environment.NewLine} 1.Read {System.Environment.NewLine} 2.Session read {System.Environment.NewLine} 3.Dead letter {System.Environment.NewLine} 4.Exit {System.Environment.NewLine}");
                string option = Console.ReadLine();
                if (option == "1")
                {
                   await ReceiveMessagesWithoutSessionAsync(receiver);
                }
                else if (option == "2")
                {
                    // Try to receive messages with a session receiver
                    sessionReceiver = await client.AcceptNextSessionAsync(queueName);
                    Console.WriteLine($"Session {sessionReceiver.SessionId} is locked and receiving messages...");

                    // If session is enabled, handle messages from session
                    if (sessionReceiver != null)
                    {
                        await ReceiveMessageWithSessionAsync(sessionReceiver);
                    }
    
                }
                else if (option == "3")
                {
                    await ReceiveDeadLetterMessageAsync(client,queueName);
                }
                else if (option == "4")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid option.");
                }
            }
        }

        private static async Task ReceiveMessagesWithoutSessionAsync(ServiceBusReceiver receiver)
        {
            Console.WriteLine("Reading message from queue...");

            while (true)
            {
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));

                if (message!=null)
                {
                    Console.WriteLine($"Received message: {message.Body.ToString()}");

                    Console.WriteLine($"MessageId: {message.MessageId}, ContentType:{message.ContentType}");

                    await receiver.CompleteMessageAsync(message);  
                }
                else
                {
                    Console.WriteLine("No more messages in the queue.");
                    break;
                }
            }
        }

        private static async Task ReceiveMessageWithSessionAsync(ServiceBusSessionReceiver receiver)
        {
            Console.WriteLine("Reading messages from session");

            while (true) 
            {
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
                if (message != null)
                {
                    // Handle the received message
                    Console.WriteLine($"Received message: {message.Body.ToString()}");
                    Console.WriteLine($"SessionId: {message.SessionId}, MessageId: {message.MessageId}");

                    // Access message properties
                    Console.WriteLine($"MessageId: {message.MessageId}, ContentType: {message.ContentType}");

                    // Complete the message (remove from the queue)
                    await receiver.CompleteMessageAsync(message);
                }
                else
                {
                    Console.WriteLine("No more messages in the session");
                    break;
                }    
            }
            await receiver.CloseAsync();
        }

        private static async Task ReceiveDeadLetterMessageAsync(ServiceBusClient client, string queueName)
        {
            //path
            var deadLetterQueue = $"{queueName}/$DeadLetterQueue";

            ServiceBusReceiver deadLetterReceiver = client.CreateReceiver(deadLetterQueue);

            Console.WriteLine("Receiving message from dead letter queue");
            while (true)
            {
                ServiceBusReceivedMessage message = await deadLetterReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));

                if (message != null)
                {
                    //handle dead messgae
                    Console.WriteLine($"Received dead letter message {message.Body.ToString()}");
                    Console.WriteLine($"DeadLetterReason: {message.DeadLetterReason}, DeadLetterErrorDescription: {message.DeadLetterErrorDescription}");

                    // Complete the message (remove from the dead-letter queue)
                    await deadLetterReceiver.CompleteMessageAsync(message);
                }
                else
                {
                    Console.WriteLine("No more messages in the dead-letter queue.");
                    break;
                }
            }

            await deadLetterReceiver.DisposeAsync();
        }

    }
}