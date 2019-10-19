using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TestMvc.Models;
using X.PagedList;

namespace TestMvc.Controllers
{
    public class HistoryController : Controller
    {
        public IMongoCollection<ShortenUrlModel> ShortenCollection { get; set; }
        public HistoryController()
        {
            var client = new MongoClient("mongodb://admin:mana1234@ds016098.mlab.com:16098/shortenurl?retryWrites=false");
            var database = client.GetDatabase("shortenurl");
            ShortenCollection = database.GetCollection<ShortenUrlModel>("shorten2");
        }

        //public IActionResult Index()
        //{
        //    var historyResult = ShortenCollection.Find(it => true).SortByDescending(it => it.CreationDateTime).ToList();
        //    return View(historyResult);
        //}

        public IActionResult Index(int? page, string search)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var shortenUrl = ShortenCollection.Find(it => true)
             .SortByDescending(it => it.CreationDateTime)
             //.Skip((pageNumber-1)* pageSize)
             //.Limit(pageSize)
             .ToList();
            if (!String.IsNullOrEmpty(search))
            {
                var allHistory = ShortenCollection.Find(it => it.FullUrl.Contains(search)
                 || it.ShortenUrl.Contains(search))
                .SortByDescending(it => it.CreationDateTime)
                .ToList();
                return View(allHistory.ToPagedList(pageNumber, pageSize));
            }
            return View(shortenUrl.ToPagedList(pageNumber, pageSize));
        }

        public IActionResult DeleteUrl(string id)
        {
            ShortenCollection.DeleteOne(it => it.Id == id);
            return RedirectToAction("Index");
        }

        [HttpPost("{id}")]
        public IActionResult EditModel(string id, string newFullUrl, string newCustom)
        {
            try
            {
                var fullUrl = "";
                var shortUrl = "";
                var custom = "";
                var host = "https://testgun.azurewebsites.net/";

                if (!String.IsNullOrEmpty(newFullUrl) && !String.IsNullOrEmpty(newCustom))
                {
                    if (!IsFullExist(newFullUrl) && !IsShortExist(newCustom))
                    {
                        fullUrl = newFullUrl;
                        shortUrl = $"{host}{newCustom}";
                        custom = newCustom;
                        var defFullAndCustom = Builders<ShortenUrlModel>.Update
                           .Set(it => it.FullUrl, fullUrl)
                           .Set(it => it.ShortenUrl, shortUrl)
                           .Set(it => it.Custom, custom)
                           .Set(it => it.CreationDateTime, DateTime.Now);
                        ShortenCollection.UpdateOne(it => it.Id == id, defFullAndCustom);
                    }
                }
                else if (!String.IsNullOrEmpty(newFullUrl) && String.IsNullOrEmpty(newCustom))
                {
                    if (!IsFullExist(newFullUrl))
                    {
                        fullUrl = newFullUrl;
                        var defFull = Builders<ShortenUrlModel>.Update
                          .Set(it => it.FullUrl, fullUrl)
                          .Set(it => it.CreationDateTime, DateTime.Now);
                        ShortenCollection.UpdateOne(it => it.Id == id, defFull);
                    }
                }
                else if (String.IsNullOrEmpty(newFullUrl) && !String.IsNullOrEmpty(newCustom))
                {
                    if (!IsShortExist(newCustom))
                    {
                        shortUrl = $"{host}{newCustom}";
                        custom = newCustom;
                        var defCustom = Builders<ShortenUrlModel>.Update
                          .Set(it => it.ShortenUrl, shortUrl)
                          .Set(it => it.Custom, custom)
                          .Set(it => it.CreationDateTime, DateTime.Now);
                        ShortenCollection.UpdateOne(it => it.Id == id, defCustom);
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index");
            }
            catch (Exception err)
            {
                throw err;
            }

        }

        public bool IsFullExist(string fullUrl)
        {
            var qry = ShortenCollection.Find(it => it.FullUrl == fullUrl).ToList();
            return (qry.Any()) ? true : false;
        }

        public bool IsShortExist(string shortUrl)
        {
            var qry = ShortenCollection.Find(it => it.ShortenUrl.Contains($"/{shortUrl}")).ToList();
            return (qry.Any()) ? true : false;
        }

    }
}