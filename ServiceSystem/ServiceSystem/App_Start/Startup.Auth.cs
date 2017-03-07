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
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

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

            app.UseFacebookAuthentication(
                appId: "345453675848203",
                appSecret: "f08f2116a756a511d419441ba3fe8e99"
                );

            var facebookOptions = new FacebookAuthenticationOptions()
            {
                AppId = "345453675848203",
                AppSecret = "f08f2116a756a511d419441ba3fe8e99",
                BackchannelHttpHandler = new Facebook.FacebookBackChannelHandler(),
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ExternalCookie,
                Scope = { "email" },
                UserInformationEndpoint = "https://graph.facebook.com/v2.4/me?fields=id,email,name"
                //Provider = new FacebookAuthenticationProvider()
                //{
                //    OnAuthenticated = (context) =>
                //    {       
                //        context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:email", context.Email));
                //        return Task.FromResult(0);
                //    }
                //}
        };

            //facebookOptions.Scope.Add("email");
            app.UseFacebookAuthentication(facebookOptions);

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
    }
}
