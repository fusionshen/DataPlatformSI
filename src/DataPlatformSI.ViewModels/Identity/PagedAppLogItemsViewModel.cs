using cloudscribe.Web.Pagination;
using DataPlatformSI.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.ViewModels.Identity
{
    public class PagedAppLogItemsViewModel
    {
        public PagedAppLogItemsViewModel()
        {
            Paging = new PaginationSettings();
        }

        public string LogLevel { get; set; } = string.Empty;

        public List<AppLogItem> AppLogItems { get; set; }

        public PaginationSettings Paging { get; set; }
    }
}