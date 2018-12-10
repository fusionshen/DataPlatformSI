using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.ViewModels.Identity
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "邮箱必填")]
        //[Remote("ValidateUsername", "Account",
        //    AdditionalFields = nameof(Username) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [Display(Name = "邮件")]
        public string Email { get; set; }

        [Required(ErrorMessage = "用户名必填")]
        //[Remote("ValidateUsername", "Register",
        //    AdditionalFields = nameof(Email) + "," + ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [RegularExpression("^[a-zA-Z0-9_-]{4,16}$", ErrorMessage = "用户名不满足规范")]
        [Display(Name = "用户名")]
        public string Username { get; set; }

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

        public string PhoneNumber { get; set; }

        public DateTimeOffset? BirthDate { get; set; }

        public string Location { get; set; }

        public int[] RoleIds { get; set; }
    }
}