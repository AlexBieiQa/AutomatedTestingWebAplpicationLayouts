using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Extensions;
using WebApplication1.Enums;
using WebApplication1.Models;

namespace WebApplication1.Helpers
{
    public class TestsHelper
    {
        public ApplicationDbContext db { get; set; } = new ApplicationDbContext();


        public void StartTest(int testId, int siteId)
        {
            var site = db.Sites.ToList().FirstOrDefault(c => c.Id == siteId);

            if (site == null)
            {
                throw new Exception("No site");
            }
            var test = db.Tests.ToList().FirstOrDefault(c => c.Id == testId);

            if (test == null)
            {
                throw new Exception("No test");
            }
            var timespan = TimeSpan.FromMinutes(100);
            var driver = new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), new FirefoxOptions(), timespan);
            
            var height = driver.Manage().Window.Size.Height;
            var width = driver.Manage().Window.Size.Width;
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            foreach (var link in site.Links)
            {


                var browserHeight = height - Convert.ToInt32((string)js.ExecuteScript(@"return window.innerHeight + '';"));



                driver.Url = link.ValueUrl;
                
                string maxSiteHeight = (string)js.ExecuteScript(@"
                    return Math.max(
                        document.documentElement.clientHeight,
                        document.body.scrollHeight,
                        document.documentElement.scrollHeight,
                        document.body.offsetHeight,
                        document.documentElement.offsetHeight
                    ) +'';
            ");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd-hhmm-ss");
                var setHeight = Convert.ToInt32(maxSiteHeight) + browserHeight;
                driver.Manage().Window.Size = new Size(width, setHeight);


                var screenshot = driver.TakeScreenshot();


                var baseString = screenshot.AsBase64EncodedString;
                //screenshot.SaveAsFile("Test" + timestamp + ".png", ImageFormat.Png);
                var newScreenshot = new Entities.Screenshot()
                {
                    Site = site,
                    Test = test,
                    Date = DateTime.Now,
                    ScreenBase64 = baseString,
                    ScreenStatus = ScreenshotStatus.Valid,
                    ScreenType = ScreenshotType.Reference

                };
                db.Screenshots.Add(newScreenshot);

            //driver.Close();
            }
            db.SaveChanges();


        }
    }
}