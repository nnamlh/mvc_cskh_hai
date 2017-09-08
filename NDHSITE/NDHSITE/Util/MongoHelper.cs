using MongoDB.Driver;
using NDHSITE.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace NDHSITE.Util
{
    public class MongoHelper
    {
        IMongoDatabase db = null;

        public MongoHelper()
        {

            string value = ConfigurationManager.AppSettings["MongoServer"];
            if (value == null)
                value = "mongodb://localhost:27017";

            var client = new MongoClient(value);

            db = client.GetDatabase("NDHLog");
        }

        public bool checkLoginSession(string user, string token)
        {
            var collection = db.GetCollection<MongoAPIAuthHistory>("APIAuthHistory");
            var builder = Builders<MongoAPIAuthHistory>.Filter;

            var filter = builder.Eq("UserLogin", user) & builder.Eq("Token", token) & builder.Eq("IsExpired", 0);

            var data = collection.Find<MongoAPIAuthHistory>(filter).FirstOrDefault();

            return data == null ? false : true;
        }
    }
}