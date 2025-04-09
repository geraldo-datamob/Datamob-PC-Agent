using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TelemetryCollector.Common.Interfaces;

namespace TelemetryCollector.Windows.Collectors
{
    public class WindowsTelemetryCollector : ITelemetryCollector
    {
        public async Task<TelemetryData> CollectTelemetryDataAsync()
        {
            var telemetryData = new TelemetryData
            {
                Bios = await GetBiosInfoAsync(),
                Cpu = await GetCpuInfoAsync(),
                Disk = await GetDiskInfoAsync(),
                Gpu = await GetGpuInfoAsync(),
                Memory = await GetMemoryInfoAsync(),
                Motherboard = await GetMotherboardInfoAsync(),
                System = await GetSystemInfoAsync()
            };

            return telemetryData;
        }

        private async Task<BiosInfo> GetBiosInfoAsync()
        {
            return new BiosInfo
            {
                Version = await ExecuteCommand("wmic bios get version"),
                Manufacturer = await ExecuteCommand("wmic bios get manufacturer"),
                SerialNumber = await ExecuteCommand("wmic bios get serialnumber")
            };
        }

        private async Task<CpuInfo> GetCpuInfoAsync()
        {
            return new CpuInfo
            {
                Name = await ExecuteCommand("wmic cpu get name"),
                ProcessorId = await ExecuteCommand("wmic cpu get processorid"),
                ThreadCount = int.Parse(await ExecuteCommand("wmic cpu get numberoflogicalprocessors")),
                NumberOfCores = int.Parse(await ExecuteCommand("wmic cpu get numberofcores")),
                SerialNumber = await ExecuteCommand("wmic cpu get serialnumber")
            };
        }

        private async Task<DiskInfo> GetDiskInfoAsync()
        {
            return new DiskInfo
            {
                Size = await ExecuteCommand("wmic diskdrive get size"),
                Model = await ExecuteCommand("wmic diskdrive get model"),
                SerialNumber = await ExecuteCommand("wmic diskdrive get serialnumber")
            };
        }

        private async Task<GpuInfo> GetGpuInfoAsync()
        {
            return new GpuInfo
            {
                Name = await ExecuteCommand("wmic path win32_videocontroller get name"),
                DriverVersion = await ExecuteCommand("wmic path win32_videocontroller get driverversion")
            };
        }

        private async Task<MemoryInfo> GetMemoryInfoAsync()
        {
            return new MemoryInfo
            {
                Speed = await ExecuteCommand("wmic memorychip get speed"),
                Capacity = await ExecuteCommand("wmic memorychip get capacity"),
                PartNumber = await ExecuteCommand("wmic memorychip get partnumber"),
                SerialNumber = await ExecuteCommand("wmic memorychip get serialnumber")
            };
        }

        private async Task<MotherboardInfo> GetMotherboardInfoAsync()
        {
            return new MotherboardInfo
            {
                Product = await ExecuteCommand("wmic baseboard get product"),
                Manufacturer = await ExecuteCommand("wmic baseboard get manufacturer"),
                SerialNumber = await ExecuteCommand("wmic baseboard get serialnumber")
            };
        }

        private async Task<SystemInfo> GetSystemInfoAsync()
        {
            return new SystemInfo
            {
                Type = "Windows",
                Model = await ExecuteCommand("wmic computersystem get model"),
                ServiceTag = await ExecuteCommand("wmic bios get serialnumber"),
                Manufacturer = await ExecuteCommand("wmic computersystem get manufacturer")
            };
        }

        private async Task<string> ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();
                return await process.StandardOutput.ReadToEndAsync();
            }
        }
    }
}