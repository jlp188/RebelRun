using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API.Results;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.Entity.Infrastructure.Interception;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private AuthRepository _repo = null;
        private CampaignEntities _repo2 = null;
        private string _viid = "";

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        public AccountController()
        {
            _repo = new AuthRepository();
            _repo2 = new CampaignEntities();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await _repo.RegisterUser(userModel);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            string redirectUri = string.Empty;

            if (error != null)
            {
                return BadRequest(Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            var redirectUriValidationResult = ValidateClientAndRedirectUri(this.Request, ref redirectUri);

            if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
            {
                return BadRequest(redirectUriValidationResult);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            if (string.IsNullOrWhiteSpace(_viid))
            {
                _viid = "";
            }
            externalLogin.Viid = _viid;

            var cEmail = externalLogin.pbEmail;
            Campaign emailTest = _repo2.Campaigns.FirstOrDefault(cmpn => cmpn.Email == cEmail);
            if (emailTest != null)
            {
                if (string.IsNullOrWhiteSpace(externalLogin.Viid))
                {
                    if (emailTest.Email.ToString().Trim() != externalLogin.pbEmail.Trim())
                    {
                        return BadRequest("Customer does not match for this email address." +
                            " If your Facebook or Google email does not match the email address" + 
                            " you used to register then you must use the conventional login procedure.");
                    }
                }
                else
                {
                    if (emailTest.ID.ToString().Trim() != _viid.Trim() || emailTest.Email.ToString().Trim() != externalLogin.pbEmail.Trim())
                    {
                        return BadRequest("Customer IDs do not match for this email address." +
                             " If your Facebook or Google email does not match the email address" +
                             " you used to register then you must use the conventional login procedure.");
                    }
                }
            }
            else
            {
                return BadRequest("Account for this email address does not exist." +
                             " If your Facebook or Google email does not match the email address" +
                             " you used to register then you must use the conventional login procedure.");
            }

            /*
             * At this point, we know the email address associated with the social login matches a Campaign record.
             * We need to determine if there is a User record matching that email. If so then we can create a login record
             * to match this social login provider. If not then add User record and social Login record. 
            */

            externalLogin.Viid = emailTest.ID.ToString();

            bool hasRegistered;

            if (!string.IsNullOrWhiteSpace(emailTest.Uid)) // If Uid exists in Campaign record
            {
                IdentityUser user = await _repo.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));

                hasRegistered = user != null;

                if (!hasRegistered)
                {
                    // Is there a user record? If so then if emails are the same then create social Login record
                    IdentityUser user2 = _repo.FindUserByID(emailTest.Uid);
                    if (user2 != null)
                    {
                        if (emailTest.Email.Trim() != user2.Email.Trim())
                        {
                            return BadRequest("Customer ID does not exist.");
                        }
                        else
                        {
                            UserLoginInfo Login = new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey);

                            IdentityResult result = await _repo.AddLoginAsync(user2.Id, Login);
                            if (!result.Succeeded)
                            {
                                return GetErrorResult(result);
                            }
                            user = await _repo.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));
                            hasRegistered = user != null;
                        }
                    }
                }
            }
            else
            {
                //var cvid = Int32.Parse(_viid); // externalLogin.Viid);
                //Campaign success = _repo2.Campaigns.FirstOrDefault(cmpn => cmpn.ID == cvid);
                //if (success != null)
                //{
                //    if (success.Email.Trim() != externalLogin.pbEmail.Trim())
                //    {
                //        return BadRequest("Customer IDs do not match.");
                //    }
                //}
                //else // We have a Campaign record matching provider email but we were passed a null or empty ViID.
                //{
                //    return BadRequest("Customer ID does not exist.");                    
                //}

                //user = new IdentityUser() { UserName = externalLogin.UserName.Replace(" ", "_"), Email = externalLogin.pbEmail }; //Viid };//



                UserModel usrReg = new UserModel() { UserName = externalLogin.UserName.Replace(" ", "_"), Password = externalLogin.Viid, ConfirmPassword = externalLogin.Viid };

                IdentityResult result = await _repo.RegisterUser(usrReg);//CreateAsync(user);
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                IdentityUser user = await _repo.FindUser(usrReg.UserName, usrReg.Password);//.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));
                user.Email = externalLogin.pbEmail;
                user.EmailConfirmed = true;
                result = await _repo.UpdateUser(user);


                //else
                //{
                //Campaign cmpn = new Campaign() { ID = Int32.Parse(externalLogin.Viid), Uid = user.Id, Email = user.Email };

                //var cvid = Int32.Parse(externalLogin.Viid);
                //Campaign success = _repo2.Campaigns.FirstOrDefault(cmpn => cmpn.ID == cvid);
                //success.Uid = user.Id;
                //success.LastLoginDate = DateTime.Now;
                //_repo2.SaveChanges();


                //if (success == null)
                //{
                //    return IHttpActionResult;
                //}
                //}

                //var info = new ExternalLoginInfo()
                //{
                //    DefaultUserName = externalLogin.UserName.Replace(" ", "_"), //.Viid,//
                //    Login = new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey)
                //};

                UserLoginInfo Login = new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey);

                result = await _repo.AddLoginAsync(user.Id, Login);
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                emailTest.Uid = user.Id;
                emailTest.LastLoginDate = DateTime.Now;
                _repo2.SaveChanges();

                var accessTokenResponse = GenerateLocalAccessTokenResponse(user.UserName.Replace(" ", "_"));
                //await ObtainLocalAccessToken(externalLogin.LoginProvider, externalLogin.ExternalAccessToken);
                user = await _repo.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));
                hasRegistered = user != null;
            }
                //}
                //else
                //{
                //    var uid = user.Id;
                //    //var cvid = Int32.Parse(externalLogin.Viid);
                //    Campaign success = _repo2.Campaigns.FirstOrDefault(cmpn => cmpn.Uid == uid);
                //    //success.Uid = user.Id;
                //    success.LastLoginDate = DateTime.Now;
                //    _repo2.SaveChanges();
                //    externalLogin.Viid = success.ID.ToString();
                //};

            redirectUri = string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}&pbEmail={5}",
                                            redirectUri,
                                            externalLogin.ExternalAccessToken,
                                            externalLogin.LoginProvider,
                                            hasRegistered.ToString(),
                                            externalLogin.Viid,
                                            externalLogin.pbEmail); //.UserName);

            return Redirect(redirectUri);

        }

        // POST api/Account/RegisterExternal
        [AllowAnonymous]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //var verifiedAccessToken = await VerifyExternalAccessToken(model.Provider, model.ExternalAccessToken);
            //if (verifiedAccessToken == null)
            //{
            //    return BadRequest("Invalid Provider or External Access Token");
            //}

            var verifiedAccessToken = await GetUserIdwithAccessToken(model.Provider, model.ExternalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            IdentityUser user = await _repo.FindAsync(new UserLoginInfo(model.Provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                return BadRequest("External user is already registered");
            }

            user = new IdentityUser() { UserName = model.UserName };

            IdentityResult result = await _repo.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            var info = new ExternalLoginInfo()
            {
                DefaultUserName = model.UserName,
                Login = new UserLoginInfo(model.Provider, verifiedAccessToken.user_id)
            };

            result = await _repo.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            //generate access token response
            var accessTokenResponse = GenerateLocalAccessTokenResponse(model.UserName);

            return Ok(accessTokenResponse);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ObtainLocalAccessToken")]
        public async Task<IHttpActionResult> ObtainLocalAccessToken(string provider, string externalAccessToken) //RegisterExternalBindingModel model)// 
        {
            //string provider = model.Provider;
            //string externalAccessToken = model.ExternalAccessToken;

            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
            {
                return BadRequest("Provider or external access token is not sent");
            }

            var verifiedAccessToken = await VerifyExternalAccessToken(provider, externalAccessToken); //GetUserIdwithAccessToken(provider, externalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            IdentityUser user = await _repo.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            if (!hasRegistered)
            {
                return BadRequest("External user is not registered");
            }

            //generate access token response
            var accessTokenResponse = GenerateLocalAccessTokenResponse(user.UserName);

            return Ok(accessTokenResponse);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
                _repo2.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers

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
                        ModelState.AddModelError("", error);
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

        private string ValidateClientAndRedirectUri(HttpRequestMessage request, ref string redirectUriOutput)
        {

            Uri redirectUri;

            var redirectUriString = GetQueryString(Request, "redirect_uri");

            if (string.IsNullOrWhiteSpace(redirectUriString))
            {
                return "redirect_uri is required";
            }

            bool validUri = Uri.TryCreate(redirectUriString, UriKind.Absolute, out redirectUri);

            if (!validUri)
            {
                return "redirect_uri is invalid";
            }

            var clientId = GetQueryString(Request, "client_id");

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return "client_Id is required";
            }

            var client = _repo.FindClient(clientId);

            if (client == null)
            {
                return string.Format("Client_id '{0}' is not registered in the system.", clientId);
            }

            if (!string.Equals(client.AllowedOrigin, redirectUri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase))
            {
                return string.Format("The given URL is not allowed by Client_id '{0}' configuration.", clientId);
            }

            redirectUriOutput = redirectUri.AbsoluteUri;

            _viid = GetQueryString(Request, "vi_id");

            if (string.IsNullOrWhiteSpace(_viid) || _viid == "undefined")
            {
                _viid = "";
            }

            return string.Empty;

        }

        private string GetQueryString(HttpRequestMessage request, string key)
        {
            var queryStrings = request.GetQueryNameValuePairs();

            if (queryStrings == null) return null;

            var match = queryStrings.FirstOrDefault(keyValue => string.Compare(keyValue.Key, key, true) == 0);

            if (string.IsNullOrEmpty(match.Value)) return null;

            return match.Value;
        }

        private async Task<ParsedExternalAccessToken> GetUserIdwithAccessToken(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            var client = new HttpClient();

            // Add a new Request Message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.pushbullet.com/v2/users/me");

            // Add our custom headers
            requestMessage.Headers.Add("Access-Token", accessToken);
            //requestMessage.Headers.Add("Content-Type", "application/json");

            // Send the request to the server
            HttpResponseMessage response = await client.SendAsync(requestMessage);

            //// Just as an example I'm turning the response into a string here
            //string responseAsString = await response.Content.ReadAsStringAsync();
            //var uri = new Uri("https://api.pushbullet.com/v2/users/me");//verifyTokenEndPoint);
            //client.DefaultRequestHeaders.Add("Accept", "application/json");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            //client.DefaultRequestHeaders.Add("Access-Token", accessToken);
            //var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                //JObject jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                parsedToken.user_name = jObj.name;
                parsedToken.user_id = jObj.iden;

            }

            return parsedToken;
        }


        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken) //RegisterExternalBindingModel model)//
        {
            ParsedExternalAccessToken parsedToken = null;

            var verifyTokenEndPoint = "";

            if (provider == "Facebook")
            {
                //You can get it from here: https://developers.facebook.com/tools/accesstoken/
                //More about debug_tokn here: http://stackoverflow.com/questions/16641083/how-does-one-get-the-app-access-token-for-debug-token-inspection-on-facebook
                var appToken = "685513344958073|hv5pu5vzdTzLrbYpC3YEk2gkkS8";//"xxxxxx"; //
                verifyTokenEndPoint = string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}", accessToken, appToken);
            }
            else if (provider == "Google")
            {
                verifyTokenEndPoint = string.Format("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}", accessToken);
            }
            //else if (provider == "Pushbullet")
            //{
            //    parsedToken = new ParsedExternalAccessToken();
            //    parsedToken.user_id = model.Viid; //.UserName;
            //    parsedToken.app_id = Startup.pushbulletAuthOptions.ClientId;

            //    return parsedToken;
            //    //verifyTokenEndPoint = string.Format("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}", accessToken);
            //}
            else
            {
                return null;
            }

            var client = new HttpClient();
            var uri = new Uri(verifyTokenEndPoint); //"https://api.pushbullet.com/v2/users/me");//
            //client.DefaultRequestHeaders.Add("Accept", "application/json");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            //client.DefaultRequestHeaders.Add("Access-Token", accessToken);
            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                if (provider == "Facebook")
                {
                    parsedToken.user_id = jObj["data"]["user_id"];
                    parsedToken.app_id = jObj["data"]["app_id"];

                    if (!string.Equals(Startup.facebookAuthOptions.AppId, parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else if (provider == "Google")
                {
                    parsedToken.user_id = jObj["user_id"];
                    parsedToken.app_id = jObj["audience"];

                    if (!string.Equals(Startup.googleAuthOptions.ClientId, parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                }

            }

            return parsedToken;
        }

        private JObject GenerateLocalAccessTokenResponse(string userName)
        {

            var tokenExpiration = TimeSpan.FromDays(1);

            ClaimsIdentity identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);

            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
            identity.AddClaim(new Claim("role", "user"));

            var props = new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
            };

            var ticket = new AuthenticationTicket(identity, props);

            var accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);

            JObject tokenResponse = new JObject(
                                        new JProperty("userName", userName),
                                        new JProperty("access_token", accessToken),
                                        new JProperty("token_type", "bearer"),
                                        new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString()),
                                        new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
                                        new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString())
        );

            return tokenResponse;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }
            public string ExternalAccessToken { get; set; }
            public string Viid { get; set; }
            public string Uid { get; set; }
            public string pbEmail { get; set; }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer) || String.IsNullOrEmpty(providerKeyClaim.Value))
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
                    UserName = identity.FindFirstValue(ClaimTypes.Name), //"viid"), //
                    ExternalAccessToken = identity.FindFirstValue("ExternalAccessToken"),
                    Viid = identity.FindFirstValue("viid"),
                    //Uid =
                    pbEmail = identity.FindFirstValue(ClaimTypes.Email)
                };
            }
        }

        #endregion
    }
}
