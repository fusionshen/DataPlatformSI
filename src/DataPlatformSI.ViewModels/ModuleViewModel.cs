using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataPlatformSI.ViewModels
{
    public class ModuleViewModel
    {
        [HiddenInput]
        public string Id { set; get; }

        [Required(ErrorMessage = "模块名必填")]
        [Display(Name = "模块名")]
        public string Name { set; get; }

        [Required(ErrorMessage = "命名空间必填")]
        [Display(Name = "命名空间")]
        public string SpaceName { set; get; }

        [Display(Name = "显示名称")]
        public string DisplayName { set; get; }

        [Display(Name = "描述")]
        public string Description { set; get; }

        [Display(Name = "版本号")]
        public string Version { set; get; }
    }
}