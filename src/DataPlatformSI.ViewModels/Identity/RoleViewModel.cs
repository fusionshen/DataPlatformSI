using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataPlatformSI.ViewModels.Identity
{
    public class RoleViewModel
    {
        [HiddenInput]
        public string Id { set; get; }

        [Required(ErrorMessage = "(*)")]
        [Display(Name = "角色名")]
        public string Name { set; get; }
    }
}
