using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
            public double? Hiti { get; set; }
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

        public static IConfigurationRoot Configuration;

        static  void Main(string[] args)
        {

            Configuration = InitConfig();

            //##################### 

            // Check if we should fetch
            int fetchCountToday = WeatherRepo.GetTodayFetchCount(Configuration.GetConnectionString("FishingLogDatabase"));
            DateTime now = DateTime.Now;

            if (fetchCountToday >= 3)
            {
                Console.WriteLine("Fetch limit (3 times) for today reached. Skipping.");
                return;
            }

            var lastFetch = WeatherRepo.GetLastFetchTime(Configuration.GetConnectionString("FishingLogDatabase"));

            if (lastFetch.HasValue)
            {
                TimeSpan sinceLast = now - lastFetch.Value;

                // Enforce 3-hour gap if it's earlier in the day
                if (sinceLast.TotalHours < 3 && now.Hour < 21) // Give flexibility near end of day
                {
                    Console.WriteLine($"Last fetch was {sinceLast.TotalMinutes:F0} minutes ago. Waiting at least 3 hours between fetches.");
                    return;
                }
            }

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
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<DataObject>>().Result;
                
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
                    weather.Nr_Vedurstofa = d.Nr_Vedurstofa;
                    weather.PntX = d.PntX;
                    weather.PntY = d.PntY;
                    weather.Raki = d.Raki;
                    weather.Sjavarhaed = d.Sjavarhaed;
                    weather.Vindatt = d.Vindatt;
                    weather.VindattAsc = d.VindattAsc;
                    weather.VindattStDev = d.VindattStDev;
                    weather.Vindhradi = d.Vindhradi;
                    weather.Vindhvida = d.Vindhvida;

                    WeatherRepo.Insert(weather, Configuration.GetConnectionString("FishingLogDatabase"));
                }
                // Log fetch after successful insert
                WeatherRepo.LogFetch(Configuration.GetConnectionString("FishingLogDatabase"));
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
