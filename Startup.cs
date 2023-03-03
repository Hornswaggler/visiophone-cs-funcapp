using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using vp;
using vp.services;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Azure.Functions.Identity.Web.Extensions;
using Microsoft.Azure.Cosmos;

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
            builder.Services.AddSingleton<ISamplePackService, SamplePackService>();
            builder.Services.AddSingleton<IPurchaseService, PurchaseService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IStripeService, StripeService>();
            builder.Services.AddSingleton<IValidationService, ValidationService>();
            builder.Services.AddSingleton<IStorageService, StorageService>();

            CosmosClient client = new(
                connectionString: Config.CosmosConnectionString
            );

            builder.Services.AddSingleton(s => client);

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
