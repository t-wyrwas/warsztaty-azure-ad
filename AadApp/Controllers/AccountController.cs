using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IdentityModel.Claims;
using System.Linq;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using AadApp.ViewModels;

namespace AadApp.Controllers
{
    public class AccountController : Controller
    {
        private string graphResourceID = "https://graph.windows.net"; // AAD Graph, nie Microsoft Graph

        public void Login()
        {
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" }, 
                ConfigurationManager.AppSettings["ida:TenantId"]);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> UserDetails()
        {
            var principal = HttpContext.GetOwinContext().Authentication.User;

            string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];

            try
            {
                Uri servicePointUri = new Uri(graphResourceID);
                Uri serviceRoot = new Uri(servicePointUri, tenantId);
                ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot,
                      async () => await GetTokenForApplication());

                var result = await activeDirectoryClient.Users.ExecuteAsync();

                var users = result.CurrentPage.Select(u => new UserDetails
                {
                    UserName = u.UserPrincipalName,
                    FirstName = u.GivenName,
                    LastName = u.Surname
                }).ToList();

                return View(users);
            }
            catch (AdalException e)
            {
                return View("Error");
            }
            catch (Exception e)
            {
                return View("Relogin");
            }


        }

        public ActionResult SignOut()
        {
            string callbackUrl = Url.Action("SignOutCallback", "Account", routeValues: null, protocol: Request.Url.Scheme);

            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationType);

            return Redirect("SignOutCallback");
        }

        public ActionResult SignOutCallback()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public async Task<string> GetTokenForApplication()
        {
            var principal = HttpContext.GetOwinContext().Authentication.User;

            var signedInUserID = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userObjectID = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            var clientId = ConfigurationManager.AppSettings["ida:ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            var aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
            var tenantID = ConfigurationManager.AppSettings["ida:TenantId"];

            // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)
            ClientCredential clientcred = new ClientCredential(clientId, clientSecret);
            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's database
            AuthenticationContext authenticationContext = new AuthenticationContext(aadInstance + tenantID);
            //AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenSilentAsync(graphResourceID, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(graphResourceID, clientcred);
            return authenticationResult.AccessToken;
        }
    }
}