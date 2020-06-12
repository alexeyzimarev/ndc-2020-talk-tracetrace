using System.Threading.Tasks;
using MassTransit;
using MongoDB.Driver;
using TraceTrace.Contracts;
using static MongoDB.Driver.Builders<TraceTrace.RemoteNode.Weather.WeatherDocument>;

namespace TraceTrace.RemoteNode.Weather
{
    public class WeatherModule : IConsumer<Commands.UpdateWeather>
    {
        readonly IMongoCollection<WeatherDocument> _collection;

        public WeatherModule(IMongoCollection<WeatherDocument> collection) => _collection = collection;

        public Task Consume(ConsumeContext<Commands.UpdateWeather> context)
            => _collection.UpdateOneAsync(
                Filter.Eq(x => x.Id, context.Message.Id),
                Update.Set(x => x.Date, context.Message.Date)
                    .Set(x => x.Summary, context.Message.Summary)
                    .Set(x => x.TemperatureC, context.Message.TemperatureC),
                new UpdateOptions {IsUpsert = true},
                context.CancellationToken
            );
    }
}
