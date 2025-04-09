namespace TelemetryCollector.Common.Interfaces;
public interface ITelemetryCollector
{
    Task<TelemetryData> CollectTelemetryDataAsync();
}