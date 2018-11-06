using System.ComponentModel.DataAnnotations;

namespace DataPlatformSI.ViewModels.Identity
{
    public class SearchUsersViewModel
    {
        [Display(Name = "TextToFind")]
        public string TextToFind { set; get; }

        [Display(Name = "IsPartOfEmail")]
        public bool IsPartOfEmail { set; get; }

        [Display(Name = "IsUserId")]
        public bool IsUserId { set; get; }

        [Display(Name = "IsPartOfName")]
        public bool IsPartOfName { set; get; }

        [Display(Name = "IsPartOfLastName")]
        public bool IsPartOfLastName { set; get; }

        [Display(Name = "IsPartOfUserName")]
        public bool IsPartOfUserName { set; get; }

        [Display(Name = "IsPartOfLocation")]
        public bool IsPartOfLocation { set; get; }

        [Display(Name = "HasEmailConfirmed")]
        public bool HasEmailConfirmed { set; get; }

        [Display(Name = "UserIsActive")]
        public bool UserIsActive { set; get; }

        [Display(Name = "ShowAllUsers")]
        public bool ShowAllUsers { set; get; }

        [Display(Name = "UserIsLockedOut")]
        public bool UserIsLockedOut { set; get; }

        [Display(Name = "HasTwoFactorEnabled")]
        public bool HasTwoFactorEnabled { set; get; }

        [Display(Name = "MaxNumberOfRows")]
        [Required(ErrorMessage = "(*)")]
        //[Range(1, 1000, ErrorMessage = "عدد وارد شده باید در بازه 1 تا 1000 تعیین شود")]
        public int MaxNumberOfRows { set; get; }

        public PagedUsersListViewModel PagedUsersList { set; get; }

        public SearchUsersViewModel()
        {
            ShowAllUsers = true;
            MaxNumberOfRows = 7;
        }
    }
}