using MVC_Store.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class HomeController : Controller
    {

        Product pd = new Product();
        Cart cart = Models.Cart.getInstance();
        Post p = new Post();


        public ActionResult Index()
        {
            cart.Delete<CartProduct>("cart");
            ViewBag.userLog = false;
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

        public ActionResult LogIn() {
            ViewBag.userLog = false;
            return View();
        }

        public ActionResult Register()
        {
            ViewBag.userLog = false;
            return View();
        }

        public ActionResult SearchFunction(string search_text) { // Preguntar al profe...
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
                    string user = Request.Cookies["UserSettings"]["User"];
                    ViewBag.userList = new User().getUsersSearch(search_text, user);
                    ViewBag.actualUser = Request.Cookies["UserSettings"]["User"];
                }
            }
            catch (Exception)
            {
                ViewBag.userLog = false;
                ViewBag.userList = new List<User> { };
            }
            var model = new Product
            {
                products = pd.getSpecificProducts(search_text),
                imgs = pd.getImages(),
            };
            
            if (search_text.Equals("")) {
                model.products = new List<Product> { };
                ViewBag.userList = new List<User> { };
            }
            ViewBag.search_text = search_text;
            
            return View(model);
        }

        public static bool IsFollow(string actualUser, string user_email) {
            User user = new User();
            bool exist = user.existRelationUsers(actualUser, user_email);
            return exist;
        }

        public string getActualUser() {
            return Request.Cookies["UserSettings"]["User"];
        }

        public ActionResult Cart() {
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
                else {
                    ViewBag.userLog = false;
                }
            }
            catch (Exception) {
                ViewBag.userLog = false;
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
            foreach (var i in productsList) {
                cart.set<CartProduct>("cart", i);
            }
            if (rango == 0)
            {
                ViewBag.buyButton = false;
            }
            else {
                ViewBag.buyButton = true;
            }
            ViewBag.list = productsList;
            ViewBag.list2 = cartProducts;
            return View();
        }

        public ActionResult addToCart(string name, string description, double price, int units, int id, string id_img) {
            ViewBag.name = name;
            ViewBag.description = description;
            ViewBag.price = price;
            ViewBag.units = units;
            ViewBag.product_id = id;
            ViewBag.id_img = id_img;
            ViewBag.unitsWanted = 1;
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
            return View();
        }

        public ActionResult removeCartItem(int id) {
            long rango = cart.Rango("cart");
            List<CartProduct> productList = new List<CartProduct> { };
            for (int i = 0; i < rango; i++)
            {
                productList.Add(cart.get<CartProduct>("cart"));
            }
            List<CartProduct> cartList = new List<CartProduct> { };
            foreach (var cp in productList) {
                if (cp.id != id) {
                    cartList.Add(cp);
                }
            }
            foreach (var i in cartList) {
                cart.set<CartProduct>("cart", i);
            }
            return RedirectToAction("Cart", "Home");
        }









    }
}