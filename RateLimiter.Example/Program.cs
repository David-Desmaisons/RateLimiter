using System;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            Console.WriteLine("Enter api key:");
            var apiKey = Console.ReadLine();
            var client = new OpenWeatherMapClient(apiKey);

            do
            {
                Console.WriteLine("Enter city name:");
                var city = Console.ReadLine();
                var data = await client.GetWeather(city);
                var message = (data == null) ? "No information" : $"Weather: {string.Join(",", data.weather.Select(w => w.description))}";
                Console.WriteLine(message);
                Console.WriteLine("Press y to continue");
            } while (Console.ReadLine() == "y");
        }
    }
}
