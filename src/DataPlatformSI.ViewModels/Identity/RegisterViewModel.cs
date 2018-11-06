using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.ViewModels.Identity
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "(*)")]
        [Display(Name = "用户名")]
        [Remote("ValidateUsername", "Register",
            AdditionalFields = nameof(Email) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [RegularExpression("^[a-zA-Z_]*$", ErrorMessage = "用户名不满足规范")]
        public string Username { get; set; }

        [Display(Name = "名")]
        [Required(ErrorMessage = "(*)")]
        [StringLength(450)]
        //[RegularExpression(@"^[\u0600-\u06FF,\u0590-\u05FF\s]*$",
        //                  ErrorMessage = "لطفا تنها از حروف فارسی استفاده نمائید")]
        public string FirstName { get; set; }

        [Display(Name = "姓")]
        [Required(ErrorMessage = "(*)")]
        [StringLength(450)]
        //[RegularExpression(@"^[\u0600-\u06FF,\u0590-\u05FF\s]*$",
        //                  ErrorMessage = "لطفا تنها از حروف فارسی استفاده نمائید")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "(*)")]
        //[Remote("ValidateUsername", "Register",
        //    AdditionalFields = nameof(Username) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        //[EmailAddress(ErrorMessage = "لطفا آدرس ایمیل معتبری را وارد نمائید.")]
        [Display(Name = "邮件地址")]
        public string Email { get; set; }

        [Required(ErrorMessage = "(*)")]
        //[StringLength(100, ErrorMessage = "{0} باید حداقل {2} و حداکثر {1} حرف باشند.", MinimumLength = 6)]
        //[Remote("ValidatePassword", "Register",
        //    AdditionalFields = nameof(Username) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "(*)")]
        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        //[Compare(nameof(Password), ErrorMessage = "کلمات عبور وارد شده با هم تطابق ندارند")]
        public string ConfirmPassword { get; set; }
    }
}