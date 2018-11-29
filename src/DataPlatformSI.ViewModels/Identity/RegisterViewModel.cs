using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.ViewModels.Identity
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "邮箱必填")]
        //[Remote("ValidateUsername", "Register",
        //    AdditionalFields = nameof(Username) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [Display(Name = "邮件")]
        public string Email { get; set; }

        [Required(ErrorMessage = "用户名必填")]
        [Display(Name = "用户名")]
        //[Remote("ValidateUsername", "Register",
        //    AdditionalFields = nameof(Email) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [RegularExpression("^[a-zA-Z0-9_-]{4,16}$", ErrorMessage = "用户名不满足规范")]
        public string Username { get; set; }

        [Required(ErrorMessage = "密码必填")]
        [StringLength(100, ErrorMessage = "{0}必须至少为{2}且最多为{1}个字符", MinimumLength = 6)]
        //[Remote("ValidatePassword", "Register",
        //    AdditionalFields = nameof(Username) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "重复密码必填")]
        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare(nameof(Password), ErrorMessage = "输入的密码不匹配")]
        public string ConfirmPassword { get; set; }

        //[Required(ErrorMessage = "(*)")]
        [StringLength(450)]
        //[RegularExpression(@"^[\u0600-\u06FF,\u0590-\u05FF\s]*$",
        //                  ErrorMessage = "لطفا تنها از حروف فارسی استفاده نمائید")]
        [Display(Name = "名")]
        public string FirstName { get; set; }

        //[Required(ErrorMessage = "(*)")]
        [StringLength(450)]
        //[RegularExpression(@"^[\u0600-\u06FF,\u0590-\u05FF\s]*$",
        //                  ErrorMessage = "لطفا تنها از حروف فارسی استفاده نمائید")]
        [Display(Name = "姓")]
        public string LastName { get; set; }

       
    }
}