using Microsoft.Extensions.Configuration;
using System.Reflection;
using vp;
using vp.utilities;

var cla = Environment.GetCommandLineArgs();

Dictionary<string, string> settings = new Dictionary<string, string>();
foreach(var parameter in cla)
{
    if(parameter.IndexOf("=") > 0)
    {
        var parts = parameter.Split("=");
        settings.Add(parts[0], parts[1]);
    }
}

if (!settings.ContainsKey("SETTINGS") || !settings.ContainsKey("SAMPLE_DATA_PATH"))
{
    var error = "Failed to find configuration file";
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(error);
    Console.ForegroundColor = ConsoleColor.White;

    throw new Exception(error);
}

var config = new ConfigurationBuilder()
    .AddJsonFile(settings["SETTINGS"], optional: true, reloadOnChange: true)
    .Build();

Environment.SetEnvironmentVariable("SAMPLE_DATA_PATH", settings["SAMPLE_DATA_PATH"]);

var appSettingsProvider = config.Providers. ToList()[0];

var dataElement = appSettingsProvider.GetType().GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic);
var dataValue = (Dictionary<string, string>)dataElement.GetValue(appSettingsProvider);
foreach (var item in dataValue)
{
    var key = item.Key;
    if(key.IndexOf(":") > 0)
    {
        var parts = item.Key.Split(":");
        key = parts[1];
    }
    Environment.SetEnvironmentVariable(key, item.Value);
    Console.WriteLine(item);
}

var fred = Config.CosmosConnectionString;

var utilities = new Utilities();


for(int i = 0; i < 100; i++)
{
    await utilities.InitializeEnvironmentData(true);
}

