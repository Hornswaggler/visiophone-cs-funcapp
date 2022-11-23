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
//using Azure.Functions.Identity.Web.Extensions;
using Microsoft.IdentityModel.Logging;
using System.Collections.Generic;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
            IdentityModelEventSource.ShowPII = true; //Add this line

            //THIRD TIME A CHARM?

            //IList<string> validissuers = new List<string>()
            //{
            //    Configuration.GetAuthority(),
            //};
           
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>("https://visiophoneb2c.b2clogin.com/tfp/visiophone.wtf/B2C_1_SIGN_IN/v2.0/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());

            var openidconfig = configManager.GetConfigurationAsync().Result;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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


            //SECOND TRY...

            //services.AddAuthentication(sharedOptions =>
            //{
            //    sharedOptions.DefaultScheme = Constants.Bearer;
            //    sharedOptions.DefaultChallengeScheme = Constants.Bearer;
            //})
            //    .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));


            //FIRST TRY:

            //services.AddAuthentication(sharedOptions =>
            //    {
            //        sharedOptions.DefaultScheme = Constants.Bearer;
            //        sharedOptions.DefaultChallengeScheme = Constants.Bearer;
            //    })
            //    .AddArmToken()
            //    .AddScriptAuthLevel()
            //    .AddMicrosoftIdentityWebApi(Configuration)
            //    .EnableTokenAcquisitionToCallDownstreamApi()
            //    .AddInMemoryTokenCaches();

            //services
            //    .AddAuthorization(options => options.AddScriptPolicies());

            //services
            //    .AddAuthLevelAuthorizationHandler()
            //    .AddNamedAuthLevelAuthorizationHandler()
            //    .AddFunctionAuthorizationHandler();
            //}
            //}
        }
    }

}
