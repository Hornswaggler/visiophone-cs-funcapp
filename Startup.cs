using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Authentication;
using vp;
using vp.services;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Azure.Functions.Identity.Web.Extensions;
using vp.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace vp
{
    public class Startup : FunctionsStartup
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
                .AddConfiguration(configuration)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(Configuration);
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(Config.MongoConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            builder.Services.AddSingleton((s) => new MongoClient(settings));
            builder.Services.AddTransient<ISampleService, SampleService>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IStripeService, StripeService>();
            builder.Services.AddTransient<ICheckoutSessionService, CheckoutSessionService>();

            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddArmToken()
                .AddScriptAuthLevel()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ =>
                {
                    _.TokenValidationParameters = VPTokenValidationParamters.tokenValidationParameters;
                    _.RequireHttpsMetadata = false;
                });

            services
                .AddAuthorization(options => options.AddScriptPolicies());

            services
                .AddAuthLevelAuthorizationHandler()
                .AddNamedAuthLevelAuthorizationHandler()
                .AddFunctionAuthorizationHandler();
        }
    }

}
