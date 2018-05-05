using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.IdentityModel.Claims;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using AadApp.ViewModels;

namespace AadApp.Controllers
{
    public class AccountController : Controller
    {
        public void Login()
        {
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" }, 
                ConfigurationManager.AppSettings["ida:TenantId"]);
        }

        [Authorize]
        [HttpGet]
        public ActionResult UserDetails()
        {
            var user = HttpContext.GetOwinContext().Authentication.User;

            return View(new UserDetails
            {
                UserName = user.FindFirst(ClaimTypes.Name)?.Value,
                FirstName = user.FindFirst(ClaimTypes.GivenName)?.Value,
                LastName = user.FindFirst(ClaimTypes.Surname)?.Value,
            });
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
    }
}