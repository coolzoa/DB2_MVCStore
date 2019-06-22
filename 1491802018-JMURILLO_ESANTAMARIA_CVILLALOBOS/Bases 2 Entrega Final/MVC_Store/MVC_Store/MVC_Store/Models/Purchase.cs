using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Models
{
    public class Purchase
    {
       public int id { get; set; }
       public  int productid { get; set; }
        public int units { get; set; }
       public  string date { get; set; }
       public  string user { get; set; }
    }
}