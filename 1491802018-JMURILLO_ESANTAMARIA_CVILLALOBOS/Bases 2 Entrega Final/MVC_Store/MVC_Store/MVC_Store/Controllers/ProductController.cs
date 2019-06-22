using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using StackExchange.Redis;
using System.Diagnostics;
using Newtonsoft.Json;
using MVC_Store.Models;

namespace MVC_Store.Controllers
{
    public class ProductController : Controller    
    {

        Cart cart = Models.Cart.getInstance();
        Post post = new Post();
        Product pd = new Product();

        public ActionResult ViewProduct(string name, string description, double price, int units, int product_id, string id_img) {
            ViewBag.name = name;
            ViewBag.description = description;
            ViewBag.price = price;
            ViewBag.units = units;
            ViewBag.id_product = product_id;
            ViewBag.id_img = id_img;
            List<Calification> califications = pd.getRatings(product_id);
            ViewBag.califications = califications;
            try
            {
                if (Request.Cookies["UserSettings"]["Status"].Equals("Y"))
                {
                    ViewBag.userAdmin = true;
                }
            }
            catch (Exception) { 
                ViewBag.userAdmin = false;
            }
            try
            {
                if (!Request.Cookies["UserSettings"]["User"].Equals(""))
                {
                    ViewBag.userLog = true;
                    ViewBag.actualUser = Request.Cookies["UserSettings"]["User"];
                    List<Purchase> listPurchase = pd.getPurchases(Request.Cookies["UserSettings"]["User"]);
                    foreach (var j in listPurchase) {
                        if (j.productid == product_id) {
                            ViewBag.iHaveThis = true;
                        }
                    }
                                        
                    Calification cal;
                    foreach (var i in califications) {
                        if (i.idproduct == product_id && i.iduser.Equals(Request.Cookies["UserSettings"]["User"])) {
                            cal = i;
                            ViewBag.thereIsRating = true;
                            ViewBag.cal = cal;
                            break;
                        }
                    }
                }
            }
            catch (Exception) {
                ViewBag.iHaveThis = false;
                ViewBag.userLog = false;
            }
            List<Post> commentsList = post.getProductPosts(product_id);
            ViewBag.comments = commentsList;

            List<Product> randomProducts = pd.getProducts();
            List<Purchase> randomPurchase = pd.getAllHistory();
            foreach (var i in randomProducts) {
                if (i.id == product_id) {
                    randomProducts.Remove(i);
                    break;
                }
            }
            List<Product> selected = new List<Product> { };
            List<Purchase> random = new List<Purchase> { };
            foreach (var i in randomPurchase) {
                if (i.productid != product_id) {
                    random.Add(i);
                }
            }
            int n = 0;
            while (n < 3) {
                foreach (var i in random) {
                    bool metalo = true;
                    foreach (var j in selected) {
                        if (i.productid == j.id) {
                            metalo = false;
                        }
                    }
                    if (metalo == true) {
                        selected.Add(pd.getSingleProduct(i.productid));
                    }
                }
                n++;
            }
            ViewBag.selected = selected;
            return View();
        }

        public ActionResult setRating(int id_product, int rating)
        {
            pd.addRating(rating, id_product, Request.Cookies["UserSettings"]["User"]);
            Product product = pd.getSingleProduct(id_product);
            return RedirectToAction("ViewProduct", "Product", new { name = product.name, description = product.description, price = product.price, units = product.units, product_id = product.id, id_img = product.img_id });
        }

        public ActionResult addProductToCart(int id, string units)
        {
            int iUnits = Convert.ToInt32(units);
            CartProduct cp = new CartProduct(id, iUnits);
            cart.set<CartProduct>("cart", cp);
            return View();
        }

        public ActionResult uploadComment(string item_comment) {
            int id = Convert.ToInt32(TempData["Data1"]);
            string pName = TempData["Data2"].ToString();
            string pDescription = TempData["Data3"].ToString();
            double pPrice = Convert.ToDouble(TempData["Data4"]);
            int pUnits = Convert.ToInt32(TempData["Data5"]);
            string pImg_id = TempData["Data6"].ToString();
            string user_id = Request.Cookies["UserSettings"]["User"];
            int postId = post.giveId();
            post.addPost(postId, id, user_id, item_comment);
            return RedirectToAction("ViewProduct", "Product", new { name = pName, description = pDescription, price = pPrice, units = pUnits, product_id = id, id_img = pImg_id });
        }

        public ActionResult deleteComment(int postid) {
            int id = Convert.ToInt32(TempData["Data1"]);
            string pName = TempData["Data2"].ToString();
            string pDescription = TempData["Data3"].ToString();
            double pPrice = Convert.ToDouble(TempData["Data4"]);
            int pUnits = Convert.ToInt32(TempData["Data5"]);
            string pImg_id = TempData["Data6"].ToString();
            string user_id = Request.Cookies["UserSettings"]["User"];
            post.deletePost(postid, user_id, id);
            return RedirectToAction("ViewProduct", "Product", new { name = pName, description = pDescription, price = pPrice, units = pUnits, product_id = id, id_img = pImg_id });
        }



    }


}