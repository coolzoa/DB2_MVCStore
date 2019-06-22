using StackExchange.Redis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Models
{
    public sealed class Cart
    {
        private readonly ConnectionMultiplexer redisConnection;
        private static Cart instance = null;

        private Cart() {
            this.redisConnection = ConnectionMultiplexer.Connect("localhost");
        }

        public static Cart getInstance() {
            if (instance == null) {
                instance = new Cart();
            }
            return instance;
        }

        public void set<T>(string key, T objectToCache) where T : class {
            var db = this.redisConnection.GetDatabase();
            db.ListLeftPush(key, JsonConvert.SerializeObject(objectToCache, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            }));
        }

        public T get<T>(string key) where T : class {
            var db = this.redisConnection.GetDatabase();
            var redisObject = db.ListRightPop(key);
            if (redisObject.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(redisObject, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
            }
            else {
                return (T)null;
            }
        }

        public long Rango(string key) {
            var db = this.redisConnection.GetDatabase();
            long size = db.ListLength(key);
            return size;
        }

        public void Delete<T>(string key) where T : class {
            var db = this.redisConnection.GetDatabase();
            db.KeyDelete(key);
        }
    }
}