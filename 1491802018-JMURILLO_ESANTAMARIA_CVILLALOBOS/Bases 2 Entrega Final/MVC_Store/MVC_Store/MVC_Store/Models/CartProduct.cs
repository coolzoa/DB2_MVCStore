using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Models
{
    public class CartProduct
    {
        public int id { get; set; }
        public int units { get; set; }

        public CartProduct(int id, int units)
        {
            this.id = id;
            this.units = units;
        }

        public int getId() { return this.id; }
        public int getUnits() { return this.units; }
    }
}