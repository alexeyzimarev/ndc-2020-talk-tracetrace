using System;

namespace TraceTrace.RemoteNode.Weather
{
    public class WeatherDocument
    {
        public string   Id           { get; set; }
        public DateTime Date         { get; set; }
        public int      TemperatureC { get; set; }
        public string   Summary      { get; set; }
    }
}
