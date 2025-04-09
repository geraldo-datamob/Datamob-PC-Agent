using TelemetryCollector.Common.Interfaces;
using TelemetryCollector.Linux.Collectors;
using TelemetryCollector.MacOS.Collectors;
using TelemetryCollector.Windows.Collectors;

namespace TelemetryCollector.Core.Factories
{
    public static class TelemetryCollectorFactory
    {
        public static ITelemetryCollector CreateTelemetryCollector()
        {
            string os = Environment.OSVersion.Platform.ToString();

            switch (os)
            {
                case "Unix":
                    return new LinuxTelemetryCollector();
                case "MacOSX":
                    return new MacOSTelemetryCollector();
                case "Win32NT":
                    return new WindowsTelemetryCollector();
                default:
                    throw new NotSupportedException($"Operating system '{os}' is not supported.");
            }
        }
    }
}