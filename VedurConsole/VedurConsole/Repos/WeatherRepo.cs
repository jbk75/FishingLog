using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VedurConsole.Models;

namespace VedurConsole.Repos
{
    public static class WeatherRepo
    {

        public static void Insert(Weather weatherItem, string connectionString)
        {
            using (var context = new WeatherContext(connectionString))
            {
                context.Weather.Add(weatherItem);
                context.SaveChanges();
            }
        }

        public static string GetLatestDags(string connectionString)
        {
            using (var context = new WeatherContext(connectionString))
            {
                return context.Weather
                              .Where(w => w.Dags != null)
                              .OrderByDescending(w => w.Dags)
                              .Select(w => w.Dags)
                              .FirstOrDefault();
            }
        }
    }

}
