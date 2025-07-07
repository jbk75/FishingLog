using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using VedurConsole.Models;
using VedurConsole.Repos;


namespace VedurConsole
{
#pragma warning disable S1118 // Utility classes should not have public constructors
    internal class Program
#pragma warning restore S1118 // Utility classes should not have public constructors
    {
        public static IConfigurationRoot Configuration;

        private static IConfigurationRoot InitConfig()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            return builder.Build();
        }

        static void Main(string[] args)
        {
            try
            {
                Configuration = InitConfig();
                string connectionString = Configuration.GetConnectionString("FishingLogDatabase");

                int fetchCountToday = 0;
                try
                {
                    fetchCountToday = WeatherRepo.GetTodayFetchCount(connectionString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching today's fetch count: {ex.Message}");
                    return;
                }

                DateTime now = DateTime.Now;

                if (fetchCountToday >= 3)
                {
                    Console.WriteLine("Fetch limit (3 times) for today reached. Skipping.");
                    return;
                }

                DateTime? lastFetch = null;
                try
                {
                    lastFetch = WeatherRepo.GetLastFetchTime(connectionString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching last fetch time: {ex.Message}");
                    return;
                }

                if (lastFetch.HasValue)
                {
                    TimeSpan sinceLast = now - lastFetch.Value;
                    if (sinceLast.TotalHours < 3 && now.Hour < 21)
                    {
                        Console.WriteLine($"Last fetch was {sinceLast.TotalMinutes:F0} minutes ago. Waiting at least 3 hours between fetches.");
                        return;
                    }
                }

                string weatherAPIUrl = "https://gagnaveita.vegagerdin.is/api/vedur2014_1";

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(weatherAPIUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                    HttpResponseMessage response;
                    try
                    {
                        response = client.GetAsync("").Result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"HTTP request failed: {ex.Message}");
                        return;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        IEnumerable<WeatherStationData> dataObjects;
                        try
                        {
                            var stream = response.Content.ReadAsStreamAsync().Result;

                            XmlSerializer serializer = new XmlSerializer(typeof(WeatherStationList));
                            var result = (WeatherStationList)serializer.Deserialize(stream);

                            dataObjects = result.Items;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to parse XML response: {ex.Message}");
                            return;
                        }

                        foreach (var d in dataObjects)
                        {
                            try
                            {
                                //Console.WriteLine(d.Breidd);

                                var weather = new Weather
                                {
                                    Breidd = d.Breidd,
                                    Daggarmark = d.Daggarmark,
                                    Dags = d.ParsedDags,
                                    Haed = d.Haed,
                                    Hiti = d.Hiti,
                                    Lengd = d.Lengd,
                                    Loftthrystingur = d.Loftthrystingur,
                                    Nafn = d.Nafn,
                                    Nr = d.Nr,
                                    Nr_Vedurstofa = d.Nr_Vedurstofa,
                                    PntX = d.PntX,
                                    PntY = d.PntY,
                                    Raki = d.Raki,
                                    Sjavarhaed = d.Sjavarhaed,
                                    Vindatt = d.Vindatt,
                                    VindattAsc = d.VindattAsc,
                                    VindattStDev = d.VindattStDev,
                                    Vindhradi = d.Vindhradi,
                                    Vindhvida = d.Vindhvida
                                };

                                WeatherRepo.Insert(weather, connectionString);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to insert weather data: {ex.Message}");
                            }
                        }

                        try
                        {
                            WeatherRepo.LogFetch(connectionString);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to log fetch: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{(int)response.StatusCode} ({response.ReasonPhrase})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error occurred: {ex.Message}");
            }
        }
    }
}