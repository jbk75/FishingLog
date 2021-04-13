using System;
using System.Collections.Generic;
using System.Text;
using VedurConsole.Models;

namespace VedurConsole.Repos
{
    public static class WeatherRepo
    {

        public static void Insert(Weather weatherItem)
        {
            using (var context = new WeatherContext())
            {
                //var weather = 
                //    new Models.Weather 
                //    {  
                //        Url = "http://example.com" };
                context.Weather.Add(weatherItem);
                context.SaveChanges();
            }
        }
    }
}
