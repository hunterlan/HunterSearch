using System.Runtime.InteropServices;

namespace HunterSearch.WinAPI.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct FILETIME
{
    public uint dwLowDateTime;
    public uint dwHighDateTime;
}