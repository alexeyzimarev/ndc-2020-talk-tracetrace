using Jaeger;
using Jaeger.Samplers;
using MassTransit;
using MassTransit.OpenTracing;
using MassTransit.PrometheusIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OpenTracing.Contrib.Mongo;
using OpenTracing.Util;
using Prometheus;
using TraceTrace.RemoteNode.Infrastructure;
using TraceTrace.RemoteNode.Weather;

namespace TraceTrace.RemoteNode
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            const string appName = "trace-remote";
            
            var database = ConfigureMongo(Configuration["MongoConnectionString"]);
            services.AddSingleton(_ => database.GetCollection<WeatherDocument>("remoteWeather"));

            var rmqSettings = new RabbitMqSettings(Configuration.GetSection("RabbitMq"));

            services.AddMassTransit(
                r =>
                {
                    r.AddConsumer<WeatherModule>();

                    r.AddBus(
                        ctx =>
                            Bus.Factory.CreateUsingRabbitMq(
                                cfg =>
                                {
                                    cfg.Host(
                                        rmqSettings.Uri,
                                        h =>
                                        {
                                            h.Username(rmqSettings.Username);
                                            h.Password(rmqSettings.Password);
                                        }
                                    );
                                    cfg.ConfigureEndpoints(ctx);
                                    cfg.PropagateOpenTracingContext();
                                    cfg.UsePrometheusMetrics(serviceName: appName);
                                }
                            )
                    );
                }
            );
            services.AddMassTransitHostedService();
            
            #region OpenTracing

            // go to http://localhost:16686 for Jaeger
            services.AddOpenTracing();

            var tracer = new Tracer.Builder(appName)
                .WithSampler(new ConstSampler(true))
                .Build();
            GlobalTracer.Register(tracer);

            #endregion OpenTracing
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseHttpMetrics();

            app.UseEndpoints(
                endpoints => endpoints.MapMetrics()
            );
        }

        static IMongoDatabase ConfigureMongo(string connectionString)
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            //var client = new MongoClient(settings);
            var client = new TracingMongoClient(settings);
            return client.GetDatabase(MongoUrl.Create(connectionString).DatabaseName);
        }
    }
}
