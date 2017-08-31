using HAIAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Configuration;

namespace HAIAPI.Util
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

        public void createHistoryAPI(MongoHistoryAPI data)
        {
            var collection = db.GetCollection<MongoHistoryAPI>("APIHistory");
            collection.InsertOneAsync(data);

        }
        // auth history
        public void createAuthHistory(MongoAPIAuthHistory data)
        {
            var collection = db.GetCollection<MongoAPIAuthHistory>("APIAuthHistory");
            collection.InsertOneAsync(data);

        }

        public bool checkLoginSession(string user, string token)
        {
            var collection = db.GetCollection<MongoAPIAuthHistory>("APIAuthHistory");
            var builder = Builders<MongoAPIAuthHistory>.Filter;

            var filter = builder.Eq("UserLogin", user) & builder.Eq("Token", token) & builder.Eq("IsExpired", 0);

            var data = collection.Find<MongoAPIAuthHistory>(filter).FirstOrDefault();

            return data == null ? false : true;
        }

        public void checkAndCreateAutHistory(string user, string token, string role, string device, string os, string imei)
        {
            var collection = db.GetCollection<MongoAPIAuthHistory>("APIAuthHistory");
            var builder = Builders<MongoAPIAuthHistory>.Filter;
          //  var filter = builder.Eq("UserLogin", user) & builder.Eq("IsExpired", 0);
            var data = collection.Find<MongoAPIAuthHistory>(builder.Eq("UserLogin", user) & builder.Eq("IsExpired", 0)).ToList(); 

            foreach(var item in data)
            {
                var update = Builders<MongoAPIAuthHistory>.Update.Set("IsExpired", 1);
                var result = collection.UpdateOneAsync(Builders<MongoAPIAuthHistory>.Filter.Eq("Id", item.Id), update);
            }

            var authHistory = new MongoAPIAuthHistory()
            {
                Device = device,
                Imei = imei,
                OS = os,
                UserRole = role,
                Token = token,
                UserLogin = user,
                IsExpired = 0,
                LoginTime = DateTime.Now
            };

            collection.InsertOneAsync(authHistory);
        }

        // logout
        public void saveLogout(string user, string token)
        {
            var collection = db.GetCollection<MongoLogoutHistory>("LogoutHistory");

            var data = new MongoLogoutHistory()
            {
                LogoutTime = DateTime.Now,
                Token = token,
                UserLogin = user
            };

            collection.InsertOneAsync(data);
        }
    }

}