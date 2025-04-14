using System.Text.Json;
using Quartz;
using TelemetryCollector.Common.Interfaces;

namespace TelemetryCollector.App.Jobs;

[DisallowConcurrentExecution]
public class TelemetryJob : IJob
{
    private readonly ITelemetryCollector _telemetryCollector;

    public TelemetryJob(ITelemetryCollector telemetryCollector)
    {
        _telemetryCollector = telemetryCollector;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            Console.WriteLine($"Collecting telemetry at: {DateTime.Now}");

            var telemetryData = await _telemetryCollector.CollectTelemetryDataAsync();
            var jsonOutput = JsonSerializer.Serialize(telemetryData,
                new JsonSerializerOptions { WriteIndented = true });

            Console.WriteLine(jsonOutput);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error collecting telemetry: {ex.Message}");
        }
    }
}
