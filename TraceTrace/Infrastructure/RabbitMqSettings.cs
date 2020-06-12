using System;
using Microsoft.Extensions.Configuration;

namespace TraceTrace.Infrastructure
{
    public class RabbitMqSettings
    {
        public string Username { get; }
        public string Password { get; }
        public string Host     { get; }

        public Uri Uri => new Uri(Host);

        public RabbitMqSettings(IConfiguration configuration)
        {
            Host     = configuration["Host"];
            Username = configuration["Username"];
            Password = configuration["Password"];
        }
    }
}
