using MVC_Store.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class AdminController : Controller
    {

        Product pd = new Product();

        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.userLog = true;
            if (Request.Cookies["UserSettings"]["Status"].Equals("Y"))
            {
                ViewBag.userAdmin = true;
            }
            else
            {
                ViewBag.userAdmin = false;
            }
            var model = new Product
            {
                products = pd.getProducts(),
                imgs = pd.getImages()
            };
            return View(model);
        }

        public ActionResult SearchUser(string search_text) {
            ViewBag.userLog = true;
            if (Request.Cookies["UserSettings"]["Status"].Equals("Y"))
            {
                ViewBag.userAdmin = true;
            }
            else
            {
                ViewBag.userAdmin = false;
            }
            if (search_text.Equals("")) {
                ViewBag.userList = new List<User> { };
                return View("~/Views/Admin/SearchUser.cshtml");
            }
            ViewBag.userList = new User().getUsers(search_text);
            return View();
        }

        public ActionResult LoadViewSearch()
        {
            ViewBag.userLog = true;
            if (Request.Cookies["UserSettings"]["Status"].Equals("Y"))
            {
                ViewBag.userAdmin = true;
            }
            else
            {
                ViewBag.userAdmin = false;
            }
            ViewBag.userList = new List<User>();
            return View("~/Views/Admin/SearchUser.cshtml");
        }

        public ActionResult GiveUpgrade(string email) {
            ViewBag.userLog = true;
            if (Request.Cookies["UserSettings"]["Status"].Equals("Y"))
            {
                ViewBag.userAdmin = true;
            }
            else
            {
                ViewBag.userAdmin = false;
            }
            new User().upgradeAccount(email);
            ViewBag.email = email;
            return View();
        }

        public ActionResult AddProductView() {
            ViewBag.userLog = true;
            return View();
        }

        public ActionResult AddProduct(string p_name, string p_description, string p_price, string p_units, HttpPostedFileBase theFile) {
            string theFileName = Path.GetFileName(theFile.FileName);
            System.Windows.Forms.MessageBox.Show(theFileName);
            byte[] thePictureAsBytes = new byte[theFile.ContentLength];
            using (BinaryReader theReader = new BinaryReader(theFile.InputStream))
            {
                thePictureAsBytes = theReader.ReadBytes(theFile.ContentLength);
            }
            string thePictureDataAsString = Convert.ToBase64String(thePictureAsBytes);
            MongoPicture thePicture = new MongoPicture()
            {
                file_name = theFileName,
                PictureDataAsString = thePictureDataAsString
            };
            double dPrice = double.Parse(p_price);
            int iUnits = int.Parse(p_units);
            pd = new Product {
                name = p_name,
                description = p_description,
                price = dPrice,
                units = iUnits,
                img_id = pd.insertPicture(thePicture)
            };
            pd.addProduct(pd, thePicture);

            return RedirectToAction("Index", "User");
        }

    }
}