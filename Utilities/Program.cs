using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using vp.utilities;

class Program
{
    public static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        ILogger log = loggerFactory.CreateLogger<Program>();

        Dictionary<string, string> settings = new Dictionary<string, string>();
        foreach (var parameter in args)
        {
            if (parameter.IndexOf("=") > 0)
            {
                var parts = parameter.Split("=");
                if(parts.Length == 2)
                {
                    settings.Add(parts[0], parts[1]);
                }
            } else
            {
                settings.Add(parameter, "true");
            }
        }


        if (!settings.ContainsKey("SETTINGS") || !settings.ContainsKey("SAMPLE_DATA_PATH"))
        {
            var error = new Exception("Failed to find configuration file");
            log.LogError(error.Message);
            throw error;
        }



        var config = new ConfigurationBuilder()
            .AddJsonFile(settings["SETTINGS"], optional: true, reloadOnChange: true)
            .Build();

        Environment.SetEnvironmentVariable("SAMPLE_DATA_PATH", settings["SAMPLE_DATA_PATH"]);

        var appSettingsProvider = config.Providers.ToList()[0];

        var dataElement = appSettingsProvider.GetType().GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic);
        var dataValue = (Dictionary<string, string>)dataElement.GetValue(appSettingsProvider);
        foreach (var item in dataValue)
        {
            var key = item.Key;
            if (key.IndexOf(":") > 0)
            {
                var parts = item.Key.Split(":");
                key = parts[1];
            }
            Environment.SetEnvironmentVariable(key, item.Value);
        }

        var utilities = new Utilities(log);

        if (settings.ContainsKey("delete") || settings.ContainsKey("reset"))
        {
            log.LogInformation("Deleting existing Data");
            await utilities.DeleteDatabase();
            await utilities.DeleteStorage();
        }

        if(settings.ContainsKey("create") || settings.ContainsKey("reset"))
        {
            log.LogInformation("Initializing Storage and Database");
            await utilities.InitializeStorage();
            await utilities.InitializeDatabaseSchema();
        }

        if(settings.ContainsKey("seed") || settings.ContainsKey("reset"))
        {
            log.LogInformation("Seeding data");
            await utilities.InitializeEnvironmentData(true);
        }
    }
}




