using MongoDB.Bson;
using MVC_Store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class UserController : Controller
    {

        Product pd = new Product();
        Cart cart = Models.Cart.getInstance();

        // GET: User
        public ActionResult Register(string email, string first, string last, string password_1, string password_2)
        {
            if (email.Equals("") || first.Equals("") || last.Equals("") || password_1.Equals("") || password_2.Equals("")) {
                System.Windows.Forms.MessageBox.Show("[ ! ] Please fill all the spaces.");
                return View("~/Views/Home/Register.cshtml");
            }
            if (password_1.Equals(password_2))
            {
                bool new_user = new User().createUser(email, first, last, password_1);
                if (new_user == false) {
                    return View("~/Views/Home/Register.cshtml");
                }
                return RedirectToAction("LogIn", "Home");
            }
            System.Windows.Forms.MessageBox.Show("[ ! ] The passwords don't match.");
            return View("~/Views/Home/Register.cshtml");
        }

        public ActionResult LogIn(string email, string password) {
            if (email.Equals("") || password.Equals("")) {
                System.Windows.Forms.MessageBox.Show("[ ! ] Please fill all the spaces.");
                return View("~/Views/Home/LogIn.cshtml");
            }
            bool log = new User().logIn(email, password);
            if (log == false) {
                return View("~/Views/Home/LogIn.cshtml");
            }
            string status = new User().getStatus(email);
            return RedirectToAction("Create", "Cookie", new { user = email, status = status });
        }

        public ActionResult LogOut() {
            cart.Delete<CartProduct>("cart");
            return RedirectToAction("Delete", "Cookie");
        }

        public ActionResult Admin() {
            ViewBag.userLog = true;
            if (Request.Cookies["UserSettings"]["Status"].Equals("Y"))
            {
                ViewBag.userAdmin = true;
            }
            else
            {
                ViewBag.userAdmin = false;
            }
            return View("~/Views/Admin/Index.cshtml");
        }

        public ActionResult Index() {
            try
            {
                if (Request.Cookies["UserSettings"]["Status"].Equals("Y"))
                {
                    ViewBag.userAdmin = true;
                }
            }
            catch (Exception)
            {
                ViewBag.userAdmin = false;
            }
            try
            {
                if (!Request.Cookies["UserSettings"]["User"].Equals(""))
                {
                    ViewBag.userLog = true;
                }
                else
                {
                    ViewBag.userLog = false;
                }
            }
            catch (Exception)
            {
                ViewBag.userLog = false;
            }
            var model = new Product
            {
                products = pd.getProducts(),
                imgs = pd.getImages()
            };
            return View(model);
        }

        public FileContentResult ShowPicture(string id)
        {
            FileContentResult result = pd.getPhoto(id);
            return result;
        }

        public ActionResult UserProfile() {
            User user = new User().getActualUser(Request.Cookies["UserSettings"]["User"]);
            ViewBag.email = user.email;
            ViewBag.first = user.first;
            ViewBag.last = user.last;
            List<Purchase> history = pd.getPurchases(Request.Cookies["UserSettings"]["User"]);
            int l = 0;
            foreach (var i in history) { l++; }
            if (l > 0)
            {
                ViewBag.l = true;
            }
            else {
                ViewBag.l = false;
            }
            ViewBag.history = history;
            List<Product> listProductsA = new List<Product> { };
            foreach (var i in history) {
                
                listProductsA.Add(pd.getSingleProduct(i.productid));
                
            }
            List<Product> listProducts = new List<Product> { };
            foreach (var i in listProductsA) {
                bool repeat = false;
                foreach (var j in listProducts) {
                    if (i.id == j.id) {
                        repeat = true;
                        break;
                    }
                }
                if (repeat == false) {
                    listProducts.Add(i);
                }
                
            }
            ViewBag.listProducts = listProducts;
            return View();
        }

        public ActionResult viewUserHistory(string email) {
            User user = new User().getActualUser(email);
            ViewBag.email = user.email;
            ViewBag.first = user.first;
            ViewBag.last = user.last;
            List<Purchase> history = pd.getPurchases(email);
            int l = 0;
            foreach (var i in history) { l++; }
            if (l > 0)
            {
                ViewBag.l = true;
            }
            else
            {
                ViewBag.l = false;
            }
            ViewBag.history = history;
            List<Product> listProductsA = new List<Product> { };
            foreach (var i in history)
            {

                listProductsA.Add(pd.getSingleProduct(i.productid));

            }
            List<Product> listProducts = new List<Product> { };
            foreach (var i in listProductsA)
            {
                bool repeat = false;
                foreach (var j in listProducts)
                {
                    if (i.id == j.id)
                    {
                        repeat = true;
                        break;
                    }
                }
                if (repeat == false)
                {
                    listProducts.Add(i);
                }

            }
            ViewBag.listProducts = listProducts;
            return View();
        }

        public ActionResult FollowNewUser(string email, string txt) {
            User user = new User();
            user.createUserRelation(Request.Cookies["UserSettings"]["User"], email);
            return RedirectToAction("SearchFunction", "Home", new { search_text = txt });
        }

        public ActionResult UnfollowUser(string email, string txt)
        {
            User user = new Models.User();
            user.deleteRelationUsers(Request.Cookies["UserSettings"]["User"], email);
            return RedirectToAction("SearchFunction", "Home", new { search_text = txt });
        }


        public ActionResult addToCart(string unitsWanted)
        {
            int id = Convert.ToInt32(TempData["Data1"]);
            int u = Convert.ToInt32(unitsWanted);
            CartProduct cp = new CartProduct(id, u);
            cart.set<CartProduct>("cart", cp);
            return RedirectToAction("Index", "User");
        }

        public ActionResult buyItems() {
            try
            {
                if (Request.Cookies["UserSettings"]["Status"].Equals("Y"))
                {
                    ViewBag.userAdmin = true;
                }
            }
            catch (Exception)
            {
                ViewBag.userAdmin = false;
            }
            try
            {
                if (!Request.Cookies["UserSettings"]["User"].Equals(""))
                {
                    ViewBag.userLog = true;
                }
                else
                {
                    ViewBag.userLog = false;
                    return RedirectToAction("LogIn", "Home");
                }
            }
            catch (Exception)
            {
                ViewBag.userLog = false;
                return RedirectToAction("LogIn", "Home");
            }
            List<CartProduct> productsList = new List<CartProduct> { };
            long rango = cart.Rango("cart");
            for (int i = 0; i < rango; i++)
            {
                productsList.Add(cart.get<CartProduct>("cart"));
            }
            List<Product> cartProducts = new List<Product> { };
            for (int i = 0; i < rango; i++)
            {
                int id = productsList[i].id;
                cartProducts.Add(pd.getSingleProduct(id));
            }
            foreach (var i in cartProducts) {
                foreach (var j in productsList) {
                    if (i.id == j.id)
                    {
                        pd.addToHistory(i.id, j.units, Request.Cookies["UserSettings"]["User"]);
                        pd.reduceProductUnits(i.id, i.units - j.units);
                        
                    }
                }
            }
            return View();
        }
    }
}