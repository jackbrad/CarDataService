using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PowerBiRealTime
{
    public abstract class TokenServiceBase : ITokenService
    {
        protected readonly TokenServiceConfiguration Config;
        protected AuthenticationResult LocalResult;
        protected string LocalToken;

        protected TokenServiceBase(TokenServiceConfiguration config)
        {
            Config = config;
        }

        public abstract string Get();
    }
}