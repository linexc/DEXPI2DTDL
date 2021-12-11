using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;

namespace CreateAndInitialization
{
    class Program
    {
        private const string adtInstanceUrl = "https://Yogurtmachine.api.wcus.digitaltwins.azure.net/models?api-version=2020-10-31";

        static async Task Main(string[] args)
        {
            string token; 
            // Read token from local file
            using (StreamReader readtext = new StreamReader(@"D:\Studium\SemesterArbeit\Thesis\token.txt"))
            {
                token = readtext.ReadLine();
            }

            // Upload models
            //await CreateModel(token);

            //Add Twin, whose $dtID is PTLB, and ID is PTLB too. One of them should be renamed.  For uploading twins, each of them need a specific url
            string postUrl = "https://Yogurtmachine.api.wcus.digitaltwins.azure.net/digitaltwins/PTLB?api-version=2020-10-31";
            await AddTwin(postUrl, token);
   
        }

        private static async Task CreateModel(string token)
        {
            Console.WriteLine($"Upload a model");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(adtInstanceUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            // OAuth 2.0 authentication using bearer token 
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + token);

            // Read local Json file
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                StreamReader model = new StreamReader(@"D:\Studium\SemesterArbeit\Sync+Share\Semesterarbeit -- Yu Mu\Material\MyJogurt\DEXPI\test.json");
                string json = model.ReadToEnd();
                streamWriter.Write(json);
            }

            // Catch response from server
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }

        private static async Task AddTwin(string postUrl, string token)
        {
            Console.WriteLine($"Add Twin");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";

            // OAuth 2.0 authentication using bearer token 
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + token);

            // Read local Json file
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                StreamReader model = new StreamReader(@"D:\Studium\SemesterArbeit\Sync+Share\Semesterarbeit -- Yu Mu\Material\MyJogurt\DEXPI\DT\PTLB.json");
                string json = model.ReadToEnd();
                streamWriter.Write(json);
            }
           
            // Catch response from server
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
            
        }

    }
}
