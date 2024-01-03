using System.Runtime.InteropServices;
using HunterSearch.WinAPI;
using HunterSearch.WinAPI.Structures;

namespace HunterSearch.Console;

internal class DeviceStatistics : IDeviceStatistics
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
        FILETIME idleTime, kernelTime, userTime;
        Kernel.GetSystemTimes(out idleTime, out kernelTime, out userTime);

        SYSTEMTIME idleSystemTime, kernelSystemTime, userSystemTime;
        Kernel.FileTimeToSystemTime(ref idleTime, out idleSystemTime);
        Kernel.FileTimeToSystemTime(ref kernelTime, out kernelSystemTime);
        Kernel.FileTimeToSystemTime(ref userTime, out userSystemTime);

        long idleTicks = (((long)idleTime.dwHighDateTime) << 32) | idleTime.dwLowDateTime;
        long kernelTicks = (((long)kernelTime.dwHighDateTime) << 32) | kernelTime.dwLowDateTime;
        long userTicks = (((long)userTime.dwHighDateTime) << 32) | userTime.dwLowDateTime;

        var idlePercent = (double)idleTicks / (kernelTicks + userTicks) * 100;

        return Math.Round(100 - idlePercent, 2);
    }
}