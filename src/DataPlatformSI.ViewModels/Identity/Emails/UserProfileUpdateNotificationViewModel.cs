using DataPlatformSI.Entities.Identity;

namespace DataPlatformSI.ViewModels.Identity.Emails
{
    public class UserProfileUpdateNotificationViewModel : EmailsBase
    {
        public User User { set; get; }
    }
}
