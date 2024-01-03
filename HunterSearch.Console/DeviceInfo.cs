namespace HunterSearch.Console;

public class DeviceInfo(long currentTimestamp, string ipAddress, double cpuUsage, double ramUsage)
{
    public long CurrentTimestamp { get; set; } = currentTimestamp;
    public string IpAddress { get; set; } = ipAddress;
    public double CpuUsage { get; set; } = cpuUsage;
    public double RamUsage { get; set; } = ramUsage;

    public override string ToString()
    {
        return $"{CurrentTimestamp}\t{IpAddress}\t{CpuUsage}\t{RamUsage}";
    }
}