using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Entities
{
    public class Link
    {
        public int Id { get; set; }
        public virtual Site Site { get; set; }
        public virtual Screenshot Screenshot { get; set; }
        public string ValueUrl { get; set; }
        

    }
}