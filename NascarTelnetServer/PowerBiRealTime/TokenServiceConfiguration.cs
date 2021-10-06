using System;
using System.Security;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PowerBiRealTime
{
    public class TokenServiceConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public string Authority { get; set; }
        public Uri RedirectUri { get; set; }
        public TokenCache TokenCache { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}