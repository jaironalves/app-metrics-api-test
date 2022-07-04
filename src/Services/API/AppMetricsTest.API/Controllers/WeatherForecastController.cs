using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppMetricsTest.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IHttpClientFactory httpClientFactory, ILogger<WeatherForecastController> logger)
        {
            this.httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var request = new HttpRequestMessage(
            HttpMethod.Post,
            "http://localhost:6000/execute");

            var command = GetCommand();

            var post = new PostRequest()
            {
                Command = command,
                Session = GetSession(command),
                Alternate = new Random().Next(1, 100) % 2 == 0
            };

            string jsonString = JsonSerializer.Serialize(post);
            request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            //request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "YourApp");

            try
            {
                var client = httpClientFactory.CreateClient("post");
                var response = await client.SendAsync(request);

                var rng = new Random();
                return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray());
            }
            catch (Exception)
            {

                return BadRequest();
            }


            /*

            RoutePattern routePattern = null;

            var endother = HttpContext.GetEndpoint();

            var queryFeature = HttpContext.Features[typeof(Microsoft.AspNetCore.Http.Features.IHttpRequestFeature)]
                                           as Microsoft.AspNetCore.Http.Features.IHttpRequestFeature;

            var endpointFeature = HttpContext.Features[typeof(Microsoft.AspNetCore.Http.Features.IEndpointFeature)]
                                           as Microsoft.AspNetCore.Http.Features.IEndpointFeature;
           

            var endpoint = (endpointFeature?.Endpoint as RouteEndpoint);

            if (endpoint != null)
            {
                routePattern = endpoint.RoutePattern;                
            }

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
            */
        }

        private static string GetSession(string command)
        {
            if (command.Equals("Login"))
                return string.Empty;

            var randomValue = new Random().Next(1, 5);

            return randomValue switch
            {
                1 => "SESSION01",
                2 => "SESSION02",
                3 => "SESSION03",
                4 => "SESSION04",
                _ => "",
            };
            
        }

        private string GetCommand()
        {
            var randomValue = new Random().Next(1, 4);

            return randomValue switch
            {
                1 => "Login",
                2 => "Send",
                3 => "Alter",
                4 => "Delete",
                _ => "Login",
            };
        }

        [HttpGet("{id}")]
        public IEnumerable<WeatherForecast> Get(string id)
        {
            RoutePattern routePattern = null;

            var endpointFeature = HttpContext.Features[typeof(Microsoft.AspNetCore.Http.Features.IEndpointFeature)]
                                           as Microsoft.AspNetCore.Http.Features.IEndpointFeature;
            var endpoint = endpointFeature?.Endpoint;

            if (endpoint != null)
            {
                routePattern = (endpoint as RouteEndpoint)?.RoutePattern;
            }

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
