using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TelemetryCollector.Common.Interfaces;

namespace TelemetryCollector.MacOS.Collectors
{
    public class MacOSTelemetryCollector : ITelemetryCollector
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
                Version = await ExecuteCommand("system_profiler SPFirmwareDataType | grep 'Version'"),
                Manufacturer = await ExecuteCommand("system_profiler SPFirmwareDataType | grep 'Vendor'"),
                SerialNumber = await ExecuteCommand("system_profiler SPFirmwareDataType | grep 'Serial Number'")
            };
        }

        private async Task<CpuInfo> GetCpuInfoAsync()
        {
            return new CpuInfo
            {
                Name = await ExecuteCommand("sysctl -n machdep.cpu.brand_string"),
                ProcessorId = await ExecuteCommand("sysctl -n machdep.cpu.vendor"),
                ThreadCount = int.Parse(await ExecuteCommand("sysctl -n hw.logicalcpu")),
                NumberOfCores = int.Parse(await ExecuteCommand("sysctl -n hw.physicalcpu")),
                SerialNumber = await ExecuteCommand("sysctl -n machdep.cpu.serial_number")
            };
        }

        private async Task<DiskInfo> GetDiskInfoAsync()
        {
            return new DiskInfo
            {
                Size = await ExecuteCommand("diskutil info / | grep 'Total Size'"),
                Model = await ExecuteCommand("diskutil info / | grep 'Device Model'"),
                SerialNumber = await ExecuteCommand("diskutil info / | grep 'Device Identifier'")
            };
        }

        private async Task<GpuInfo> GetGpuInfoAsync()
        {
            return new GpuInfo
            {
                Name = await ExecuteCommand("system_profiler SPDisplaysDataType | grep 'Chipset Model'"),
                DriverVersion = await ExecuteCommand("system_profiler SPDisplaysDataType | grep 'Driver Version'")
            };
        }

        private async Task<MemoryInfo[]> GetMemoryInfoAsync()
        {
            return new[]
            {
                new MemoryInfo
                {
                    Speed = await ExecuteCommand("system_profiler SPMemoryDataType | grep 'Speed'"),
                    Capacity = await ExecuteCommand("system_profiler SPMemoryDataType | grep 'Size'"),
                    PartNumber = await ExecuteCommand("system_profiler SPMemoryDataType | grep 'Part Number'"),
                    SerialNumber = await ExecuteCommand("system_profiler SPMemoryDataType | grep 'Serial Number'")
                }
            };
        }

        private async Task<MotherboardInfo> GetMotherboardInfoAsync()
        {
            return new MotherboardInfo
            {
                Product = await ExecuteCommand("system_profiler SPHardwareDataType | grep 'Model Identifier'"),
                Manufacturer = await ExecuteCommand("system_profiler SPHardwareDataType | grep 'Manufacturer'"),
                SerialNumber = await ExecuteCommand("system_profiler SPHardwareDataType | grep 'Serial Number'")
            };
        }

        private async Task<SystemInfo> GetSystemInfoAsync()
        {
            return new SystemInfo
            {
                Type = "MacOS",
                Model = await ExecuteCommand("sysctl -n hw.model"),
                ServiceTag = await ExecuteCommand("system_profiler SPHardwareDataType | grep 'Serial Number'"),
                Manufacturer = "Apple"
            };
        }

        private async Task<string> ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
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