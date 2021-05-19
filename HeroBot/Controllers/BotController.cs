using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Communication.Chat;
using HeroBot.Models;
using HeroBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HeroBot.Controllers
{
    [ApiController]
    // [Route("Bot")]
    public class BotController : Controller
    {
        // GET: /<controller>/
        //public IActionResult Index()
        //{
        //    return View();
        //}

        private readonly IBotServices _botServices;
        private readonly IWeatherServices _weatherServices;
        private readonly ILogger<BotController> _logger;
        private readonly IConfiguration _configuration;

        public BotController(ILogger<BotController> logger, IBotServices botServices, IWeatherServices weatherServices, IConfiguration configuration)
        {
            _logger = logger;
            _botServices = botServices;
            _weatherServices = weatherServices;
            _configuration = configuration;
        }

        [Route("processMessage")]
        [HttpPost]
        public async Task<IActionResult> ProcessMessage(ClientChatMessage message)
        {
            var weatherClient = _botServices.LuisWeatherRuntimeClient(_configuration);
            var ApplicationId = _configuration["LuisWeatherAppId"];
            try
            {
                var result = await weatherClient.Prediction.ResolveAsync(ApplicationId, message.Content.Message);
                var WeatherDetails = new JObject();
                if(result.Entities.Count > 0)
                {
                    WeatherDetails = await _weatherServices.GetCurrentWeather(result.Entities[0].Entity);
                    return Ok($"Weather Details:\n\nLooks like {WeatherDetails.SelectToken("weather[0].description")}\n\nTemperature: {WeatherDetails.SelectToken("main.temp")}\n\nFeels like: {WeatherDetails.SelectToken("main.feels_like")}\n\nHumidity: {WeatherDetails.SelectToken("main.humidity")}");

                }
                // var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nSomething went wrong. Please Make sure your app is published and try again.\n");
                return BadRequest();
            }

        }
    }
}
