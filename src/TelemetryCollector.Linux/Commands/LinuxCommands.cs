using System.Diagnostics;

namespace TelemetryCollector.Linux.Commands
{
    public static class LinuxCommands
    {
        public static string GetTotalRamCommand => "free -b | awk '/^Mem:/{print $2}'";
        public static string GetProcessorCommand => "dmidecode -t processor | grep -i 'Version:' | head -1 | cut -d':' -f2 | xargs";
        public static string GetTotalStorageCommand => "lsblk -b -o SIZE | grep -v 'SIZE' | awk '{sum += $1} END {print sum}'";
        public static string GetCpuNameCommand => "dmidecode -t processor | grep -i 'Version:' | head -1 | cut -d':' -f2 | xargs";
        public static string GetCpuProcessorIdCommand => "dmidecode -t processor | grep -i 'ID:' | head -1 | cut -d':' -f2 | xargs";
        public static string GetCpuThreadCountCommand => "nproc";
        public static string GetCpuNumberOfCoresCommand => "dmidecode -t processor | grep -i 'Core Count' | cut -d':' -f2 | xargs";
        public static string GetCpuSerialNumberCommand => "dmidecode -t processor | grep -i 'Serial Number' | cut -d':' -f2 | xargs";
        public static string GetGpuNameCommand => "lspci | grep -i 'vga|3d|display' | cut -d':' -f3";
        public static string GetGpuDriverVersionCommand => "glxinfo | grep 'OpenGL version'";
        public static string GetBiosVersionCommand => "dmidecode -t bios | grep -i 'Version' | cut -d':' -f2 | xargs";
        public static string GetBiosManufacturerCommand => "dmidecode -t bios | grep -i 'Vendor' | cut -d':' -f2 | xargs";
        public static string GetBiosSerialNumberCommand => "dmidecode -t bios | grep -i 'Serial Number' | cut -d':' -f2 | xargs";
        public static string GetDiskSizeCommand => "lsblk -b -o SIZE | grep -v 'SIZE' | head -1";
        public static string GetDiskModelCommand => "lsblk -o MODEL | grep -v 'MODEL' | head -1";
        public static string GetDiskSerialNumberCommand => "lsblk -o SERIAL | grep -v 'SERIAL' | head -1";
        public static string GetMemorySpeedCommand => "dmidecode -t 17 | grep -i 'Speed:' | cut -d':' -f2 | xargs";
        public static string GetMemoryCapacityCommand => "dmidecode -t 17 | grep -i 'Size:' | grep -v 'No Module' | cut -d':' -f2 | xargs";
        public static string GetMemoryPartNumberCommand => "dmidecode -t 17 | grep -i 'Part Number:' | cut -d':' -f2 | xargs";
        public static string GetMemorySerialNumberCommand => "dmidecode -t 17 | grep -i 'Serial Number:' | cut -d':' -f2 | xargs";
        public static string GetSystemTypeCommand => "dmidecode -t system | grep -i 'Family' | cut -d':' -f2 | xargs";
        public static string GetSystemModelCommand => "dmidecode -t system | grep -i 'Product Name' | cut -d':' -f2 | xargs";
        public static string GetSystemServiceTagCommand => "dmidecode -t system | grep -i 'Serial Number' | cut -d':' -f2 | xargs";
        public static string GetSystemManufacturerCommand => "dmidecode -t system | grep -i 'Manufacturer' | cut -d':' -f2 | xargs";
        public static string GetMotherboardProductCommand => "dmidecode -t baseboard | grep -i 'Product Name' | cut -d':' -f2 | xargs";
        public static string GetMotherboardManufacturerCommand => "dmidecode -t baseboard | grep -i 'Manufacturer' | cut -d':' -f2 | xargs";
        public static string GetMotherboardSerialNumberCommand => "dmidecode -t baseboard | grep -i 'Serial Number' | cut -d':' -f2 | xargs";
    }
}