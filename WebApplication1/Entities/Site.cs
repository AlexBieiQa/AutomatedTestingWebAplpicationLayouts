using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.Entities
{
    public class Site
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public ApplicationUser User { get; set; }


        public virtual List<Screenshot> Screenshots { get; set; }
        public virtual List<Link> Links { get; set; }
    }
}