using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TestMvc.Models;

namespace TestMvc.Controllers
{
    public class ShortenUrlController : Controller
    {
        public IMongoCollection<ShortenUrlModel> ShortenCollection { get; set; }
        public ShortenUrlController()
        {
            var client = new MongoClient("mongodb://admin:mana1234@ds016098.mlab.com:16098/shortenurl?retryWrites=false");
            var database = client.GetDatabase("shortenurl");
            ShortenCollection = database.GetCollection<ShortenUrlModel>("shorten");
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index([FromForm]ShortenUrlModel model)
        {
            const string charT = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var rnd = new Random();
            if (!String.IsNullOrEmpty(model.FullUrl))
            {
                var isFullUrlExist = ShortenCollection.Find(it => it.FullUrl == model.FullUrl).ToList();
                var cusUrl = (String.IsNullOrEmpty(model.CustomUrl)) ? "" : model.CustomUrl;
                var isCustomExited = ShortenCollection.Find(it => it.ShortUrl.ToLower().Contains(cusUrl.ToLower())).ToList();
                if (!isFullUrlExist.Any() && !isCustomExited.Any())
                {
                    var randomPrefix = Enumerable.Repeat(charT, 8)
                   .Select(it => it[rnd.Next(charT.Length)]);
                    var randomForShorten = String.Join("", randomPrefix);
                    var newUrl = (String.IsNullOrEmpty(model.CustomUrl)) ? $"https://testgun.azurewebsites.net/{randomForShorten}"
                        : $"https://testgun.azurewebsites.net/{model.CustomUrl}";
                    var newData = new ShortenUrlModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullUrl = model.FullUrl,
                        ShortUrl = newUrl,
                        CustomUrl = model.CustomUrl,
                        CreationDateTime = DateTime.Now
                    };
                    ShortenCollection.InsertOne(newData);
                }
                else if (isFullUrlExist.Any() && String.IsNullOrEmpty(model.CustomUrl))
                {
                    ViewBag.showMessage = isFullUrlExist.FirstOrDefault().FullUrl;
                }
                else if (isFullUrlExist.Any() && !String.IsNullOrEmpty(model.CustomUrl))
                {
                    ViewBag.showMessage = "Can't be generate becaues url be used";
                }
            }
            else
            {
                ViewBag.showMessage = "Please input Full Url";
            }
            return View();
        }
    }
}