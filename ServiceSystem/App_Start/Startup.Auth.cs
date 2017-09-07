using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Owin;
using ServiceSystem.Providers;
using ServiceSystem.Models;
using Microsoft.Owin.Security.Facebook;
using ServiceSystem.Facebook;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ServiceSystem
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true
            };

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "671006846403592",
            //    appSecret: "7f50b5cf42903a76670884d579ec396e");

            var facebookOptions = new FacebookAuthenticationOptions()
            {
                AppId = "1105617286210100",
                AppSecret = "fcadc466d472b4a81e6a396170405e43",
                BackchannelHttpHandler = new FacebookBackChannelHandler(),
                Provider = new Microsoft.Owin.Security.Facebook.FacebookAuthenticationProvider()
                {
                    OnAuthenticated = (context) =>
                    {
                        context.Identity.AddClaim(new Claim("FacebookAccessToken", context.AccessToken));
                        return Task.FromResult(0);
                    }
                },
                UserInformationEndpoint = "https://graph.facebook.com/v2.8/me?fields=id,email"
            };

            facebookOptions.Scope.Add("email");
            app.UseFacebookAuthentication(facebookOptions);

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "676553562771-bcr0vujgvkhgv36dcrf59v8ii55k6q1a.apps.googleusercontent.com",
                ClientSecret = "S7pK7bb0E_HPhcgETMZsG-VI"
            });
        }
    }
}
