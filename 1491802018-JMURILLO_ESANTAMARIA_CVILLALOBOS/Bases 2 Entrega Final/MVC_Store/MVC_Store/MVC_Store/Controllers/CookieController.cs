using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class CookieController : Controller
    {
        // GET: Cookie
        public ActionResult Create(string user, string status)
        {
            HttpCookie cookie = new HttpCookie("UserSettings");
            cookie["User"] = user;
            cookie["Status"] = status;
            Response.Cookies.Add(cookie);
            return RedirectToAction("Index", "User");
        }
            

        public ActionResult Delete()
        {
            HttpCookie cookie = new HttpCookie("UserSettings");
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
            return RedirectToAction("Index", "Home");
        }
          
        
    }
}