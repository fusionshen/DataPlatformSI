using DataPlatformSI.Entities.Identity;
using System.Collections.Generic;

namespace DataPlatformSI.ViewModels.Identity
{
    public class TodayBirthDaysViewModel
    {
        public List<User> Users { set; get; }

        public AgeStatViewModel AgeStat { set; get; }
    }
}
