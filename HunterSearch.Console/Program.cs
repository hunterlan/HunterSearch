using System.Diagnostics;
using Elastic.Clients.Elasticsearch;
using HunterSearch.Console;
using HunterSearch.Console.Factories;

const string hostUrl = "http://localhost:9200";
const string indexName = "<index_number>_performance_data";
var deviceStatistics = new WindowsStatisticsFactory().CreateDeviceStatistics();

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
    Console.WriteLine($"Reason: {pingResponse.ElasticsearchServerError!.Error.Reason}");
    Environment.Exit(0);
}

Console.WriteLine();

var creationOperation = await client.Indices.CreateAsync(indexName);

if (creationOperation.IsSuccess())
{
    Console.WriteLine("Index created successfully");
}
else if (creationOperation.ElasticsearchServerError!.Error.Type is "resource_already_exists_exception")
{
    Console.WriteLine("Index is already created. Skip the operation.");
}
else
{
    Console.WriteLine("Failed to create new index. Continuous execution isn't possible.");
    Console.WriteLine($"Reason: {creationOperation.ElasticsearchServerError!.Error.Reason}");
    Environment.Exit(0);
}

var countMinutes = 0;
while (countMinutes < 15)
{
    var dateTime = DateTime.Now;
    var timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    var ip = "0.0.0.0";
    var cpuUsage = deviceStatistics.GetCpuUsage();
    var ramUsage = deviceStatistics.GetRamUsage();
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
        Console.WriteLine($"Reason: {creationOperation.ElasticsearchServerError!.Error.Reason}");
    }

    Thread.Sleep(60000);
    countMinutes++;
}

Console.WriteLine("Trying to get all device info logs");
var searchResponse = await client.SearchAsync<DeviceInfo>(s => 
    s.Index(indexName).From(0).Size(20));
var logs = new List<DeviceInfo>();

if (searchResponse.IsSuccess())
{
    Console.WriteLine("Data was retrieved successfully.");
    logs = searchResponse.Documents.ToList();
}
else
{
    Console.WriteLine("There was an error during retrieving data. Continuous execution isn't possible.");
    Console.WriteLine($"Reason: {creationOperation.ElasticsearchServerError!.Error.Reason}");
    Environment.Exit(0);
}

await using(TextWriter tw = new StreamWriter(indexName + ".txt", false))
{
    foreach (var log in logs)
    {
        tw.WriteLine(log.ToString());
    }
}