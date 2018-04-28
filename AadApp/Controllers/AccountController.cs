using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AadApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string email)
        {
            //do stuff
            return Redirect("/");
        }

        [HttpGet]
        public ActionResult UserDetails()
        {
            return View();
        }


    }
}