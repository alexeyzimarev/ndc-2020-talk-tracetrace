using Jaeger;
using Jaeger.Samplers;
using MassTransit;
using MassTransit.OpenTracing;
using MassTransit.PrometheusIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OpenTracing.Contrib.Mongo;
using OpenTracing.Util;
using Prometheus;
using TraceTrace.Infrastructure;
using PrometheusMetrics = TraceTrace.Metrics.PrometheusMetrics;

namespace TraceTrace
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            const string appName = "trace-trace";

            PrometheusMetrics.TryConfigure(appName);

            var database = ConfigureMongo(Configuration["MongoConnectionString"]);
            services.AddSingleton(database);

            #region MassTransit
            var rmqSettings = new RabbitMqSettings(Configuration.GetSection("RabbitMq"));

            services.AddMassTransit(
                r =>
                {
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
                                    cfg.PropagateOpenTracingContext();
                                    cfg.UsePrometheusMetrics(serviceName: appName);
                                }
                            )
                    );
                }
            );
            services.AddMassTransitHostedService();
            #endregion MassTransit

            #region OpenTracing

            // go to http://localhost:16686 for Jaeger
            services.AddOpenTracing();

            var tracer = new Tracer.Builder(appName)
                .WithSampler(new ConstSampler(true))
                .Build();
            GlobalTracer.Register(tracer);

            #endregion OpenTracing

            services.AddControllers();
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo {Title = "My API", Version = "v1"}));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

            app.UseRouting();
            // go to http://localhost:5000 for metrics
            app.UseHttpMetrics();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapMetrics();
                }
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
