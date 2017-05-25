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

            MD5 md5Hash = MD5.Create();
            foreach (var link in site.Links)
            {

                InitDriver(driver, link);
                OpenQA.Selenium.Screenshot screenshot = null;


                try
                {
                    screenshot = driver.TakeScreenshot();

                }
                catch (Exception ex)
                {

                    driver.Quit();
                    driver = new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), new FirefoxOptions(),
                        timespan);
                    InitDriver(driver, link);
                    screenshot = driver.TakeScreenshot();

                }

                if (screenshot == null)
                    continue;



                //screenshot.SaveAsFile("Test" + timestamp + ".png", ImageFormat.Png);
                var newScreenshot = new Entities.Screenshot()
                {
                    Site = site,
                    Test = test,
                    Date = DateTime.Now,
                    ScreenStatus = ScreenshotStatus.Valid,
                    ScreenType = ScreenshotType.Reference

                };



                string hash = GetMd5Hash(md5Hash, screenshot.AsBase64EncodedString);

                newScreenshot.ScreenBase64 = hash;

                db.Screenshots.Add(newScreenshot);


                var guid = Guid.NewGuid().ToString();

                var path =
                    Path.Combine(
                        HostingEnvironment.MapPath(
                            Path.Combine(new string[]
                            {
                                    "~/App_LocalResources/Screenshots/", site.Id.ToString(),
                                    test.Date.ToString("yyyyMMddTHHmmss"), guid + ".png"
                            })));
                (new FileInfo(path)).Directory.Create();

                newScreenshot.ImgUrl =
                    Path.Combine(new string[]
                        {site.Id.ToString(), test.Date.ToString("yyyyMMddTHHmmss"), guid + ".png"});

                db.SaveChanges();
                File.WriteAllBytes(path, screenshot.AsByteArray);


            }



        }
        public void InitDriver(FirefoxDriver driver, Link link)
        {

            var height = driver.Manage().Window.Size.Height;
            var width = driver.Manage().Window.Size.Width;
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            //var browserHeight = height - Convert.ToInt32((string)js.ExecuteScript(@"return window.innerHeight + '';"));


            driver.Url = link.ValueUrl;


            //string maxSiteHeight = (string)js.ExecuteScript(@"
            //        return Math.max(
            //            document.documentElement.clientHeight,
            //            document.body.scrollHeight,
            //            document.documentElement.scrollHeight,
            //            document.body.offsetHeight,
            //            document.documentElement.offsetHeight
            //        ) +'';
            //    ");

            //var setHeight = Convert.ToInt32(maxSiteHeight) + browserHeight;
            //driver.Manage().Window.Size = new Size(width, setHeight);
        }

        public string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

    }





}
