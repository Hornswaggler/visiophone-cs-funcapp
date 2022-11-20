using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Security.Authentication;
using vp;
using vp.services;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs.Host.Bindings;

[assembly: FunctionsStartup(typeof(Startup))]
namespace vp
{
    public class Startup: FunctionsStartup
    {
        IConfiguration Configuration { get; set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {

            var executionContextOptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>().Value;

            var currentDirectory = executionContextOptions.AppDirectory;
            
            var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();


            Configuration = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddConfiguration(configuration) // Add the original function configuration 
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(Configuration);

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(Config.MongoConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            builder.Services.AddSingleton((s) => new MongoClient(settings));
            builder.Services.AddTransient<ISampleService, SampleService>();
            builder.Services.AddTransient<IUserService, UserService>();

            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = Microsoft.Identity.Web.Constants.Bearer;
                sharedOptions.DefaultChallengeScheme = Microsoft.Identity.Web.Constants.Bearer;
            })
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));
        }
    }

}
