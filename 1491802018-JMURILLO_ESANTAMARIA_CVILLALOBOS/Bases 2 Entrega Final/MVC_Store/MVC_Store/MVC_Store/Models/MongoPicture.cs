using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Models
{
    public class MongoPicture
    {
        public ObjectId id { get; set; }
        public string file_name { get; set; }
        public string PictureDataAsString { get; set; }
    }
}