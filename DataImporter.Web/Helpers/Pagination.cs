using JW;
using System;

namespace DataImporter.Web.Helpers
{
    public class Pagination
    {
        public Pagination(string baseUrl, string pageTag, int totalItems, int currentPage, int pageSize = 10, int maxPages = 10)
        {
            this.Pager = new Pager(totalItems, currentPage, pageSize);
            this.BaseUrl = baseUrl;
            this.PageTag = pageTag;
        }

        public Pagination(Func<int, string> onClick, int totalItems, int currentPage, int pageSize = 10, int maxPages = 10)
        {
            this.Pager = new Pager(totalItems, currentPage, pageSize);
            this.OnClick = onClick;
        }

        public Pager Pager { get; set; }

        public string BaseUrl { get; set; }

        public string PageTag { get; set; }

        public Func<int, string> OnClick { get; set; }
    }
}
