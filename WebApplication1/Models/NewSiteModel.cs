using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class NewSiteModel
    {

        public string Name { get; set; }


        public string Url { get; set; }

        public List<string> Links { get; set; }
    }
}