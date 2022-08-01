using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Security.Authentication;
using vp;
using vp.services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace vp
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(Config.MongoConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            builder.Services.AddSingleton((s) => new MongoClient(settings));
            builder.Services.AddTransient<ISampleService, SampleService>();
        }
    }

}
