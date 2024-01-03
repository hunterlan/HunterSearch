using System.Runtime.InteropServices;

namespace HunterSearch.WinAPI.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEMTIME
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