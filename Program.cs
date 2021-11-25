using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;

namespace ClientAppAuthenticationCode
{
    class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine("helllo world");
            string adtInstanceUrl = "https://Yogurtmachine.api.wcus.digitaltwins.azure.net";

            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);
            Console.WriteLine($"Service client created – ready to go");

            Console.WriteLine();
            Console.WriteLine($"Upload a model");
            string dtdl = File.ReadAllText(@"D:\Studium\SemesterArbeit\Thesis\Pizza.json");
            var models = new List<string> { dtdl };
            // Upload the model to the service

            try
            {
                await client.CreateModelsAsync(models);
                Console.WriteLine("Models uploaded to the instance:");
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Upload model error: {e.Status}: {e.Message}");
            }


            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = "dtmi:example:SampleModel;1";
            twinData.Contents.Add("data", $"Hello World!");

            string prefix = "sampleTwin-";
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    twinData.Id = $"{prefix}{i}";
                    await client.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
                    Console.WriteLine($"Created twin: {twinData.Id}");
                }
                catch (RequestFailedException e)
                {
                    Console.WriteLine($"Create twin error: {e.Status}: {e.Message}");
                }
            }
        }
    }
}
