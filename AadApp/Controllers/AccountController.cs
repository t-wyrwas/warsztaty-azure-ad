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
    }
}