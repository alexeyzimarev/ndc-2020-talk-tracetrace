using System;

namespace TraceTrace.Contracts
{
    public static class Commands
    {
        public class UpdateWeather
        {
            public string   Id           { get; set; }
            public DateTime Date         { get; set; }
            public int      TemperatureC { get; set; }
            public string   Summary      { get; set; }
        }
    }
}
