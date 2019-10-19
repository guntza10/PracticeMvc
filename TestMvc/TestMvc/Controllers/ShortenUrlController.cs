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
            ShortenCollection = database.GetCollection<ShortenUrlModel>("shorten2");
        }

        public IActionResult Index()
        {
            var historyResult = ShortenCollection.Find(it => true).SortByDescending(it => it.CreationDateTime).ToList();
            return View(new IndexViewModel
            {
                history = historyResult,
                alertMessage = "",
                colorAlert = true
            });
        }

        [HttpGet("{url}")]
        public IActionResult Index(string url)
        {
            var result = ShortenCollection.Find(it => it.ShortenUrl.Contains(url)).FirstOrDefault();
            if (result != null)
            {
                return Redirect(result.FullUrl);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Index([FromForm]IndexViewModel model)
        {
            const string charT = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var data = new ShortenUrlModel();
            var rnd = new Random();
            model.colorAlert = false;
            if (!String.IsNullOrEmpty(model.shortUrlModel.FullUrl))
            {
                var isFullUrlExist = ShortenCollection.Find(it => it.FullUrl == model.shortUrlModel.FullUrl).ToList();
                var randomPrefix = Enumerable.Repeat(charT, 8)
                        .Select(it => it[rnd.Next(charT.Length)]);
                var randomForShorten = String.Join("", randomPrefix);
                var newUrl = (String.IsNullOrEmpty(model.shortUrlModel.Custom)) ?
                    $"https://testgun.azurewebsites.net/{randomForShorten}"
                    : $"https://testgun.azurewebsites.net/{model.shortUrlModel.Custom}";

                if (isFullUrlExist.Any() && String.IsNullOrEmpty(model.shortUrlModel.Custom))
                {
                    model.alertMessage = isFullUrlExist.FirstOrDefault().ShortenUrl;
                }
                else if (isFullUrlExist.Any() && !String.IsNullOrEmpty(model.shortUrlModel.Custom))
                {
                    model.alertMessage = "Full Url be generated,can't generate with custom!";
                    model.colorAlert = true;
                }
                else if (!isFullUrlExist.Any() && !String.IsNullOrEmpty(model.shortUrlModel.Custom))
                {
                    try
                    {
                        var isCustomExited = ShortenCollection.Find(it => it.ShortenUrl.Contains($"/{model.shortUrlModel.Custom}")).ToList();
                        if (!isCustomExited.Any())
                        {
                            var newData = new ShortenUrlModel
                            {
                                Id = Guid.NewGuid().ToString(),
                                FullUrl = model.shortUrlModel.FullUrl,
                                ShortenUrl = newUrl,
                                Custom = model.shortUrlModel.Custom,
                                CreationDateTime = DateTime.Now
                            };
                            ShortenCollection.InsertOne(newData);
                            model.alertMessage = newUrl;
                        }
                        else
                        {
                            model.alertMessage = "Can't be generate becaues url be used";
                            model.colorAlert = true;
                        }
                    }
                    catch (Exception err)
                    {
                        throw err;
                    }
                }
                else if (!isFullUrlExist.Any() && String.IsNullOrEmpty(model.shortUrlModel.Custom))
                {
                    try
                    {
                        var isCustomExited = ShortenCollection.Find(it => it.ShortenUrl.Contains($"/{randomForShorten}")).ToList();
                        if (!isCustomExited.Any())
                        {
                            var newData = new ShortenUrlModel
                            {
                                Id = Guid.NewGuid().ToString(),
                                FullUrl = model.shortUrlModel.FullUrl,
                                ShortenUrl = newUrl,
                                Custom = model.shortUrlModel.Custom,
                                CreationDateTime = DateTime.Now
                            };
                            ShortenCollection.InsertOne(newData);
                            model.alertMessage = newUrl;
                        }
                        else
                        {
                            model.alertMessage = "Can't be generate becaues url be used";
                            model.colorAlert = true;
                        }
                    }
                    catch (Exception err)
                    {
                        throw err;
                    }
                }
            }
            else
            {
                model.alertMessage = "Please input Full Url";
                model.colorAlert = true;
                data = new ShortenUrlModel();
            }
            var historyResult = ShortenCollection.Find(it => true).SortByDescending(it => it.CreationDateTime).ToList();
            return View(new IndexViewModel
            {
                shortUrlModel = data,
                alertMessage = model.alertMessage,
                colorAlert = model.colorAlert,
                history = historyResult

            });
            //return RedirectToAction("Index", new { shModel = data, message = model.alertMessage, color = model.colorAlert });
        }

        //public IActionResult DeleteUrl(string id)
        //{
        //    ShortenCollection.DeleteOne(it => it.Id == id);
        //    return RedirectToAction("Index");
        //}

        //[HttpPost("{id}")]
        //public IActionResult EditModel(string id, string newFullUrl, string newCustom)
        //{
        //    try
        //    {
        //        var fullUrl = "";
        //        var shortUrl = "";
        //        var custom = "";
        //        var host = "https://testgun.azurewebsites.net/";

        //        if (!String.IsNullOrEmpty(newFullUrl) && !String.IsNullOrEmpty(newCustom))
        //        {
        //            if (!IsFullExist(newFullUrl) && !IsShortExist(newCustom))
        //            {
        //                fullUrl = newFullUrl;
        //                shortUrl = $"{host}{newCustom}";
        //                custom = newCustom;
        //                var defFullAndCustom = Builders<ShortenUrlModel>.Update
        //                   .Set(it => it.FullUrl, fullUrl)
        //                   .Set(it => it.ShortenUrl, shortUrl)
        //                   .Set(it => it.Custom, custom)
        //                   .Set(it => it.CreationDateTime, DateTime.Now);
        //                ShortenCollection.UpdateOne(it => it.Id == id, defFullAndCustom);
        //            }
        //        }
        //        else if (!String.IsNullOrEmpty(newFullUrl) && String.IsNullOrEmpty(newCustom))
        //        {
        //            if (!IsFullExist(newFullUrl))
        //            {
        //                fullUrl = newFullUrl;
        //                var defFull = Builders<ShortenUrlModel>.Update
        //                  .Set(it => it.FullUrl, fullUrl)
        //                  .Set(it => it.CreationDateTime, DateTime.Now);
        //                ShortenCollection.UpdateOne(it => it.Id == id, defFull);
        //            }
        //        }
        //        else if (String.IsNullOrEmpty(newFullUrl) && !String.IsNullOrEmpty(newCustom))
        //        {
        //            if (!IsShortExist(newCustom))
        //            {
        //                shortUrl = $"{host}{newCustom}";
        //                custom = newCustom;
        //                var defCustom = Builders<ShortenUrlModel>.Update
        //                  .Set(it => it.ShortenUrl, shortUrl)
        //                  .Set(it => it.Custom, custom)
        //                  .Set(it => it.CreationDateTime, DateTime.Now);
        //                ShortenCollection.UpdateOne(it => it.Id == id, defCustom);
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction("Index");
        //        }
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception err)
        //    {
        //        throw err;
        //    }

        //}

        //public bool IsFullExist(string fullUrl)
        //{
        //    var qry = ShortenCollection.Find(it => it.FullUrl == fullUrl).ToList();
        //    return (qry.Any()) ? true : false;
        //}

        //public bool IsShortExist(string shortUrl)
        //{
        //    var qry = ShortenCollection.Find(it => it.ShortenUrl.Contains($"/{shortUrl}")).ToList();
        //    return (qry.Any()) ? true : false;
        //}
        //public IActionResult Index(ShortenUrlModel shModel, string message, bool color)
        //{
        //    var historyResult = ShortenCollection.Find(it => true).SortByDescending(it => it.CreationDateTime).ToList();
        //    return View(new IndexViewModel
        //    {
        //        shortUrlModel = shModel,
        //        history = historyResult,
        //        alertMessage = message,
        //        colorAlert = color
        //    });
        //}
    }
}

//var historyResult = ShortenCollection.Find(it => true).SortByDescending(it => it.CreationDateTime).ToList();
//return View(new IndexViewModel
//{
//    shortUrlModel = data,
//    alertMessage = model.alertMessage,
//    colorAlert = model.colorAlert,
//    history = historyResult
//});