using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PowerBiRealTime
{
    public class AzureAdToken : TokenServiceBase
    {
        public AzureAdToken(TokenServiceConfiguration config) : base(config) { }

        public override string Get()
        {
            return GetAuthenticationResult().AccessToken;
        }

        public AuthenticationResult GetAuthenticationResult()
        {
            var ctx = new AuthenticationContext(Config.Authority, Config.TokenCache);
            try
            {
                if (LocalResult == null)
                {
                    LocalResult = ctx.AcquireTokenAsync(Config.ResourceId, Config.ClientId, Config.RedirectUri, new PlatformParameters(PromptBehavior.Auto)).Result;
                }
                else
                {
                    LocalResult = ctx.AcquireTokenSilentAsync(Config.Resource, Config.ClientId).Result;
                }
                return LocalResult;
            }
            catch (AdalException e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
    }
}