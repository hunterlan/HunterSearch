using HunterSearch.Console.StatisticImplementations;

namespace HunterSearch.Console.Factories;

public class WindowsStatisticsFactory : IDeviceStatisticsFactory
{
    public IDeviceStatistics CreateDeviceStatistics()
    {
        return new WindowsDeviceStatistics();
    }
}