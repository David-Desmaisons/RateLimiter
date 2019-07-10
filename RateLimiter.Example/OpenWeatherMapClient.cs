using ComposableAsync;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RateLimiter.Example
{
    public class OpenWeatherMapClient
    {
        private readonly HttpClient _Client;
        private readonly string _ApiKey;

        public OpenWeatherMapClient(string apiKey)
        {
            _ApiKey = apiKey;
            var handler = TimeLimiter
                            .GetFromMaxCountByInterval(60, TimeSpan.FromMinutes(1))
                            .AsDelegatingHandler();
            _Client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/")
            };
        }

        public async Task<WeatherDto> GetWeather(string city, string countryCode = null)
        {
            var query = $"weather?q={city}{(countryCode == null ? "" : $",{countryCode}")}&appid={_ApiKey}";
            var response = await _Client.GetAsync(query);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsAsync<WeatherDto>();
        }
    }
}
