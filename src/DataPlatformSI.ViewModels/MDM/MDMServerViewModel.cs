using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DataPlatformSI.ViewModels.MDM
{
    public class MDMServerViewModel
    {
        [HiddenInput]
        public string Id { set; get; }

        [Required(ErrorMessage = "主数据服务器名称必填")]
        [Display(Name = "主数据服务器名")]
        public string Name { set; get; }
    }
}