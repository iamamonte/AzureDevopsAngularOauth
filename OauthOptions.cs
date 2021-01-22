using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevopsAngularOauth
{
    public class OauthOptions
    {
        public const string NodeName = "OauthOptions";
        public const string RefreshTokenClaimType = "AzureRefreshToken";
        public string AppId { get; set; }
        public string CallbackUrl { get; set; }
        public string Scope { get; set; }
        public string AppSecret { get; set; }
        public string ClientSecret{ get; set; }


    }
}
