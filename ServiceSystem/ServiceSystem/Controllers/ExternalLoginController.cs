using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ServiceSystem.Models;
using Facebook;
using System.Runtime.Caching;
using System.Net.Http;
using static ServiceSystem.Controllers.AccountController;
using Newtonsoft.Json;
using System.Net.Http.Formatting;

namespace ServiceSystem.Controllers
{
    public class ExternalLoginController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ExternalLoginController()
        {
        }

        public ExternalLoginController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        public ActionResult ExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }
            // Request a redirect to the external login provider
            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "ExternalLogin", null));
            }
            else
            {
                return RedirectToAction("Index", "Service", null);
            }
        }


        //public async Task<ActionResult> ExternalLoginCallback()
        //{
        //    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
        //    if (loginInfo == null)
        //    {
        //        return RedirectToAction("Index", "Service", null);
        //    }

        //    loginInfo.DefaultUserName = loginInfo.Email;

        //    var identity = AuthenticationManager.GetExternalIdentity(DefaultAuthenticationTypes.ExternalCookie);
        //    var access_token = identity.FindFirstValue("FacebookAccessToken");

        //    ExternalLoginData toPass = new ExternalLoginData()
        //    {
        //        LoginProvider = loginInfo.Login.LoginProvider,
        //        ProviderKey = loginInfo.Login.ProviderKey,
        //        UserName = loginInfo.Email
        //    };

        //    using (HttpClient client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //        client.BaseAddress = new Uri("http://localhost:49332/");
        //        HttpResponseMessage mes = client.PostAsJsonAsync("api/Account/PostExternalLogin", JsonConvert.SerializeObject(toPass)).Result;
        //    }

        //        return RedirectToAction("Main", "Service", null);
        //}

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index", "Service", null);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    return RedirectToAction("ExternalLoginConfirmation");
            }
        }

        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            if(User.Identity.IsAuthenticated)
            {

                ObjectCache cache = MemoryCache.Default;
                
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.BaseAddress = new Uri("http://localhost:49332");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cache["access_token"] as string);

                    HttpResponseMessage mes = client.GetAsync("/api/Account/Logout").Result;
                }
            }
            return RedirectToAction("Index", "Service", null);
        }

        

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginConfirmation()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();

                var user = new ApplicationUser
                {
                     Email = info.Email,
                     FirstName = info.DefaultUserName,
                     LastName = "",
                     FatherName = "",
                     EmailConfirmed = true,
                     Organisation = "",
                     UserName = info.Email
                };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Index", "Service", null);
                    }
                }
            }

            return RedirectToAction("Index", "Service", null);
        }

        public async Task<ActionResult> LoginConfirmation(string name, string password)
        {
            await SignInManager.PasswordSignInAsync(name, password, isPersistent: false, shouldLockout:false);

            return RedirectToAction("Index", "Service", null);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}