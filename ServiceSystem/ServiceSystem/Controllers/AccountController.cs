using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using ServiceSystem.Models;
using ServiceSystem.Providers;
using ServiceSystem.Results;
using System.Net.Mail;
using System.Net;
using System.Data.SqlClient;
using System.Net.Http.Formatting;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using System.Security.Principal;


namespace ServiceSystem.Controllers
{

    public static class IDentityExtension
    {
        public static string GetFirstName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FirstName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetLastName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("LastName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetFatherName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FatherName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetOrganisation(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Organisation");
            return (claim != null) ? claim.Value : string.Empty;
        }
    }

    public class LoginTokenResult
    {

        public override string ToString()
        {
            return AccessToken;
        }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }

    }


    [System.Web.Http.Authorize]
    [System.Web.Http.RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }


        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private bool IsEmailConfirmed(string mail)
        {
            bool toReturn = false;

            

            using (SqlConnection con = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT * FROM AspNetUsers WHERE (Email=@mail) AND (EmailConfirmed=1)";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, con);
                da.SelectCommand.Parameters.AddWithValue("@mail", mail);

                try
                {
                    DataSet set = new DataSet();

                    da.Fill(set);

                    if(set.Tables[0].Rows.Count > 0)
                    {
                        toReturn = true;
                    }
                }
                catch
                {
                    toReturn = false;
                }
            }

            return toReturn;
        }

        private class TokenParams
        {
            public string username { get; set; }
            public string password { get; set; }
            public string grant_type { get; set; }
        }

        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage CanLogin(Tuple<string, string> tuple)
        {
            string email = tuple.Item1;
            string password = tuple.Item2;

            string toReturn = "";

            bool isConfirmed = IsEmailConfirmed(email);

            if (isConfirmed)
            {
                return Request.CreateResponse(HttpStatusCode.OK, toReturn);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Email is not confirmed");
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null,
                FatherName = User.Identity.GetFatherName(),
                FirstName = User.Identity.GetFirstName(),
                LastName = User.Identity.GetLastName(),
                Organisation = User.Identity.GetOrganisation()
            };
        }

       


        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> SetNewUserPassword(Tuple<string, string, string> data)
        {
            string request_id = data.Item1;
            string password = data.Item2;

            using (SqlConnection con = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT * FROM ChangePasswordLog WHERE REQUEST_ID=@id; DELETE FROM ChangePasswordLog WHERE REQUEST_ID=@id;";

                SqlCommand cmd = new SqlCommand(cmdString, con);

                cmd.Parameters.AddWithValue("@id", request_id);


                try
                {
                    con.Open();

                    DateTime date;
                    string email = null;

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {

                        if (rdr.Read())
                        {
                            email = rdr["EMAIL"].ToString();
                            date = Convert.ToDateTime(rdr["REQUEST_TIME"].ToString());
                        }
                    }

                    string id = GetUserId(con, email);

                    try
                    {
                        SetNullPassword(id);

                        await SetPassword(new SetPasswordBindingModel { Id = id, NewPassword = data.Item2, ConfirmPassword = data.Item3 });

                        return Request.CreateResponse(HttpStatusCode.OK, "OK");
                    }
                    catch
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error");
                    }
                }
                catch
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error");
                }
            }
        }

        private void SetNullPassword(string id)
        {
            using (SqlConnection con = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "UPDATE AspNetUsers SET PasswordHash=NULL WHERE Id=@id;";

                SqlCommand cmd = new SqlCommand(cmdString, con);

                cmd.Parameters.AddWithValue("@id", id);

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                }
            }
        }

        private string GetUserId(SqlConnection con, string email)
        {
            string toReturn = "";

            string cmdString = "SELECT ID FROM AspNetUsers WHERE Email=@email;";

            SqlCommand cmd = new SqlCommand(cmdString, con);

            cmd.Parameters.AddWithValue("@email", email);

            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    return rdr["ID"].ToString();
                }
            }

            return toReturn;
        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage ChangeUserPassword(string email)
        {
            string guid = Guid.NewGuid().ToString();

            string letterSent = WriteChangePasswordLetter(email, guid);

            if(letterSent == "OK")
            {
                using (SqlConnection con = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
                { 
                    string cmdString = "INSERT INTO ChangePasswordLog VALUES(GETDATE(), @RequestId, @mail);";

                    SqlCommand cmd = new SqlCommand(cmdString, con);

                    cmd.Parameters.AddWithValue("@RequestId", guid);
                    cmd.Parameters.AddWithValue("@mail", email);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();

                        return Request.CreateResponse(HttpStatusCode.OK, "Letter was sent");
                    }
                    catch(Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, letterSent);
            }
        }


        
        private string WriteChangePasswordLetter(string email, string guid)
        {
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUserName = "btsemail1@gmail.com";
            string smtpUserPass = "btsadmin";

            SmtpClient client = new SmtpClient(smtpHost, smtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(smtpUserName, smtpUserPass);
            client.EnableSsl = true;

            string msgFrom = smtpUserName;
            string msgTo = email;
            string msgSubject = "Password Notification";

            string msgBody = "Please follow this link: http://localhost:49332/Service/SetNewPassword?request_id=" + 
                guid + " to change your password";

            MailMessage message = new MailMessage(msgFrom, msgTo, msgSubject, msgBody);

            message.IsBodyHtml = true;

            try
            {
                client.Send(message);

                return "OK";
            }
            catch(Exception ex)
            {
                return ex.Message;

            }
        }

        [HttpGet]
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(model.Id, model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(model.Id, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        public async Task<HttpResponseMessage> PostExternalLogin([FromBody]string toPass)
        {

            ExternalLoginData data = JsonConvert.DeserializeObject<ExternalLoginData>(toPass);
            if (data == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error");
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(data.LoginProvider,
                data.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                
                 ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);

                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = data.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        private string CreateConfirmationToken()
        {
            return Guid.NewGuid().ToString();
        }

        private bool SetEmailConfirmed(SqlConnection con, string confirmationToken)
        {
            bool toReturn = false;

            string cmdString = "UPDATE AspNetUsers SET EmailConfirmed=1 WHERE ConfirmationToken = @token";

            SqlCommand cmd = new SqlCommand(cmdString, con);

            cmd.Parameters.AddWithValue("@token", confirmationToken);

            try
            {
                cmd.ExecuteNonQuery();
                toReturn = true;
            }
            catch
            {
                toReturn = false;
            }
            finally
            {
                con.Close();
            }

            return toReturn;
        }

        private bool ConfirmAccount(string confirmationToken)
        {
            bool toReturn = false;

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT * FROM AspNetUsers WHERE ConfirmationToken=@token;";

                SqlCommand cmd = new SqlCommand(cmdString, connection);
                cmd.Parameters.AddWithValue("@token", confirmationToken);

                try
                {
                    connection.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    if(rdr.Read())
                    {
                        rdr.Close();
                        toReturn = SetEmailConfirmed(connection, confirmationToken);
                    }
                }
                catch
                {
                    toReturn = false;
                }
            }

            return toReturn; 
        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage RegisterConfirmation(string token)
        {
            if (ConfirmAccount(token))
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Email confirmed");
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotModified, "Wrong confirmation credentials");

        }

        private void SendEmailConfirmation(string to, string username, string confirmationToken)
        {
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUserName = "btsemail1@gmail.com";
            string smtpUserPass = "btsadmin";

            SmtpClient client = new SmtpClient(smtpHost, smtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(smtpUserName, smtpUserPass);
            client.EnableSsl = true;

            string msgFrom = smtpUserName;
            string msgTo = to;
            string msgSubject = "Password Notification";

            string msgBody = "Dear " + username + ", <br/><br/>";
            msgBody += "Please follow this link: http://localhost:49332/Service/ConfirmMail?token=" + confirmationToken + " to confirm your account";
            MailMessage message = new MailMessage(msgFrom, msgTo, msgSubject, msgBody);

            message.IsBodyHtml = true;

            client.Send(message);
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                FatherName = model.FatherName,
                Organisation = model.Organisation,
                ConfirmationToken = CreateConfirmationToken(),
                EmailConfirmed = false
            };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            else
            {
                SendEmailConfirmation(user.Email, user.FirstName, user.ConfirmationToken);
            }

            return Ok();
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal()
        {

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = info.Email, Email = info.Email };

            IdentityResult result = await UserManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);

            if (!result.Succeeded)
            {
                return GetErrorResult(result); 
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        public class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
