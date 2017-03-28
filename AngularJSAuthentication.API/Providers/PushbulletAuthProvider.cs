using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security.Pushbullet;

namespace AngularJSAuthentication.API.Providers
{
    public class PushbulletAuthProvider : PushbulletAuthenticationProvider
    {
        public override Task Authenticated(PushbulletAuthenticatedContext context)
        {
            context.Identity.AddClaim(new Claim("ExternalAccessToken", context.AccessToken));
            //context.Identity.AddClaim(new Claim("viid", context.Properties.Dictionary["viid"].ToString()));//"Viid", "54321"));
            return Task.FromResult<object>(null);
        }
    }
}
