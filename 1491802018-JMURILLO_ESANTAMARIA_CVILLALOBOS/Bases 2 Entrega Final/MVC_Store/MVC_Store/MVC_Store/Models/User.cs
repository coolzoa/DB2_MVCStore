using MySql.Data.MySqlClient;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MVC_Store.Models
{
    public class User
    {
        public string email { get; set; }
        public string first { get; set; }
        public string last { get; set; }
        public string password { get; set; }
        public string status { get; set; }

        private bool connection_opens;
        private MySqlConnection connection;

        public User() { }

        public bool createUser(string email, string first, string last, string password)
        {
            getConnection();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = string.Format("insert into shop.users (email, first, last, password, status)" +
                    "values ('" + email + "', '" + first + "', '" + last + "', '" + password + "', 'N');");
                cmd.ExecuteNonQuery();
                connection.Close();
                addUserNode(email);
                return true;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("[ ! ] There is already an account with the inserted email.");
                connection.Close();
                return false;
            }
        }

        public bool logIn(string email, string password)
        {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select * from shop.users where email = '" + email + "' and password = '" + password + "';");
            MySqlDataReader reader = cmd.ExecuteReader();
            string selected_email;
            try
            {
                reader.Read();
                if (reader.IsDBNull(0) == false)
                {
                    selected_email = reader.GetString(0);
                }
                else
                {
                    selected_email = null;
                }
                reader.Close();
            }
            catch (MySqlException e) {
                System.Windows.Forms.MessageBox.Show("[ ! ] The email or the password are incorrect.");
                connection.Close();
                return false;
            }
            if (email.Equals(selected_email)) {
                connection.Close();
                return true;
            }
            System.Windows.Forms.MessageBox.Show("[ ! ] The email or the password are incorrect.");
            connection.Close();
            return false;
        }

        public string getStatus(string email) {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select * from shop.users where email = '" + email + "';");
            MySqlDataReader reader = cmd.ExecuteReader();
            string status;
            try
            {
                reader.Read();
                status = reader.GetString(4);
                reader.Close();
            }
            catch (MySqlException e)
            {
                status = "N";
            }
            return status;
        }

        public List<User> getUsers(string search) {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select email, first, last, status from shop.users where email like '" + search + "%' or first like '" + search + "%';");
            MySqlDataReader reader = cmd.ExecuteReader();
            var users = new List<User>();
            while (reader.Read()) {
                if (reader.GetString(3).Equals('N') || reader.GetString(3).Equals("N"))
                {
                    User user = new User();
                    user.email = reader.GetString(0);
                    user.first = reader.GetString(1);
                    user.last = reader.GetString(2);
                    users.Add(user);
                }
            }
            connection.Close();
            return users;

        }

        public List<User> getUsersSearch(string search, string user_email) {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select email, first, last, status from shop.users where email like '" + search + "%' or first like '" + search + "%';");
            MySqlDataReader reader = cmd.ExecuteReader();
            var users = new List<User>();
            while (reader.Read())
            {
                if (!user_email.Equals(reader.GetString(0)))
                {
                    User user = new User();
                    user.email = reader.GetString(0);
                    user.first = reader.GetString(1);
                    user.last = reader.GetString(2);
                    users.Add(user);
                }
            }
            connection.Close();
            return users;
        }

        public User getActualUser(string user_email) {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("select * from shop.users where email = '" + user_email +"';");
            MySqlDataReader reader = cmd.ExecuteReader();
            User user = new User();
            reader.Read();
            user.email = reader.GetString(0);
            user.first = reader.GetString(1);
            user.last = reader.GetString(2);
            connection.Close();
            return user;
        }

        public void upgradeAccount(string email) {
            getConnection();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = string.Format("update shop.users set status = 'Y' where email = '" + email + "';");
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void getConnection()
        {
            connection_opens = false;
            connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnMySQL"].ConnectionString;
            if (Open_Local_Connection()) {
                connection_opens = true;
            }
        }

        private bool Open_Local_Connection() {
            try {
                connection.Open();
                return true;
            }
            catch (Exception e) {
                return false;
            }
        }

        public void addUserNode(string user_email) {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "1234");
            var newUser = new User { email = user_email};
            client.Connect();
            client.Cypher
                .Create("(user:User {newUser})")
                .WithParam("newUser", newUser)
                .ExecuteWithoutResults();
        }

        public void createUserRelation(string email_userA, string email_userB) {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "1234");
            var userA = new User { email = email_userA };
            var userB = new User { email = email_userB };
            client.Connect();
            client.Cypher.Match("(user_A:User)", "(user_B:User)")
                         .Where((User user_A) => user_A.email == userA.email)
                         .AndWhere((User user_B) => user_B.email == userB.email)
                         .CreateUnique("(user_A) - [:Follows] -> (user_B)")
                         .ExecuteWithoutResults();
        }

        public bool existRelationUsers(string userA, string userB) {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "1234");
            User user_A = new User { email = userA };
            User user_B = new User { email = userB };
            client.Connect();
            bool exist = (
                client.Cypher.Match("(user_1)-[:Follows]->(user_2)")
                             .Where((User user_1) => user_1.email == user_A.email)
                             .AndWhere((User user_2) => user_2.email == user_B.email)
                             .Return<Node<User>>("user_1")
                             .Results
                             .Count() == 1
            );
            return exist;
        }

        public void deleteRelationUsers(string usr1, string usr2) {
            User user1 = new User { email = usr1 };
            User user2 = new User { email = usr2 };
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "1234");
            client.Connect();
            client.Cypher.Match("(user_1)-[r]->(user_2)")
                         .Where((User user_1) => user_1.email == user1.email)
                         .AndWhere((User user_2) => user_2.email == user2.email)
                         .AndWhere("Type(r) = 'Follows'")
                         .Delete("r").ExecuteWithoutResults();
        }

    }
}