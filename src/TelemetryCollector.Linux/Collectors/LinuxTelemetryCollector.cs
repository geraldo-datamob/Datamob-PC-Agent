using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TelemetryCollector.Common.Interfaces;

namespace TelemetryCollector.Linux.Collectors
{
    public class LinuxTelemetryCollector : ITelemetryCollector
    {

        private readonly string _username = "geraldo-neto";
        private readonly string _password = "n2e4t0o4GAN$";

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
                Version = await ExecuteCommand("sudo dmidecode -t bios | grep 'Version' | cut -d ':' -f2 | xargs"),
                Manufacturer = await ExecuteCommand("sudo dmidecode -t bios | grep 'Vendor' | cut -d ':' -f2 | xargs"),
                SerialNumber = await ExecuteCommand("sudo dmidecode -t system | grep 'Serial Number' | cut -d ':' -f2 | xargs")
            };
        }

        private async Task<CpuInfo> GetCpuInfoAsync()
        {
            return new CpuInfo
            {
                Name = await ExecuteCommand("cat /proc/cpuinfo | grep 'model name' | head -1 | cut -d ':' -f2 | xargs"),
                ProcessorId = await ExecuteCommand("cat /proc/cpuinfo | grep 'vendor_id' | head -1 | cut -d ':' -f2 | xargs"),
                ThreadCount = int.Parse(await ExecuteCommand("nproc")),
                NumberOfCores = int.Parse(await ExecuteCommand("sudo dmidecode -t processor | grep -i 'Core Count' | cut -d':' -f2 | xargs")),
                SerialNumber = await ExecuteCommand("sudo dmidecode -t processor | grep 'ID' | cut -d ':' -f2 | xargs")
            };
        }

        private async Task<DiskInfo> GetDiskInfoAsync()
        {
            return new DiskInfo
            {
                Size = await ExecuteCommand("lsblk -b -o SIZE | grep -v 'SIZE' | awk '{sum += $1} END {print sum}'"),
                Model = await ExecuteCommand("lsblk -o MODEL | grep -v 'MODEL' | head -1"),
                SerialNumber = await ExecuteCommand("lsblk -o SERIAL | grep -v 'SERIAL' | head -1")
            };
        }

        private async Task<GpuInfo> GetGpuInfoAsync()
        {
            return new GpuInfo
            {
                Name = await ExecuteCommand("lspci | grep -i 'vga' | cut -d ':' -f3 | xargs"),
                DriverVersion = await ExecuteCommand("lspci -v -s $(lspci | grep -i 'vga' | cut -d ' ' -f 1) | grep 'Kernel driver'")
            };
        }

        private async Task<MemoryInfo> GetMemoryInfoAsync()
        {
            return new MemoryInfo
            {
                Speed = await ExecuteCommand("sudo dmidecode -t memory | grep 'Speed' | cut -d ':' -f2 | xargs"),
                Capacity = await ExecuteCommand("sudo dmidecode -t memory | grep 'Size' | grep -v 'No Module' | cut -d ':' -f2 | xargs"),
                PartNumber = await ExecuteCommand("sudo dmidecode -t memory | grep 'Part Number' | cut -d ':' -f2 | xargs"),
                SerialNumber = await ExecuteCommand("sudo dmidecode -t memory | grep 'Serial Number' | cut -d ':' -f2 | xargs")
            };
        }

        private async Task<MotherboardInfo> GetMotherboardInfoAsync()
        {
            return new MotherboardInfo
            {
                Product = await ExecuteCommand("sudo dmidecode -t baseboard | grep 'Product Name' | cut -d ':' -f2 | xargs"),
                Manufacturer = await ExecuteCommand("sudo dmidecode -t baseboard | grep 'Manufacturer' | cut -d ':' -f2 | xargs"),
                SerialNumber = await ExecuteCommand("sudo dmidecode -t baseboard | grep 'Serial Number' | cut -d ':' -f2 | xargs")
            };
        }

        private async Task<SystemInfo> GetSystemInfoAsync()
        {
            return new SystemInfo
            {
                Type = "Linux",
                Model = await ExecuteCommand("uname -m"),
                ServiceTag = await ExecuteCommand("sudo dmidecode -t system | grep 'Serial Number' | cut -d ':' -f2 | xargs"),
                Manufacturer = await ExecuteCommand("sudo dmidecode -t system | grep 'Manufacturer' | cut -d ':' -f2 | xargs")
            };
        }

        private async Task<string> ExecuteCommand_old(string command)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"echo '{_password}' | sudo -S -u {_username} {command}\"",
                //Arguments = $"-c \"{command}\"",
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

        private async Task<string> ExecuteCommand_old1(string command)
        {
            var isSudoCommand = command.StartsWith("sudo ");
            if (isSudoCommand)
            {
                command = $"echo '{_password}' | {command}";
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    error.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Command failed with error: {error}");
            }

            return output.ToString().Trim();
        }
    
        private async Task<string> ExecuteCommand(string command)
        {
            var isSudoCommand = command.StartsWith("sudo ");
            if (isSudoCommand)
            {
                // Replace sudo with sudo -S to read password from stdin
                command = command.Replace("sudo ", "sudo -S ");
                // Prepare command with password input
                command = $"echo '{_password}' | {command}";
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    error.AppendLine(e.Data);
            };

            try 
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Command failed with error: {error}");
                }

                return output.ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute command: {command}. Error: {ex.Message}");
            }
        }

    }
}