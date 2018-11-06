using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataPlatformSI.ViewModels.Identity
{
    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Display(Name = "代码")]
        [Required(ErrorMessage = "(*)")]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "记住浏览器")]
        public bool RememberBrowser { get; set; }

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; }
    }
}