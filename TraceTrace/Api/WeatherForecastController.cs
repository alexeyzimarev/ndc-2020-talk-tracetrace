using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TraceTrace.Api.Models;
using TraceTrace.Contracts;
using TraceTrace.Metrics;

namespace TraceTrace.Api
{
    [ApiController]
    [Route("weather")]
    public class WeatherForecastController : ControllerBase
    {
        static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        readonly IMongoDatabase   _database;
        readonly IPublishEndpoint _publisher;

        public WeatherForecastController(IMongoDatabase database, IPublishEndpoint publisher)
        {
            _database  = database;
            _publisher = publisher;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();

            return Enumerable.Range(1, 5)
                .Select(
                    index => new WeatherForecast
                    {
                        Date         = DateTime.Now.AddDays(index),
                        TemperatureC = rng.Next(-20, 55),
                        Summary      = Summaries[rng.Next(Summaries.Length)]
                    }
                )
                .ToArray();
        }

        [HttpPost]
        public async Task<ActionResult> Post()
        {
            var rng = new Random();

            var forecast = new WeatherForecast
            {
                Id           = Guid.NewGuid().ToString("N"),
                Date         = DateTime.Now.AddDays(1),
                TemperatureC = rng.Next(-20, 55),
                Summary      = Summaries[rng.Next(Summaries.Length)]
            };

            const string collectionName = "weather";

            // await _database.GetCollection<WeatherForecast>(collectionName).InsertOneAsync(forecast);

            #region Metrics

            await PrometheusMetrics.Measure(
                () => _database.GetCollection<WeatherForecast>(collectionName).InsertOneAsync(forecast),
                PrometheusMetrics.DbUpdateTimer(collectionName),
                PrometheusMetrics.DBUpdateErrorCounter(collectionName)
            );

            #endregion Metrics

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Put()
        {
            var rng = new Random();

            var message = new Commands.UpdateWeather
            {
                Id           = Guid.NewGuid().ToString("N"),
                Date         = DateTime.Now.AddDays(1),
                TemperatureC = rng.Next(-20, 55),
                Summary      = Summaries[rng.Next(Summaries.Length)]
            };

            await _publisher.Publish(message);

            return Ok();
        }
    }
}
