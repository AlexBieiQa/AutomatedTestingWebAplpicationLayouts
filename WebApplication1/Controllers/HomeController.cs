using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hangfire;
using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using WebApplication1.Models;
using WebApplication1.Entities;
using WebApplication1.Enums;
using WebApplication1.Helpers;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        public ApplicationDbContext db { get; set; } = new ApplicationDbContext();



        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Sites");
            return View();
        }

        public ActionResult Tutorial()
        {
            return View();
        }

        [Authorize]
        public ActionResult Sites()
        {
            var userId = User.Identity.GetUserId();

            var user = ExtensionMethods.GetUserById(userId, db);
            if (user == null)
                return Content("User not found");        
            var model = user.Sites.Where(z => z.User.Id == user.Id).ToList();
            
            return View(model);
        }

        public ActionResult RemoveSite(int? siteId)
        {
            
            var site = db.Sites.FirstOrDefault(z => z.Id == siteId);
            if (site != null)
           
            {
                db.Screenshots.RemoveRange(site.Screenshots);
                db.Tests.RemoveRange(site.Tests);
                db.Links.RemoveRange(site.Links);
                db.Sites.Remove(site);
                db.SaveChanges();
            }
            else
            {
                return Content("Not Ok");
            }
            return Redirect(Request.UrlReferrer?.PathAndQuery ?? "/Home/Index");

        }

        [Authorize]
        public ActionResult SitePages(int? SiteId)
        {

            var links = db.Links.Where(z => z.Site.Id == SiteId).ToList();

            return View(links);
        }

        [Authorize]
        public ActionResult Tests()
        {
            var userId = User.Identity.GetUserId();

            var user = ExtensionMethods.GetUserById(userId, db);
            if (user == null)
                return Content("User not found");

            var sites = user.Sites;


            return View(sites);

        }

        [Authorize]
        public ActionResult TestResult(int? SiteId)
        {

            var site = db.Sites.FirstOrDefault(z =>z.Id == SiteId);

            var siteTests = site.Tests;

            var lastTest = siteTests.FirstOrDefault(s => s.Date == siteTests.Max(f => f.Date));

            var refTest =
                site.Tests.FirstOrDefault(z => z.Screenshots.Any(p => p.ScreenType == ScreenshotType.Reference));
            if (lastTest == null || refTest == null)
            {
                return RedirectToAction("Index");
            }

            var invalidScreens = new List<InvalidScreenshots>();

            foreach (var scr in refTest.Screenshots)
            {
                var invalidScreen = lastTest.Screenshots.FirstOrDefault(z => z.ScreenStatus == ScreenshotStatus.Invalid &&  z.Link.ValueUrl == scr.Link.ValueUrl);
                if (invalidScreen != null)
                {
                    invalidScreens.Add(new InvalidScreenshots()
                    {
                        Url = scr.Link.ValueUrl,
                        NewImgUrl = invalidScreen.ImgUrl.GetScrUrl(),
                        RefImgUrl = scr.ImgUrl.GetScrUrl()
                    });
                }
            }

            var model = new TestResultModel();
            model.Screenshots = invalidScreens;

            if (site.Tests.Count == 1)
            {
                model.TestStatusResult = Enums.TestResult.NoNewTests;
                return View(model);
            }
            if (site.Links.Count != lastTest.Screenshots.Count)
            {
                model.TestStatusResult = Enums.TestResult.Running;
                return View(model);
            }
            if (model.Screenshots.Count == 0)
            {
                model.TestStatusResult = Enums.TestResult.Success;
                return View(model);
            }
            
            model.TestStatusResult = Enums.TestResult.HaveFailedScreenshots;

            return View(model);


        }

        [Authorize]
        public ActionResult Screenshots(int? SiteId)
        {

            var tests = db.Tests;
            if (SiteId == null && tests == null )
            {
                return RedirectToAction("Index");
            }
            
            var test = db.Tests.FirstOrDefault(z => z.Site.Id == SiteId && z.Screenshots.Any(p => p.ScreenType == ScreenshotType.Reference));

            

            return View(test);
        }


        [HttpGet]
        [Authorize]
        public ActionResult AddNewSite()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddNewSite(NewSiteModel model)
        {

            var userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(z => z.Id == userId);

            var newSite = new Site()
            {
                Name = model.Name,
                Url = model.Url,
                User = user
            };


            var links = model.Links.Select(z => new Link() { ValueUrl = z, Site = newSite }).ToList();


            //newSite.Links = new List<Link>();
            //newSite.Links.AddRange(links);

            db.Sites.Add(newSite);

            db.Links.AddRange(links);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult StartTest(int SiteId, ScreenshotType type)
        {
            var Site = db.Sites.FirstOrDefault(z => z.Id == SiteId);

            if (Site == null)
            {
                return Json(new { success = false, message = "Site not found " }, JsonRequestBehavior.AllowGet);
            }

            if (type == ScreenshotType.New)
            {
                if (!Site.Tests.Any(z => z.Screenshots.Any(p => p.ScreenType == ScreenshotType.Reference)))
                {
                    return Json(new { success = false, message = "You need to run reference test first" },
                        JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var refTests = Site.Tests.Where(z => z.Screenshots.Any(p => p.ScreenType == ScreenshotType.Reference));
                db.Tests.RemoveRange(refTests);
                db.SaveChanges();
            }


            var Test = new Test()

            {
                Site = Site,
                Date = DateTime.Now
            };

            db.Tests.Add(Test);
            db.SaveChanges();

            var testHelper = new TestsHelper();

            BackgroundJob.Enqueue(() => testHelper.StartTest(Test.Id, Site.Id, type));

            //return Json(new { success = true, url = "/hangfire/jobs/processing" }, JsonRequestBehavior.AllowGet);
            return Json(new { success = true, url = "/Home/TestResult?SiteId=" + Site.Id }, JsonRequestBehavior.AllowGet);

        }

        [Authorize]
        public ActionResult GetLinksFromSite(string Url)
        {
            if (String.IsNullOrEmpty(Url))
                return Json(new { success = false, message = "Empty url" });

            if (!Url.Contains("http"))
            {
                Url = "https://" + Url;
            }


            var siteUrl = Url;
            var doc = new HtmlWeb().Load(siteUrl);


            var linkedPages = (doc.DocumentNode.Descendants("a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(u => !String.IsNullOrEmpty(u) && (u.Contains(siteUrl) || u.StartsWith("/")))).ToList();





            var stack = new Stack<string>(linkedPages);
            while (stack.Any())
            {
                try
                {
                    var tempDoc = new HtmlWeb().Load(stack.First());
                    var additionalLinks = tempDoc.DocumentNode.Descendants("a")
                        .Select(a => a.GetAttributeValue("href", null))
                        .Where(u => !String.IsNullOrEmpty(u) && (u.Contains(siteUrl) || u.StartsWith("/")) && linkedPages.All(k => k != u));
                    linkedPages.AddRange(additionalLinks);


                    foreach (var link in additionalLinks)
                    {
                        stack.Push(link);
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }

                stack.Pop();

            }
            linkedPages.RemoveAll(u => u.EndsWith(".pdf"));
            linkedPages.RemoveAll(u => u.EndsWith(".doc"));
            linkedPages.RemoveAll(u => u.EndsWith(".docx"));
            linkedPages.RemoveAll(u => u.EndsWith(".jpeg"));
            linkedPages.RemoveAll(u => u.EndsWith(".jpg"));
            linkedPages.RemoveAll(u => u.EndsWith(".png"));
            linkedPages.RemoveAll(u => u.EndsWith(".bmp"));
            linkedPages.RemoveAll(u => u.EndsWith(".gif"));
            linkedPages.RemoveAll(u => u.EndsWith(".rss"));
            linkedPages.RemoveAll(u => u.EndsWith(".gzip"));
            linkedPages.RemoveAll(u => u.EndsWith(".zip"));



            if (linkedPages.All(z => z != siteUrl))
                linkedPages.Add(siteUrl);

            if (siteUrl.EndsWith("/"))
                siteUrl = siteUrl.Substring(0, siteUrl.Length - 1);
            var list = new List<string>();
            list.AddRange(linkedPages.Where(z => !z.StartsWith("/"))
                    .ToList());
            list.AddRange(linkedPages.Where(z => z.StartsWith("/")).Select(p => siteUrl + p).ToList());


            return Json(list.Distinct(), JsonRequestBehavior.AllowGet);
        }

        //[Authorize]
        //public ActionResult TakeUrlScreenshots()
        //{

        //}


    }

}