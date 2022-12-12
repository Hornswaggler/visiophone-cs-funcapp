using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace vp
{
    public class VPTokenValidationParamters
    {
        public static TokenValidationParameters tokenValidationParameters;
        
        static VPTokenValidationParamters()
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(Config.AuthOpenidConnectConfig, new OpenIdConnectConfigurationRetriever());
            var openidconfig = configManager.GetConfigurationAsync().Result;

            tokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = Config.AuthValidAudience,

                ValidateIssuer = true,
                ValidIssuers = new[] { Config.AuthValidIssuer },

                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openidconfig.SigningKeys,

                RequireExpirationTime = true,
                ValidateLifetime = true,
                RequireSignedTokens = true,
            };
        }
    }
}
