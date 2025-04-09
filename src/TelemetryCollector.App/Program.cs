using TelemetryCollector.Core.Factories;
using System;
using System.Text.Json;
using TelemetryCollector.Common.Interfaces;

class Program
{
    static async Task Main(string[] args)
    {
        ITelemetryCollector telemetryCollector = TelemetryCollectorFactory.CreateTelemetryCollector();

        var telemetryData = await telemetryCollector.CollectTelemetryDataAsync();

        string jsonOutput = JsonSerializer.Serialize(telemetryData, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(jsonOutput);
    }
}