using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Models
{
    public class Post
    {

        public int postid { get; set; }
        public int productid { get; set; }
        public string userid { get; set; }
        public string text { get; set; }
        public List<Post> posts { get; set; }

        private static Cluster cluster = Cluster.Builder().AddContactPoint("127.0.0.1").Build();

        public int giveId() {
            ISession session = cluster.Connect("postsdb");
            int max = -1;
            string transaction = "select postid from post;";
            RowSet results = session.Execute(transaction);
            foreach (Row row in results.GetRows()) {
                int temp = row.GetValue<int>("postid");
                if (max < temp) {
                    max = temp;
                }
            }
            return ++max;
        }

        public void addPost(int Postid, int idProduct, string idUser, string text)
        {
            ISession session = cluster.Connect("postsdb");
            string transaction = "insert into post(postid, productid, userid, text) values (" +
                Postid + "," +
                idProduct + ",'" +
                idUser + "'," +
                "'"+  text + "'" + ");";
            session.Execute(transaction);
        }

        public bool isUserPost( int idPost, string idUser, int idProduct)
        {
            ISession session = cluster.Connect("postsdb");
            string transaction = "select * from post where " +
                "postid = " + idPost + ";";
            RowSet results = session.Execute(transaction);
            foreach (Row row in results.GetRows())
            {
                Post temp = new Post();
                temp.productid = row.GetValue<int>("productid");
                temp.userid = row.GetValue<string>("userid");
                if (temp.userid.Equals(idUser) && temp.productid == idProduct)
                {
                    return true;
                }
            }
            return false;
        }

        public void modifyPost(int idPost, string idUser, int idProduct, string newText)
        {
            if (isUserPost(idPost, idUser, idProduct))
            {
                ISession session = cluster.Connect("postsdb");
                string transaction = "update post set text = " +
                    "'" + newText + "'" + "where " +
                    "postid = " + idPost + ";";
                session.Execute(transaction);
            }
        }

        public void deletePost(int idPost, string idUser, int idProduct)
        {
            if (isUserPost(idPost, idUser, idProduct))
            {
                ISession session = cluster.Connect("postsdb");
                string transaction = "delete from post where " +
                    "postid = " + idPost + ";";
                session.Execute(transaction);
            }
        }

        public List<Post> getProductPosts(int idProduct)
        {
            ISession session = cluster.Connect("postsdb");
            string transaction = "select * from post where " +
                "productid = " + idProduct + " allow filtering;";
            RowSet rows = session.Execute(transaction);
            List<Post> posts = new List<Post>();
            foreach(Row row in rows)
            {
                Post temp = new Post();
                temp.postid = row.GetValue<int>("postid");
                temp.productid = row.GetValue<int>("productid");
                temp.userid = row.GetValue<string>("userid");
                temp.text = row.GetValue<string>("text");
                posts.Add(temp);
            }
            return posts;
        }
    }
}