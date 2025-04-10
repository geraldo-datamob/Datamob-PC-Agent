public class TelemetryData
{
    public BiosInfo Bios { get; set; }
    public CpuInfo Cpu { get; set; }
    public DiskInfo Disk { get; set; }
    public GpuInfo Gpu { get; set; }
    public MemoryInfo[] Memory { get; set; }
    public MotherboardInfo Motherboard { get; set; }
    public SystemInfo System { get; set; }
}