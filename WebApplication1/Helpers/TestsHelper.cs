using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Extensions;
using WebApplication1.Entities;
using WebApplication1.Enums;
using WebApplication1.Models;

namespace WebApplication1.Helpers
{
    public class TestsHelper
    {
        public ApplicationDbContext db { get; set; } = new ApplicationDbContext();


        private int StartHeight { get; set; }
        private int BrowserHeight { get; set; }
        private int StartWidth { get; set; }
        private FirefoxDriver driver { get; set; }
        private TimeSpan timespan { get; set; }
        private IJavaScriptExecutor js { get; set; }



        public TestsHelper()
        {
            timespan = TimeSpan.FromMinutes(100);

            driver = new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), new FirefoxOptions(), timespan);

            StartHeight = driver.Manage().Window.Size.Height;
            StartWidth = driver.Manage().Window.Size.Width;
            js = (IJavaScriptExecutor)driver;

            BrowserHeight = StartHeight - Convert.ToInt32(js.ExecuteScript(@"return window.innerHeight;"));
        }

        public void StartTest(int testId, int siteId, ScreenshotType type)
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

            foreach (var link in site.Links)
            {

                InitDriver(driver, js, link);
                OpenQA.Selenium.Screenshot screenshot = null;


                //Getting the screenshot
                try
                {
                    screenshot = driver.TakeScreenshot();

                }
                catch (Exception ex)
                {

                    driver.Quit();
                    driver = new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), new FirefoxOptions(),
                        timespan);
                    InitDriver(driver,  js, link);
                    screenshot = driver.TakeScreenshot();

                }

                if (screenshot == null)
                    continue;



                //Saving screenshot
                var newScreenshot = new Entities.Screenshot()
                {
                    Site = site,
                    Test = test,
                    Date = DateTime.Now,
                    ScreenType = type
                };


                string hash = ExtensionMethods.GetMd5Hash(screenshot.AsBase64EncodedString);

                newScreenshot.ScreenBase64 = hash;


 
                var imageName  = Guid.NewGuid().ToString();

                var path =
                    Path.Combine(
                        HostingEnvironment.MapPath(
                            Path.Combine(new string[]
                            {
                                    "~/Content/Screenshots/", site.Id.ToString(),
                                    test.Date.ToString("yyyyMMddTHHmmss"), imageName + ".png"
                            })));
                (new FileInfo(path)).Directory.Create();

                newScreenshot.ImgUrl =
                    Path.Combine(new string[]
                        {site.Id.ToString(), test.Date.ToString("yyyyMMddTHHmmss"), imageName + ".png"});

                File.WriteAllBytes(path, screenshot.AsByteArray);

                if (newScreenshot.ScreenType == ScreenshotType.Reference)
                {
                    newScreenshot.ScreenStatus = ScreenshotStatus.Valid;
                    link.Screenshot = newScreenshot;
                }
                else
                {
                    if (link.Screenshot.ScreenBase64 != newScreenshot.ScreenBase64)
                        newScreenshot.ScreenStatus = ScreenshotStatus.Invalid;
                    else
                        newScreenshot.ScreenStatus = ScreenshotStatus.Valid;
                }


                db.Screenshots.Add(newScreenshot);



                db.SaveChanges();

            }

            driver.Quit();

        }


        //Driver resize method
        public void InitDriver(FirefoxDriver driver, IJavaScriptExecutor js, Link link)
        {
            driver.Url = link.ValueUrl;
            driver.Manage().Window.Size = new Size(StartWidth+1, StartHeight+1);

            var maxSiteHeight = Convert.ToInt32(js.ExecuteScript(@"
                    return Math.max(
                        document.documentElement.clientHeight,
                        document.body.scrollHeight,
                        document.documentElement.scrollHeight,
                        document.body.offsetHeight,
                        document.documentElement.offsetHeight
                    );
                "));
            
            var setHeight = Convert.ToInt32(maxSiteHeight) + BrowserHeight;
            driver.Manage().Window.Size = new Size(StartWidth, setHeight);
        }

       
    }





}
