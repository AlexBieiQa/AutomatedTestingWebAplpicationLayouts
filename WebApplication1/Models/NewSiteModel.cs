using System.Collections.Generic;
using WebApplication1.Enums;

namespace WebApplication1.Models
{
    public class NewSiteModel
    {

        public string Name { get; set; }
        public int SiteId { get; set; }
        public string Url { get; set; }
        public int CountOfPages { get; set; }
        public StatusSitesEnum Status { get; set; } = StatusSitesEnum.Failed;
        public string PictureUrl { get; set; }
    }
}