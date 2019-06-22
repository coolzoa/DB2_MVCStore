using MongoDB.Bson;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Models
{
    public class Product
    {

        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public double price { get; set; }
        public int units { get; set; }
        public string img_id { get; set; }

        public List<Product> products { get; set; }
        public List<MongoPicture> imgs { get; set; }
        public List<User> users { get; set; }
        

        private bool connection_open;
        private MySqlConnection connection;

        private MongoClient client = new MongoClient("mongodb://localhost");

        public int addProduct(Product new_pd, MongoPicture img) {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = String.Format("insert into shop.product (name, description, price, units, photo_id) values (" +
                "'" + new_pd.name + "'," +
                "'" + new_pd.description + "'," +
                "'" + new_pd.price + "'," +
                "'" + new_pd.units + "'," +
                "'" + new_pd.img_id + "');");
            int mod = cmd.ExecuteNonQuery();
            connection.Close();
            return mod;
        }

        public string insertPicture(MongoPicture img) {
            try
            {
                string databaseName = "PhotosDB";
                var thePictureDB = client.GetDatabase(databaseName);
                string theCollectionName = "Pictures";
                thePictureDB.GetCollection<MongoPicture>(theCollectionName).InsertOne(img);
                return img.id.ToString();
            }
            catch (Exception) {
                return "";
            }
        }

        public List<Product> getProducts()
        {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText =
                string.Format("select * from product");
            MySqlDataReader reader = cmd.ExecuteReader();

            var products = new List<Product>();
            while (reader.Read())
            {
                Product product = new Product();
                product.id = (int)reader["id"];
                product.name = reader.GetString("name");
                product.description = reader.GetString("description");
                product.price = reader.GetDouble("price");
                product.units = (int)reader["units"];
                product.img_id = reader.GetString("photo_id");
                products.Add(product);
            }
            connection.Close();
            return products;
        }

        public List<Product> getSpecificProducts(string text) {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select * from shop.product where name like '" + text +"%';");
            MySqlDataReader reader = cmd.ExecuteReader();
            var products = new List<Product>();
            while (reader.Read())
            {
                Product product = new Product();
                product.id = (int)reader["id"];
                product.name = reader.GetString("name");
                product.description = reader.GetString("description");
                product.price = reader.GetDouble("price");
                product.units = (int)reader["units"];
                product.img_id = reader.GetString("photo_id");

                products.Add(product);
            }
            connection.Close();
            return products;
        }

        public List<MongoPicture> getImages()
        {
            string databaseName = "PhotosDB";
            var thePictureDB = client.GetDatabase(databaseName);
            string theCollectionName = "Pictures";
            return thePictureDB.GetCollection<MongoPicture>(theCollectionName).AsQueryable().ToList();
        }

        public FileContentResult getPhoto(string id)
        {
            var _id = new ObjectId(id);
            string databaseName = "PhotosDB";
            var thePictureDB = client.GetDatabase(databaseName);
            string theCollectionName = "Pictures";
            var thePicture = thePictureDB.GetCollection<MongoPicture>(theCollectionName).Find(p => p.id == _id).SingleOrDefault();
            var thePictureDataAsBytes = Convert.FromBase64String(thePicture.PictureDataAsString);
            return new FileContentResult(thePictureDataAsBytes, "image/jpeg");
        }

        public Product getSingleProduct(int id)
        {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select * from shop.product where id = '" + id + "';");
            MySqlDataReader reader = cmd.ExecuteReader();
            Product product = new Product();
            reader.Read();
            product.id = id;
            product.name = reader.GetString("name");
            product.description = reader.GetString("description");
            product.price = reader.GetDouble("price");
            product.units = (int)reader["units"];
            product.img_id = reader.GetString("photo_id");
            connection.Close();
            return product;

        }

        public void reduceProductUnits(int id, int newUnits)
        {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("update shop.product set units = " + newUnits + " where id = " + id + ";");
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void addToHistory(int idProduct, int units, string user)
        {
            {
                getConnection();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = string.Format("insert into shop.history (productid,units,date,user) values(" +
                    idProduct + "," +
                    units+ "," + "'" +
                    DateTime.Now.ToString("yyyy-MM-dd H:mm") + "'" + ","
                    + "'" + user+ "'" + ");");
               cmd.ExecuteNonQuery();
                connection.Close();

            }
        }

        public List<Purchase> getPurchases(string userid)
        {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select * from shop.history where user = "+
                "'" + userid + "'" + ";");
           MySqlDataReader reader = cmd.ExecuteReader();

            var purchases = new List<Purchase>();
            while (reader.Read())
            {
                Purchase purchase = new Purchase();
                purchase.id = (int)reader["id"];
                purchase.productid = (int)reader["productid"];
                purchase.units = (int)reader["units"];
                purchase.date = reader.GetString("date");
                purchase.user = userid;
                purchases.Add(purchase);
            }
            connection.Close();
            return purchases;

        }

        public List<Calification> getRatings(int idProduct)
        {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select * from shop.rating where idproduct = " +
                idProduct + ";");
            MySqlDataReader reader = cmd.ExecuteReader();
            List<Calification> ratings  =new List<Calification>();
            while (reader.Read())
            {
                Calification temp = new Calification();
                temp.id = (int)reader["id"];
                temp.rating = (int)reader["rating"];
                temp.idproduct = (int)reader["idproduct"];
                temp.iduser = reader.GetString("iduser");
                ratings.Add(temp);
            }
            connection.Close();
            return ratings;
        }

        public void addRating(int rating, int productid, string idUser)
        {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("insert into shop.rating(rating,idproduct,iduser) values(" +
                rating + "," +
                productid + "," +
                "'" + idUser + "'" + ");");
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void modifyRating (int idProduct, string idUser, int newRating)
        {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("update shop.rating set rating = " +
                newRating + "where idproduct = " + idProduct + "and iduser = " +
                "'" + idUser + "'" + ";");
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public List<Purchase> getAllHistory() {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select * from shop.history;");
            MySqlDataReader reader = cmd.ExecuteReader();
            List<Purchase> list = new List<Purchase> { };
            while (reader.Read()) {
                Purchase p = new Purchase();
                p.id = (int)reader["id"];
                p.productid = (int)reader["productid"];
                p.units = (int)reader["units"];
                p.date = reader.GetString("date");
                p.user = reader.GetString("user");
                list.Add(p);
            }
            connection.Close();
            return list;
        }

        private void getConnection() {
            connection_open = false;
            connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnMySQL"].ConnectionString;
            if (Open_Local_Connection())
            {
                connection_open = true;
            }
        }

        private bool Open_Local_Connection() {
            try {
                connection.Open();
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        

    }
}