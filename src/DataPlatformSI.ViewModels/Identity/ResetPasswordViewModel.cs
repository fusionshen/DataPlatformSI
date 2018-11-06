using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.ViewModels.Identity
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "(*)")]
        //[EmailAddress(ErrorMessage = "لطفا آدرس ایمیل معتبری را وارد نمائید.")]
        [Display(Name = "邮箱地址")]
        public string Email { get; set; }

        [Required(ErrorMessage = "(*)")]
        //[StringLength(100, ErrorMessage = "{0} باید حداقل {2} و حداکثر {1} حرف باشند.", MinimumLength = 6)]
        //[Remote("ValidatePassword", "ForgotPassword",
        //    AdditionalFields = nameof(Email) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "(*)")]
        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        //[Compare(nameof(Password), ErrorMessage = "کلمات عبور وارد شده با هم تطابق ندارند")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
