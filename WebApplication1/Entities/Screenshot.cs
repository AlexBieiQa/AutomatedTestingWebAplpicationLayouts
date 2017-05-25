using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Entities
{
    public class Screenshot
    {
        public int Id { get; set; }
        public Site Site { get; set; }

        public string ImgUrl { get; set; }
        public int Date { get; set; }
        public string ScreenBase64 { get; set; }


    }
}