using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using VedurConsole.Models;
using VedurConsole.Repos;

namespace VedurConsole
{
    class Program
    {

        public class DataObject
        {
            public string Dags { get; set; }
            public string Breidd { get; set; }
            public string Daggarmark { get; set; }
            public string Haed { get; set; }
            public double Hiti { get; set; }
            public string Lengd { get; set; }
            public string Loftthrystingur { get; set; }
            public string Nafn { get; set; }
            public int Nr { get; set; }
            public string Nr_Vedurstofa { get; set; }
            public string PntX { get; set; }
            public string PntY { get; set; }
            public string Raki { get; set; }
            public string Sjavarhaed { get; set; }
            public string Vindatt { get; set; }
            public string VindattAsc { get; set; }
            public string VindattStDev { get; set; }
            public string Vindhradi { get; set; }
            public string Vindhvida { get; set; }

        }

        private static IConfigurationRoot InitConfig()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static T InitOptions<T>()
    where T : new()
        {
            var config = InitConfig();
            return config.Get<T>();
        }

        static  void Main(string[] args)
        {

            var cfg = InitOptions<AppConfig>();

            //##################### 

            var weatherAPIUrl = "https://gagnaveita.vegagerdin.is/api/vedur2014_1";
   
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(weatherAPIUrl);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync("").Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<DataObject>>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                foreach (var d in dataObjects)
                {
                    Console.WriteLine("{0}", d.Breidd);
                    var weather = new Weather();
                    weather.Breidd = d.Breidd;
                    weather.Daggarmark = d.Daggarmark;
                    weather.Dags = d.Dags;
                    weather.Haed = d.Haed;
                    weather.Hiti = d.Hiti;
                    weather.Lengd = d.Lengd;
                    weather.Loftthrystingur = d.Loftthrystingur;
                    weather.Nafn = d.Nafn;
                    weather.Nr = d.Nr;
                    weather.NrVedurstofa = d.Nr_Vedurstofa;
                    weather.PntX = d.PntX;
                    weather.PntY = d.PntY;
                    weather.Raki = d.Raki;
                    weather.Sjavarhaed = d.Sjavarhaed;
                    weather.Vindatt = d.Vindatt;
                    weather.VindattAsc = d.VindattAsc;
                    weather.VindattStDev = d.VindattStDev;
                    weather.Vindhradi = d.Vindhradi;
                    weather.Vindhvida = d.Vindhvida;

                    WeatherRepo.Insert(weather);
                }
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }



            // Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();
        }
    }
}
