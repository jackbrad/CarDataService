using System;
using System.IO;
using System.Net;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;

namespace PowerBiRealTime
{
    public class AzureAdHeadlessToken : TokenServiceBase
    {
        public AzureAdHeadlessToken(TokenServiceConfiguration config) : base(config) { }

        public override string Get()
        {
            if (string.IsNullOrEmpty(LocalToken))
            {
                //LocalToken = GetTokenManually();
                LocalToken = GetToken().AccessToken;
            }
            return LocalToken;
        }

        private AuthenticationResult GetToken()
        {
            var ctx = new AuthenticationContext(Config.Authority, Config.TokenCache);
            try
            {
                if (LocalResult == null)
                {
                    LocalResult = ctx.AcquireTokenAsync(Config.Resource, Config.ClientId, new UserPasswordCredential(Config.User, Config.Password)).Result;
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

        private string GetTokenManually()
        {
            var reqUri = Config.Authority;
            var postData = $"resource={Config.ResourceId}&client_id={Config.ClientId}&grant_type=password&username={Config.User}&password={Config.Password.Encode()}&scope=openid&client_secret={Config.ClientSecret.Encode()}";

            var wc = new WebClient();
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            try
            {
                var response = wc.UploadString(reqUri, "POST", postData);
                var tokenData = JObject.Parse(response);
                var ats = tokenData["access_token"].Value<string>();
                return ats;
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
            return string.Empty;
        }
    }
}