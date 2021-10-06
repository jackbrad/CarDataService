using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PowerBiRealTime
{
    public interface ITokenService
    {
        string Get();
    }
}