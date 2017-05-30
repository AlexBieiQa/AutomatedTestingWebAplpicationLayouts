using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Enums;

namespace WebApplication1.Entities
{
    public class Screenshot
    {
        public int Id { get; set; }
        public Site Site { get; set; }
        public Test Test { get; set; }
        public virtual Link Link { get; set; }

        public string ImgUrl { get; set; }
        public DateTime Date { get; set; }
        public string ScreenBase64 { get; set; }
        public ScreenshotType ScreenType { get; set; }
        public ScreenshotStatus ScreenStatus { get; set; }

    }
}