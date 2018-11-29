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

        [Required(ErrorMessage = "角色名必填")]
        [Display(Name = "角色名")]
        public string Name { set; get; }

        [Required(ErrorMessage = "显示名称必填")]
        [Display(Name = "显示名称")]
        public string DisplayName { set; get; }

        [Display(Name = "描述")]
        public string Description  { set; get; }

        public string[] ActionIds { set; get; }
    }
}
