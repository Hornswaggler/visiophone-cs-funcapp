using vp;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Security.Authentication;
using vp.Services;

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

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl("mongodb://visiophone-mongo:bbIaen7eZHDpClIx8uZLhZMOubGgdwbmZPx6UIgqopyAz19G969Gzm16IeWa0ta7ymp5hO02QRLBM4mvoHLWnQ==@visiophone-mongo.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@visiophone-mongo@"));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            builder.Services.AddSingleton((s) => new MongoClient(settings));
            builder.Services.AddTransient<ISampleService, SampleService>();
        }
    }

}
