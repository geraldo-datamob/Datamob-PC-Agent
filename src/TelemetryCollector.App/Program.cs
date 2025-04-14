using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelemetryCollector.App.Jobs;
using TelemetryCollector.Core.Factories;
using TelemetryCollector.Common.Interfaces;

class Program
{
    static async Task Main(string[] args)
    {
        // Create host builder
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices((hostContext, services) =>
        {
            // Add logging
            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });

            // Register the telemetry collector
            services.AddSingleton(TelemetryCollectorFactory.CreateTelemetryCollector());

            // Configure Quartz
            services.AddQuartz(q =>
            {
                // Register the job
                var jobKey = new JobKey("TelemetryJob", "TelemetryGroup");
                q.AddJob<TelemetryJob>(opts => opts.WithIdentity(jobKey));

                // Create a trigger
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("TelemetryTrigger", "TelemetryGroup")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(20)
                        .RepeatForever()));
            });

            // Add the Quartz.NET hosted service
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        });

        // Build and run the host
        var host = builder.Build();
        await host.RunAsync();
    }
}