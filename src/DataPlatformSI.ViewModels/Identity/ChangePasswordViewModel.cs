using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.ViewModels.Identity
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "(*)")]
        [DataType(DataType.Password)]
        [Display(Name = "原密码")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "(*)")]
        //[StringLength(100, ErrorMessage = "{0} باید حداقل {2} و حداکثر {1} حرف باشند.", MinimumLength = 6)]
        [Remote("ValidatePassword", "ChangePassword",
            AdditionalFields = ViewModelConstants.AntiForgeryToken, HttpMethod = "POST")]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "(*)")]
        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare(nameof(NewPassword), ErrorMessage = "两次密码输入不一致")]
        public string ConfirmPassword { get; set; }

        public DateTimeOffset? LastUserPasswordChangeDate { get; set; }
    }
}
