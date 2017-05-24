using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Entities
{
    public class Link
    {
        public int Id { get; set; }
        public Site Site { get; set; }
        public Screenshot Screenshot { get; set; }
        public string ValueUrl { get; set; }
        

    }
}