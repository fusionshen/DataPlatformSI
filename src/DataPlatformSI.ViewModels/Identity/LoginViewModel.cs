using System.ComponentModel.DataAnnotations;

namespace DataPlatformSI.ViewModels.Identity
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "(*)")]
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [Required(ErrorMessage = "(*)")]
        [Display(Name = "密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "记住密码")]
        public bool RememberMe { get; set; }
    }
}
