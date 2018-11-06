using DataPlatformSI.Entities.Identity;

namespace DataPlatformSI.ViewModels.Identity.Emails
{
    public class ChangePasswordNotificationViewModel : EmailsBase
    {
        public User User { set; get; }
    }
}