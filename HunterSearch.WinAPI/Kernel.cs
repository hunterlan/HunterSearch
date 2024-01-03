using System.Runtime.InteropServices;
using HunterSearch.WinAPI.Structures;

namespace HunterSearch.WinAPI;

/*
 * Only God and MSDN knows how it works, WinAPI is cursed thing 💀
 */
public class Kernel
{
    [DllImport("kernel32.dll")]
    public static extern bool GetSystemTimes(
        out FILETIME lpIdleTime,
        out FILETIME lpKernelTime,
        out FILETIME lpUserTime
    );
    
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FileTimeToSystemTime(
        ref FILETIME lpFileTime,
        out SYSTEMTIME lpSystemTime
    );
    
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
}