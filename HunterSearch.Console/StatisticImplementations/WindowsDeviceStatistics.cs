using System.Diagnostics;
using System.Runtime.InteropServices;
using HunterSearch.WinAPI;
using HunterSearch.WinAPI.Structures;

namespace HunterSearch.Console.StatisticImplementations;

internal class WindowsDeviceStatistics : IDeviceStatistics
{
    public double GetRamUsage()
    {
        var memoryStatus = new MEMORYSTATUSEX();
        memoryStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

        var result = Kernel.GlobalMemoryStatusEx(ref memoryStatus);

        if (result == false)
        {
            throw new Exception("Failed to get memory information");
        }

        var totalMemoryGb = (double)memoryStatus.ullTotalPhys / (1024 * 1024 * 1024);
        var availableMemoryGb = (double)memoryStatus.ullAvailPhys / (1024 * 1024 * 1024);

        return Math.Round(totalMemoryGb - availableMemoryGb, 2);
    }

    public double GetCpuUsage()
    {
        var idlePercent = new PerformanceCounter("Processor", "% Idle Time", "_Total");

        return Math.Round(100 - idlePercent.NextValue(), 2);
    }
}