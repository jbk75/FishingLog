using Microsoft.EntityFrameworkCore;
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

        public static int GetTodayFetchCount(string connectionString)
        {
            using (var context = new WeatherContext(connectionString))
            {
                var today = DateTime.Today;
                return context.WeatherFetchLog.Count(f => f.FetchedAt.Date == today);
            }
        }

        public static DateTime? GetLastFetchTime(string connectionString)
        {
            using (var context = new WeatherContext(connectionString))
            {
                return context.WeatherFetchLog
                              .OrderByDescending(f => f.FetchedAt)
                              .Select(f => (DateTime?)f.FetchedAt)
                              .FirstOrDefault();
            }
        }

        public static void LogFetch(string connectionString)
        {
            using (var context = new WeatherContext(connectionString))
            {
                context.WeatherFetchLog.Add(new WeatherFetchLog());
                context.SaveChanges();
            }
        }

    }

}
