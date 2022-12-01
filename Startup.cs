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
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Azure.Functions.Identity.Web.Extensions;

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
                .AddConfiguration(configuration) // Add the original function configuration 
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(Configuration);

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(Config.MongoConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            builder.Services.AddSingleton((s) => new MongoClient(settings));
            builder.Services.AddTransient<ISampleService, SampleService>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IStripeService, StripeService>();

            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
           
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>("https://visiophoneb2c.b2clogin.com/tfp/visiophone.wtf/B2C_1_SIGN_IN/v2.0/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            var openidconfig = configManager.GetConfigurationAsync().Result;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddArmToken()
                .AddScriptAuthLevel()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ =>
                {
                    _.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidAudience = "0134f7f5-3b4a-4e3f-b8f7-992875ad538f",

                        ValidateIssuer = true,
                        ValidIssuers = new[] { "https://visiophoneb2c.b2clogin.com/tfp/26244285-b320-45e1-946a-99b199d5424e/b2c_1_sign_in/v2.0/" },

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = openidconfig.SigningKeys,

                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        RequireSignedTokens = true,
                    };

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
