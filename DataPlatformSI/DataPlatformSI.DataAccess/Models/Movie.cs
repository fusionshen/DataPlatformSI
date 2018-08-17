using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.DataAccess.Models
{
    public class Movie
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public int Year { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public bool IsCheckedOut
        {
            get { return DueDate.HasValue; }
        }
    }
}
