using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Entities
{
    public class Test
    {
        public int Id { get; set; }
        public Site Site { get; set; }
        public DateTime Date { get; set; }
        public virtual List<Screenshot> Screenshots { get; set; }
                
    }
}