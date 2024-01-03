using Elastic.Clients.Elasticsearch;
using System.Runtime.InteropServices;

[DllImport("kernel32.dll")]
static extern bool GetSystemTimes(
    out FILETIME lpIdleTime,
    out FILETIME lpKernelTime,
    out FILETIME lpUserTime
);

[DllImport("kernel32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool FileTimeToSystemTime(
    ref FILETIME lpFileTime,
    out SYSTEMTIME lpSystemTime
);

[DllImport("kernel32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

const string hostUrl = "http://localhost:9200";
const string indexName = "s28619_performance_data";

var settings = new ElasticsearchClientSettings(new Uri(hostUrl));
var client = new ElasticsearchClient(settings);

var pingOperation = client.PingAsync();
var dotCounter = 0;

while (!pingOperation.IsCompleted)
{
    switch (dotCounter)
    {
        case 0:
            Console.Write("Trying to connect to " + hostUrl);
            break;
        case 4:
            Console.Clear();
            dotCounter = -1;
            break;
        default:
            Console.Write(".");
            Thread.Sleep(500);
            break;
    }

    dotCounter++;
}
Console.Write("\n");

var pingResponse = pingOperation.Result;

if (pingResponse.IsSuccess())
{
    Console.WriteLine("Connection is established.");
}
else
{
    Console.WriteLine("Failed to connect. Continuous execution isn't possible.");
    Environment.Exit(0);
}

Console.WriteLine();

var creationOperation = await client.Indices.CreateAsync(indexName);

if (creationOperation.IsSuccess())
{
    Console.WriteLine("Index created successfully");
}
else
{
    Console.WriteLine("Failed to create new index. Continuous execution isn't possible.");
    Environment.Exit(0);
}

var countMinutes = 0;
while (countMinutes < 15)
{
    var dateTime = DateTime.Now;
    var timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    var ip = "0.0.0.0";
    var cpuUsage = GetCpuUsage();
    var ramUsage = GetRamUsage();
    Console.WriteLine($"CPU usage on {countMinutes + 1} minute is {cpuUsage}%");
    Console.WriteLine($"RAM usage on {countMinutes + 1} minute is {ramUsage} GB");
    Console.WriteLine("");
    var currentDeviceInfo = new DeviceInfo(timestamp, ip, cpuUsage, ramUsage);
    var sendResponse = await client.IndexAsync(currentDeviceInfo, indexName);
    if (sendResponse.IsSuccess())
    {
        Console.WriteLine("This data was successfully sent.");
    }
    else
    {
        Console.WriteLine("Error occured during sending this data.");
    }

    Thread.Sleep(60000);
    countMinutes++;
}

return;
/*
 * Only God and MSDN knows how it works, WinAPI is cursed thing 💀
 */
double GetRamUsage()
{
    var memoryStatus = new MEMORYSTATUSEX();
    memoryStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

    var result = GlobalMemoryStatusEx(ref memoryStatus);

    if (result == false)
    {
        throw new Exception("Failed to get memory information");
    }

    var totalMemoryGb = (double)memoryStatus.ullTotalPhys / (1024 * 1024 * 1024);
    var availableMemoryGb = (double)memoryStatus.ullAvailPhys / (1024 * 1024 * 1024);

    return Math.Round(totalMemoryGb - availableMemoryGb, 2);
}

double GetCpuUsage()
{
    FILETIME idleTime, kernelTime, userTime;
    GetSystemTimes(out idleTime, out kernelTime, out userTime);

    SYSTEMTIME idleSystemTime, kernelSystemTime, userSystemTime;
    FileTimeToSystemTime(ref idleTime, out idleSystemTime);
    FileTimeToSystemTime(ref kernelTime, out kernelSystemTime);
    FileTimeToSystemTime(ref userTime, out userSystemTime);

    long idleTicks = (((long)idleTime.dwHighDateTime) << 32) | idleTime.dwLowDateTime;
    long kernelTicks = (((long)kernelTime.dwHighDateTime) << 32) | kernelTime.dwLowDateTime;
    long userTicks = (((long)userTime.dwHighDateTime) << 32) | userTime.dwLowDateTime;

    var idlePercent = (double)idleTicks / (kernelTicks + userTicks) * 100;

    return Math.Round(100 - idlePercent, 2);
}

internal class DeviceInfo(long currentTimestamp, string ipAddress, double cpuUsage, double ramUsage)
{
    public long CurrentTimestamp { get; set; } = currentTimestamp;
    public string IpAddress { get; set; } = ipAddress;
    public double CpuUsage { get; set; } = cpuUsage;
    public double RamUsage { get; set; } = ramUsage;
}

[StructLayout(LayoutKind.Sequential)]
struct SYSTEMTIME
{
    public ushort wYear;
    public ushort wMonth;
    public ushort wDayOfWeek;
    public ushort wDay;
    public ushort wHour;
    public ushort wMinute;
    public ushort wSecond;
    public ushort wMilliseconds;
}

[StructLayout(LayoutKind.Sequential)]
struct FILETIME
{
    public uint dwLowDateTime;
    public uint dwHighDateTime;
}

[StructLayout(LayoutKind.Sequential)]
struct MEMORYSTATUSEX
{
    public uint dwLength;
    public uint dwMemoryLoad;
    public ulong ullTotalPhys;
    public ulong ullAvailPhys;
    public ulong ullTotalPageFile;
    public ulong ullAvailPageFile;
    public ulong ullTotalVirtual;
    public ulong ullAvailVirtual;
    public ulong ullAvailExtendedVirtual;
}