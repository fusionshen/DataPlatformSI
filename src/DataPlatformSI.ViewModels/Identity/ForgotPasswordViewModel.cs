using System.ComponentModel.DataAnnotations;
namespace DataPlatformSI.ViewModels.Identity
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "(*)")]
        //[EmailAddress(ErrorMessage = "لطفا آدرس ایمیل معتبری را وارد نمائید.")]
        [Display(Name = "邮箱地址")]
        public string Email { get; set; }
    }
}
