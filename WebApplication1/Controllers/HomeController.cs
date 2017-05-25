using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using WebApplication1.Models;
using WebApplication1.Entities;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        public ApplicationDbContext db { get; set; } = new ApplicationDbContext();

        public ActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
                return RedirectToAction("Sites");
            return View();
        }


        [Authorize]
        public ActionResult Sites()
        {
            var userId = User.Identity.GetUserId();
            var sites = db.Sites.Where(z => z.User.Id == userId).ToList();

            var model = db.Sites.Select(s => new NewSiteModel
            {
                SiteId = s.Id,
                Name = s.Name,
                CountOfPages = s.Links.Count,
                Url = s.Url

            }).ToList();

            return View(model);
        }

        [Authorize]
        public ActionResult SitePages(int SiteId)
        {
            var links = db.Links.Where(z => z.Site.Id == SiteId).ToList();
            
            return View(links);
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


            var links = db.Links.Select(z => new Link { ValueUrl = z.ValueUrl, Site = newSite}).ToList();

            //newSite.Links = new List<Link>();
            //newSite.Links.AddRange(links);

            db.Sites.Add(newSite);

            db.Links.AddRange(links);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult GetLinksFromSite(string Url)
        {
            if (!Url.Contains("http"))
            {
                Url = "https://" + Url;
            }
            var siteUrl = Url;
            var doc = new HtmlWeb().Load(siteUrl);


            var linkedPages = (doc.DocumentNode.Descendants("a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(u => !String.IsNullOrEmpty(u) && u.Contains(siteUrl))).ToList();

            var stack = new Stack<string>(linkedPages);
            while (stack.Any())
            {
                var tempDoc = new HtmlWeb().Load(stack.First());
                var additionalLinks = tempDoc.DocumentNode.Descendants("a")
                    .Select(a => a.GetAttributeValue("href", null))
                    .Where(u => !String.IsNullOrEmpty(u) && u.Contains(siteUrl) && linkedPages.All(k => k != u));
                linkedPages.AddRange(additionalLinks);


                foreach (var link in additionalLinks)
                {
                    stack.Push(link);
                }

                stack.Pop();
            }

            return Json(linkedPages, JsonRequestBehavior.AllowGet);
        }

        //[Authorize]
        //public ActionResult TakeUrlScreenshots()
        //{
            
        //}


    }

}