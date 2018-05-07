using System;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.IdentityModel.Claims;
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
            //todo
        }

        [Authorize]
        [HttpGet]
        public ActionResult UserDetails()
        {
            //todo
            return View();
        }

        public ActionResult SignOut()
        {
            //todo

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